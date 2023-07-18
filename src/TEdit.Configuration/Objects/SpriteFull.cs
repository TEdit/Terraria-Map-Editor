using System.Collections.Generic;
using System.Linq;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects
{

    public class SpriteFull 
    {
        public ushort Tile { get; set; }
        public string Name { get; set; }
        public Vector2Short[] SizeTiles { get; set; }
        public Vector2Short SizePixelsRender { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public bool IsAnimated { get; set; }
        public Dictionary<int, SpriteSub> Styles { get; } = new Dictionary<int, SpriteSub>();
        public SpriteSub Default => Styles.Values.FirstOrDefault();
    }

    public class SpriteSub
    {
        public ushort Tile { get; set; }
        public int Style { get; set; }
        public TEditColor StyleColor { get; set; }
        public Vector2Short UV { get; set; }
        public Vector2Short SizeTiles { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public FrameAnchor Anchor { get; set; }
        public string Name { get; set; }
    }
}
