using Avalonia.Rendering.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEdit.Desktop.Editor;

public interface IMouseTool
{
    public ICustomDrawOperation? DrawTool { get; }
}
