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
        [NonSerialized] private readonly ObservableCollection<ColorProperty> _tiles = new ObservableCollection<ColorProperty>();
        [NonSerialized] private readonly ObservableCollection<ColorProperty> _walls = new ObservableCollection<ColorProperty>();
        private bool _IsEraser;
        private TilePickerLiquidProperty _Liquid;
        private TilePickerProperty _Tile;
        private TilePickerProperty _TileMask;
        private TilePickerProperty _Wall;
        private TilePickerProperty _WallMask;

        public TilePicker()
        {
            for (int i = 0; i < byte.MaxValue; i++)
            {
                if (WorldSettings.Walls[i].Name != "UNKNOWN")
                    _walls.Add(WorldSettings.Walls[i]);
            }

            for (int i = 0; i < byte.MaxValue; i++)
            {
                if (WorldSettings.Tiles[i].Name != "UNKNOWN" && !WorldSettings.Tiles[i].IsFramed)
                    _tiles.Add(WorldSettings.Tiles[i]);
            }

            _Wall = new TilePickerProperty {IsActive = false, Value = 0};
            _Tile = new TilePickerProperty {IsActive = true, Value = 0};
            _Liquid = new TilePickerLiquidProperty {IsActive = false, IsLava = false};

            _WallMask = new TilePickerProperty {IsActive = false, Value = 0};
            _TileMask = new TilePickerProperty {IsActive = false, Value = 0};
            _IsEraser = false;
        }

        public ObservableCollection<ColorProperty> Walls
        {
            get { return _walls; }
        }

        public ObservableCollection<ColorProperty> Tiles
        {
            get { return _tiles; }
        }

        public bool IsEraser
        {
            get { return _IsEraser; }
            set { SetProperty(ref _IsEraser, ref value, "IsEraser");}
        }

        public TilePickerProperty Wall
        {
            get { return _Wall; }
            set { SetProperty(ref _Wall, ref value, "Wall");}
        }

        public TilePickerProperty Tile
        {
            get { return _Tile; }
            set { SetProperty(ref _Tile, ref value, "Tile");}
        }

        public TilePickerLiquidProperty Liquid
        {
            get { return _Liquid; }
            set { SetProperty(ref _Liquid, ref value, "Liquid");}
        }

        public TilePickerProperty WallMask
        {
            get { return _WallMask; }
            set { SetProperty(ref _WallMask, ref value, "WallMask");}
        }

        public TilePickerProperty TileMask
        {
            get { return _TileMask; }
            set { SetProperty(ref _TileMask, ref value, "TileMask");}
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
            set { SetProperty(ref _IsActive, ref value, "IsActive");}
        }

        public byte Value
        {
            get { return _value; }
            set { SetProperty(ref _value, ref value, "Value");}
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
            set { SetProperty(ref _IsActive, ref value, "IsActive"); }
        }

        public bool IsLava
        {
            get { return _IsLava; }
            set { SetProperty(ref _IsLava, ref value, "IsLava"); }
        }
    }
}