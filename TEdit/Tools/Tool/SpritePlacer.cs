using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;

namespace TEdit.Tools.Tool
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 10)]
    public class SpritePlacer : ToolBase
    {
        [Import]
        private ToolProperties _properties;

        [Import("World", typeof(World))]
        private World _world;

        [Import]
        private WorldRenderer _renderer;

        [Import] private SpritePicker _spritePicker;

        public SpritePlacer()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/spawn.png"));
            _Name = "Sprite Placer Tool";
            _Type = ToolType.Pencil;
            IsActive = false;
        }

        #region Properties

        private readonly BitmapImage _Image;
        private readonly string _Name;

        private readonly ToolType _Type;
        private bool _IsActive;

        public override string Name
        {
            get { return _Name; }
        }

        public override ToolType Type
        {
            get { return _Type; }
        }

        public override BitmapImage Image
        {
            get { return _Image; }
        }

        public override bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
                if (_IsActive)
                {
                    _properties.MinHeight = 1;
                    _properties.MinWidth = 1;
                    _properties.MaxHeight = 1;
                    _properties.MaxWidth = 1;
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            if (_spritePicker.SelectedSprite != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var cursprite = _spritePicker.SelectedSprite;
                    PlaceSprite(e.Tile, (byte) cursprite.ID, cursprite.Size, cursprite.UpperLeft);
                    //PlaceBrownChest(e.Tile);
                }
                return true;
            }
            return false;
        }


        public override bool MoveTool(TileMouseEventArgs e)
        {
            return false;
        }

        private void PlaceBrownChest(PointInt32 p)
        {
            // anchor at top-right of chest
            int x = p.X;
            int y = p.Y;

            // Validate free space
            if (!_world.Tiles[x, y].IsActive && !_world.Tiles[x + 1, y].IsActive && !_world.Tiles[x, y + 1].IsActive && !_world.Tiles[x + 1, y + 1].IsActive)
            {
                // Validate floor    WorldSettings.Tiles[].IsSolid
                if ((_world.Tiles[x, y + 2].IsActive && (WorldSettings.Tiles[_world.Tiles[x, y + 2].Type].IsSolid)) &&
                    (_world.Tiles[x + 1, y + 2].IsActive && (WorldSettings.Tiles[_world.Tiles[x + 1, y + 2].Type].IsSolid)))
                {
                    // Place Chest
                    var tp = new TilePicker();
                    SetTileSprite(new PointInt32(x, y), new PointShort(0, 0), 21);
                    SetTileSprite(new PointInt32(x, y + 1), new PointShort(0, 18), 21);
                    SetTileSprite(new PointInt32(x + 1, y), new PointShort(18, 0), 21);
                    SetTileSprite(new PointInt32(x + 1, y + 1), new PointShort(18, 18), 21);
                }
            }
        }

        private void PlaceSprite(PointInt32 location, byte type, PointShort size, PointShort upperLeft = new PointShort())
        {
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    var curTile = _world.Tiles[location.X + x, location.Y + y];
                    curTile.IsActive = true;
                    curTile.Type = type;
                    curTile.Frame = new PointShort((short)(upperLeft.X + (x * 18)), (short)(upperLeft.Y + (y * 18)));
                    _renderer.UpdateWorldImage(new PointInt32(location.X + x, location.Y + y));
                }
            }

            if (type == 21)
                _world.Chests.Add(new Chest { Location = location });
            else if (type == 55 || type == 85)
                _world.Signs.Add(new Sign{Location =  location});
        }

        private void SetTileSprite(PointInt32 point, PointShort frame, byte type)
        {
            var curTile = _world.Tiles[point.X, point.Y];
            if (curTile != null)
            {
                curTile.IsActive = true;
                curTile.Type = type;
                curTile.Frame = frame;
                _renderer.UpdateWorldImage(point);
            }
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            // Do nothing on release
            return false;
        }

        public override WriteableBitmap PreviewTool()
        {
            if (_spritePicker.SelectedSprite != null)
            {
                return new WriteableBitmap(
                _spritePicker.SelectedSprite.Size.X,
                _spritePicker.SelectedSprite.Size.Y,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            }
            
            return new WriteableBitmap(
                1,
                1,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        }
    }
}

