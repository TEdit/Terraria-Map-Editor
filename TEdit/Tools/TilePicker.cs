using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using TEdit.Common;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;

namespace TEdit.Tools
{
    [Serializable]
    [Export]
    public class TilePicker : ObservableObject
    {
        [NonSerialized] private readonly ObservableCollection<TileColor> _tiles = new ObservableCollection<TileColor>();
        [NonSerialized] private readonly ObservableCollection<TileColor> _walls = new ObservableCollection<TileColor>();
        private bool _IsEraser;
        private TilePickerLiquidProperty _Liquid;
        private TilePickerProperty _Tile;
        private TilePickerProperty _TileMask;
        private TilePickerProperty _Wall;
        private TilePickerProperty _WallMask;

        public TilePicker()
        {
            for (int i = 0; i < TileProperties.MaxWallTypes; i++)
            {
                _walls.Add(Settings.Walls[i]);
            }

            for (int i = 0; i < TileProperties.MaxTileTypes; i++)
            {
                if (TileProperties.TileSolid[i])
                    _tiles.Add(Settings.Tiles[i]);
            }

            _Wall = new TilePickerProperty {IsActive = false, Value = 0};
            _Tile = new TilePickerProperty {IsActive = true, Value = 0};
            _Liquid = new TilePickerLiquidProperty {IsActive = false, IsLava = false};

            _WallMask = new TilePickerProperty {IsActive = false, Value = 0};
            _TileMask = new TilePickerProperty {IsActive = false, Value = 0};
            _IsEraser = false;
        }

        public ObservableCollection<TileColor> Walls
        {
            get { return _walls; }
        }

        public ObservableCollection<TileColor> Tiles
        {
            get { return _tiles; }
        }

        public bool IsEraser
        {
            get { return _IsEraser; }
            set
            {
                if (_IsEraser != value)
                {
                    _IsEraser = value;
                    RaisePropertyChanged("IsEraser");
                }
            }
        }

        public TilePickerProperty Wall
        {
            get { return _Wall; }
            set
            {
                if (_Wall != value)
                {
                    _Wall = value;
                    RaisePropertyChanged("Wall");
                }
            }
        }

        public TilePickerProperty Tile
        {
            get { return _Tile; }
            set
            {
                if (_Tile != value)
                {
                    _Tile = value;
                    RaisePropertyChanged("Tile");
                }
            }
        }

        public TilePickerLiquidProperty Liquid
        {
            get { return _Liquid; }
            set
            {
                if (_Liquid != value)
                {
                    _Liquid = value;
                    RaisePropertyChanged("Liquid");
                }
            }
        }

        public TilePickerProperty WallMask
        {
            get { return _WallMask; }
            set
            {
                if (_WallMask != value)
                {
                    _WallMask = value;
                    RaisePropertyChanged("WallMask");
                }
            }
        }

        public TilePickerProperty TileMask
        {
            get { return _TileMask; }
            set
            {
                if (_TileMask != value)
                {
                    _TileMask = value;
                    RaisePropertyChanged("TileMask");
                }
            }
        }
    }

    [Serializable]
    public class TilePickerProperty : ObservableObject
    {
        private bool _IsActive;

        private byte _value;

        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        public byte Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }
    }

    [Serializable]
    public class TilePickerLiquidProperty : ObservableObject
    {
        private bool _IsActive;

        private bool _IsLava;

        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        public bool IsLava
        {
            get { return _IsLava; }
            set
            {
                if (_IsLava != value)
                {
                    _IsLava = value;
                    RaisePropertyChanged("IsLava");
                }
            }
        }
    }
}