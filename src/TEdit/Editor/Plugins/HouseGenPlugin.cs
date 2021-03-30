using System;
using System.Windows;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins
{
    public class HouseGenPlugin : BasePlugin
    {
        HouseGenPluginView view;

        public HouseGenPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Procedural House Generator";
        }

        /* private void PerformReplace()
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
        */

        /*
        private void dummy_func()
        {
            ClipboardBuffer buffer = null;
            int w = 0,h = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Tile curTile = (Tile)world.Tiles[x + area.X, y + area.Y].Clone();

                    if (Tile.IsChest(curTile.Type))
                    {
                        if (buffer.GetChestAtTile(x, y) == null)
                        {
                            var anchor = world.GetAnchor(x + area.X, y + area.Y);
                            if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                            {
                                var data = world.GetChestAtTile(x + area.X, y + area.Y);
                                if (data != null)
                                {
                                    var newChest = data.Copy();
                                    newChest.X = x;
                                    newChest.Y = y;
                                    buffer.Chests.Add(newChest);
                                }
                            }
                        }
                    }
                    if (Tile.IsSign(curTile.Type))
                    {
                        if (buffer.GetSignAtTile(x, y) == null)
                        {
                            var anchor = world.GetAnchor(x + area.X, y + area.Y);
                            if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                            {
                                var data = world.GetSignAtTile(x + area.X, y + area.Y);
                                if (data != null)
                                {
                                    var newSign = data.Copy();
                                    newSign.X = x;
                                    newSign.Y = y;
                                    buffer.Signs.Add(newSign);
                                }
                            }
                        }
                    }
                    if (Tile.IsTileEntity(curTile.Type))
                    {
                        if (buffer.GetTileEntityAtTile(x, y) == null)
                        {
                            var anchor = world.GetAnchor(x + area.X, y + area.Y);
                            if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                            {
                                var data = world.GetTileEntityAtTile(x + area.X, y + area.Y);
                                if (data != null)
                                {
                                    var newEntity = data.Copy();
                                    newEntity.PosX = (short)x;
                                    newEntity.PosY = (short)y;
                                    buffer.TileEntities.Add(newEntity);
                                }
                            }
                        }
                    }
                    buffer.Tiles[x, y] = curTile;
                }
            }

            buffer.RenderBuffer();
            return buffer;
        }
    }
     */      
        public override void Execute()
        {
            if (view == null)
            {
                view = new();
                view.Owner = Application.Current.MainWindow;
                view.WorldViewModel = _wvm;
                view.DataContext = view;
                view.Show();
                _wvm.SelectedTabIndex = 3;
            }
            else
            {
                view.Show();
                _wvm.SelectedTabIndex = 3;
            }
        }
    }
}
