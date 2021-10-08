using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEdit.Configuration
{
    public class SaveConfiguration
    {
        public Dictionary<int, SaveVersionData> SaveVersions { get; set; }
    }

    public class SaveVersionData
    {
        public string GameVersion { get; set; }
        public int SaveVersion { get; set; }
        public HashSet<int> FramedTileIds { get; set; }
        public bool[] GetFrames()
        {

            var max = FramedTileIds.Max();
            var frames = new bool[max + 1];

            for (int i = 0; i <= max; i++)
            {
                frames[i] = FramedTileIds.Contains(i);
            }

            return frames;
        }
    }
}
