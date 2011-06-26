using System;
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ToolProperties : ObservableObject
    {
        private WriteableBitmap _Image;
        private ToolAnchorMode _Mode;
        private PointInt32 _Offset;
        private ToolBrushShape _brushShape;

        public ToolProperties()
        {
            this._Height = 10;
            this._Width = 10;
            this._MinHeight = 1;
            this._MinWidth = 1;
            this._MaxHeight = 100;
            this._MaxWidth = 100;
            this._IsSquare = true;
            this._brushShape = ToolBrushShape.Round;
            this._Mode = ToolAnchorMode.Center;
            CalcOffset();
            AnchorModes = Enum.GetValues(typeof(ToolAnchorMode));
            BrushShapes = Enum.GetValues(typeof(ToolBrushShape));
        }

        public Array AnchorModes { get; set; }
        public Array BrushShapes { get; set; }

        public ToolAnchorMode Mode
        {
            get { return _Mode; }
            set
            {
                if (_Mode != value)
                {
                    _Mode = value;
                    RaisePropertyChanged("Mode");
                    CalcOffset();
                }
            }
        }

        public ToolBrushShape BrushShape
        {
            get { return _brushShape; }
            set
            {
                if (_brushShape != value)
                {
                    _brushShape = value;
                    RaisePropertyChanged("BrushShape");
                    CalcOffset();
                }
            }
        }

        private int _Width;
        public int Width
        {
            get { return this._Width; }
            set
            {
                int validWidth = value;
                if (validWidth < MinWidth)
                    validWidth = MinWidth;

                if (validWidth > MaxWidth)
                    validWidth = MaxWidth;

                if (this._Width != validWidth)
                {
                    this._Width = validWidth;

                    if (this.IsSquare)
                        this.Height = validWidth;

                    this.RaisePropertyChanged("Width");
                    CalcOffset();
                }
            }
        }

        private int _Height;
        public int Height
        {
            get { return this._Height; }
            set
            {
                int validHeight = value;
                if (validHeight < MinHeight)
                    validHeight = MinHeight;

                if (validHeight > MaxHeight)
                    validHeight = MaxHeight;

                if (this._Height != validHeight)
                {
                    this._Height = validHeight;

                    if (this.IsSquare)
                        this.Width = validHeight;
                    

                    this.RaisePropertyChanged("Height");
                    CalcOffset();
                }
            }
        }

        private int _MinWidth;
        public int MinWidth
        {
            get { return this._MinWidth; }
            set
            {
                if (value > MaxWidth)
                    MaxWidth = value;

                if (this._MinWidth != value)
                {
                    this._MinWidth = value;
                    this.RaisePropertyChanged("MinWidth");
                    Width = _Width; // Validate Width 
                }
            }
        }

        private int _MinHeight;
        public int MinHeight
        {
            get { return this._MinHeight; }
            set
            {
                if (value > MaxHeight)
                    MaxHeight = value;

                if (this._MinHeight != value)
                {
                    this._MinHeight = value;
                    this.RaisePropertyChanged("MinHeight");
                    Height = _Height; // Validate Height
                }
            }
        }

        private int _MaxWidth;
        public int MaxWidth
        {
            get { return this._MaxWidth; }
            set
            {
                if (value < MinWidth)
                    MinWidth = value;

                if (this._MaxWidth != value)
                {
                    this._MaxWidth = value;
                    this.RaisePropertyChanged("MaxWidth");
                    Width = _Width; // Validate Width 
                }
            }
        }

        private int _MaxHeight;
        public int MaxHeight
        {
            get { return this._MaxHeight; }
            set
            {
                if (value < MinHeight)
                    MinHeight = value;

                if (this._MaxHeight != value)
                {
                    this._MaxHeight = value;
                    this.RaisePropertyChanged("MaxHeight");
                    Height = _Height; // Validate Height
                }
            }
        }

        private bool _IsSquare;
        public bool IsSquare
        {
            get { return this._IsSquare; }
            set
            {
                if (this._IsSquare != value)
                {
                    this._IsSquare = value;
                    this.RaisePropertyChanged("IsSquare");
                }
            }
        }

        //public int RadiusX
        //{
        //    get { return (int) Math.Floor(this.Width/2.0D); }
        //}
        //public int RadiusY
        //{
        //    get { return (int)Math.Floor(this.Height / 2.0D); }
        //}

        public PointInt32 Offset
        {
            get { return _Offset; }
            set
            {
                if (_Offset != value)
                {
                    _Offset = value;
                    RaisePropertyChanged("Offset");
                }
            }
        }

        public WriteableBitmap Image
        {
            get { return _Image; }
            set
            {
                if (_Image != value)
                {
                    _Image = value;
                    RaisePropertyChanged("Image");
                }
            }
        }


        public event EventHandler ToolPreviewRequest;

        protected virtual void OnToolPreviewRequest(object sender, EventArgs e)
        {
            if (ToolPreviewRequest != null)
                ToolPreviewRequest(sender, e);

        }

			

        private void CalcOffset()
        {
            switch (Mode)
            {
                case ToolAnchorMode.Center:
                    Offset = new PointInt32(this.Width / 2, this.Height / 2);
                    break;
                case ToolAnchorMode.TopLeft:
                    Offset = new PointInt32(0, 0);
                    break;
                case ToolAnchorMode.TopCenter:
                    Offset = new PointInt32(this.Width / 2, 0);
                    break;
                case ToolAnchorMode.TopRight:
                    Offset = new PointInt32(this.Width, 0);
                    break;
                case ToolAnchorMode.MiddleRight:
                    Offset = new PointInt32(this.Width, this.Height / 2);
                    break;
                case ToolAnchorMode.BottomRight:
                    Offset = new PointInt32(this.Width, this.Height);
                    break;
                case ToolAnchorMode.BottomCenter:
                    Offset = new PointInt32(this.Width / 2, this.Height);
                    break;
                case ToolAnchorMode.BottomLeft:
                    Offset = new PointInt32(0, this.Height);
                    break;
                case ToolAnchorMode.MiddleLeft:
                    Offset = new PointInt32(0, this.Height / 2);
                    break;
                default:
                    Offset = new PointInt32(0, 0);
                    break;
            }

            OnToolPreviewRequest(this, new EventArgs());
        }
    }
}