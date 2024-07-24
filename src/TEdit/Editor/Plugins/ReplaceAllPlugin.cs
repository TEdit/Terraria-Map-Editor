using System.Windows;
using TEdit.Configuration;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins
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

            for (int x = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X : 0; x < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X + _wvm.Selection.SelectionArea.Width : _wvm.CurrentWorld.TilesWide); x++)
            {
                for (int y = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y : 0; y < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y + _wvm.Selection.SelectionArea.Height : _wvm.CurrentWorld.TilesHigh); y++)
                {
                    bool doReplaceTile = false;
                    bool doReplaceWall = false;

                    Tile curTile = _wvm.CurrentWorld.Tiles[x, y];
					
					// Updated condition to handle air tiles (tileMask == -1).
                    if (replaceTiles)
                    {
                        if ((_wvm.Selection.IsValid(x, y)) && ((curTile.IsActive && curTile.Type == tileMask && _wvm.TilePicker.TileMaskMode == MaskMode.Match)
                            || (!curTile.IsActive && _wvm.TilePicker.TileMaskMode == MaskMode.Empty)
                            || (curTile.Type != tileMask && _wvm.TilePicker.TileMaskMode == MaskMode.NotMatching)
                            || (tileMask == -1 && !curTile.IsActive))) // Added condition to handle tileMask == -1.
                        {
                            doReplaceTile = true;
                        }
                    }

                    if (replaceWalls)
                    {
                        if ((_wvm.Selection.IsValid(x, y)) && (curTile.Wall == wallMask && _wvm.TilePicker.WallMaskMode == MaskMode.Match)
                            || (curTile.Wall == 0 && _wvm.TilePicker.WallMaskMode == MaskMode.Empty)
                            || (curTile.Wall != wallMask && _wvm.TilePicker.WallMaskMode == MaskMode.NotMatching))
                        {
                            doReplaceWall = true;
                        }
                    }

                    if (doReplaceTile || doReplaceWall)
                    {
                        _wvm.UndoManager.SaveTile(x, y);

                        if (doReplaceTile)
                        {
                            if (tileTarget == -1)
                            {
								// If tileTarget is -1, make the tile inactive.
                                curTile.IsActive = false;
                            }
                            else
                            {
                                curTile.Type = (ushort)tileTarget;
                                curTile.IsActive = true;

                                if (WorldConfiguration.TileProperties[curTile.Type].IsSolid)
                                {
                                    curTile.U = -1;
                                    curTile.V = -1;
                                    BlendRules.ResetUVCache(_wvm, x, y, 1, 1);
                                }

                                if (_wvm.TilePicker.TilePaintActive)
                                {
                                    curTile.TileColor = (byte)_wvm.TilePicker.TileColor;
                                }
                            }
                        }

                        if (doReplaceWall)
                        {
                            if (wallTarget == -1)
                            {
                                curTile.Wall = 0;
                            }
                            else
                            {
                                curTile.Wall = (byte)wallTarget;

                                if (_wvm.TilePicker.WallPaintActive)
                                {
                                    if (curTile.Wall != 0)
                                    {
                                        curTile.WallColor = (byte)_wvm.TilePicker.WallColor;
                                    }
                                    else
                                    {
                                        curTile.WallColor = (byte)0;
                                    }
                                }
                            }
                        }

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