using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TEdit.Mcp;

internal sealed record OperationInfo(Guid Id, string Status, string? ErrorCode, int? ChangedTiles, bool PartialChanges,
    Scripting.ScriptOutput? Script = null);
internal sealed record EditOutcome(string Status, string? ErrorCode, int ChangedTiles, Scripting.ScriptOutput? Script = null);
internal sealed record Admission(string? ErrorCode, OperationInfo? Operation = null);

// A UUIDv7 timestamp lets us reject retries even after pruning their ledger records.
internal sealed class McpOperations(Guid instance, TimeProvider clock, Func<Guid, bool> authorized)
{
    private sealed class Entry(Guid owner, Guid request, string payload)
    {
        public readonly Guid Id = Guid.NewGuid();
        public readonly Guid Owner = owner, Request = request;
        public readonly string Payload = payload;
        public readonly CancellationTokenSource Cancellation = new();
        public Task Completion = Task.CompletedTask;
        public DateTimeOffset? Finished;
        public string Status = "running";
        public Task CancellationCompleted = Task.CompletedTask;
        public void RequestCancel()
        {
            if (Status != "running") return;
            Status = "cancel_requested";
            CancellationCompleted = Cancellation.CancelAsync();
        }
        public EditOutcome? Outcome;
        public OperationInfo Snapshot() => new(Id, Status, Outcome?.ErrorCode ?? (Status == "failed" ? "operation_failed" : null),
            Outcome?.ChangedTiles, Status != "succeeded" && Outcome?.ChangedTiles > 0, Outcome?.Script);
    }
    private readonly object _sync = new();
    private readonly List<Entry> _entries = [];
    private bool _stopping;
    private static readonly TimeSpan Retention = TimeSpan.FromMinutes(10);

    public Admission Start(Guid owner, Guid expectedInstance, Guid request, string payloadHash, Func<CancellationToken, Task> work) =>
        StartEdit(owner, expectedInstance, request, payloadHash, async ct => { await work(ct); return null; });

    public Admission StartEdit(Guid owner, Guid expectedInstance, Guid request, string payloadHash, Func<CancellationToken, Task<EditOutcome?>> work)
    {
        lock (_sync)
        {
            if (!authorized(owner)) return new("profile_revoked");
            if (_stopping) return new("stopping");
            if (instance != expectedInstance) return new("instance_changed");
            var now = clock.GetUtcNow();
            Prune(now);
            if (request.Version != 7 || (request.Variant & 0xc) != 8) return new("invalid_request_id");
            var issued = Issued(request);
            if (issued > now.AddSeconds(5).ToUnixTimeMilliseconds()) return new("invalid_request_id");
            var existing = _entries.Find(e => e.Owner == owner && e.Request == request);
            if (existing != null) return existing.Payload == payloadHash ? new(null, existing.Snapshot()) : new("payload_conflict");
            if (issued <= (now - Retention).ToUnixTimeMilliseconds()) return new("expired_request");
            if (owner != Guid.Empty && _entries.Count(e => e.Owner == owner) >= 32) return new("profile_ledger_full");
            if (_entries.Count >= 128) return new("ledger_full");
            var entry = new Entry(owner, request, payloadHash);
            _entries.Add(entry);
            entry.Completion = Task.Run(async () =>
            {
                string status;
                EditOutcome? outcome = null;
                try { outcome = await work(entry.Cancellation.Token); status = outcome?.Status ?? "succeeded"; }
                catch (OperationCanceledException) when (entry.Cancellation.IsCancellationRequested) { status = "canceled"; }
                catch { status = "failed"; }
                lock (_sync) { entry.Finished = clock.GetUtcNow(); entry.Outcome = outcome; entry.Status = status; }
            });
            return new(null, entry.Snapshot());
        }
    }

    private static long Issued(Guid id) => Convert.ToInt64(id.ToString("N")[..12], 16);

    private void Prune(DateTimeOffset now) => _entries.RemoveAll(e =>
    {
        if (!e.Completion.IsCompleted || !e.CancellationCompleted.IsCompleted || e.Finished > now - Retention || Issued(e.Request) > (now - Retention).ToUnixTimeMilliseconds()) return false;
        e.Cancellation.Dispose();
        return true;
    });

    // For a request that is still associated with a protocol call. Dispose the
    // registration as soon as its ID is returned; polling clients cancel explicitly.
    public CancellationTokenRegistration ObserveRequest(Guid owner, Guid operation, CancellationToken request) =>
        request.Register(() => Find(owner, operation, true));

    public OperationInfo[] History()
    {
        lock (_sync) { Prune(clock.GetUtcNow()); return _entries.Select(e => e.Snapshot()).ToArray(); }
    }

    // The local Options UI may manage any operation. Remote calls always supply an owner.
    public OperationInfo? Find(Guid? owner, Guid id, bool cancel)
    {
        lock (_sync)
        {
            Prune(clock.GetUtcNow());
            var e = _entries.Find(x => x.Id == id && (owner == null || owner == x.Owner));
            if (cancel) e?.RequestCancel();
            return e?.Snapshot();
        }
    }

    public Task DrainAsync(Guid? owner = null)
    {
        lock (_sync)
        {
            if (owner == null) _stopping = true;
            var entries = _entries.Where(e => owner == null || owner == e.Owner).ToArray();
            foreach (var e in entries) e.RequestCancel();
            return Task.WhenAll(entries.SelectMany(e => new[] { e.CancellationCompleted, e.Completion }));
        }
    }
}
