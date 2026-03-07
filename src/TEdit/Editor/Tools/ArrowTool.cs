using System;
using System.Windows.Media.Imaging;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Geometry;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class ArrowTool : BaseTool
{
    //private ChestPopup _chestPopup;
    //private SignPopup _signPopup;

    public ArrowTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/cursor.png"));
        SymbolIcon = SymbolRegular.Cursor24;
        ToolType = ToolType.Pixel;
        Name = "Arrow";
    }

    private bool _secondaryClick;
    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);
        if (actions.Contains("editor.secondary"))
            _secondaryClick = true;
    }

    public override void MouseUp(TileMouseState e)
    {
        if (!_secondaryClick)
            return;

        _secondaryClick = false;

        Tile curTile = _wvm.CurrentWorld.Tiles[e.Location.X, e.Location.Y];

        // Check interactive tile objects first (chests, signs, tile entities, xmas trees)
        // so they take priority over wire trace highlighting.
        // Proximity search in GetChestAtTile/GetSignAtTile/GetTileEntityAtTile handles
        // mod tiles that lack sprite JSON for anchor calculation.

        // Try chest (vanilla types first, then proximity search handles mod chests)
        Chest chest = _wvm.CurrentWorld.GetChestAtTile(e.Location.X, e.Location.Y, true);
        if (chest != null)
        {
            _wvm.SelectedChest = chest.Copy();
            return;
        }

        // Try sign
        Sign sign = _wvm.CurrentWorld.GetSignAtTile(e.Location.X, e.Location.Y, true);
        if (sign != null)
        {
            _wvm.SelectedSign = sign.Copy();
            return;
        }

        // Try tile entity
        TileEntity te = _wvm.CurrentWorld.GetTileEntityAtTile(e.Location.X, e.Location.Y, true);
        if (te != null)
        {
            _wvm.SelectedTileEntity = te.Copy();
            return;
        }

        // Christmas tree
        if (curTile.Type == (int)TileType.ChristmasTree)
        {
            Vector2Int32 XmasLocation = _wvm.CurrentWorld.GetXmas(e.Location.X, e.Location.Y);
            _wvm.SelectedXmasStar = _wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V & 7;
            _wvm.SelectedXmasGarland = (_wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V >> 3) & 7;
            _wvm.SelectedXmasBulb = (_wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V >> 6) & 0xf;
            _wvm.SelectedXmasLight = (_wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V >> 10) & 0xf;
            _wvm.SelectedXmas = XmasLocation;
            return;
        }

        // Wire trace is lowest priority — only if no interactive object was found
        if (curTile.HasWire)
        {
            PerformWireTrace(e.Location);
        }
    }
}
