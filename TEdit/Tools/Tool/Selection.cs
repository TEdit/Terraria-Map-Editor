using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.TerrariaWorld;

namespace TEdit.Tools.Tool
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 2)]
    public class Selection : ToolBase
    {
        [Import]
        private ToolProperties _properties;
        [Import]
        private SelectionArea _selection;


        private PointInt32 _startselection;
        [Import("World", typeof(World))]
        private World _world;

        public Selection()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/shape_square.png"));
            _Name = "Selection";
            _Type = ToolType.Selection;
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
            if (e.LeftButton == MouseButtonState.Pressed)
                _startselection = e.Tile;
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Released)
            {
                _selection.Deactive();
            }
            return true;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _selection.SetRectangle(_startselection, e.Tile);
            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            // Do nothing on release
            return true;
        }

        public override WriteableBitmap PreviewTool()
        {
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