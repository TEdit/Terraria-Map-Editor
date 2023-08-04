using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Geometry;
using TEdit.Configuration;
using TEdit.UI;

namespace TEdit.Editor.Tools;

public sealed class ArrowTool : BaseTool
{
    //private ChestPopup _chestPopup;
    //private SignPopup _signPopup;

    public ArrowTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/cursor.png"));
        ToolType = ToolType.Pixel;
        Name = "Arrow";
    }

    private bool _rightClick;
    public override void MouseDown(TileMouseState e)
    {
        if (e.RightButton == MouseButtonState.Pressed)
            _rightClick = true;
    }

    public override void MouseUp(TileMouseState e)
    {
        if (!_rightClick)
            return;

        _rightClick = false;

        Tile curTile = _wvm.CurrentWorld.Tiles[e.Location.X, e.Location.Y];
        if (curTile.IsChest())
        {
            Chest chest = _wvm.CurrentWorld.GetChestAtTile(e.Location.X, e.Location.Y, true);
            if (chest != null)
            {
                _wvm.SelectedChest = chest.Copy();
                return;
            }
        }
        else if (curTile.IsSign())
        {
            Sign sign = _wvm.CurrentWorld.GetSignAtTile(e.Location.X, e.Location.Y, true);
            if (sign != null)
            {
                _wvm.SelectedSign = sign.Copy();
                return;
            }
        }
        else if (curTile.IsTileEntity())
        {
            TileEntity te = _wvm.CurrentWorld.GetTileEntityAtTile(e.Location.X, e.Location.Y, true);
            if (te != null)
            {
                _wvm.SelectedTileEntity = te.Copy();
            }
        }           
        else if (curTile.Type == (int)TileType.ChristmasTree)
        {
            Vector2Int32 XmasLocation = _wvm.CurrentWorld.GetXmas(e.Location.X, e.Location.Y);
            _wvm.SelectedXmasStar = _wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V & 7;
            _wvm.SelectedXmasGarland = (_wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V >> 3) & 7;
            _wvm.SelectedXmasBulb = (_wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V >> 6) & 0xf;
            _wvm.SelectedXmasLight = (_wvm.CurrentWorld.Tiles[XmasLocation.X, XmasLocation.Y].V >> 10) & 0xf;
            _wvm.SelectedXmas = XmasLocation;
        }
    }
}
