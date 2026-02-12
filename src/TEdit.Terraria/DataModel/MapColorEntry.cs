using System.Text.Json.Serialization;
using TEdit.Common;

namespace TEdit.Terraria.DataModel;

public class MapColorEntry
{
    public int? TileId { get; set; }
    public int? WallId { get; set; }
    public int? SubId { get; set; }
    public int? PaintId { get; set; }

    public TEditColor Color { get; set; }

    public bool BuildSafe { get; set; }
}
