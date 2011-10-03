using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.ViewModels;

namespace TEdit.Views
{
    /// <summary>
    /// Interaction logic for WorldImageView.xaml
    /// </summary>
    public partial class WorldImageView : UserControl
    {
        private Point _mouseDownAbsolute;

        public WorldImageView()
        {
            InitializeComponent();
        }

        private void ViewportMouseMove(object sender, MouseEventArgs e)
        {
            var vm = (WorldViewModel)DataContext;

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

                var partView = (ScrollViewer)FindName("WorldScrollViewer");
                if (partView != null)
                {
                    var currentScrollPosition = new Point(partView.HorizontalOffset, partView.VerticalOffset);
                    Point currentMousePosition = e.GetPosition(this);
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
            var cargs = new TileMouseEventArgs
                            {
                                Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                                LeftButton = e.LeftButton,
                                RightButton = e.RightButton,
                                MiddleButton = e.MiddleButton,
                                WheelDelta = 0
                            };


            _mouseDownAbsolute = e.GetPosition(this);

            var vm = (WorldViewModel)DataContext;
            if (vm.MouseDownCommand.CanExecute(cargs))
                vm.MouseDownCommand.Execute(cargs);
        }

        private void ViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var cargs = new TileMouseEventArgs
                            {
                                Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                                LeftButton = e.LeftButton,
                                RightButton = e.RightButton,
                                MiddleButton = e.MiddleButton,
                                WheelDelta = e.Delta
                            };

            var vm = (WorldViewModel)DataContext;
            var partView = (ScrollViewer)FindName("WorldScrollViewer");

            double initialZoom = vm.Zoom;
            var initialScrollPosition = new Point(partView.HorizontalOffset, partView.VerticalOffset);
            var initialCenterTile =
                new PointInt32((int)(partView.HorizontalOffset / initialZoom + (partView.ActualWidth / 2) / initialZoom),
                               (int)(partView.VerticalOffset / initialZoom + (partView.ActualHeight / 2) / initialZoom));

            if (vm.MouseWheelCommand.CanExecute(cargs))
                vm.MouseWheelCommand.Execute(cargs);

            double finalZoom = vm.Zoom;
            //var finalScrollPosition = new Point(partView.HorizontalOffset, partView.VerticalOffset);
            double zoomRatio = 1 - finalZoom / initialZoom;
            var scaleCenterTile = new PointInt32(
                (int)(initialCenterTile.X - ((cargs.Tile.X - initialCenterTile.X) * zoomRatio)),
                (int)(initialCenterTile.Y - ((cargs.Tile.Y - initialCenterTile.Y) * zoomRatio)));
            ScrollToTile(scaleCenterTile);
        }

        private void ViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            var cargs = new TileMouseEventArgs
                            {
                                Tile = GetTileAtPixel(e.GetPosition((IInputElement)sender)),
                                LeftButton = e.LeftButton,
                                RightButton = e.RightButton,
                                MiddleButton = e.MiddleButton,
                                WheelDelta = 0
                            };

            var vm = (WorldViewModel)DataContext;
            if (vm.MouseUpCommand.CanExecute(cargs))
                vm.MouseUpCommand.Execute(cargs);
        }

        private void ViewportMouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (WorldViewModel)DataContext;
            vm.IsMouseContained = true;
        }

        private void ViewportMouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (WorldViewModel)DataContext;
            vm.IsMouseContained = false;
        }

        private void ScrollToTile(PointInt32 tile)
        {
            var vm = (WorldViewModel)DataContext;
            double zoom = vm.Zoom;
            var partView = (ScrollViewer)FindName("WorldScrollViewer");
            if (partView != null)
            {
                partView.ScrollToHorizontalOffset((tile.X * zoom) - (partView.ActualWidth / 2.0));
                partView.ScrollToVerticalOffset((tile.Y * zoom) - (partView.ActualHeight / 2.0));
            }
        }

        private PointInt32 GetTileAtPixel(Point pixel)
        {
            return new PointInt32((int)pixel.X, (int)pixel.Y);
        }
    }
}