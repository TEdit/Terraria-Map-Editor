using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common.Reactive;
using System.Collections.Generic;
using System.Linq;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects
{
    public class SpriteFull : ObservableObject
    {
        public ushort Tile { get; set; }
        public WriteableBitmap Preview { get; set; }
        public string Name { get; set; }
        public Vector2Short[] SizeTiles { get; set; }

        public Vector2Short GetSizeTiles(short v)
        {
            if (SizeTiles.Length > 1)
            {
                int row = v / SizePixelsInterval.Y;
                return SizeTiles[row];
            }

            return SizeTiles[0];
        }

        public Vector2Short SizePixelsRender { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public bool IsPreviewTexture { get; set; }
        public bool IsAnimated { get; set; }



        public SpriteSub Default => Styles.Values.FirstOrDefault();

        public Dictionary<int, SpriteSub> Styles { get; } = new Dictionary<int, SpriteSub>();

        public void GeneratePreview()
        {
            var bmp = new WriteableBitmap(SizeTexture.X, SizeTexture.Y, 96, 96, PixelFormats.Bgra32, null);
            var c = World.TileProperties[Tile].Color;
            bmp.Clear(Color.FromArgb(c.A, c.R, c.G, c.B));
            Preview = bmp;
            IsPreviewTexture = false;
        }

        /// <summary>
        /// Get a Sprite from one of it's UV's
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public KeyValuePair<int, SpriteSub> GetStyleFromUV(Vector2Short uv)
        {
            var renderUV = TileProperty.GetRenderUV(Tile, uv.X, uv.Y);

            foreach (var kvp in Styles)
            {
                if (kvp.Value.UV == uv) return kvp;

                if (renderUV.X >= kvp.Value.UV.X &&
                    renderUV.Y >= kvp.Value.UV.Y &&
                    renderUV.X < kvp.Value.UV.X + (kvp.Value.SizePixelsInterval.X * kvp.Value.SizeTiles.X) &&
                    renderUV.Y < kvp.Value.UV.Y + (kvp.Value.SizePixelsInterval.Y * kvp.Value.SizeTiles.Y))
                {
                    return kvp;
                }
            }
            return default;
        }

    }
}
