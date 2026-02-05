using TEdit.Geometry;

namespace TEdit.Terraria;

public partial class Vector2Int32Observable : ReactiveObject
{
    [Reactive] private int _x;
    [Reactive] private int _y;

    public Vector2Int32Observable() { }

    public Vector2Int32Observable(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public Vector2Int32 ToVector2Int32() => new Vector2Int32(X, Y);
    public static implicit operator Vector2Int32(Vector2Int32Observable v) => v.ToVector2Int32();
    public static implicit operator Vector2Int32Observable(Vector2Int32 v) => new Vector2Int32Observable(v.X, v.Y);
}
