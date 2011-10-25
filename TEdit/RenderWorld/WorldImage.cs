using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.Generic;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.RenderWorld
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorldImage : ObservableObject
    {
        private readonly Dictionary<string, WriteableBitmap> _layers = new Dictionary<string, WriteableBitmap>() {
            { "TilesPixel", null },
            { "Walls",      null },
            { "TilesBack",  null },
            { "TilesFront", null },
            { "Liquid",     null },
        };

        public static readonly string[] LayerList = { "TilesPixel", "Walls", "TilesBack", "TilesFront", "Liquid" };
        public static readonly Dictionary<string, int> Bpp = new Dictionary<string, int>() {
            { "TilesPixel", 4 },   // TODO: Change to 3; need conversion tools in place, though //
            { "Walls",      4 },
            { "TilesBack",  4 },
            { "TilesFront", 4 },
            { "Liquid",     4 },
        };
        public static readonly Dictionary<string, SizeInt32> TileSize = new Dictionary<string, SizeInt32>() {
            { "TilesPixel", new SizeInt32(1, 1) },
            { "Walls",      new SizeInt32(8, 8) },
            { "TilesBack",  new SizeInt32(8, 8) },
            { "TilesFront", new SizeInt32(8, 8) },
            { "Liquid",     new SizeInt32(8, 8) },
        };


        public WriteableBitmap Image
        {
            get { return _layers["TilesPixel"]; }
            set { _layers["TilesPixel"] = value; }
        }

        public Dictionary<string, WriteableBitmap> Layer
        {
            get { return _layers; }
        }

    }
}