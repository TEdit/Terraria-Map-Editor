using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.ViewModels
{
    [Export]
    public class WorldViewModel : ObservableObject
    {
        public WorldViewModel()
        {
            //this._bmp = new WriteableBitmap
            this._world = new World();
            this.World.Header.MaxTiles = new PointInt32(1200, 4200);
        }

        private World _world;
        public World World
        {
            get { return this._world; }
            set
            {
                if (this._world != value)
                {
                    this._world = value;
                    this.RaisePropertyChanged("World");
                    this.RaisePropertyChanged("WorldZoomedHeight");
                    this.RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }


        public double WorldZoomedHeight
        {
            get
            {
                if (this._WorldImage != null)
                    return this._WorldImage.PixelHeight * this._Zoom;


                return this.World.Header.MaxTiles.Y;
            }
        }

        public double WorldZoomedWidth
        {
            get
            {
                if (this._WorldImage != null)
                    return this._WorldImage.PixelWidth * this._Zoom;

                return this.World.Header.MaxTiles.X;
            }
        }

        private double _Zoom = 1;
        public double Zoom
        {
            get { return this._Zoom; }
            set
            {
                var limitedZoom = value;
                limitedZoom = Math.Min(Math.Max(limitedZoom, 0.05), 1000);

                if (this._Zoom != limitedZoom)
                {
                    this._Zoom = limitedZoom;
                    this.RaisePropertyChanged("Zoom");
                    this.RaisePropertyChanged("WorldZoomedHeight");
                    this.RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        private WriteableBitmap _WorldImage;
        public WriteableBitmap WorldImage
        {
            get { return this._WorldImage; }
            set
            {
                if (this._WorldImage != value)
                {
                    this._WorldImage = value;
                    this.RaisePropertyChanged("WorldImage");
                    this.RaisePropertyChanged("WorldZoomedHeight");
                    this.RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        private bool _isMouseContained;
        public bool IsMouseContained
        {
            get
            {
                return this._isMouseContained;
            }
            set
            {
                if (this._isMouseContained != value)
                {
                    this._isMouseContained = value;
                    this.RaisePropertyChanged("IsMouseContained");
                }
            }
        }

        private ICommand _mouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new RelayCommand<TileMouseEventArgs>(OnMouseOverPixel)); }
        }

        private ICommand _mouseDownCommand;
        public ICommand MouseDownCommand
        {
            get { return _mouseDownCommand ?? (_mouseDownCommand = new RelayCommand<TileMouseEventArgs>(OnMouseDownPixel)); }
        }

        private ICommand _mouseUpCommand;
        public ICommand MouseUpCommand
        {
            get { return _mouseUpCommand ?? (_mouseUpCommand = new RelayCommand<TileMouseEventArgs>(OnMouseUpPixel)); }
        }

        private ICommand _mouseWheelCommand;
        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new RelayCommand<TileMouseEventArgs>(OnMouseWheel)); }
        }



        private void OnMouseOverPixel(TileMouseEventArgs e)
        {
            this.MouseOverTile = e.Tile;
        }

        private void OnMouseDownPixel(TileMouseEventArgs e)
        {
            this.MouseDownTile = e.Tile;
        }

        private void OnMouseUpPixel(TileMouseEventArgs e)
        {
            this.MouseUpTile = e.Tile;
        }

        private void OnMouseWheel(TileMouseEventArgs e)
        {
            if (e.WheelDelta > 0)
                this.Zoom = this.Zoom * 1.1;
            if (e.WheelDelta < 0)
                this.Zoom = this.Zoom * 0.9;

        }

        private PointInt32 _mouseOverTile;
        public PointInt32 MouseOverTile
        {
            get { return this._mouseOverTile; }
            set
            {
                if (this._mouseOverTile != value)
                {
                    this._mouseOverTile = value;
                    this.RaisePropertyChanged("MouseOverTile");
                }
            }
        }

        private PointInt32 _mouseDownTile;
        public PointInt32 MouseDownTile
        {
            get { return this._mouseDownTile; }
            set
            {
                if (this._mouseDownTile != value)
                {
                    this._mouseDownTile = value;
                    this.RaisePropertyChanged("MouseDownTile");
                }
            }
        }

        private PointInt32 _mouseUpTile;
        public PointInt32 MouseUpTile
        {
            get { return this._mouseUpTile; }
            set
            {
                if (this._mouseUpTile != value)
                {
                    this._mouseUpTile = value;
                    this.RaisePropertyChanged("MouseUpTile");
                }
            }
        }

    }
}
