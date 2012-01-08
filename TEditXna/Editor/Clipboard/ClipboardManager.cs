using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using BCCL.Utility;
using TEditXna.ViewModel;
using XNA = Microsoft.Xna.Framework;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Clipboard
{
    public class ClipboardManager : ObservableObject
    {
        private ClipboardBuffer _buffer;
        private bool _pasteEmpty = true;
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
            try
            {
                var buffer = ClipboardBuffer.Load(filename);
                if (buffer != null)
                {
                    buffer.RenderBuffer();
                    LoadedBuffers.Add(buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Schematic File Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    if (curTile.Type == 21) // 21 Chest , 55 sign
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
                    if (curTile.Type == 55 || curTile.Type == 85) // 21 Chest , 55 sign
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
                    if (world.ValidTileLocation(new Vector2Int32(x + anchor.X, y + anchor.Y)))
                    {
                        //HistMan.AddTileToBuffer(x + anchor.X, y + anchor.Y, ref world.Tiles[x + anchor.X, y + anchor.Y]);

                        Tile curTile = (Tile)buffer.Tiles[x, y].Clone();

                        if (!PasteEmpty && (curTile.Liquid == 0 && !curTile.IsActive && curTile.Wall == 0 && !curTile.HasWire))
                        {
                            // skip tiles that are empty if paste empty is not true
                            continue;
                        }

                        // Remove overwritten chests data
                        if (world.Tiles[x + anchor.X, y + anchor.Y].Type == 21)
                        {
                            var data = world.GetChestAtTile(x + anchor.X, y + anchor.Y);
                            if (data != null)
                            {
                                _wvm.UndoManager.Buffer.Chests.Add(data);
                                world.Chests.Remove(data);
                            }
                        }

                        // Remove overwritten sign data
                        if (world.Tiles[x + anchor.X, y + anchor.Y].Type == 55 || world.Tiles[x + anchor.X, y + anchor.Y].Type == 85)
                        {
                            var data = world.GetSignAtTile(x + anchor.X, y + anchor.Y);
                            if (data != null)
                            {
                                _wvm.UndoManager.Buffer.Signs.Add(data);
                                world.Signs.Remove(data);
                            }
                        }


                        // Add new chest data
                        if (curTile.Type == 21)
                        {
                            if (world.GetChestAtTile(x + anchor.X, y + anchor.Y) == null)
                            {
                                var data = buffer.GetChestAtTile(x, y);
                                if (data != null) // allow? chest copying may not work...
                                {
                                    // Copied chest
                                    var newChest = data.Copy();
                                    newChest.X = x + anchor.X;
                                    newChest.Y =  y + anchor.Y;
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
                        if (curTile.Type == 55 || curTile.Type == 85)
                        {
                            if (world.GetSignAtTile(x + anchor.X, y + anchor.Y) == null)
                            {
                                var data = buffer.GetSignAtTile(x, y);
                                if (data != null)
                                {
                                    // Copied sign
                                    var newSign = data.Copy();
                                    newSign.X = x + anchor.X;
                                    newSign.Y =  y + anchor.Y;
                                    world.Signs.Add(newSign);
                                }
                                else
                                {
                                    world.Signs.Add(new Sign(x + anchor.X, y + anchor.Y, string.Empty));
                                }
                            }
                        }
                        _wvm.UndoManager.SaveTile(x + anchor.X, y + anchor.Y);
                        world.Tiles[x + anchor.X, y + anchor.Y] = curTile;
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();
        }
    }
}