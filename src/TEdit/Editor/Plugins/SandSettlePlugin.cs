using TEdit.ViewModel;
using System.Linq;
using SharpDX.Direct2D1;
using System.Collections.Generic;
using TEdit.Terraria;
using TEdit.Render;

namespace TEdit.Editor.Plugins
{
    public sealed class SandSettlePlugin : BasePlugin
    {
        public SandSettlePlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Settle Sand";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
                return;
            int[] _tileSand = {
                53,  // Sand Block
                112, // Ebonsand Block
                116, // Pearlsand
                123, // silt
                224, // slush block
                234, // Crimsand block
            };

            for (int y = _wvm.CurrentWorld.TilesHigh - 1; y > 0; y--)
            {
                for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
                {
                    var curTile = _wvm.CurrentWorld.Tiles[x, y];
                    if (_tileSand.Contains(curTile.Type))
                    {
                        // check if tile below current tile is empty and move sand to there if it is.
                        int shiftAmmount = 1;
                        while (shiftAmmount + y < _wvm.CurrentWorld.TilesHigh && !_wvm.CurrentWorld.Tiles[x, y + shiftAmmount].IsActive)
                            shiftAmmount++;
                        shiftAmmount--;

                        if (shiftAmmount > 0)
                        {
                            var belowTile = _wvm.CurrentWorld.Tiles[x, y + shiftAmmount];
                            if (!belowTile.IsActive)
                            {
                                _wvm.UndoManager.SaveTile(x, y + shiftAmmount);
                                _wvm.UndoManager.SaveTile(x, y);
                                belowTile.IsActive = true;
                                belowTile.Type = curTile.Type;
                                curTile.IsActive = false;
                                BlendRules.ResetUVCache(_wvm, x, y, 1, 1 + shiftAmmount);
                                _wvm.UpdateRenderPixel(x, y);
                                _wvm.UpdateRenderPixel(x, y + shiftAmmount);
                            }
                        }
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();
        }
    }

    public sealed class SpriteDebuggerPlugin : BasePlugin
    {
        public SpriteDebuggerPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Generate Debug Sprites";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
                return;

            int x = 0;
            int y = 0;

            int currentMaxX = 0;
            foreach (var sprite in World.Sprites2)
            {
                int spriteTilesX = sprite.SizeTexture.Width / sprite.SizePixelsInterval.Width;
                int spriteTilesY = sprite.SizeTexture.Height / sprite.SizePixelsInterval.Height;

                if (sprite.IsAnimated)
                {
                     spriteTilesX = sprite.SizeTiles[0].X;
                     spriteTilesY = sprite.SizeTiles[0].Y;
                }

                // loop to next column
                if (y + spriteTilesY > _wvm.CurrentWorld.TilesHigh)
                {
                    x = x + currentMaxX;
                    currentMaxX = 0;
                    y = 0;
                }

                if (spriteTilesX > currentMaxX) { currentMaxX = spriteTilesX; }


                foreach (var style in sprite.Styles)
                {
                    var s = style.Value;
                    var tileOffset = s.UV / s.SizePixelsInterval;

                    style.Value?.Place(tileOffset.X + x, tileOffset.Y + y, _wvm);
                }

                y += spriteTilesY;
            }

            _wvm.UndoManager.SaveUndo();
        }
    }
}
