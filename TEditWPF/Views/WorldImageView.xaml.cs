using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;
using TEditWPF.ViewModels;

namespace TEditWPF.Views
{
    /// <summary>
    /// Interaction logic for WorldImageView.xaml
    /// </summary>
    public partial class WorldImageView : UserControl
    {
        public WorldImageView()
        {
            InitializeComponent();
        }

        private Point _mouseDownAbsolute;

        private void ViewportMouseMove(object sender, MouseEventArgs e)
        {
            var vm = (WorldViewModel)this.DataContext;

            var cargs = new TileMouseEventArgs
            {
                Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.MiddleButton,
                WheelDelta = 0
            };


            if (cargs.MiddleButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.ScrollAll;

                var partView = (ScrollViewer)this.FindName("WorldScrollViewer");
                if (partView != null)
                {
                    var currentScrollPosition = new Point(partView.HorizontalOffset, partView.VerticalOffset);
                    var currentMousePosition = e.GetPosition(this);
                    var delta = new Point(currentMousePosition.X - _mouseDownAbsolute.X,
                                          currentMousePosition.Y - _mouseDownAbsolute.Y);


                    partView.ScrollToHorizontalOffset(currentScrollPosition.X + (delta.X) / 128.0);
                    partView.ScrollToVerticalOffset(currentScrollPosition.Y + (delta.Y) / 128.0);
                }
            }
            else
            {
                Cursor = Cursors.Cross;
            }

            if (vm.MouseMoveCommand.CanExecute(cargs))
                vm.MouseMoveCommand.Execute(cargs);

        }

        private void ViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cargs = new TileMouseEventArgs()
                                          {
                                              Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                                              LeftButton = e.LeftButton,
                                              RightButton = e.RightButton,
                                              MiddleButton = e.MiddleButton,
                                              WheelDelta = 0
                                          };


            _mouseDownAbsolute = e.GetPosition(this);

            var vm = (WorldViewModel)this.DataContext;
            if (vm.MouseDownCommand.CanExecute(cargs))
                vm.MouseDownCommand.Execute(cargs);
        }

        private void ViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var cargs = new TileMouseEventArgs()
            {
                Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.MiddleButton,
                WheelDelta = e.Delta
            };

            var vm = (WorldViewModel)this.DataContext;
            if (vm.MouseWheelCommand.CanExecute(cargs))
                vm.MouseWheelCommand.Execute(cargs);

            ScrollToTile(cargs.Tile);
        }

        private void ViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            var cargs = new TileMouseEventArgs()
            {
                Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.MiddleButton,
                WheelDelta = 0
            };

            var vm = (WorldViewModel)this.DataContext;
            if (vm.MouseUpCommand.CanExecute(cargs))
                vm.MouseUpCommand.Execute(cargs);
        }

        private void ViewportMouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (WorldViewModel)this.DataContext;
            vm.IsMouseContained = true;
        }

        private void ViewportMouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (WorldViewModel)this.DataContext;
            vm.IsMouseContained = false;
        }

        private void ScrollToTile(PointInt32 tile)
        {
            var vm = (WorldViewModel)this.DataContext;
            var zoom = vm.Zoom;
            var partView = (ScrollViewer)this.FindName("WorldScrollViewer");
            if (partView != null)
            {
                partView.ScrollToHorizontalOffset((tile.X * zoom) - (partView.ActualWidth / 2.0));
                partView.ScrollToVerticalOffset((tile.Y * zoom) - (partView.ActualHeight / 2.0));
            }
        }

        private PointInt32 GetTileAtPixel(System.Windows.Point pixel)
        {
            return new PointInt32((int)pixel.X, (int)pixel.Y);
        }


    }
}
