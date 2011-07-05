using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using TEdit.Common;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;
using TEdit.Tools.History;

namespace TEdit.Tools.Clipboard
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ClipboardManager : ObservableObject
    {
        [Import]
        private HistoryManager HistMan;
        private Queue<HistoryTile> history;

        private ClipboardBuffer _Buffer;
        public ClipboardBuffer Buffer
        {
            get { return this._Buffer; }
            set
            {
                if (this._Buffer != value)
                {
                    this._Buffer = value;
                    this.RaisePropertyChanged("Buffer");
                }
            }
        }

        private readonly ObservableCollection<ClipboardBuffer> _LoadedBuffers = new ObservableCollection<ClipboardBuffer>();
        public ObservableCollection<ClipboardBuffer> LoadedBuffers
        {
            get { return _LoadedBuffers; }
        }

        public void ClearBuffers()
        {
            Buffer = null;
            LoadedBuffers.Clear();
        }

        public ClipboardBuffer GetBufferedRegion(World world, Int32Rect area)
        {
            var buffer = new ClipboardBuffer(new PointInt32(area.Width, area.Height));

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
                                var newChest = Utility.DeepCopy(data);
                                newChest.Location = new PointInt32(x, y);
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
                                var newSign = Utility.DeepCopy(data);
                                newSign.Location = new PointInt32(x, y);
                                buffer.Signs.Add(newSign);
                            }
                        }
                    }


                    buffer.Tiles[x, y] = curTile;
                }
            }

            return buffer;
        }

        public void PasteBufferIntoWorld(World world, ClipboardBuffer buffer, PointInt32 anchor)
        {
            history = new Queue<HistoryTile>();
            for (int x = 0; x < buffer.Size.X; x++)
            {
                for (int y = 0; y < buffer.Size.Y; y++)
                {
                    if (world.IsPointInWorld(x + anchor.X, y + anchor.Y))
                    {
                        history.Enqueue(new HistoryTile(new PointInt32(x + anchor.X, y + anchor.Y), (Tile)world.Tiles[x + anchor.X, y + anchor.Y].Clone()));
                        Tile curTile = (Tile)buffer.Tiles[x, y].Clone();

                        // Remove overwritten chests data
                        if (world.Tiles[x + anchor.X, y + anchor.Y].Type == 21)
                        {
                            var data = world.GetChestAtTile(x + anchor.X, y + anchor.Y);
                            if (data != null)
                                world.Chests.Remove(data);
                        }

                        // Remove overwritten sign data
                        if (world.Tiles[x + anchor.X, y + anchor.Y].Type == 55 || world.Tiles[x + anchor.X, y + anchor.Y].Type == 85)
                        {
                            var data = world.GetSignAtTile(x + anchor.X, y + anchor.Y);
                            if (data != null)
                                world.Signs.Remove(data);
                        }


                        // Add new chest data
                        if (curTile.Type == 21)
                        {
                            if (world.GetChestAtTile(x + anchor.X, y + anchor.Y) == null)
                            {
                                var data = buffer.GetChestAtTile(x, y);
                                if (data != null && false) // disallow chest copying
                                {
                                    // Copied chest
                                    var newChest = Utility.DeepCopy(data);
                                    newChest.Location = new PointInt32(x + anchor.X, y + anchor.Y);
                                    world.Chests.Add(newChest);
                                }
                                else
                                {
                                    // Empty chest
                                    world.Chests.Add(new Chest { Location = new PointInt32(x + anchor.X, y + anchor.Y) });
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
                                    var newSign = Utility.DeepCopy(data);
                                    newSign.Location = new PointInt32(x + anchor.X + 1, y + anchor.Y + 1);
                                    world.Signs.Add(newSign);
                                }
                                else
                                {
                                    world.Signs.Add(new Sign("", new PointInt32(x + anchor.X + 1, y + anchor.Y + 1)));
                                }
                            }
                        }

                        world.Tiles[x + anchor.X, y + anchor.Y] = curTile;
                    }
                }
            }

            HistMan.AddUndo(history);
        }
    }
}