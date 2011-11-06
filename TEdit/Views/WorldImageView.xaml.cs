using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.ViewModels;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace TEdit.Views
{
    /// <summary>
    /// Interaction logic for WorldImageView.xaml
    /// </summary>
    public partial class WorldImageView : UserControl
    {
        private Point _mouseDownAbsolute;
        private Point _scrollDownAbsolute;

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

            var partView = (ScrollViewer)FindName("WorldScrollViewer");
            
            // Standard middle-button crazy scroll
            if (partView != null && cargs.MiddleButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.ScrollAll;

                var currentScrollPosition = new Point(partView.HorizontalOffset, partView.VerticalOffset);
                Point currentMousePosition = e.GetPosition(this);
                var delta = new Point(currentMousePosition.X - _mouseDownAbsolute.X,
                                      currentMousePosition.Y - _mouseDownAbsolute.Y);

                partView.ScrollToHorizontalOffset(currentScrollPosition.X + (delta.X) / 128.0);
                partView.ScrollToVerticalOffset  (currentScrollPosition.Y + (delta.Y) / 128.0);

                ViewportRerender(sender, null);  // scroll change needs a re-render
            }
            // Google-like drag/drop scroll
            else if (partView != null && cargs.RightButton == MouseButtonState.Pressed && (vm.ActiveTool == null || vm.ActiveTool.Name == "Arrow"))
            {
                Cursor = Cursors.SizeAll;

                Point currentMousePosition = e.GetPosition(this);
                var delta = new Point(currentMousePosition.X - _mouseDownAbsolute.X,
                                      currentMousePosition.Y - _mouseDownAbsolute.Y);

                partView.ScrollToHorizontalOffset(_scrollDownAbsolute.X - delta.X);  // (dragging means going the opposite direction of the mouse)
                partView.ScrollToVerticalOffset  (_scrollDownAbsolute.Y - delta.Y);

                ViewportRerender(sender, null);  // scroll change needs a re-render
            }
            else {
                Cursor = Cursors.Cross;
            }

            if (vm.MouseMoveCommand.CanExecute(cargs))
                vm.MouseMoveCommand.Execute(cargs);

            if (cargs.LeftButton == MouseButtonState.Pressed || cargs.RightButton == MouseButtonState.Pressed)
                ViewportRerender(sender, null);  // may be potentially drawing
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

            var partView = (ScrollViewer)FindName("WorldScrollViewer");
            if (partView != null) _scrollDownAbsolute = new Point(partView.HorizontalOffset, partView.VerticalOffset);

            var vm = (WorldViewModel)DataContext;
            if (vm.MouseDownCommand.CanExecute(cargs))
                vm.MouseDownCommand.Execute(cargs);

            ViewportRerender(sender, null);  // potential for button effects to trigger tile changes
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

            ViewportRerender(sender, null);  // scroll/zoom change needs a re-render
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
            
            ViewportRerender(sender, null);  // potential for button effects to trigger tile changes
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
        private RectI TileViewportRect() {
            var vm = (WorldViewModel)DataContext;
            var partView = (ScrollViewer)FindName("WorldScrollViewer");

            var xy   = new PointInt32((int)(partView.HorizontalOffset / vm.Zoom), (int)(partView.VerticalOffset / vm.Zoom));
            var size = new SizeInt32 ((int)(partView.ActualWidth      / vm.Zoom), (int)(partView.ActualHeight   / vm.Zoom));
            return new RectI(xy, size);
        }

        private PointInt32 GetTileAtPixel(Point pixel)
        {
            return new PointInt32((int)pixel.X, (int)pixel.Y);
        }

        private void ViewportRerender(object sender, RoutedEventArgs e) {
            var vm = (WorldViewModel)DataContext;

            // Firstly, let's not refresh more than we have to
            TimeSpan interval = DateTime.Now - vm.LastRender;
            if (interval.Ticks < TimeSpan.TicksPerSecond / (vm.Zoom - 3)) return;  // ends up being 4fps for 7x, 7fps for 10x, 10fps for 13x, etc.

            // Zoom check (only render at 700+% zoom)
            if (vm.Zoom < 7.0 || !vm.CanRenderTextures()) {
                vm.WorldImage.Rendered = null;
                return;
            }

            // Make sure that the scroll position is accurate (even after a ScrollToTile call)
            // (this may re-call ViewportRerender once more)
            var partView = (ScrollViewer)FindName("WorldScrollViewer");
            partView.UpdateLayout();

            var rect = TileViewportRect() - new PointInt32(5, 5) + new SizeInt32(10, 10);  // allow for a small overlap
            rect.Rebound(vm.World.Header.WorldBounds);

            vm.RenderTextures(rect, partView, (Image)FindName("WorldImageRendered"));
        }
    }
}