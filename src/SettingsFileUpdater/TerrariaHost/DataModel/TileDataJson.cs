using System.Collections.Generic;
using System.Text.Json.Serialization;
using TEdit.Common;
using TEdit.Geometry;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class TileDataJson
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    [JsonPropertyOrder(2)]
    public TEditColor Color { get; set; }

    public bool IsFramed { get; set; }
    public bool IsSolid { get; set; }
    public bool IsSolidTop { get; set; }
    public bool IsLight { get; set; }
    public bool SaveSlope { get; set; }
    public bool IsGrass { get; set; }
    public bool IsPlatform { get; set; }
    public bool IsCactus { get; set; }
    public bool IsStone { get; set; }
    public bool CanBlend { get; set; }
    public int? MergeWith { get; set; }

    public Vector2Short TextureGrid { get; set; } = new Vector2Short(16, 16);
    public Vector2Short FrameGap { get; set; } = new Vector2Short(2, 2);
    public Vector2Short[] FrameSize { get; set; } = new[] { new Vector2Short(1, 1) };

    public List<FrameDataJson> Frames { get; set; }
}

public class FrameDataJson
{
    public string Name { get; set; } = "Default";
    public string Variety { get; set; }
    public Vector2Short UV { get; set; } = Vector2Short.Zero;
    public Vector2Short Size { get; set; } = new Vector2Short(1, 1);
    public string Anchor { get; set; }
}
