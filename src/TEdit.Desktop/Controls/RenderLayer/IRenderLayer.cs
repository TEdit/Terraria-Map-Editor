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
using Size = Avalonia.Size;

namespace TEdit.Desktop.Controls.RenderLayer;

public interface IRenderLayer
{
    Size SizePixels { get; }
    Size SizeTiles { get; }
    bool Enabled { get; set; }

    void Render(DrawingContext context, Rect worldSourceRect, Rect viewportRect);
}
