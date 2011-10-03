using System;
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.Tools
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Serializable]
    public class ToolProperties : ObservableObject
    {
        private int _Height;
        private WriteableBitmap _Image;
        private bool _IsOutline;
        private bool _IsSquare;
        private int _MaxHeight;
        private int _MaxOutlineThickness;
        private int _MaxWidth;
        private int _MinHeight;
        private int _MinWidth;
        private ToolAnchorMode _Mode;
        private PointInt32 _Offset;
        private int _OutlineThickness;
        private int _Width;
        private ToolBrushShape _brushShape;

        public ToolProperties()
        {
            _MaxOutlineThickness = 10;
            _OutlineThickness = 1;
            _Height = 10;
            _Width = 10;
            _MinHeight = 1;
            _MinWidth = 1;
            _MaxHeight = 100;
            _MaxWidth = 100;

            _IsSquare = true;
            _brushShape = ToolBrushShape.Round;
            _Mode = ToolAnchorMode.Center;
            CalcOffset();
            AnchorModes = Enum.GetValues(typeof (ToolAnchorMode));
            BrushShapes = Enum.GetValues(typeof (ToolBrushShape));
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

        public int Width
        {
            get { return _Width; }
            set
            {
                int validWidth = value;
                if (validWidth < MinWidth)
                    validWidth = MinWidth;

                if (validWidth > MaxWidth)
                    validWidth = MaxWidth;

                if (_Width != validWidth)
                {
                    _Width = validWidth;

                    if (IsSquare)
                        Height = validWidth;

                    RaisePropertyChanged("Width");
                    CalcOffset();
                }
            }
        }

        public int Height
        {
            get { return _Height; }
            set
            {
                int validHeight = value;
                if (validHeight < MinHeight)
                    validHeight = MinHeight;

                if (validHeight > MaxHeight)
                    validHeight = MaxHeight;

                if (_Height != validHeight)
                {
                    _Height = validHeight;

                    if (IsSquare)
                        Width = validHeight;


                    RaisePropertyChanged("Height");
                    CalcOffset();
                }
            }
        }

        public int MinWidth
        {
            get { return _MinWidth; }
            set
            {
                if (value > MaxWidth)
                    MaxWidth = value;

                if (_MinWidth != value)
                {
                    _MinWidth = value;
                    RaisePropertyChanged("MinWidth");
                    Width = _Width; // Validate Width 
                }
            }
        }

        public int MinHeight
        {
            get { return _MinHeight; }
            set
            {
                if (value > MaxHeight)
                    MaxHeight = value;

                if (_MinHeight != value)
                {
                    _MinHeight = value;
                    RaisePropertyChanged("MinHeight");
                    Height = _Height; // Validate Height
                }
            }
        }

        public int MaxWidth
        {
            get { return _MaxWidth; }
            set
            {
                if (value < MinWidth)
                    MinWidth = value;

                if (_MaxWidth != value)
                {
                    _MaxWidth = value;
                    RaisePropertyChanged("MaxWidth");
                    Width = _Width; // Validate Width 
                }
            }
        }

        public int MaxHeight
        {
            get { return _MaxHeight; }
            set
            {
                if (value < MinHeight)
                    MinHeight = value;

                if (_MaxHeight != value)
                {
                    _MaxHeight = value;
                    RaisePropertyChanged("MaxHeight");
                    Height = _Height; // Validate Height
                }
            }
        }

        public bool IsSquare
        {
            get { return _IsSquare; }
            set
            {
                if (_IsSquare != value)
                {
                    _IsSquare = value;
                    RaisePropertyChanged("IsSquare");
                }
            }
        }

        public bool IsOutline
        {
            get { return _IsOutline; }
            set
            {
                if (_IsOutline != value)
                {
                    _IsOutline = value;
                    RaisePropertyChanged("IsOutline");
                }
            }
        }

        public int OutlineThickness
        {
            get { return _OutlineThickness; }
            set
            {
                if (_OutlineThickness != value)
                {
                    _OutlineThickness = value;
                    RaisePropertyChanged("OutlineThickness");
                }
            }
        }


        public int MaxOutlineThickness
        {
            get { return _MaxOutlineThickness; }
            set
            {
                int validValue = value;
                if (validValue < 1)
                    validValue = 1;

                if (_MaxOutlineThickness != validValue)
                {
                    _MaxOutlineThickness = validValue;
                    RaisePropertyChanged("MaxOutlineThickness");

                    if (_OutlineThickness > _MaxOutlineThickness)
                    {
                        OutlineThickness = _MaxOutlineThickness;
                    }
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
                    Offset = new PointInt32(Width/2, Height/2);
                    break;
                case ToolAnchorMode.TopLeft:
                    Offset = new PointInt32(0, 0);
                    break;
                case ToolAnchorMode.TopCenter:
                    Offset = new PointInt32(Width/2, 0);
                    break;
                case ToolAnchorMode.TopRight:
                    Offset = new PointInt32(Width, 0);
                    break;
                case ToolAnchorMode.MiddleRight:
                    Offset = new PointInt32(Width, Height/2);
                    break;
                case ToolAnchorMode.BottomRight:
                    Offset = new PointInt32(Width, Height);
                    break;
                case ToolAnchorMode.BottomCenter:
                    Offset = new PointInt32(Width/2, Height);
                    break;
                case ToolAnchorMode.BottomLeft:
                    Offset = new PointInt32(0, Height);
                    break;
                case ToolAnchorMode.MiddleLeft:
                    Offset = new PointInt32(0, Height/2);
                    break;
                default:
                    Offset = new PointInt32(0, 0);
                    break;
            }
            MaxOutlineThickness = (int) Math.Max(1, Math.Min(Math.Floor(Height/2.0), Math.Floor(Width/2.0)));

            OnToolPreviewRequest(this, new EventArgs());
        }
    }
}