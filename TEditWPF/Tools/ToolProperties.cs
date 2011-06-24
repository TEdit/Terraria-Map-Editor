using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.Tools
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ToolProperties : ObservableObject
    {
        public ToolProperties()
        {
            Mode = ToolAnchorMode.Center;
        }

        private ToolAnchorMode _Mode;
        public ToolAnchorMode Mode
        {
            get { return this._Mode; }
            set
            {
                if (this._Mode != value)
                {
                    this._Mode = value;
                    this.RaisePropertyChanged("Mode");
                    this.CalcOffset();
                }
            }
        }

        private ToolShape _Shape;
        public ToolShape Shape
        {
            get { return this._Shape; }
            set
            {
                if (this._Shape != value)
                {
                    this._Shape = value;
                    this.RaisePropertyChanged("Shape");
                }
            }
        }



        private int _Radius;
        public int Radius
        {
            get { return this._Radius; }
            set
            {
                if (this._Radius != value)
                {
                    this._Radius = value;
                    this.RaisePropertyChanged("Radius");
                    this.Size = new SizeInt32(this._Radius * 2, this._Radius * 2);
                }
            }
        }

        private SizeInt32 _Size;
        public SizeInt32 Size
        {
            get { return this._Size; }
            set
            {
                if (this._Size != value)
                {
                    this._Size = value;
                    this.RaisePropertyChanged("Size");
                    this.CalcOffset();
                }
            }
        }

        private PointInt32 _Offset;
        public PointInt32 Offset
        {
            get { return this._Offset; }
            set
            {
                if (this._Offset != value)
                {
                    this._Offset = value;
                    this.RaisePropertyChanged("Offset");
                }
            }
        }

        private void CalcOffset()
        {
            switch (Mode)
            {
                case ToolAnchorMode.Center:
                    Offset = new PointInt32(_Size.Width / 2, _Size.Height / 2);
                    break;
                case ToolAnchorMode.TopLeft:
                    Offset = new PointInt32(0, 0);
                    break;
                case ToolAnchorMode.TopCenter:
                    Offset = new PointInt32(_Size.Width / 2, 0);
                    break;
                case ToolAnchorMode.TopRight:
                    Offset = new PointInt32(_Size.Width, 0);
                    break;
                case ToolAnchorMode.MiddleRight:
                    Offset = new PointInt32(_Size.Width, _Size.Height / 2);
                    break;
                case ToolAnchorMode.BottomRight:
                    Offset = new PointInt32(_Size.Width, _Size.Height);
                    break;
                case ToolAnchorMode.BottomCenter:
                    Offset = new PointInt32(_Size.Width / 2, _Size.Height);
                    break;
                case ToolAnchorMode.BottomLeft:
                    Offset = new PointInt32(0, _Size.Height);
                    break;
                case ToolAnchorMode.MiddleLeft:
                    Offset = new PointInt32(0, _Size.Height / 2);
                    break;
                default:
                    Offset = new PointInt32(0, 0);
                    break;
            }

        }


        private WriteableBitmap _Image;
        public WriteableBitmap Image
        {
            get { return this._Image; }
            set
            {
                if (this._Image != value)
                {
                    this._Image = value;
                    this.RaisePropertyChanged("Image");
                }
            }
        }
    }


}