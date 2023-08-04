using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class PaintProperty 
{
    public PaintProperty()
    {

    }

    public PaintProperty(int id, string name, TEditColor color)
    {
        Color = color;
        Id = id;
        Name = name;
    }

    public TEditColor Color { get; set; } = TEditColor.Magenta;
    public int Id { get; set; } = -1;
    public string Name { get; set; } = "UNKNOWN";
}
