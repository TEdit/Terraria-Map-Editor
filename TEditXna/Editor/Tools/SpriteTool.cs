using System;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using TEditXNA.Terraria;
using TEditXna.ViewModel;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Tools
{
    public sealed class SpriteTool : BaseTool
    {
        public SpriteTool(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/sprite.png"));
            Name = "Sprite";
            IsActive = false;
            ToolType = ToolType.Pixel;
        }

        public override bool PreviewIsTexture
        {
            get
            {
                if (_wvm.SelectedSprite != null)
                    return _wvm.SelectedSprite.IsPreviewTexture;
                return false;
            }
        }

        public override void MouseDown(TileMouseState e)
        {
            if (_wvm.SelectedSprite == null)
                return;
            
            Vector2Short[,] tiles = _wvm.SelectedSprite.GetTiles();
            for (int x = 0; x < _wvm.SelectedSprite.Size.X; x++)
            {
                int tilex = x + e.Location.X;
                for (int y = 0; y < _wvm.SelectedSprite.Size.Y; y++)
                {
                    int tiley = y + e.Location.Y;

                    _wvm.UndoManager.SaveTile(tilex, tiley);
                    Tile curtile = _wvm.CurrentWorld.Tiles[tilex, tiley];
                    curtile.IsActive = true;
                    curtile.Type = _wvm.SelectedSprite.Tile;
                    curtile.U = tiles[x, y].X;
                    curtile.V = tiles[x, y].Y;
                    _wvm.UpdateRenderPixel(tilex, tiley);
                }
            }
            
            _wvm.UndoManager.SaveUndo();

            /* Heathtech */
            BlendRules.ResetUVCache(_wvm, e.Location.X, e.Location.Y, _wvm.SelectedSprite.Size.X, _wvm.SelectedSprite.Size.Y);
        }

        public override WriteableBitmap PreviewTool()
        {
            if (_wvm.SelectedSprite != null)
                return _wvm.SelectedSprite.Preview;
            return base.PreviewTool();
        }
    }
}