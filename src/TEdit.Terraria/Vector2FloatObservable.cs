using TEdit.Geometry;

namespace TEdit.Terraria;

public partial class Vector2FloatObservable : ReactiveObject
{
    [Reactive] private float _x;
    [Reactive] private float _y;

    public Vector2FloatObservable() { }

    public Vector2FloatObservable(float x, float y)
    {
        _x = x;
        _y = y;
    }

    public Vector2Float ToVector2Float() => new Vector2Float(X, Y);
    public static implicit operator Vector2Float(Vector2FloatObservable v) => v.ToVector2Float();
    public static implicit operator Vector2FloatObservable(Vector2Int32 v) => new Vector2FloatObservable(v.X, v.Y);
}
