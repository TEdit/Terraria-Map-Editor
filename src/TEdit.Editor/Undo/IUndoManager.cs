using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Editor.Undo;

public interface IUndoManager : IDisposable
{
    Task StartUndoAsync();

    void SaveTile(World world, Vector2Int32 location, bool removeEntities = false);
    void SaveTile(World world, int x, int y, bool removeEntities = false);

    Task SaveUndoAsync();

    Task<IReadOnlyList<Vector2Int32>> UndoAsync(World world);
    Task<IReadOnlyList<Vector2Int32>> RedoAsync(World world);
}
