using System.Collections.Generic;
using System.Linq;
using TEdit.ViewModel;

namespace TEdit.Scripting.Api;

public class ToolsApi
{
    private readonly WorldViewModel _wvm;

    public ToolsApi(WorldViewModel wvm)
    {
        _wvm = wvm;
    }

    public List<string> ListTools()
    {
        return _wvm.Tools.Select(t => t.Name).ToList();
    }

    public void CopySelection()
    {
        _wvm.EditCopy();
    }

    public int GetTilePickerTile() => _wvm.TilePicker.Tile;
    public int GetTilePickerWall() => _wvm.TilePicker.Wall;
}
