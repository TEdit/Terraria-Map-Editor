using System.Windows;
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


            if (_wvm.TilePicker.PaintMode == PaintMode.TileAndWall)
            {
                if (_wvm.TilePicker.TileStyleActive)
                    replaceTiles = true;
                if (_wvm.TilePicker.WallStyleActive)
                    replaceWalls = true;
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
            int tileMask = _wvm.TilePicker.TileMask;
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
                        if ((_wvm.Selection.IsValid(x,y)) && (curTile.IsActive && curTile.Type == tileMask && _wvm.TilePicker.TileMaskMode == MaskMode.Match)
                            || (!curTile.IsActive && _wvm.TilePicker.TileMaskMode == MaskMode.Empty)
                            || (curTile.Type != tileMask && _wvm.TilePicker.TileMaskMode == MaskMode.NotMatching))
                        {
                            doReplaceTile = true;
                        }
                    }

                    if (replaceWalls)
                    {
                        if ((_wvm.Selection.IsValid(x, y)) && (curTile.Wall == wallMask && _wvm.TilePicker.WallMaskMode == MaskMode.Match)
                            || (curTile.Wall == 0 && _wvm.TilePicker.WallMaskMode == MaskMode.Empty)
                            || (curTile.Wall != wallMask && _wvm.TilePicker.TileMaskMode == MaskMode.NotMatching))
                        {
                            doReplaceWall = true;
                        }
                    }

                    if (doReplaceTile || doReplaceWall)
                    {
                        _wvm.UndoManager.SaveTile(x, y);

                        if (doReplaceTile)
                            curTile.Type = (ushort)tileTarget;

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
            view.Owner = Application.Current.MainWindow;
            view.DataContext = _wvm;
            if (view.ShowDialog() == true)
            {
                PerformReplace();
            }
        }
    }
}
