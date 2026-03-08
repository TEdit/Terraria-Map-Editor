using System.Collections.Generic;

namespace TEdit.Terraria.DataModel;

public class MorphGroup
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "tile";
    public Dictionary<string, MorphGroupVariant> Variants { get; set; } = new();
}

public class MorphGroupVariant
{
    public ushort? TileId { get; set; }
    public short? FrameU { get; set; }
    public short? FrameV { get; set; }
    public short? FrameWidth { get; set; }
    public short? FrameHeight { get; set; }
    public bool Delete { get; set; }
}
