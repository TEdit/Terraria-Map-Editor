using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.TerrariaWorld;

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
    }
}