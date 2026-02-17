using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;

namespace TEdit.Editor.Tools;

public sealed class BiomeTool : BrushToolBase
{
    public BiomeTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Name = "Biome";
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/biome.png"));
    }

    protected override void FillSolid(IList<Vector2Int32> area)
    {
    }

    protected override void FillHollow(IList<Vector2Int32> area, IList<Vector2Int32> interrior)
    {
        IEnumerable<Vector2Int32> border = area.Except(interrior);

    }
}
