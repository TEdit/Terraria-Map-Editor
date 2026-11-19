using Shouldly;
using TEdit.Scripting.Execution;
using Xunit;

namespace TEdit.Tests.Scripting;

public class WorldOperationCoordinatorTests
{
    [Fact]
    public async Task ChildOperationRetainsGateAfterParentReturns()
    {
        var gate = new WorldOperationCoordinator();
        var world = new object();
        gate.TrySetWorld(world);
        gate.TryAcquire(true, null, out var parent);
        WorldOperationCoordinator.Lease? child;
        using (parent!.Enter())
        {
            gate.TryAcquire(false, null, out child, allowReentry: true).ShouldBe(OperationRejection.None);
            await Task.Run(() => gate.CanMutate.ShouldBeTrue());
        }
        gate.IsOwner.ShouldBeFalse();
        parent.Dispose();
        gate.IsBusy.ShouldBeTrue();
        gate.TryAcquire(true, null, out _).ShouldBe(OperationRejection.Busy);
        child!.Dispose();
        gate.IsBusy.ShouldBeFalse();
        gate.GetRevision(world)!.Value.Revision.ShouldBe(1);
    }

    [Fact]
    public void NestedReadCannotPromoteToWriteAndStaleNestedRequestIsRejected()
    {
        var gate = new WorldOperationCoordinator();
        var world = new object();
        gate.TrySetWorld(world);
        var revision = gate.GetRevision(world);
        gate.TryAcquire(false, revision, out var read);
        using (read)
        using (read!.Enter())
        {
            gate.TryAcquire(true, revision, out _, allowReentry: true).ShouldBe(OperationRejection.Busy);
            gate.NotifyMutation();
            gate.TryAcquire(false, revision, out _, allowReentry: true).ShouldBe(OperationRejection.StaleWorld);
        }
    }

    [Fact]
    public void NestedReadDoesNotInheritMutationAuthority()
    {
        var gate = new WorldOperationCoordinator();
        gate.TrySetWorld(new object());
        gate.TryAcquire(true, null, out var write);
        using (write)
        using (write!.Enter())
        {
            gate.TryAcquire(false, null, out var read, allowReentry: true).ShouldBe(OperationRejection.None);
            using (read)
            using (read!.Enter())
            {
                gate.CanMutate.ShouldBeFalse();
                gate.TryAcquire(true, null, out _, allowReentry: true).ShouldBe(OperationRejection.Busy);
                gate.TrySetWorld(new object(), allowOwnedChange: true).ShouldBe(OperationRejection.Busy);
            }
            gate.CanMutate.ShouldBeTrue();
        }
    }

    [Fact]
    public void MissingWorldAndStaleRequestsAreRejected()
    {
        var gate = new WorldOperationCoordinator();
        gate.TryAcquire(true, null, out _).ShouldBe(OperationRejection.NoWorld);
        var world = new object();
        gate.TrySetWorld(world).ShouldBe(OperationRejection.None);
        var before = gate.GetRevision(world)!.Value;
        gate.TryAcquire(true, before, out var edit).ShouldBe(OperationRejection.None);
        edit!.Dispose();
        gate.GetRevision(world).ShouldBe(before with { Revision = 1 });
        gate.TryAcquire(true, before, out _).ShouldBe(OperationRejection.StaleWorld);
    }

    [Fact]
    public void ReadersWritersAndWorldReplacementExcludeOneAnother()
    {
        var gate = new WorldOperationCoordinator();
        var world = new object();
        gate.TrySetWorld(world);
        var before = gate.GetRevision(world);
        gate.TryAcquire(false, before, out var read).ShouldBe(OperationRejection.None);
        gate.TryAcquire(true, before, out _).ShouldBe(OperationRejection.Busy);
        gate.TryAcquire(false, before, out _).ShouldBe(OperationRejection.Busy);
        gate.TrySetWorld(new object()).ShouldBe(OperationRejection.Busy);
        read!.Dispose();
        gate.GetRevision(world).ShouldBe(before);
        gate.TryAcquire(true, before, out var write).ShouldBe(OperationRejection.None);
        gate.TryAcquire(false, null, out _).ShouldBe(OperationRejection.Busy);
        write!.Dispose();
    }

    [Fact]
    public void ReplacementChangesIdentityAndDoubleDisposeDoesNotInvalidateNewLease()
    {
        var gate = new WorldOperationCoordinator();
        var world = new object();
        gate.TrySetWorld(world);
        var first = gate.GetRevision(world);
        gate.TrySetWorld(world);
        gate.GetRevision(world).ShouldBe(first);
        gate.TryAcquire(true, null, out var edit);
        edit!.Dispose();
        gate.TryAcquire(false, null, out var read);
        edit.Dispose();
        gate.IsBusy.ShouldBeTrue();
        read!.Dispose();
        gate.GetRevision(world)!.Value.Revision.ShouldBe(1);
        world = new object();
        gate.GetRevision(world).ShouldBeNull();
        gate.TrySetWorld(world);
        gate.GetRevision(world)!.Value.WorldId.ShouldNotBe(first!.Value.WorldId);
        gate.TryAcquire(true, first, out _).ShouldBe(OperationRejection.StaleWorld);
        gate.TrySetWorld(null);
        gate.GetRevision(world).ShouldBeNull();
    }

    [Fact]
    public async Task ConcurrentAcquisitionHasExactlyOneOwner()
    {
        var gate = new WorldOperationCoordinator();
        var world = new object();
        gate.TrySetWorld(world);
        var attempts = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
        {
            var rejection = gate.TryAcquire(true, null, out var lease);
            return (rejection, lease);
        })));
        attempts.Count(a => a.rejection == OperationRejection.None).ShouldBe(1);
        attempts.Count(a => a.rejection == OperationRejection.Busy).ShouldBe(15);
        foreach (var attempt in attempts) attempt.lease?.Dispose();
        gate.IsBusy.ShouldBeFalse();
    }
}
