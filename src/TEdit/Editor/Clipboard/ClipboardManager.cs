﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.ViewModel;
using XNA = Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using System.Collections.Generic;
using System.Linq;

namespace TEdit.Editor.Clipboard
{
    public class ClipboardManager : ObservableObject
    {
        private ClipboardBuffer _buffer;
        private bool _pasteEmpty = true;
        private bool _pasteTiles = true;
        private bool _pasteWalls = true;
        private bool _pasteLiquids = true;
        private bool _pasteWires = true;
        private bool _pasteSprites = true;
        private readonly WorldViewModel _wvm;
        public ClipboardManager(WorldViewModel worldView)
        {
            _wvm = worldView;
        }

        public bool PasteEmpty
        {
            get { return _pasteEmpty; }
            set { Set(nameof(PasteEmpty), ref _pasteEmpty, value); }
        }
        public bool PasteTiles
        {
            get { return _pasteTiles; }
            set { Set(nameof(PasteTiles), ref _pasteTiles, value); }
        }
        public bool PasteWalls
        {
            get { return _pasteWalls; }
            set { Set(nameof(PasteWalls), ref _pasteWalls, value); }
        }
        public bool PasteLiquids
        {
            get { return _pasteLiquids; }
            set { Set(nameof(PasteLiquids), ref _pasteLiquids, value); }
        }
        public bool PasteWires
        {
            get { return _pasteWires; }
            set { Set(nameof(PasteWires), ref _pasteWires, value); }
        }
        public bool PasteSprites
        {
            get { return _pasteSprites; }
            set { Set(nameof(PasteSprites), ref _pasteSprites, value); }
        }
        public ClipboardBuffer Buffer
        {
            get { return _buffer; }
            set { Set(nameof(Buffer), ref _buffer, value); }
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

        public void PasteBufferIntoWorld(Vector2Int32 anchor)
        {
            if (Buffer == null) return;
            if (!PasteTiles && !PasteLiquids && !PasteWalls && !PasteWires) return;

            ErrorLogging.TelemetryClient?.TrackEvent("Paste");

            _wvm.Selection.IsActive = false; // clear selection when pasting to prevent "unable to use pencil" issue
            World world = _wvm.CurrentWorld;
            ClipboardBuffer buffer = _wvm.Clipboard.Buffer;
            for (int x = 0; x < buffer.Size.X; x++)
            {
                for (int y = 0; y < buffer.Size.Y; y++)
                {
                    int worldX = x + anchor.X;
                    int worldY = y + anchor.Y;

                    if (!world.ValidTileLocation(new Vector2Int32(worldX, worldY))) { continue; }

                    //HistMan.AddTileToBuffer(worldX, worldY, ref world.UndoTiles[worldX, worldY]);
                    Tile worldTile = world.Tiles[worldX, worldY];
                    Tile curTile = (Tile)buffer.Tiles[x, y].Clone();

                    if (!PasteTiles)
                    {
                        curTile.IsActive = worldTile.IsActive;
                        curTile.Type = worldTile.Type;
                        curTile.TileColor = worldTile.TileColor;
                        curTile.U = worldTile.U;
                        curTile.V = worldTile.V;
                        curTile.BrickStyle = worldTile.BrickStyle;
                    }

                    if (!PasteEmpty && curTile.IsEmpty)
                    {
                        // skip tiles that are empty if paste empty is not true
                        continue;
                    }

                    if (!PasteWalls)
                    {
                        // if pasting walls is disabled, use the existing wall
                        curTile.Wall = worldTile.Wall;
                        curTile.WallColor = worldTile.WallColor;
                    }

                    if (!PasteLiquids)
                    {
                        // if pasting liquids is disabled, use any existing liquid
                        curTile.LiquidAmount = worldTile.LiquidAmount;
                        curTile.LiquidType = worldTile.LiquidType;
                    }

                    if (!PasteWires)
                    {
                        // if pasting wires is disabled, use any existing wire
                        curTile.WireRed = worldTile.WireRed;
                        curTile.WireGreen = worldTile.WireGreen;
                        curTile.WireBlue = worldTile.WireBlue;
                        curTile.WireYellow = worldTile.WireYellow;
                        curTile.Actuator = worldTile.Actuator;
                        curTile.InActive = worldTile.InActive;
                    }

                    if (!PasteSprites)
                    {
                        // if pasting sprites is disabled, discard them.
                        // check if sprite has more then one tile state.
                        if (World.TileProperties[curTile.Type].Frames.Count() > 0)
                        {
                            // Change Sprite To Air
                            curTile.U = 0;
                            curTile.IsActive = false;
                        }
                    }

                    // save undo
                    _wvm.UndoManager.SaveTile(worldX, worldY);

                    // update world tile
                    world.Tiles[worldX, worldY] = curTile;

                    //  Update chest/sign data only if we've pasted tiles
                    if (PasteTiles)
                    {
                        // Add new chest data
                        if (Tile.IsChest(curTile.Type))
                        {
                            var existingChest = world.GetChestAtTile(worldX, worldY);
                            if (existingChest != null) { world.Chests.Remove(existingChest); }

                            var data = buffer.GetChestAtTile(x, y);
                            if (data != null) // allow? chest copying may not work...
                            {
                                // Copied chest
                                var newChest = data.Copy();
                                newChest.X = worldX;
                                newChest.Y = worldY;
                                world.Chests.Add(newChest);
                            }
                        }

                        // Add new sign data
                        if (Tile.IsSign(curTile.Type))
                        {
                            if (world.GetSignAtTile(worldX, worldY) == null)
                            {
                                var data = buffer.GetSignAtTile(x, y);
                                if (data != null)
                                {
                                    // Copied sign
                                    var newSign = data.Copy();
                                    newSign.X = worldX;
                                    newSign.Y = worldY;
                                    world.Signs.Add(newSign);
                                }
                            }
                        }

                        // Add new tile entity data
                        if (Tile.IsTileEntity(curTile.Type))
                        {
                            if (world.GetTileEntityAtTile(worldX, worldY) == null)
                            {
                                var data = buffer.GetTileEntityAtTile(x, y);
                                if (data != null)
                                {
                                    // Copied sign
                                    var newEntity = data.Copy();
                                    newEntity.PosX = (short)(worldX);
                                    newEntity.PosY = (short)(worldY);
                                    world.TileEntities.Add(newEntity);
                                }
                            }
                        }
                    }

                }
            }

            _wvm.UndoManager.SaveUndo();
            _wvm.CurrentWorld.UpgradeLegacyTileEntities();

            /* Heathtech */
            BlendRules.ResetUVCache(_wvm, anchor.X, anchor.Y, buffer.Size.X, buffer.Size.Y);
        }

        // Reverse the buffer along the x- or y- axis
        public void Flip(ClipboardBuffer buffer, bool flipX, bool rotate)
        {
            ClipboardBuffer flippedBuffer = new ClipboardBuffer(buffer.Size);
            //var sprites = new Dictionary<Vector2Int32, Sprite>();
            var spriteSizes = new Dictionary<Vector2Int32, Vector2Short>();
            int maxX = buffer.Size.X - 1;
            int maxY = buffer.Size.Y - 1;
            for (int x = 0; x <= maxX; x++)
            {
                for (int y = 0; y <= maxY; y++)
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
                    var tileProperties = World.TileProperties[tile.Type];
                    flippedBuffer.Tiles[bufferX, bufferY] = (Tile)tile;

                    // locate all the sprites and make a list
                    if (tileProperties.IsFramed)
                    {
                        var loc = new Vector2Int32(x, y);
                        if (tileProperties.IsOrigin(tile.GetUV()))
                        {
                            Vector2Short tileSize = tileProperties.GetFrameSize(tile.V);
                            spriteSizes[loc] = tileSize;
                        }
                    }
                    else
                    {
                        if (flipX)
                        {
                            //  Ignore multi-width objects when flipping on x-axis

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


                    }
                }
            }

            foreach (var item in spriteSizes)
            {
                var flipOrigin = FlipFramed(buffer.Size, item.Key, item.Value, flipX);

                for (int y = 0; y < item.Value.Y; y++)
                {
                    int sourceY = y + item.Key.Y;
                    int targetY = y + flipOrigin.Y;

                    for (int x = 0; x < item.Value.X; x++)
                    {
                        try
                        {
                            int sourceX = x + item.Key.X;
                            int targetX = x + flipOrigin.X;

                            Tile tile = (Tile)buffer.Tiles[sourceX, sourceY].Clone();

                            if (tile.Type == (uint)TileType.JunctionBox)
                            {
                                if (tile.U == 18) { tile.U = 36; }
                                else if (tile.U == 36) { tile.U = 18; }
                            }

                            flippedBuffer.Tiles[targetX, targetY] = (Tile)tile;
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            foreach (var chest in buffer.Chests)
            {
                var flipOrigin = FlipFramed(buffer.Size, new Vector2Int32(chest.X, chest.Y), new Vector2Short(2, 2), flipX);
                chest.X = flipOrigin.X;
                chest.Y = flipOrigin.Y;
                flippedBuffer.Chests.Add(chest);
            }

            foreach (var sign in buffer.Signs)
            {
                var flipOrigin = FlipFramed(buffer.Size, new Vector2Int32(sign.X, sign.Y), new Vector2Short(2, 2), flipX);
                sign.X = flipOrigin.X;
                sign.Y = flipOrigin.Y;
                flippedBuffer.Signs.Add(sign);
            }

            foreach (var te in buffer.TileEntities)
            {
                var tileProperties = World.TileProperties[(int)te.TileType];
                Vector2Short tileSize = tileProperties.FrameSize[0];

                var flipOrigin = FlipFramed(buffer.Size, new Vector2Int32(te.PosX, te.PosY), tileSize, flipX);
                te.PosX = (short)flipOrigin.X;
                te.PosY = (short)flipOrigin.Y;
                flippedBuffer.TileEntities.Add(te);
            }

            // Replace the existing buffer with the new one
            ClipboardBuffer rotatedBuffer = new ClipboardBuffer(new Vector2Int32(flippedBuffer.Size.Y, flippedBuffer.Size.X));
            int bufferIndex = LoadedBuffers.IndexOf(buffer);
            if (bufferIndex > -1)
            {
                if (rotate)
                {
                    // Attempt to make a new buffer
                    int FlipmaxX = flippedBuffer.Size.X - 1;
                    int FlipmaxY = flippedBuffer.Size.Y - 1;

                    // Get buffer horizontal
                    for (int x = 0; x <= FlipmaxX; x++)
                    {
                        // Get buffer vertical
                        for (int y = 0; y <= FlipmaxY; y++)
                        {
                            // Offet tiles 90
                            Tile tile = (Tile)flippedBuffer.Tiles[x, y].Clone();
                            var tileProperties = World.TileProperties[tile.Type];

                            // kill sprites
                            if (tileProperties.IsFramed)
                            {
                                tile.IsActive = false;
                            }
                            rotatedBuffer.Tiles[y, x] = (Tile)tile; // Flipping x & y causes a rotation of 90 to the right
                        }
                    }

                    // Replace Buffers
                    LoadedBuffers.Insert(bufferIndex, rotatedBuffer);
                    LoadedBuffers.RemoveAt(bufferIndex + 1);
                }
                else
                {
                    LoadedBuffers.Insert(bufferIndex, flippedBuffer);
                    LoadedBuffers.RemoveAt(bufferIndex + 1);
                }
            }

            if (rotate)
            {
                rotatedBuffer.RenderBuffer();
            }
            else
            {
                flippedBuffer.RenderBuffer();
            }

            if (Buffer == buffer)
            {
                if (rotate)
                {
                    Buffer = rotatedBuffer;
                }
                else
                {
                    Buffer = flippedBuffer;
                }
                _wvm.PreviewChange();
            }
        }

        public static Vector2Int32 FlipFramed(Vector2Int32 totalSize, Vector2Int32 origin, Vector2Short spriteSize, bool flipX)
        {
            var maxX = totalSize.X - 1;
            var maxY = totalSize.Y - 1;

            int bufferX;
            int bufferY;

            if (flipX)
            {
                // flip
                bufferX = maxX - origin.X - (spriteSize.X - 1);
                bufferY = origin.Y;
            }
            else
            {
                bufferX = origin.X;
                bufferY = maxY - origin.Y - (spriteSize.Y - 1);
            }

            return new Vector2Int32(bufferX, bufferY);
        }


        public void FlipX(ClipboardBuffer buffer)
        {
            Flip(buffer, true, false);
        }
        public void FlipY(ClipboardBuffer buffer)
        {
            Flip(buffer, false, false);
        }
        public void Rotate(ClipboardBuffer buffer)
        {
            Flip(buffer, false, true);
        }
    }
}
