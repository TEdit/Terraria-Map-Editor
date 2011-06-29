using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.Tools.Tool
{
    [Export(typeof (ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 4)]
    public class Brush : ToolBase
    {
        private bool _isActive;
        private bool _isLeftDown;
        private bool _isRightDown;
        private SizeInt32 _lastUsedSize;
        [Import] private ToolProperties _properties;


        [Import] private WorldRenderer _renderer;
        [Import] private SelectionArea _selection;

        private PointInt32 _startPoint;
        [Import] private TilePicker _tilePicker;
        [Import("World", typeof (World))] private World _world;

        public Brush()
        {
            _lastUsedSize = new SizeInt32(0, 0);
            _image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paintbrush.png"));
            _name = "Brush";
            _type = ToolType.Brush;
            _isActive = false;
        }

        #region Properties

        private readonly BitmapImage _image;
        private readonly string _name;

        private readonly ToolType _type;


        public override string Name
        {
            get { return _name; }
        }

        public override ToolType Type
        {
            get { return _type; }
        }

        public override BitmapImage Image
        {
            get { return _image; }
        }

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    RaisePropertyChanged("IsActive");

                    if (_isActive)
                    {
                        _properties.MinHeight = 2;
                        _properties.MinWidth = 2;
                        _properties.MaxHeight = 200;
                        _properties.MaxWidth = 200;

                        if (_lastUsedSize.Width > 0 && _lastUsedSize.Height > 0)
                        {
                            _properties.Height = _lastUsedSize.Height;
                            _properties.Width = _lastUsedSize.Width;
                        }
                    }
                    else
                    {
                        if ((_properties.Height > 0 && _properties.Width > 0) &&
                            (_lastUsedSize.Width != _properties.Width || _lastUsedSize.Height != _properties.Height))
                            _lastUsedSize = new SizeInt32(_properties.Height, _properties.Width);
                    }
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            if (!_isRightDown && !_isLeftDown)
                _startPoint = e.Tile;

            if ((_properties.Height > 0 && _properties.Width > 0) &&
                (_lastUsedSize.Width != _properties.Width || _lastUsedSize.Height != _properties.Height))
                _lastUsedSize = new SizeInt32(_properties.Height, _properties.Width);

            CheckDirectionandDraw(e);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            return true;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            CheckDirectionandDraw(e);
            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            CheckDirectionandDraw(e);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            return true;
        }

        private void CheckDirectionandDraw(TileMouseEventArgs e)
        {
            PointInt32 p = e.Tile;
            if (_isRightDown)
            {
                if (_isLeftDown)
                    p.X = _startPoint.X;
                else
                    p.Y = _startPoint.Y;

                DrawLine(p);
                _startPoint = p;
            }
            else if (_isLeftDown)
            {
                DrawLine(p);
                _startPoint = p;
            }
        }


        public override WriteableBitmap PreviewTool()
        {
            var bmp = new WriteableBitmap(
                _properties.Width + 1,
                _properties.Height + 1,
                96,
                96,
                PixelFormats.Bgra32,
                null);


            bmp.Clear();
            if (_properties.BrushShape == ToolBrushShape.Square)
                bmp.FillRectangle(0, 0, _properties.Width, _properties.Height, Color.FromArgb(127, 0, 90, 255));
            else
                bmp.FillEllipse(0, 0, _properties.Width, _properties.Height, Color.FromArgb(127, 0, 90, 255));
            return bmp;
        }

        private void DrawLine(PointInt32 endPoint)
        {
            foreach (PointInt32 p in WorldRenderer.DrawLine(_startPoint, endPoint))
            {
                if (_selection.SelectionVisibility == Visibility.Visible)
                {
                    // if selection is active, and point is not inside, skip point
                    if (!_selection.Rectangle.Contains(p))
                        continue;
                }

                // center
                int x0 = p.X - _properties.Offset.X;
                int y0 = p.Y - _properties.Offset.Y;

                if (_properties.BrushShape == ToolBrushShape.Square)
                {
                    _world.FillRectangle(new Int32Rect(x0, y0, _properties.Width, _properties.Height), _tilePicker);
                    if (_properties.IsOutline)
                    {
                        // eraise a center section
                        TilePicker eraser = Utility.DeepCopy(_tilePicker);
                        eraser.IsEraser = true;
                        eraser.Wall.IsActive = false; // don't erase the wall for the interrior
                        _world.FillRectangle(new Int32Rect(x0 + _properties.OutlineThickness,
                                                           y0 + _properties.OutlineThickness,
                                                           _properties.Width - (_properties.OutlineThickness*2),
                                                           _properties.Height - (_properties.OutlineThickness*2)),
                                             eraser);
                        eraser = null;
                    }
                }
                else if (_properties.BrushShape == ToolBrushShape.Round)
                {
                    _world.FillEllipse(x0, y0, x0 + _properties.Width, y0 + _properties.Height, _tilePicker);
                    if (_properties.IsOutline)
                    {
                        // eraise a center section
                        TilePicker eraser = Utility.DeepCopy(_tilePicker);
                        eraser.IsEraser = true;
                        eraser.Wall.IsActive = false; // don't erase the wall for the interrior
                        _world.FillEllipse(x0 + _properties.OutlineThickness,
                                           y0 + _properties.OutlineThickness,
                                           x0 + _properties.Width - _properties.OutlineThickness,
                                           y0 + _properties.Height - _properties.OutlineThickness, eraser);

                        eraser = null;
                    }
                }
                _renderer.UpdateWorldImage(new Int32Rect(x0, y0, _properties.Width + 1, _properties.Height + 1));
            }
        }
    }
}