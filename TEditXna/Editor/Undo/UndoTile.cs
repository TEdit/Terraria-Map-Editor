using TEdit.Geometry.Primitives;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Undo
{
    public class UndoTile
    {
        public UndoTile(Vector2Int32 location, Tile tile)
        {
            Location = location;
            Tile = tile;
        }

        public Vector2Int32 Location;
        public Tile Tile;
    }
}