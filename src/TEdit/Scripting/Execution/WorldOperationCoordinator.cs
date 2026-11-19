using System;
using System.Threading;

namespace TEdit.Scripting.Execution;

public readonly record struct WorldRevision(Guid WorldId, long Revision);

public enum OperationRejection { None, NoWorld, Busy, StaleWorld }

/// <summary>
/// An exclusive, nonblocking gate. All readers and writers must participate before
/// its revisions can be used as an external consistency guarantee.
/// </summary>
public sealed class WorldOperationCoordinator
{
    private readonly object _sync = new();
    private object? _world;
    private WorldRevision _revision;
    private Lease? _active;
    private readonly AsyncLocal<Lease?> _ambient = new();

    public event Action? StateChanged;
    public string? ActiveName { get { lock (_sync) return _active?.Name; } }
    public bool IsOwner { get { lock (_sync) return _active != null && ReferenceEquals(_ambient.Value?.Root, _active); } }
    public bool CanMutate { get { lock (_sync) return IsOwner && _ambient.Value!.Mutation; } }
    public CancellationToken ActiveCancellation { get { lock (_sync) return _active?.Cancellation.Token ?? CancellationToken.None; } }

    public void CancelActive()
    {
        CancellationTokenSource? cancellation;
        lock (_sync) cancellation = _active?.Cancellation;
        // Cancellation callbacks may dispatch to the UI, which also needs the gate.
        try { cancellation?.Cancel(); }
        catch (ObjectDisposedException) { } // The operation completed concurrently.
    }

    public bool IsBusy { get { lock (_sync) return _active != null; } }
    // A revision is meaningful only for the exact world the caller inspected.
    // A world installed outside this coordinator has no usable revision yet.
    public WorldRevision? GetRevision(object? world)
    {
        lock (_sync) return world != null && ReferenceEquals(world, _world) ? _revision : null;
    }

    public OperationRejection TrySetWorld(object? world, bool allowOwnedChange = false)
    {
        lock (_sync)
        {
            if (_active != null && !(allowOwnedChange && CanMutate)) return OperationRejection.Busy;
            if (!ReferenceEquals(world, _world))
            {
                _world = world;
                _revision = new WorldRevision(Guid.NewGuid(), 0);
            }
            return OperationRejection.None;
        }
    }

    public OperationRejection TryAcquire(bool mutation, WorldRevision? expected, out Lease? lease,
        string name = "Operation", bool allowNoWorld = false, bool allowReentry = false)
    {
        lock (_sync)
        {
            lease = null;
            if (_world == null && !allowNoWorld) return OperationRejection.NoWorld;
            if (expected.HasValue && expected.Value != _revision) return OperationRejection.StaleWorld;
            if (_active != null)
            {
                if (!allowReentry || !IsOwner || (mutation && !CanMutate)) return OperationRejection.Busy;
                _active.References++;
                lease = new Lease(this, mutation, _revision, name, _active);
                return OperationRejection.None;
            }
            lease = _active = new Lease(this, mutation, _revision, name);
        }
        if (name != "Viewport render" && name != "Mouse move") StateChanged?.Invoke();
        return OperationRejection.None;
    }

    public void NotifyMutation()
    {
        lock (_sync)
        {
            if (_world != null) _revision = _revision with { Revision = _revision.Revision + 1 };
        }
    }

    private void Release(Lease lease)
    {
        bool notify;
        lock (_sync)
        {
            if (!ReferenceEquals(_active, lease.Root)) return;
            if (--_active.References > 0) return;
            // Conservative invalidation also covers a failed or partially applied mutation.
            if (_active.Mutation) _revision = _revision with { Revision = _revision.Revision + 1 };
            _active.Cancellation.Dispose();
            notify = _active.Name != "Viewport render" && _active.Name != "Mouse move";
            _active = null;
        }
        if (notify) StateChanged?.Invoke();
    }

    public sealed class Lease : IDisposable
    {
        private WorldOperationCoordinator? _owner;
        internal bool Mutation { get; }
        internal Lease Root { get; }
        internal int References = 1;
        internal CancellationTokenSource Cancellation { get; }
        public string Name { get; }
        public WorldRevision InitialRevision { get; }

        internal Lease(WorldOperationCoordinator owner, bool mutation, WorldRevision revision, string name, Lease? root = null)
        {
            _owner = owner;
            Mutation = mutation;
            InitialRevision = revision;
            Name = name;
            Root = root ?? this;
            Cancellation = root?.Cancellation ?? new CancellationTokenSource();
        }

        public IDisposable Enter()
        {
            var owner = _owner ?? throw new ObjectDisposedException(nameof(Lease));
            var previous = owner._ambient.Value;
            owner._ambient.Value = this;
            return new OwnershipScope(() => owner._ambient.Value = previous);
        }

        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Release(this);
    }

    private sealed class OwnershipScope(Action restore) : IDisposable
    {
        private Action? _restore = restore;
        public void Dispose() => Interlocked.Exchange(ref _restore, null)?.Invoke();
    }
}
