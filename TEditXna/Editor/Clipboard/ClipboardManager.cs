using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.Utility;
using TEditXna.ViewModel;
using XNA = Microsoft.Xna.Framework;
using TEditXNA.Terraria;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Clipboard
{
    public class ClipboardManager : ObservableObject
    {
        private ClipboardBuffer _buffer;
        private bool _pasteEmpty = true;
        private bool _pasteTiles = true;
        private bool _pasteWalls = true;
        private bool _pasteLiquids = true;
        private bool _pasteWires = true;
        private readonly WorldViewModel _wvm;
        public ClipboardManager(WorldViewModel worldView)
        {
            _wvm = worldView;
        }

        public bool PasteEmpty
        {
            get { return _pasteEmpty; }
            set { Set("PasteEmpty", ref _pasteEmpty, value); }
        }
        public bool PasteTiles
        {
            get { return _pasteTiles; }
            set { Set("PasteTiles", ref _pasteTiles, value); }
        }
        public bool PasteWalls
        {
            get { return _pasteWalls; }
            set { Set("PasteWalls", ref _pasteWalls, value); }
        }
        public bool PasteLiquids
        {
            get { return _pasteLiquids; }
            set { Set("PasteLiquids", ref _pasteLiquids, value); }
        }
        public bool PasteWires
        {
            get { return _pasteWires; }
            set { Set("PasteWires", ref _pasteWires, value); }
        }
        public ClipboardBuffer Buffer
        {
            get { return _buffer; }
            set { Set("Buffer", ref _buffer, value); }
        }

        private readonly ObservableCollection<ClipboardBuffer> _loadedBuffers = new ObservableCollection<ClipboardBuffer>();
        public ObservableCollection<ClipboardBuffer> LoadedBuffers
        {
            get { return _loadedBuffers; }
        }

        public void ClearBuffers()
        {
            Buffer = null;
            LoadedBuffers.Clear();
        }

        public void Remove(ClipboardBuffer item)
        {
            if (LoadedBuffers.Contains(item))
                LoadedBuffers.Remove(item);
        }

        public void Import(string filename)
        {
            ClipboardBuffer buffer = null;
            try
            {
                buffer = ClipboardBuffer.Load(filename);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Schematic File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (buffer != null)
            {
                buffer.RenderBuffer();
                LoadedBuffers.Add(buffer);
            }

        }

        public ClipboardBuffer GetSelectionBuffer()
        {
            World world = _wvm.CurrentWorld;
            XNA.Rectangle area = _wvm.Selection.SelectionArea;
            var buffer = new ClipboardBuffer(new Vector2Int32(area.Width, area.Height));

            for (int x = 0; x < area.Width; x++)
            {
                for (int y = 0; y < area.Height; y++)
                {
                    Tile curTile = (Tile)world.Tiles[x + area.X, y + area.Y].Clone();

                    if (Tile.IsChest(curTile.Type))
                    {
                        if (buffer.GetChestAtTile(x, y) == null)
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
                    if (Tile.IsSign(curTile.Type))
                    {
                        if (buffer.GetSignAtTile(x, y) == null)
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
                    buffer.Tiles[x, y] = curTile;
                }
            }

            buffer.RenderBuffer();
            return buffer;
        }

        public void PasteBufferIntoWorld(Vector2Int32 anchor)
        {
            if (Buffer == null)
                return;
            World world = _wvm.CurrentWorld;
            ClipboardBuffer buffer = _wvm.Clipboard.Buffer;
            for (int x = 0; x < buffer.Size.X; x++)
            {
                for (int y = 0; y < buffer.Size.Y; y++)
                {
                    int worldX = x + anchor.X;
                    int worldY = y + anchor.Y;

                    if (world.ValidTileLocation(new Vector2Int32(x + anchor.X, y + anchor.Y)))
                    {
                        //HistMan.AddTileToBuffer(x + anchor.X, y + anchor.Y, ref world.UndoTiles[x + anchor.X, y + anchor.Y]);

                        Tile curTile;

                        if (PasteTiles)
                        {
                            curTile = (Tile)buffer.Tiles[x, y].Clone();
                            curTile.TileColor = buffer.Tiles[x, y].TileColor;
                        }
                        else
                        {
                            // if pasting tiles is disabled, use the existing tile with buffer's wall & extras
                            curTile = (Tile)world.Tiles[worldX, worldY].Clone();
                            curTile.Wall = buffer.Tiles[x, y].Wall;
                            curTile.WallColor = buffer.Tiles[x, y].WallColor;
                            curTile.LiquidAmount = buffer.Tiles[x, y].LiquidAmount;
                            curTile.LiquidType = buffer.Tiles[x, y].LiquidType;
                            curTile.WireRed = buffer.Tiles[x, y].WireRed;
                            curTile.WireGreen = buffer.Tiles[x, y].WireGreen;
                            curTile.WireBlue = buffer.Tiles[x, y].WireBlue;
                            curTile.WireYellow = buffer.Tiles[x, y].WireYellow;
                            curTile.Actuator = buffer.Tiles[x, y].Actuator;
                            curTile.InActive = buffer.Tiles[x, y].InActive;
                        }

                        if (!PasteEmpty && (curTile.LiquidAmount == 0 && !curTile.IsActive && curTile.Wall == 0 && !curTile.WireRed && !curTile.WireBlue && !curTile.WireGreen && !curTile.WireYellow))
                        {
                            // skip tiles that are empty if paste empty is not true
                            continue;
                        }
                        if (!PasteWalls)
                        {
                            // if pasting walls is disabled, use the existing wall
                            curTile.Wall = world.Tiles[worldX, worldY].Wall;
                            curTile.WallColor = world.Tiles[worldX, worldY].WallColor;
                        }
                        if (!PasteLiquids)
                        {
                            // if pasting liquids is disabled, use any existing liquid
                            curTile.LiquidAmount = world.Tiles[worldX, worldY].LiquidAmount;
                            curTile.LiquidType = world.Tiles[worldX, worldY].LiquidType;
                        }
                        if (!PasteWires)
                        {
                            // if pasting wires is disabled, use any existing wire
                            Tile worldTile = world.Tiles[worldX, worldY];
                            curTile.WireRed = worldTile.WireRed;
                            curTile.WireGreen = worldTile.WireGreen;
                            curTile.WireBlue = worldTile.WireBlue;
                            curTile.WireYellow = worldTile.WireYellow;
                            curTile.Actuator = worldTile.Actuator;
                            curTile.InActive = worldTile.InActive;
                        }
                        //  Update chest/sign data only if we've pasted tiles
                        if (PasteTiles)
                        {
                            // Remove overwritten chests data
                            if (Tile.IsChest(world.Tiles[x + anchor.X, y + anchor.Y].Type))
                            {
                                var data = world.GetChestAtTile(x + anchor.X, y + anchor.Y);
                                if (data != null)
                                {
                                    _wvm.UndoManager.Buffer.Chests.Add(data);
                                    world.Chests.Remove(data);
                                }
                            }

                            // Remove overwritten sign data
                            if (Tile.IsSign(world.Tiles[x + anchor.X, y + anchor.Y].Type))
                            {
                                var data = world.GetSignAtTile(x + anchor.X, y + anchor.Y);
                                if (data != null)
                                {
                                    _wvm.UndoManager.Buffer.Signs.Add(data);
                                    world.Signs.Remove(data);
                                }
                            }


                            // Add new chest data
                            if (Tile.IsChest(curTile.Type))
                            {
                                if (world.GetChestAtTile(x + anchor.X, y + anchor.Y) == null)
                                {
                                    var data = buffer.GetChestAtTile(x, y);
                                    if (data != null) // allow? chest copying may not work...
                                    {
                                        // Copied chest
                                        var newChest = data.Copy();
                                        newChest.X = x + anchor.X;
                                        newChest.Y = y + anchor.Y;
                                        world.Chests.Add(newChest);
                                    }
                                    else
                                    {
                                        // Empty chest
                                        world.Chests.Add(new Chest(x + anchor.X, y + anchor.Y));
                                    }
                                }
                            }

                            // Add new sign data
                            if (Tile.IsSign(curTile.Type))
                            {
                                if (world.GetSignAtTile(x + anchor.X, y + anchor.Y) == null)
                                {
                                    var data = buffer.GetSignAtTile(x, y);
                                    if (data != null)
                                    {
                                        // Copied sign
                                        var newSign = data.Copy();
                                        newSign.X = x + anchor.X;
                                        newSign.Y = y + anchor.Y;
                                        world.Signs.Add(newSign);
                                    }
                                    else
                                    {
                                        world.Signs.Add(new Sign(x + anchor.X, y + anchor.Y, string.Empty));
                                    }
                                }
                            }
                        }
                        _wvm.UndoManager.SaveTile(x + anchor.X, y + anchor.Y);
                        world.Tiles[x + anchor.X, y + anchor.Y] = curTile;
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();

            /* Heathtech */
            BlendRules.ResetUVCache(_wvm, anchor.X, anchor.Y, buffer.Size.X, buffer.Size.Y);
        }

        // Reverse the buffer along the x- or y- axis
        public void Flip(ClipboardBuffer buffer, bool flipX)
        {
            ClipboardBuffer flippedBuffer = new ClipboardBuffer(buffer.Size);

            for (int x = 0, maxX = buffer.Size.X - 1; x <= maxX; x++)
            {
                for (int y = 0, maxY = buffer.Size.Y - 1; y <= maxY; y++)
                {
                    int bufferX;
                    int bufferY;

                    if (flipX)
                    {
                        bufferX = maxX - x;
                        bufferY = y;
                    }
                    else
                    {
                        bufferX = x;
                        bufferY = maxY - y;
                    }

                    Tile tile = (Tile)buffer.Tiles[x, y].Clone();

                    Vector2Short tileSize = World.TileProperties[tile.Type].FrameSize;

                    if (flipX)
                    {
                        //  Ignore multi-width objects when flipping on x-axis
                        if (tileSize.X > 1)
                            ClearTile(tile);
                        // Flip brick-style
                        switch (tile.BrickStyle)
                        {
                            case BrickStyle.SlopeTopRight:
                                tile.BrickStyle = BrickStyle.SlopeTopLeft;
                                break;
                            case BrickStyle.SlopeTopLeft:
                                tile.BrickStyle = BrickStyle.SlopeTopRight;
                                break;
                            case BrickStyle.SlopeBottomRight:
                                tile.BrickStyle = BrickStyle.SlopeBottomLeft;
                                break;
                            case BrickStyle.SlopeBottomLeft:
                                tile.BrickStyle = BrickStyle.SlopeBottomRight;
                                break;
                        }
                    }

                    else
                    {
                        //  Ignore multi-height tiles when flipping on y-axis
                        if (tileSize.Y > 1)
                            ClearTile(tile);
                        // Flip brick-style
                        switch (tile.BrickStyle)
                        {
                            case BrickStyle.SlopeTopRight:
                                tile.BrickStyle = BrickStyle.SlopeBottomRight;
                                break;
                            case BrickStyle.SlopeTopLeft:
                                tile.BrickStyle = BrickStyle.SlopeBottomLeft;
                                break;
                            case BrickStyle.SlopeBottomRight:
                                tile.BrickStyle = BrickStyle.SlopeTopRight;
                                break;
                            case BrickStyle.SlopeBottomLeft:
                                tile.BrickStyle = BrickStyle.SlopeTopLeft;
                                break;
                        }
                    }

                    flippedBuffer.Tiles[bufferX, bufferY] = (Tile)tile;
                }
            }

            // Replace the existing buffer with the new one
            int bufferIndex = LoadedBuffers.IndexOf(buffer);
            if (bufferIndex > -1)
            {
                LoadedBuffers.Insert(bufferIndex, flippedBuffer);
                LoadedBuffers.RemoveAt(bufferIndex + 1);
            }

            flippedBuffer.RenderBuffer();

            if (Buffer == buffer)
            {
                Buffer = flippedBuffer;
                _wvm.PreviewChange();
            }
        }

        protected void ClearTile(Tile tile)
        {
            tile.Type = 0;
            tile.IsActive = false;
            tile.U = 0;
            tile.V = 0;
        }

        public void FlipX(ClipboardBuffer buffer)
        {
            Flip(buffer, true);
        }
        public void FlipY(ClipboardBuffer buffer)
        {
            Flip(buffer, false);
        }
    }
}
