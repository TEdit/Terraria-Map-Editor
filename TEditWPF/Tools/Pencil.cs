// -----------------------------------------------------------------------
// <copyright file="Pencil.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using TEditWPF.Infrastructure;
using TerrariaWorld.Common;
using TerrariaWorld.Game;

namespace TEditWPF.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Export(typeof(ITool))]
    public class Pencil : ITool
    {
        public string Name
        {
            get { return "Pencil"; }
        }

        public ToolType Type
        {
            get { return ToolType.Pencil; }
        }

        public bool PreviewTool(Point[] location, WriteableBitmap viewPortRegion)
        {

        }

        public bool UseTool(Point[] location, World world)
        {
        }
    }
}
