using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class SignApi
{
    private readonly World _world;

    public SignApi(World world)
    {
        _world = world;
    }

    public int Count => _world.Signs.Count;

    public List<Dictionary<string, object>> GetAll()
    {
        return _world.Signs.Select(s => new Dictionary<string, object>
        {
            { "x", s.X },
            { "y", s.Y },
            { "text", s.Text ?? "" }
        }).ToList();
    }

    public Dictionary<string, object>? GetAt(int x, int y)
    {
        var sign = _world.GetSignAtTile(x, y);
        if (sign == null) return null;

        return new Dictionary<string, object>
        {
            { "x", sign.X },
            { "y", sign.Y },
            { "text", sign.Text ?? "" }
        };
    }

    public void SetText(int x, int y, string text)
    {
        var sign = _world.GetSignAtTile(x, y);
        if (sign != null)
        {
            sign.Text = text;
        }
    }
}
