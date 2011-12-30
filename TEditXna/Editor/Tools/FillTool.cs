using System;
using System.Windows.Media.Imaging;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public sealed class FillTool : BaseTool
    {
        public FillTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paintcan.png"));
            Name = "Fill";
            ToolType = ToolType.Pixel;
        }
    }
}