/*
*                               The MIT License (MIT)
* Permission is hereby granted, free of charge, to any person obtaining a copy of
* this software and associated documentation files (the "Software"), to deal in
* the Software without restriction, including without limitation the rights to
* use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
* the Software, and to permit persons to whom the Software is furnished to do so.
*/
// Port from: https://github.com/cyotek/Cyotek.Windows.Forms.ImageBox to AvaloniaUI
// Port from: https://raw.githubusercontent.com/sn4k3/UVtools/master/UVtools.AvaloniaControls/AdvancedImageBox.cs


using Avalonia;
using Avalonia.Media;
using System;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;
using TEdit.Terraria;
using TEdit.Common;

namespace TEdit.Desktop.Controls.RenderLayer;

public class WorldRenderLayer : IRenderLayer
{
    public Size SizePixels => throw new NotImplementedException();

    public Size SizeTiles => throw new NotImplementedException();

    public bool Enabled { get; set; } = true;

    public World? World { get; set; } = null!;

    public void Render(DrawingContext context, Rect worldSourceRect, Rect viewportRect)
    {
        if (Enabled && World != null)
        {
            var world = World;
            var tiles = world.Tiles;
            var tileCount = tiles.Length;
            var tileWidth = world.TilesWide;
            var tileHeight = world.TilesHigh;
            var tileScale = 2;
            var tileOffset = new Point(0, 0);

            var tileRect = new Rect(tileOffset, new Size(tileScale, tileScale));


            for (int x = 0; x < tileWidth && x < viewportRect.Width / tileScale; x++)
            {
                for (int y = 0; y < tileHeight && y < viewportRect.Height / tileScale; y++)
                {

                    var tile = tiles[x, y];
                    if (tile.IsActive)
                    {
                        var tileX = x;
                        var tileY = y;

                        tileRect = new Rect(tileOffset + new Vector(tileX * tileScale, tileY * tileScale), new Size(tileScale, tileScale));

                        var color = PixelMap.GetTileColor(tile, TEditColor.White);

                        var tileColor = Color.FromArgb(color.A, color.R, color.G, color.B);
                        var tileBrush = new SolidColorBrush(tileColor);

                        context.FillRectangle(tileBrush, tileRect);
                    }
                }
            }
        }
    }
}
