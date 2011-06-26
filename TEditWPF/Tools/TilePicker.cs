using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using TEditWPF.Common;
using TEditWPF.RenderWorld;
using TEditWPF.TerrariaWorld;

namespace TEditWPF.Tools
{
    [Export]
    public class TilePicker : ObservableObject
    {
        public TilePicker()
        {
            for (int i = 0; i < TerrariaWorld.TileProperties.MaxWallTypes; i++)
            {
                _walls.Add(TileColors.Walls[i]);
            }

            for (int i = 0; i < TerrariaWorld.TileProperties.MaxTileTypes; i++)
            {
                if (TileProperties.TileSolid[i])
                    _tiles.Add(TileColors.Tiles[i]);
            }

            _Wall = new TilePickerProperty { IsActive = false, Value = 0 };
            _Tile = new TilePickerProperty { IsActive = true, Value = 0 };
            _Liquid = new TilePickerLiquidProperty { IsActive = false, IsLava = false};

            _WallMask = new TilePickerProperty { IsActive = false, Value = 0 };
            _TileMask = new TilePickerProperty { IsActive = false, Value = 0 };
            _IsEraser = false;
        }

        private readonly ObservableCollection<TileColor> _walls = new ObservableCollection<TileColor>();
        public ObservableCollection<TileColor> Walls
        {
            get { return _walls; }
        }

        private readonly ObservableCollection<TileColor> _tiles = new ObservableCollection<TileColor>();
        public ObservableCollection<TileColor> Tiles
        {
            get { return _tiles; }
        }

        private bool _IsEraser;
        public bool IsEraser
        {
            get { return this._IsEraser; }
            set
            {
                if (this._IsEraser != value)
                {
                    this._IsEraser = value;
                    this.RaisePropertyChanged("IsEraser");
                }
            }
        }

        private TilePickerProperty _Wall;
        public TilePickerProperty Wall
        {
            get { return this._Wall; }
            set
            {
                if (this._Wall != value)
                {
                    this._Wall = value;
                    this.RaisePropertyChanged("Wall");
                }
            }
        }

        private TilePickerProperty _Tile;
        public TilePickerProperty Tile
        {
            get { return this._Tile; }
            set
            {
                if (this._Tile != value)
                {
                    this._Tile = value;
                    this.RaisePropertyChanged("Tile");
                }
            }
        }

        private TilePickerLiquidProperty _Liquid;
        public TilePickerLiquidProperty Liquid
        {
            get { return this._Liquid; }
            set
            {
                if (this._Liquid != value)
                {
                    this._Liquid = value;
                    this.RaisePropertyChanged("Liquid");
                }
            }
        }

        private TilePickerProperty _WallMask;
        public TilePickerProperty WallMask
        {
            get { return this._WallMask; }
            set
            {
                if (this._WallMask != value)
                {
                    this._WallMask = value;
                    this.RaisePropertyChanged("WallMask");
                }
            }
        }

        private TilePickerProperty _TileMask;
        public TilePickerProperty TileMask
        {
            get { return this._TileMask; }
            set
            {
                if (this._TileMask != value)
                {
                    this._TileMask = value;
                    this.RaisePropertyChanged("TileMask");
                }
            }
        }

    }

    public class TilePickerProperty : ObservableObject
    {
        private bool _IsActive;
        public bool IsActive
        {
            get { return this._IsActive; }
            set
            {
                if (this._IsActive != value)
                {
                    this._IsActive = value;
                    this.RaisePropertyChanged("IsActive");
                }
            }
        }

        private byte _value;
        public byte Value
        {
            get { return this._value; }
            set
            {
                if (this._value != value)
                {
                    this._value = value;
                    this.RaisePropertyChanged("Value");
                }
            }
        }
    }

    public class TilePickerLiquidProperty : ObservableObject
    {
        private bool _IsActive;
        public bool IsActive
        {
            get { return this._IsActive; }
            set
            {
                if (this._IsActive != value)
                {
                    this._IsActive = value;
                    this.RaisePropertyChanged("IsActive");
                }
            }
        }

        private bool _IsLava;
        public bool IsLava
        {
            get { return this._IsLava; }
            set
            {
                if (this._IsLava != value)
                {
                    this._IsLava = value;
                    this.RaisePropertyChanged("IsLava");
                }
            }
        }
    }
}