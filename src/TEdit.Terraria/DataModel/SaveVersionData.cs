using System.Collections.Generic;

namespace TEdit.Terraria.DataModel;

public class SaveVersionData
{
    public int SaveVersion { get; set; }
    public string GameVersion { get; set; } = "";
    public int MaxTileId { get; set; }
    public int MaxWallId { get; set; }
    public int MaxItemId { get; set; }
    public int MaxNpcId { get; set; }
    public int MaxMoonId { get; set; }
    public HashSet<int> FramedTileIds { get; set; } = [];

    public bool[] GetFrames()
    {
        var tileCount = MaxTileId + 1;
        bool[] result = new bool[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            if (FramedTileIds.Contains(i)) { result[i] = true; }
        }

        return result;
    }
}
