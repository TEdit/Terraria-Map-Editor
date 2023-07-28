using System;
using System.Threading.Tasks;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Editor.Undo;

public interface IUndoManager
{
    Task StartUndoAsync();

    void SaveTile(World world, Vector2Int32 location);
    void SaveTile(World world, int x, int y);

    Task SaveUndoAsync();

    Task UndoAsync(World world);
    Task RedoAsync(World world);
}

public record UndoTile
{

    public UndoTile()
    {

    }

    public UndoTile(Tile tile, Vector2Int32 location)
    {
        Location = location;
        Tile = tile;
    }

    public UndoTile(Tile tile, int x, int y)
        : this(tile, new Vector2Int32(x, y))
    {

    }

    public Vector2Int32 Location { get; set; }
    public Tile Tile { get; set; }
}
