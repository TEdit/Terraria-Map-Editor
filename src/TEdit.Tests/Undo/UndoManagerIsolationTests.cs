using TEdit.Editor.Undo;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Undo;

public sealed class UndoManagerIsolationTests
{
    [Fact]
    public void NewManager_DoesNotRedoFilesLeftByPreviousWorld()
    {
        // World A: one edit, then undo, which leaves a redo file behind.
        var worldA = TestWorldFactory.CreateSmallWorld();
        using (var a = new UndoManager(worldA, notifyTileChanged: null, undoApplied: () => { }))
        {
            a.SaveTile(1, 1);
            worldA.Tiles[1, 1].IsActive = true;
            worldA.Tiles[1, 1].Type = 1;
            a.SaveUndo();
            a.Undo();
        }

        // World B (a fresh manager) has no history; Redo must not replay A's file.
        var worldB = TestWorldFactory.CreateSmallWorld();
        using var b = new UndoManager(worldB, notifyTileChanged: null, undoApplied: () => { });

        var changed = b.Redo();

        Assert.Empty(changed);
        Assert.False(worldB.Tiles[1, 1].IsActive);
    }

    [Fact]
    public void Dispose_RemovesTheManagersUndoFiles()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var manager = new UndoManager(world, notifyTileChanged: null, undoApplied: () => { });
        manager.SaveTile(2, 2);
        manager.SaveUndo();
        string undoFolder = System.IO.Path.GetDirectoryName(manager.GetUndoFileName())!;

        manager.Dispose();

        Assert.False(System.IO.Directory.Exists(undoFolder));
    }
}
