using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;
using TEdit.Tools.History;

namespace TEdit.Tools.Tool
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 4)]
    public class Brush : ToolBase
    {
        private bool _isActive;
        private bool _isLeftDown;
        private bool _isRightDown;
        private SizeInt32 _lastUsedSize;
        [Import]
        private ToolProperties _properties;


        [Import]
        private WorldRenderer _renderer;
        [Import]
        private SelectionArea _selection;

        private PointInt32 _startPoint;
        [Import]
        private TilePicker _tilePicker;
        [Import("World", typeof(World))]
        private World _world;

        [Import]
        private HistoryManager HistMan;
        private Queue<HistoryTile> history = new Queue<HistoryTile>();
        private bool[] tilesChecked;

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
                            _lastUsedSize = new SizeInt32(_properties.Width, _properties.Height);
                    }
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            tilesChecked = new bool[_world.Header.MaxTiles.X * _world.Header.MaxTiles.Y];

            if (!_isRightDown && !_isLeftDown)
                _startPoint = e.Tile;

            if ((_properties.Height > 0 && _properties.Width > 0) &&
                (_lastUsedSize.Width != _properties.Width || _lastUsedSize.Height != _properties.Height))
                _lastUsedSize = new SizeInt32(_properties.Width, _properties.Height);

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

            HistMan.AddUndo(history);
            history = new Queue<HistoryTile>();

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
                    FillRectangle(new Int32Rect(x0, y0, _properties.Width, _properties.Height), ref _tilePicker, ref _selection);
                    if (_properties.IsOutline)
                    {
                        // eraise a center section
                        TilePicker eraser = Utility.DeepCopy(_tilePicker);
                        eraser.IsEraser = true;
                        eraser.Wall.IsActive = false; // don't erase the wall for the interrior
                        FillRectangle(new Int32Rect(x0 + _properties.OutlineThickness,
                                                           y0 + _properties.OutlineThickness,
                                                           _properties.Width - (_properties.OutlineThickness * 2),
                                                           _properties.Height - (_properties.OutlineThickness * 2)),
                                            ref eraser, ref _selection);
                        eraser = null;
                    }
                }
                else if (_properties.BrushShape == ToolBrushShape.Round)
                {
                    FillEllipse(x0, y0, x0 + _properties.Width, y0 + _properties.Height, ref _tilePicker, ref _selection);
                    if (_properties.IsOutline)
                    {
                        // eraise a center section
                        TilePicker eraser = Utility.DeepCopy(_tilePicker);
                        eraser.IsEraser = true;
                        eraser.Wall.IsActive = false; // don't erase the wall for the interrior
                        FillEllipse(x0 + _properties.OutlineThickness,
                                           y0 + _properties.OutlineThickness,
                                           x0 + _properties.Width - _properties.OutlineThickness,
                                           y0 + _properties.Height - _properties.OutlineThickness, ref eraser, ref _selection);

                        eraser = null;
                    }
                }
                //_renderer.UpdateWorldImage(new Int32Rect(x0, y0, _properties.Width + 1, _properties.Height + 1));
            }
        }



        #region Ellipse

        public void FillEllipse(int x1, int y1, int x2, int y2, ref TilePicker tile, ref SelectionArea selection)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;


            FillEllipseCentered(xc, yc, xr, yr, ref tile, ref selection);
        }

        public void FillEllipseCentered(int xc, int yc, int xr, int yr, ref TilePicker tile, ref SelectionArea selection)
        {
            int w = _world.Header.MaxTiles.X;
            int h = _world.Header.MaxTiles.Y;


            // Init vars
            int uy, ly, lx, rx;
            int x = xr;
            int y = 0;
            int xrSqTwo = (xr * xr) << 1;
            int yrSqTwo = (yr * yr) << 1;
            int xChg = yr * yr * (1 - (xr << 1));
            int yChg = xr * xr;
            int err = 0;
            int xStopping = yrSqTwo * xr;
            int yStopping = 0;

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                uy = yc + y; // Upper half
                ly = yc - y; // Lower half
                if (uy < 0) uy = 0; // Clip
                if (uy >= h) uy = h - 1; // ...
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;

                rx = xc + x;
                lx = xc - x;
                if (rx < 0) rx = 0; // Clip
                if (rx >= w) rx = w - 1; // ...
                if (lx < 0) lx = 0;
                if (lx >= w) lx = w - 1;

                // Draw line
                for (int i = lx; i <= rx; i++)
                {
                    if (!tilesChecked[i + uy * w])
                    {
                        // Save History
                        var loc = new PointInt32(i, uy);
                        history.Enqueue(new HistoryTile(loc, (Tile)_world.Tiles[i, uy].Clone()));

                        _world.SetTileXY(ref i, ref uy, ref tile, ref selection); // Quadrant II to I (Actually two octants)
                        _renderer.UpdateWorldImage(loc);
                        tilesChecked[i + uy * w] = true;
                    }
                    if (!tilesChecked[i + ly * w])
                    {
                        var loc = new PointInt32(i, ly);
                        history.Enqueue(new HistoryTile(loc, (Tile)_world.Tiles[i, ly].Clone()));

                        _world.SetTileXY(ref i, ref ly, ref tile, ref selection);     // Quadrant III to IV   
                        _renderer.UpdateWorldImage(new PointInt32(i, ly));
                        tilesChecked[i + ly * w] = true;
                    }
                }

                y++;
                yStopping += xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
                if ((xChg + (err << 1)) > 0)
                {
                    x--;
                    xStopping -= yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                }
            }

            // ReInit vars
            x = 0;
            y = yr;
            uy = yc + y; // Upper half
            ly = yc - y; // Lower half
            if (uy < 0) uy = 0; // Clip
            if (uy >= h) uy = h - 1; // ...
            if (ly < 0) ly = 0;
            if (ly >= h) ly = h - 1;
            xChg = yr * yr;
            yChg = xr * xr * (1 - (yr << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo * yr;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                rx = xc + x;
                lx = xc - x;
                if (rx < 0) rx = 0; // Clip
                if (rx >= w) rx = w - 1; // ...
                if (lx < 0) lx = 0;
                if (lx >= w) lx = w - 1;

                // Draw line
                for (int i = lx; i <= rx; i++)
                {
                    if (!tilesChecked[i + uy * w])
                    {
                        // Save History
                        var loc = new PointInt32(i, uy);
                        history.Enqueue(new HistoryTile(loc, (Tile)_world.Tiles[i, uy].Clone()));

                        _world.SetTileXY(ref i, ref uy, ref tile, ref selection); // Quadrant II to I (Actually two octants)
                        _renderer.UpdateWorldImage(new PointInt32(i, uy));
                        tilesChecked[i + uy * w] = true;
                    }
                    if (!tilesChecked[i + ly * w])
                    {
                        var loc = new PointInt32(i, ly);
                        history.Enqueue(new HistoryTile(loc, (Tile)_world.Tiles[i, ly].Clone()));

                        _world.SetTileXY(ref i, ref ly, ref tile, ref selection);     // Quadrant III to IV  
                        _renderer.UpdateWorldImage(new PointInt32(i, ly));
                        tilesChecked[i + ly * w] = true;
                    }
                }

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    uy = yc + y; // Upper half
                    ly = yc - y; // Lower half
                    if (uy < 0) uy = 0; // Clip
                    if (uy >= h) uy = h - 1; // ...
                    if (ly < 0) ly = 0;
                    if (ly >= h) ly = h - 1;
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }

        #endregion

        public void FillRectangle(Int32Rect area, ref TilePicker tile, ref SelectionArea selection)
        {
            // validate area
            int w = _world.Header.MaxTiles.X;
            if (area.X < 0)
            {
                area.Width += area.X;
                area.X = 0;
            }
            if (area.Y < 0)
            {
                area.Height += area.Y;
                area.Y = 0;
            }
            if ((area.Y + area.Height) >= _world.Header.MaxTiles.Y)
            {
                area.Height += _world.Header.MaxTiles.Y - (area.Y + area.Height);
            }
            if ((area.X + area.Width) >= w)
            {
                area.Width += w - (area.X + area.Width);
            }

            for (int x = area.X; x < area.X + area.Width; x++)
            {
                for (int y = area.Y; y < area.Y + area.Height; y++)
                {
                    if (!tilesChecked[x + y * w])
                    {
                        // Save History
                        var loc = new PointInt32(x, y);
                        history.Enqueue(new HistoryTile(loc, (Tile)_world.Tiles[x, y].Clone()));

                        _world.SetTileXY(ref x, ref y, ref tile, ref selection);
                        _renderer.UpdateWorldImage(new PointInt32(x, y));
                        tilesChecked[x + y * w] = true;
                    }
                }
            }
        }
    }
}