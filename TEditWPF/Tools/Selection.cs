using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 2)]
    public class Selection : ToolBase
    {
        public Selection()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEditWPF;component/Tools/Images/shape_square.png"));
            _Name = "Selection";
            _Type = ToolType.Selection;
            IsActive = false;
        }

        #region Properties
        private string _Name;
        public override string Name
        {
            get { return _Name; }
        }

        private ToolType _Type;
        public override ToolType Type
        {
            get { return _Type; }
        }

        private BitmapImage _Image;
        public override BitmapImage Image
        {
            get { return _Image; }
        }

        private bool _IsActive;
        public override bool IsActive
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
        #endregion

        [Import]
        private SelectionArea _selection;

        [Import("World", typeof(World))]
        private World _world;

        

        PointInt32 _startselection;
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

        [Import]
        private ToolProperties _properties;
        public override WriteableBitmap PreviewTool()
        {
            return new WriteableBitmap(
                _properties.Size.Width,
                _properties.Size.Height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        }
    }
}
