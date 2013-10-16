using System.Windows;
using System.Windows.Input;
using BCCL.MvvmLight.Command;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public class ReplaceAllPlugin : BasePlugin
    {
        public ReplaceAllPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Replace All Tiles";
        }


        private void PerformReplace()
        {
            if (_wvm.CurrentWorld == null)
                return;

            bool replaceTiles = false;
            bool replaceWalls = false;


            switch (_wvm.TilePicker.PaintMode)
            {
                case PaintMode.Tile:
                    replaceTiles = true;
                    break;
                case PaintMode.Wall:
                    replaceWalls = true;
                    break;
                case PaintMode.TileAndWall:
                    replaceTiles = true;
                    replaceWalls = true;
                    break;
                case PaintMode.Wire:
                case PaintMode.Wire2:
                case PaintMode.Wire3:
                case PaintMode.Liquid:
                default:
                    MessageBox.Show("Set the paint mode to \"Tile\", \"Wall\" or \"Tile and Wall\" and enable masks.");
                    return;
            }

            if (replaceTiles && _wvm.TilePicker.TileMaskMode == MaskMode.Off)
            {
                MessageBox.Show("Enable masking tiles to enable replace.");
                return;
            }

            if (replaceWalls && _wvm.TilePicker.WallMaskMode == MaskMode.Off)
            {
                MessageBox.Show("Enable masking walls to enable replace.");
                return;
            }

            int wallMask = _wvm.TilePicker.WallMask;
            int tileMask = _wvm.TilePicker.WallMask;
            int tileTarget = _wvm.TilePicker.Tile;
            int wallTarget = _wvm.TilePicker.Wall;

            for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
            {
                for (int y = 0; y < _wvm.CurrentWorld.TilesHigh; y++)
                {
                    bool doReplaceTile = false;
                    bool doReplaceWall = false;

                    Tile curTile = _wvm.CurrentWorld.Tiles[x, y];
                    if (replaceTiles)
                    {
                        if (curTile.Type == tileMask || (_wvm.TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive)) doReplaceTile = true;
                    }

                    if (replaceWalls)
                    {
                        if (curTile.Wall == wallMask) doReplaceWall = true;
                    }

                    if (doReplaceTile || doReplaceWall)
                    {
                        _wvm.UndoManager.SaveTile(x, y);

                        if (doReplaceTile)
                            curTile.Type = (byte)tileTarget;

                        if (doReplaceWall)
                            curTile.Wall = (byte)wallTarget;

                        _wvm.UpdateRenderPixel(x, y);
                    }
                }
            }

            _wvm.UndoManager.SaveUndo();
        }

        public override void Execute()
        {
            ReplaceAllPluginView view = new ReplaceAllPluginView();
            view.Owner = App.Current.MainWindow;
            view.DataContext = _wvm;
            if (view.ShowDialog() == true)
            {
                PerformReplace();
            }
        }
    }
}