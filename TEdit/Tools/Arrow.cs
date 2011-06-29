using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.TerrariaWorld;
using TEdit.Views;

namespace TEdit.Tools
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 1)]
    public class Arrow : ToolBase
    {
        [Import]
        private ToolProperties _properties;
        [Import("World", typeof(World))]
        private World _world;

#if DEBUG
        // The world just isn't quite ready for this...
        private ChestEditorPopup _chestPopup = null;
#else
        private ChestsContentsPopup _chestPopup = null;
#endif

        private SignPopup _signPopup = null;

        public Arrow()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/cursor.png"));
            _Name = "Arrow";
            _Type = ToolType.Arrow;
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
                    if (_IsActive)
                    {
                        _properties.MinHeight = 1;
                        _properties.MinWidth = 1;
                        _properties.MaxHeight = 1;
                        _properties.MaxWidth = 1;
                    }
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {

            return false;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            return false;
        }

        private void ClosePopups()
        {
            if (_chestPopup != null)
            {
                _chestPopup.IsOpen = false;
                _chestPopup = null;
            }
            if (_signPopup != null)
            {
                _signPopup.IsOpen = false;
                _signPopup = null;
            }
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            ClosePopups();

            // From Terrafirma
            foreach (Chest c in _world.Chests)
            {
                //chests are 2x2, and their x/y is upper left corner
                if ((c.Location.X == e.Tile.X || c.Location.X + 1 == e.Tile.X) &&
                    (c.Location.Y == e.Tile.Y || c.Location.Y + 1 == e.Tile.Y))
                {
#if DEBUG
                    _chestPopup = new ChestEditorPopup(c);
#else
                    _chestPopup = new ChestsContentsPopup(c);
#endif
                    _chestPopup.IsOpen = true;
                }
            }
            foreach (Sign s in _world.Signs)
            {
                //signs are 2x2, and their x/y is upper left corner
                if ((s.Location.X == e.Tile.X || s.Location.X + 1 == e.Tile.X) && (s.Location.Y == e.Tile.Y || s.Location.Y + 1 == e.Tile.Y))
                {
                    _signPopup = new SignPopup(s);
                    _signPopup.IsOpen = true;
                }
            }
            return false;
        }

        public override WriteableBitmap PreviewTool()
        {
            var bmp = new WriteableBitmap(
                1,
                1,
                96,
                96,
                PixelFormats.Bgra32,
                null);


            bmp.Clear();
            bmp.SetPixel(0, 0, 127, 0, 90, 255);
            return bmp;
        }
    }
}