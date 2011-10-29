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
            { "Rendered",   null },
        };

        public static readonly string[] LayerList = { "TilesPixel", "Rendered" };
        public static readonly Dictionary<string, int> Bpp = new Dictionary<string, int>() {
            { "TilesPixel", 4 },   // TODO: Change to 3; need conversion tools in place, though //
            { "Rendered",   4 },
        };
        public static readonly Dictionary<string, SizeInt32> TileSize = new Dictionary<string, SizeInt32>() {
            { "TilesPixel", new SizeInt32(1, 1) },
            { "Rendered",   new SizeInt32(4, 4) },
        };

        public WriteableBitmap Image
        {
            get { return _layers["TilesPixel"]; }
            set { _layers["TilesPixel"] = value; }
        }

        public WriteableBitmap Rendered {
            get { return _layers["Rendered"]; }
            set { _layers["Rendered"] = value; }
        }

        public Dictionary<string, WriteableBitmap> Layer
        {
            get { return _layers; }
        }

    }
}