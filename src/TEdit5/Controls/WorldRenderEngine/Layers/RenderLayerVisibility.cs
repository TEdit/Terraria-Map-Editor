using ReactiveUI;
using System;

namespace TEdit5.Controls.WorldRenderEngine.Layers;


public class RenderLayerVisibility : ReactiveObject
{
    public bool Wall { get; set; } = true;
    public bool Tile { get; set; } = true;
    public bool Liquid { get; set; } = true;
    public bool WireRed { get; set; } = true;
    public bool WireBlue { get; set; } = true;
    public bool WireGreen { get; set; } = true;
    public bool WireYellow { get; set; } = true;
    public bool Coatings { get; set; } = true;
}

