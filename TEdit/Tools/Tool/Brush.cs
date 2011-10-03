using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
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
            HistMan.AddBufferToHistory();
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

                    if (_properties.IsOutline)
                    {
                        TilePicker outline = Utility.DeepCopy(_tilePicker);
                        outline.Wall.IsActive = false;

                        // eraise a center section
                        TilePicker eraser = Utility.DeepCopy(_tilePicker);
                        eraser.IsEraser = true;
                        eraser.Wall.IsActive = false; // don't erase the wall for the interrior

                        TilePicker wall = Utility.DeepCopy(_tilePicker);
                        wall.Tile.IsActive = false;



                        var interrior = new Int32Rect(x0 + _properties.OutlineThickness,
                                                      y0 + _properties.OutlineThickness,
                                                      _properties.Width - (_properties.OutlineThickness * 2),
                                                      _properties.Height - (_properties.OutlineThickness * 2));

                        // Erase center
                        FillRectangle(interrior, ref eraser);
                        // Fill center
                        if (wall.Wall.IsActive)
                            FillRectangle(interrior, ref wall);
                        // Draw outline
                        if (outline.Tile.IsActive)
                        {
                            for (int i = 0; i < _properties.OutlineThickness; i++)
                            {
                                DrawRectangle(new Int32Rect(x0 + i, y0 + i, _properties.Width - (i * 2) - 1, _properties.Height - (i * 2) - 1), ref outline);
                            }
                        }
                    }
                    else
                    {
                        FillRectangle(new Int32Rect(x0, y0, _properties.Width, _properties.Height), ref _tilePicker);
                    }
                }
                else if (_properties.BrushShape == ToolBrushShape.Round)
                {

                    if (_properties.IsOutline)
                    {
                        TilePicker outline = Utility.DeepCopy(_tilePicker);
                        outline.Wall.IsActive = false;

                        // eraise a center section
                        TilePicker eraser = Utility.DeepCopy(_tilePicker);
                        eraser.IsEraser = true;
                        eraser.Wall.IsActive = false; // don't erase the wall for the interrior

                        TilePicker wall = Utility.DeepCopy(_tilePicker);
                        wall.Tile.IsActive = false;


                        // Draw outline
                        if (outline.Tile.IsActive)
                        {
                            FillEllipse(x0, y0, x0 + _properties.Width, y0 + _properties.Height, ref outline);
                        }

                        // Erase center
                        FillEllipse(x0 + _properties.OutlineThickness,
                                           y0 + _properties.OutlineThickness,
                                           x0 + _properties.Width - _properties.OutlineThickness,
                                           y0 + _properties.Height - _properties.OutlineThickness, ref eraser);
                        // Fill center
                        if (wall.Wall.IsActive)
                        {
                            FillEllipse(x0 + _properties.OutlineThickness,
                                           y0 + _properties.OutlineThickness,
                                           x0 + _properties.Width - _properties.OutlineThickness,
                                           y0 + _properties.Height - _properties.OutlineThickness, ref wall);
                        }



                        eraser = null;
                    }
                    else
                    {
                        FillEllipse(x0, y0, x0 + _properties.Width, y0 + _properties.Height, ref _tilePicker);
                    }
                }
                if (_properties.IsOutline)
                    _renderer.UpdateWorldImage(new Int32Rect(x0, y0, _properties.Width + 1, _properties.Height + 1));
            }
        }

        private void UpdateTile(ref TilePicker tile, ref int x, ref int y, ref int w)
        {
            if (!tilesChecked[x + y * w] || _properties.IsOutline)
            {
                // Save History

                if (HistMan.SaveHistory)
                    HistMan.AddTileToBuffer(x, y, ref _world.Tiles[x, y]);

                _world.SetTileXY(ref x, ref y, ref tile, ref _selection);
                tilesChecked[x + y * w] = true;
                if (!_properties.IsOutline)
                    _renderer.UpdateWorldImage(new PointInt32(x, y));
            }

        }



        #region Ellipse

        public void FillEllipse(int x1, int y1, int x2, int y2, ref TilePicker tile)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;


            FillEllipseCentered(xc, yc, xr, yr, ref tile);
        }

        public void FillEllipseCentered(int xc, int yc, int xr, int yr, ref TilePicker tile)
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
                    UpdateTile(ref tile, ref i, ref uy, ref w);
                    UpdateTile(ref tile, ref i, ref ly, ref w);
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
                    UpdateTile(ref tile, ref i, ref uy, ref w);
                    UpdateTile(ref tile, ref i, ref ly, ref w);
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

        public void FillRectangle(Int32Rect area, ref TilePicker tile)
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
                    UpdateTile(ref tile, ref x, ref y, ref w);
                }
            }
        }

        #region Shapes

        public void DrawRectangle(Int32Rect area, ref TilePicker tile)
        {
            DrawRectangle(area.X, area.Y, area.X + area.Width, area.Y + area.Height, ref tile);
        }

        public void DrawRectangle(int x1, int y1, int x2, int y2, ref TilePicker tile)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = _world.Header.MaxTiles.X;
            int h = _world.Header.MaxTiles.Y;

            // Check boundaries
            if (x1 < 0)
            {
                x1 = 0;
            }
            if (y1 < 0)
            {
                y1 = 0;
            }
            if (x2 < 0)
            {
                x2 = 0;
            }
            if (y2 < 0)
            {
                y2 = 0;
            }
            if (x1 >= w)
            {
                x1 = w - 1;
            }
            if (y1 >= h)
            {
                y1 = h - 1;
            }
            if (x2 >= w)
            {
                x2 = w - 1;
            }
            if (y2 >= h)
            {
                y2 = h - 1;
            }

            // top and bottom horizontal scanlines
            for (int x = x1; x <= x2; x++)
            {
                UpdateTile(ref tile, ref x, ref y1, ref w);
                UpdateTile(ref tile, ref x, ref y2, ref w);
            }

            for (int y = y1; y <= y2; y++)
            {
                UpdateTile(ref tile, ref x1, ref y, ref w);
                UpdateTile(ref tile, ref x2, ref y, ref w);
            }
        }

        #endregion

        #region Ellipse


        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        public void DrawEllipse(int x1, int y1, int x2, int y2, ref TilePicker tile)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;
            DrawEllipseCentered(xc, yc, xr, yr, ref tile);
        }


        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        public void DrawEllipseCentered(int xc, int yc, int xr, int yr, ref TilePicker tile)
        {
            // Use refs for faster access (really important!) speeds up a lot!
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

                UpdateTile(ref tile, ref rx, ref uy, ref w);
                UpdateTile(ref tile, ref rx, ref ly, ref w);
                UpdateTile(ref tile, ref lx, ref uy, ref w);
                UpdateTile(ref tile, ref lx, ref ly, ref w);


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
                UpdateTile(ref tile, ref rx, ref uy, ref w);
                UpdateTile(ref tile, ref rx, ref ly, ref w);
                UpdateTile(ref tile, ref lx, ref uy, ref w);
                UpdateTile(ref tile, ref lx, ref ly, ref w);

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
    }
}