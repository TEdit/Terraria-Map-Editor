using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.Tools.Clipboard
{
    public partial class ClipboardBuffer : ObservableObject
    {
        public ClipboardBuffer(PointInt32 size)
        {
            Size = size;
            Tiles = new Tile[size.X, size.Y];
        }

        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        private WriteableBitmap _Preview;
        public WriteableBitmap Preview
        {
            get { return this._Preview; }
            set
            {
                if (this._Preview != value)
                {
                    this._Preview = value;
                    this.RaisePropertyChanged("Preview");
                }
            }
        }

        private PointInt32 _size;
        public PointInt32 Size
        {
            get { return this._size; }
            protected set
            {
                if (this._size != value)
                {
                    this._size = value;
                    Tiles = new Tile[_size.X, _size.Y];
                    this.RaisePropertyChanged("Size");
                }
            }
        }

        public Tile[,] Tiles { get; set; }

        private readonly ObservableCollectionEx<Chest> _Chests = new ObservableCollectionEx<Chest>();
        private readonly ObservableCollectionEx<Sign> _Signs = new ObservableCollectionEx<Sign>();

        public ObservableCollection<Chest> Chests
        {
            get { return _Chests; }
        }

        public ObservableCollection<Sign> Signs
        {
            get { return _Signs; }
        }

        public Chest GetChestAtTile(int x, int y)
        {
            return Chests.FirstOrDefault(c => (c.Location.X == x || c.Location.X == x - 1) && (c.Location.Y == y || c.Location.Y == y - 1));
        }

        public Sign GetSignAtTile(int x, int y)
        {
            return Signs.FirstOrDefault(c => (c.Location.X == x || c.Location.X == x - 1) && (c.Location.Y == y || c.Location.Y == y - 1));
        }

        public static ClipboardBuffer GetBufferedRegion(World world, Int32Rect area)
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



        public static void PasteBufferIntoWorld(World world, ClipboardBuffer buffer, PointInt32 anchor)
        {
            for (int x = 0; x < buffer.Size.X; x++)
            {
                for (int y = 0; y < buffer.Size.Y; y++)
                {
                    if (world.IsPointInWorld(x + anchor.X, y + anchor.Y))
                    {
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
                                    world.Signs.Add(new Sign("", new PointInt32(x + anchor.X+1, y + anchor.Y+1)));
                                }
                            }
                        }

                        world.Tiles[x + anchor.X, y + anchor.Y] = curTile;
                    }
                }
            }
        }

    }
}