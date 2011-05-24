using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TerrariaMapEditor.Controls
{
    public partial class WorldViewport : System.Windows.Forms.UserControl, INotifyPropertyChanged
    {
        public WorldViewport()
        {
            this.SetStyle(
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true);

            this.HorizontalScroll.Visible = true;
            this.VerticalScroll.Visible = true;

        }

        private Bitmap _backBuffer;


        private float _MaxZoom = 10000;
        /// <summary>
        /// Maximum Zoom Level
        /// </summary>
        [Category("Image"),
        Browsable(true),
        Description("Maximum Zoom Level"),
        DefaultValue(10000)]
        public float MaxZoom
        {
            get { return this._MaxZoom; }
            set
            {
                if (this._MaxZoom != value)
                {
                    this._MaxZoom = value;
                    this.RaisePropertyChanged("MaxZoom");

                    if (this.Zoom > this._MaxZoom)
                        this.Zoom = this._MaxZoom;
                }
            }
        }

        private float _MinZooom = 0.1F;
        /// <summary>
        /// Minimum zoom level
        /// </summary>
        [Category("Image"),
        Browsable(true),
        Description("Minimum zoom level"),
        DefaultValue(0.1F)]
        public float MinZooom
        {
            get { return _MinZooom; }
            set
            {
                if (this._MinZooom != value)
                {
                    this._MinZooom = value;
                    this.RaisePropertyChanged("MinZooom");

                    if (this.Zoom < this._MinZooom)
                        this.Zoom = this.MinZooom;

                }
            }
        }

        private float _Zoom;
        /// <summary>
        /// Zoom Level
        /// </summary>
        [Category("Image"),
        Browsable(true),
        Description("Zoom Level")]
        public float Zoom
        {
            get { return _Zoom; }
            set
            {
                float correctedZoom = value;

                if (this._IsAutoZoom)
                {
                    correctedZoom = CalcAutoZoom();
                }
                else
                {
                    if (correctedZoom < this._MinZooom)
                        correctedZoom = this._MinZooom;
                    if (correctedZoom > this._MaxZoom)
                        correctedZoom = this._MaxZoom;
                }

                if (this._Zoom != correctedZoom)
                {
                    this._Zoom = correctedZoom;
                    this.RaisePropertyChanged("Zoom");
                    this.SetScrollSize();
                    this.Invalidate();
                }

            }

        }

        private bool _IsAutoZoom;
        /// <summary>
        /// Autozoom to fit scren
        /// </summary>
        [Category("Image"),
        Browsable(true),
        Description("Autozoom to fit scren")]
        public bool IsAutoZoom
        {
            get { return this._IsAutoZoom; }
            set
            {
                if (this._IsAutoZoom != value)
                {
                    this._IsAutoZoom = value;
                    this.RaisePropertyChanged("IsAutoZoom");

                    if (this._IsAutoZoom)
                    {
                        this.Zoom = CalcAutoZoom();
                    }
                }
            }
        }

        private float CalcAutoZoom()
        {
            if (this._Image != null)
            {
                float yfit = (float)(this.Height) / (float)this._Image.Height; //SystemInformation.HorizontalScrollBarHeight
                return yfit;
            }
            return 1.0F;
        }

        private void SetScrollSize()
        {
            if (this._Image != null)
            {
                this.AutoScrollMinSize = new Size(
                    (int)Math.Floor((float)_Image.Size.Width * this._Zoom) - SystemInformation.VerticalScrollBarWidth,
                    (int)Math.Floor((float)_Image.Size.Height * this._Zoom) - SystemInformation.HorizontalScrollBarHeight);
            }
            else
                this.AutoScrollMinSize = this.Size;
        }

        private System.Drawing.Image _Image;
        /// <summary>
        /// World Image
        /// </summary>
        [Category("Image"),
        Browsable(true),
        Description("World Image")]
        public System.Drawing.Image Image
        {
            get { return this._Image; }
            set
            {
                if (this._Image != value)
                {
                    this._Image = value;
                    this.MinZooom = (float)this.Width / (float)this._Image.Width;
                    this.Zoom = CalcAutoZoom();
                    this.RaisePropertyChanged("Image");
                }
            }
        }

        private Point _TileLastClicked;
        /// <summary>
        /// The coordinates of the last tile clicked.
        /// </summary>
        [Category("Tile"),
        Browsable(true),
        Description("The coordinates of the last tile clicked.")]
        public Point TileLastClicked
        {
            get { return this._TileLastClicked; }
            set
            {
                if (this._TileLastClicked != value)
                {
                    this._TileLastClicked = value;
                }
            }
        }

        private Point _TileLastReleased;
        /// <summary>
        /// The coordinates of the last tile Released.
        /// </summary>
        [Category("Tile"),
        Browsable(true),
        Description("The coordinates of the last tile released.")]
        public Point TileLastReleased
        {
            get { return this._TileLastReleased; }
            set
            {
                if (this._TileLastReleased != value)
                {
                    this._TileLastReleased = value;
                }
            }
        }

        private Point _TileMouseOver;
        /// <summary>
        /// The coordinates of the last tile under the mouse.
        /// </summary>
        [Category("Tile"),
        Browsable(true),
        Description("The coordinates of the last under the mouse.")]
        public Point TileMouseOver
        {
            get { return this._TileMouseOver; }
            set
            {
                if (this._TileMouseOver != value)
                {
                    this._TileMouseOver = value;
                }
            }
        }

        private bool _IsMouseDown;
        /// <summary>
        /// Is a mouse button pressed
        /// </summary>
        [Category("Tile"),
        Browsable(true),
        Description("Is a mouse button pressed")]
        public bool IsMouseDown
        {
            get { return this._IsMouseDown; }
            set
            {
                if (this._IsMouseDown != value)
                {
                    this._IsMouseDown = value;
                    this.RaisePropertyChanged("IsMouseDown");
                }
            }
        }

        public override bool AutoScroll
        {
            get { return true; }
            set { base.AutoScroll = true; }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.IsMouseDown = true;

            Point tileAtMouse = GetTileAtPoint(e.Location);
            if (this.TileLastClicked != tileAtMouse)
            {
                this.TileLastClicked = tileAtMouse;
            }

            this.OnMouseDownTile(this, new MouseEventArgs(e.Button, e.Clicks, tileAtMouse.X, tileAtMouse.Y, e.Delta));
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.IsMouseDown = false;

            Point tileAtMouse = GetTileAtPoint(e.Location);
            this.TileLastReleased = tileAtMouse;

            this.OnMouseUpTile(this, new MouseEventArgs(e.Button, e.Clicks, tileAtMouse.X, tileAtMouse.Y, e.Delta));
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point tileAtMouse = GetTileAtPoint(e.Location);

            this.TileMouseOver = tileAtMouse;
            this.OnMouseMoveTile(this, new MouseEventArgs(e.Button, e.Clicks, tileAtMouse.X, tileAtMouse.Y, e.Delta));
            //this.Invalidate();

            base.OnMouseMove(e);
        }

        private Point GetImagePoint(Point tilePoint)
        {
            return new Point(
                (int)(tilePoint.X * this.Zoom) + this.AutoScrollPosition.X,
                (int)(tilePoint.Y * this.Zoom) + this.AutoScrollPosition.Y);
        }

        private Point GetTileAtPoint(Point imagePoint)
        {
            int xoffsetForScrollbar = 0;
            int yoffsetForScrollbar = 0;
            if (this.VScroll)
                xoffsetForScrollbar = SystemInformation.VerticalScrollBarWidth;
            if (this.HScroll)
                yoffsetForScrollbar = SystemInformation.HorizontalScrollBarHeight;

            Point anchor = GetDisplayedTileAnchor;

            float xScaledForScrollbar =(float)imagePoint.X *(float)(this.ClientRectangle.Width)   / (float)(this.ClientRectangle.Width - xoffsetForScrollbar);
            float yScaledForScrollbar = (float)imagePoint.Y* (float)(this.ClientRectangle.Height) / (float)(this.ClientRectangle.Height - yoffsetForScrollbar);

            int displayTileX = (int)(xScaledForScrollbar / this.Zoom);
            int displayTileY = (int)(yScaledForScrollbar / this.Zoom);

            return new Point(GetDisplayedTileAnchor.X + displayTileX, GetDisplayedTileAnchor.Y + displayTileY);
        }


        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Don't allow the background to paint
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            this.Invalidate();
            base.OnScroll(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this._Image != null)
                this.MinZooom = (float)this.Width / (float)this._Image.Width;
            if (this._IsAutoZoom)
                this.Zoom = CalcAutoZoom();

            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this._Image != null && this.Height > 0 && this.Width > 0)
            {
                Size backBufferSize = new Size((int)Math.Ceiling(this.Size.Width / this.Zoom),
                                               (int)Math.Ceiling(this.Size.Height / this.Zoom));

                if (backBufferSize.Height <= 0 || backBufferSize.Height <= 0)
                    return;
                
                if (this._backBuffer == null)
                {
                    this._backBuffer = new Bitmap(backBufferSize.Width, backBufferSize.Height);
                }
                else if (this._backBuffer.Size != backBufferSize)
                {
                    this._backBuffer.Dispose();
                    this._backBuffer = null;
                    this._backBuffer = new Bitmap(backBufferSize.Width, backBufferSize.Height);
                }

                Graphics backBufferG = Graphics.FromImage(_backBuffer);

                Point displayTileAnchor = GetDisplayedTileAnchor;
                backBufferG.PixelOffsetMode = PixelOffsetMode.Half;
                backBufferG.InterpolationMode = InterpolationMode.NearestNeighbor;

                RectangleF backBufferRectangle = new RectangleF(0, 0, backBufferSize.Width, backBufferSize.Height);

                backBufferG.DrawImage(
                    this._Image,
                    backBufferRectangle,
                    new RectangleF(displayTileAnchor, backBufferSize),
                    GraphicsUnit.Pixel);

                OnDrawToolOverlay(this, new PaintEventArgs(backBufferG, new Rectangle(displayTileAnchor, backBufferSize)));
                backBufferG.Dispose();
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(this._backBuffer,
                    new RectangleF(this.ClientRectangle.Location, this.ClientRectangle.Size),
                    backBufferRectangle,
                    GraphicsUnit.Pixel);
            }
        }

        private Point GetDisplayedTileAnchor
        {
            get
            {
                return new Point((int)Math.Floor(-this.AutoScrollPosition.X / this._Zoom),
                                 (int)Math.Floor(-this.AutoScrollPosition.Y / this._Zoom));
            }
        }
    }
}
