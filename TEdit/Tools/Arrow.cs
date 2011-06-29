using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.TerrariaWorld;
using TEdit.Views;

namespace TEdit.Tools
{
    [Export(typeof (ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 1)]
    public class Arrow : ToolBase
    {
        [Import] private ToolProperties _properties;
        [Import("World", typeof(World))]
        private World _world;

        public Arrow()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Tools/Images/cursor.png"));
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

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            foreach (var c in _world.Chests)
            {
                //chests are 2x2, and their x/y is upper left corner
                if ((c.Location.X == e.Tile.X || c.Location.X + 1 == e.Tile.X) && (c.Location.Y == e.Tile.Y || c.Location.Y + 1 == e.Tile.Y))
                {
                    var chestPop = new ChestsContentsPopup(c);
                    chestPop.IsOpen = true;
                }
            }
            //foreach (Sign s in signs)
            //{
            //    //signs are 2x2, and their x/y is upper left corner
            //    if ((s.x == sx || s.x + 1 == sx) && (s.y == sy || s.y + 1 == sy))
            //    {
            //        signPop = new SignPopup(s.text);
            //        signPop.IsOpen = true;
            //    }
            //}
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