using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using System.ComponentModel.Composition;
using TEditWPF.Infrastructure;
using TerrariaWorld.Game;
using TerrariaWorld.Common;

namespace TEditWPF.Views
{
    [Export]
    public class WorldImageViewModel : ObservableObject
    {
        public WorldImageViewModel()
        {
            //this._bmp = new WriteableBitmap
            _mouseOverTile = new System.Windows.Point(20, 20);
            this._world = new World();
            this.World.Header.MaxTiles = new Point(1200, 4200);
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
                }
            }
        }

        public int WorldHeight
        {
            get { return this._world.Header.MaxTiles.X; }
        }

        public int WorldWidth
        {
            get { return this._world.Header.MaxTiles.Y; }
        }

        public double WorldZoomedHeight
        {
            get { return this._world.Header.MaxTiles.X * this._Zoom; }
        }

        public double WorldZoomedWidth
        {
            get { return this._world.Header.MaxTiles.Y * this._Zoom; }
        }

        private double _ViewportWidth;
        public double ViewportWidth
        {
            get { return this._ViewportWidth; }
            set
            {
                if (this._ViewportWidth != value)
                {
                    this._ViewportWidth = value;
                    this.RaisePropertyChanged("ViewportWidth");
                    this.RaisePropertyChanged("HorizontalScrollMaximum");
                }
            }
        }

        private double _ViewportHeight;
        public double ViewportHeight
        {
            get { return this._ViewportHeight; }
            set
            {
                if (this._ViewportHeight != value)
                {
                    this._ViewportHeight = value;
                    this.RaisePropertyChanged("ViewportHeight");
                    this.RaisePropertyChanged("VerticalScrollMaximum");
                }
            }
        }

        public double HorizontalScrollMaximum
        {
            get { return this.WorldZoomedWidth - this.ViewportWidth; }
        }

        public double VerticalScrollMaximum
        {
            get { return this.WorldZoomedHeight - this.ViewportHeight; }
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
                    this.RaisePropertyChanged("HorizontalScrollMaximum");
                    this.RaisePropertyChanged("VerticalScrollMaximum");
                }
            }
        }

        WriteableBitmap _bmp;

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

        Boolean IsPointInWorld(CustomMouseEventArgs e)
        {
            return true;
        }

        private ICommand _mouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new RelayCommand<CustomMouseEventArgs>(OnMouseOverPixel)); }
        }

        private ICommand _mouseDownCommand;
        public ICommand MouseDownCommand
        {
            get { return _mouseDownCommand ?? (_mouseDownCommand = new RelayCommand<CustomMouseEventArgs>(OnMouseDownPixel)); }
        }

        private ICommand _mouseUpCommand;
        public ICommand MouseUpCommand
        {
            get { return _mouseUpCommand ?? (_mouseUpCommand = new RelayCommand<CustomMouseEventArgs>(OnMouseUpPixel)); }
        }

        private ICommand _mouseWheelCommand;
        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new RelayCommand<CustomMouseEventArgs>(OnMouseWheel)); }
        }


        private void OnMouseOverPixel(CustomMouseEventArgs e)
        {
            // TODO: Calculate tile based on zoom
            this.MouseOverTile = e.Location;
        }

        private void OnMouseDownPixel(CustomMouseEventArgs e)
        {
            // TODO: Calculate tile based on zoom
            this.MouseDownTile = e.Location;
        }

        private void OnMouseUpPixel(CustomMouseEventArgs e)
        {
            // TODO: Calculate tile based on zoom
            this.MouseUpTile = e.Location;
        }

        private void OnMouseWheel(CustomMouseEventArgs e)
        {
            if (e.WheelDelta > 0)
                this.Zoom = this.Zoom * 1.1;
            if (e.WheelDelta < 0)
                this.Zoom = this.Zoom * 0.9;
        }

        private System.Windows.Point _mouseOverTile;
        public System.Windows.Point MouseOverTile
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

        private System.Windows.Point _mouseDownTile;
        public System.Windows.Point MouseDownTile
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

        private System.Windows.Point _mouseUpTile;
        public System.Windows.Point MouseUpTile
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


        private double _scrollPositionVertical;
        public double ScrollPositionVertical
        {
            get { return this._scrollPositionVertical; }
            set
            {
                if (this._scrollPositionVertical != value)
                {
                    this._scrollPositionVertical = value;
                    this.RaisePropertyChanged("ScrollPositionVertical");
                    this.RaisePropertyChanged("ScrollPositionVerticalInverted");
                }
            }
        }

        public double ScrollPositionVerticalInverted
        {
            get { return -this._scrollPositionVertical; }
        }

        private double _scrollPositionHorizontal;
        public double ScrollPositionHorizontal
        {
            get { return this._scrollPositionHorizontal; }
            set
            {
                if (this._scrollPositionHorizontal != value)
                {
                    this._scrollPositionHorizontal = value;
                    this.RaisePropertyChanged("ScrollPositionHorizontal");
                    this.RaisePropertyChanged("ScrollPositionHorizontalInverted");
                }
            }
        }

        public double ScrollPositionHorizontalInverted
        {
            get { return -this._scrollPositionHorizontal; }
        }




    }
}
