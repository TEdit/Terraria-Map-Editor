using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Editor.Undo;

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
