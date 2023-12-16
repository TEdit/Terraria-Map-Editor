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
using Brushes = Avalonia.Media.Brushes;
using Color = Avalonia.Media.Color;
using Size = Avalonia.Size;

namespace TEdit.Desktop.Controls.RenderLayer;

public class BackgroundGridRenderLayer : IRenderLayer
{
    public Size SizePixels => throw new NotImplementedException();

    public Size SizeTiles => throw new NotImplementedException();

    public bool Enabled { get; set; } = true;

    public IImmutableSolidColorBrush GridColor { get; set; } = Brushes.DimGray;
    public IImmutableSolidColorBrush GridColorAlternate { get; set; } = new Avalonia.Media.Immutable.ImmutableSolidColorBrush(Color.FromArgb(255, 48, 48, 48));

    public void Render(DrawingContext context, Rect worldSourceRect, Rect viewportRect)
    {
        RenderGrid(context, viewportRect.Size);
    }

    private void RenderGrid(DrawingContext context, Size viewPortSize)
    {
        // Draw Grid
        int gridCellSize = 15;
        if (Enabled & gridCellSize > 0)
        {
            // draw the background
            var gridColor = GridColor;
            var altColor = GridColorAlternate;

            var currentColor = gridColor;
            for (int y = 0; y < viewPortSize.Height; y += gridCellSize)
            {
                var firstRowColor = currentColor;

                for (int x = 0; x < viewPortSize.Width; x += gridCellSize)
                {
                    context.FillRectangle(currentColor, new Rect(x, y, gridCellSize, gridCellSize));
                    currentColor = ReferenceEquals(currentColor, gridColor) ? altColor : gridColor;
                }

                if (Equals(firstRowColor, currentColor))
                    currentColor = ReferenceEquals(currentColor, gridColor) ? altColor : gridColor;
            }

        }
        /*else
        {
            context.FillRectangle(Background, new Rect(0, 0, Viewport.Width, Viewport.Height));
        }*/
    }
}
