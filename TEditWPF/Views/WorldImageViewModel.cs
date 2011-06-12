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
                }
            }
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
                }
            }
        }




    }
}
