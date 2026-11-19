using Shouldly;
using TEdit.Scripting.Execution;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class WorldChangeTrackerTests
{
    [Fact]
    public void PropertyAndNestedEntityChangesInvalidateRevision()
    {
        var world = ExecutionWorldFixture.Create();
        var npc = new NPC { DisplayName = "before" };
        world.NPCs.Add(npc);
        var gate = new WorldOperationCoordinator();
        gate.TrySetWorld(world);
        using var tracker = new WorldChangeTracker(world, gate.NotifyMutation);
        var before = gate.GetRevision(world)!.Value;
        world.Title = "changed";
        gate.GetRevision(world)!.Value.Revision.ShouldBeGreaterThan(before.Revision);
        before = gate.GetRevision(world)!.Value;
        npc.DisplayName = "after";
        gate.GetRevision(world)!.Value.Revision.ShouldBeGreaterThan(before.Revision);
        npc.Home = new(1, 2);
        before = gate.GetRevision(world)!.Value;
        npc.Home.X = 3;
        gate.GetRevision(world)!.Value.Revision.ShouldBeGreaterThan(before.Revision);
    }

    [Fact]
    public void UncoordinatedPropertyWriteIsRejectedBeforeAssignment()
    {
        var world = ExecutionWorldFixture.Create();
        var gate = new WorldOperationCoordinator();
        gate.TrySetWorld(world);
        using var tracker = new WorldChangeTracker(world, gate.NotifyMutation, () => !gate.IsBusy || gate.IsOwner);
        gate.TryAcquire(true, null, out var edit);
        using (edit)
        {
            var original = world.Title;
            Should.Throw<InvalidOperationException>(() => world.Title = "unowned");
            world.Title.ShouldBe(original);
            using (edit!.Enter()) world.Title = "owned";
            world.Title.ShouldBe("owned");
        }
    }
}
