using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.RenderWorld;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export(typeof (ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 4)]
    public class Brush : ToolBase
    {
        [Import]
        private ToolProperties _properties;
        [Import]
        private SelectionArea _selection;

        [Import("World", typeof(World))]
        private World _world;


        [Import]
        private WorldRenderer renderer;

        [Import]
        private TilePicker tilePicker;

        private bool isLeftDown;

        private PointInt32 start;

        public Brush()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEditWPF;component/Tools/Images/paintbrush.png"));
            _Name = "Brush";
            _Type = ToolType.Brush;
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
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isLeftDown = true;
                start = e.Tile;
            }
            return true;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DrawLine(e);
            }
            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            if (_selection.Rectangle.Contains(e.Tile) && isLeftDown)
            {
                _world.FillRectangle(_selection.Rectangle, tilePicker);
                renderer.UpdateWorldImage(_selection.Rectangle);
            }
            else
            {
                if (isLeftDown)
                    DrawLine(e);
            }
            isLeftDown = false;
            return true;
        }

        public override WriteableBitmap PreviewTool()
        {
            var bmp = new WriteableBitmap(
                    _properties.Size.Width,
                    _properties.Size.Height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null);


            bmp.Clear();
            if (_properties.BrushShape == ToolBrushShape.Square)
            {
                bmp.FillRectangle(0, 0, _properties.Size.Width, _properties.Size.Height, Color.FromArgb(127, 0, 90, 255));
            }
            else
            {
                bmp.FillEllipse(0, 0, _properties.Size.Width, _properties.Size.Height, Color.FromArgb(127, 0, 90, 255));
            }
            return bmp;
        }

        private void DrawLine(TileMouseEventArgs e)
        {
            foreach (PointInt32 p in WorldRenderer.DrawLine(start, e.Tile))
            {
                //_world.Tiles[p.X, p.Y].IsActive = false;
                _world.FillEllipseCentered(p.X, p.Y, _properties.RadiusX, _properties.RadiusY, tilePicker);
                renderer.UpdateWorldImage(new Int32Rect(p.X - _properties.RadiusX, p.Y - _properties.RadiusY, _properties.Size.Width, _properties.Size.Height));
            }
            start = e.Tile;
        }
    }
}