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
        private SizeInt32 _Size;

        public ToolProperties()
        {
            _Size = new SizeInt32(10,10);
            BrushShape = ToolBrushShape.Round;
            Mode = ToolAnchorMode.Center;
            CalcOffset();
        }

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
                }
            }
        }

        public SizeInt32 Size
        {
            get { return _Size; }
            set
            {
                if (_Size != value)
                {
                    _Size = value;
                    RaisePropertyChanged("Size");
                    CalcOffset();
                }
            }
        }

        public int RadiusX
        {
            get { return (int) Math.Floor(_Size.Width/2.0D); }
        }
        public int RadiusY
        {
            get { return (int)Math.Floor(_Size.Height / 2.0D); }
        }

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

        private void CalcOffset()
        {
            switch (Mode)
            {
                case ToolAnchorMode.Center:
                    Offset = new PointInt32(_Size.Width/2, _Size.Height/2);
                    break;
                case ToolAnchorMode.TopLeft:
                    Offset = new PointInt32(0, 0);
                    break;
                case ToolAnchorMode.TopCenter:
                    Offset = new PointInt32(_Size.Width/2, 0);
                    break;
                case ToolAnchorMode.TopRight:
                    Offset = new PointInt32(_Size.Width, 0);
                    break;
                case ToolAnchorMode.MiddleRight:
                    Offset = new PointInt32(_Size.Width, _Size.Height/2);
                    break;
                case ToolAnchorMode.BottomRight:
                    Offset = new PointInt32(_Size.Width, _Size.Height);
                    break;
                case ToolAnchorMode.BottomCenter:
                    Offset = new PointInt32(_Size.Width/2, _Size.Height);
                    break;
                case ToolAnchorMode.BottomLeft:
                    Offset = new PointInt32(0, _Size.Height);
                    break;
                case ToolAnchorMode.MiddleLeft:
                    Offset = new PointInt32(0, _Size.Height/2);
                    break;
                default:
                    Offset = new PointInt32(0, 0);
                    break;
            }
        }
    }
}