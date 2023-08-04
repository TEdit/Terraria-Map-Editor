using System.Collections.Generic;

namespace TEdit.Configuration;

public class SaveVersionData
{
    public int SaveVersion { get; set; }
    public string GameVersion { get; set; }
    public int MaxTileId { get; set; }
    public int MaxWallId { get; set; }
    public int MaxItemId { get; set; }
    public int MaxNpcId { get; set; }
    public int MaxMoonId { get; set; }
    public HashSet<int> FramedTileIds { get; set; }

    public bool[] GetFrames()
    {
        var frames = FramedTileIds;
        var tileCount = MaxTileId + 1;
        bool[] result = new bool[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            if (frames.Contains(i)) { result[i] = true; }
        }

        return result;
    }
}
