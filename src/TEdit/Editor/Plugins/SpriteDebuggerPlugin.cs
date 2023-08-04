using TEdit.ViewModel;
using TEdit.Terraria.Objects;
using TEdit.Terraria.Editor;
using TEdit.Configuration;

namespace TEdit.Editor.Plugins;

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
        foreach (var sprite in WorldConfiguration.Sprites2)
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
                var s = style;
                var tileOffset = s.UV / s.SizePixelsInterval;
                style?.Place(tileOffset.X + x, tileOffset.Y + y, _wvm);
            }

            y += spriteTilesY;
        }

        _wvm.UndoManager.SaveUndo();
    }
}
