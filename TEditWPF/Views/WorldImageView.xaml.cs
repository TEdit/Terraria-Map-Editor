using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TEditWPF.Common;
using TEditWPF.ViewModels;

namespace TEditWPF.Views
{
    /// <summary>
    /// Interaction logic for WorldImageView.xaml
    /// </summary>
    [Export]
    public partial class WorldImageView : UserControl
    {
        public WorldImageView()
        {
            InitializeComponent();
        }

        [Import]
        public WorldImageViewModel ViewModel
        {
            set { this.DataContext = value; }
            get { return this.DataContext as WorldImageViewModel; }
        }

        private Point _mouseDownAbsolute;

        private void ViewportMouseMove(object sender, MouseEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
            {
                Location = e.GetPosition((IInputElement)sender),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.MiddleButton,
                WheelDelta = 0
            };


            if (cargs.MiddleButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.ScrollAll;

                var partView = (ScrollViewer)this.FindName("WorldScrollViewer");
                var currentScrollPosition = new Point(partView.HorizontalOffset, partView.VerticalOffset);
                var currentMousePosition = e.GetPosition(this);
                var delta = new Point(currentMousePosition.X - _mouseDownAbsolute.X,
                                      currentMousePosition.Y - _mouseDownAbsolute.Y);


                partView.ScrollToHorizontalOffset(currentScrollPosition.X + (delta.X) / 128.0);
                partView.ScrollToVerticalOffset(currentScrollPosition.Y + (delta.Y) / 128.0);
            }
            else
            {

                Cursor = Cursors.Cross;
            }

            if (ViewModel.MouseMoveCommand.CanExecute(cargs))
                ViewModel.MouseMoveCommand.Execute(cargs);
        }

        private void ViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
                                          {
                                              Location = e.GetPosition((IInputElement)sender),
                                              LeftButton = e.LeftButton,
                                              RightButton = e.RightButton,
                                              MiddleButton = e.MiddleButton,
                                              WheelDelta = 0
                                          };


            _mouseDownAbsolute = e.GetPosition(this);

            if (ViewModel.MouseDownCommand.CanExecute(cargs))
                ViewModel.MouseDownCommand.Execute(cargs);
        }

        private void ViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
            {
                Location = e.GetPosition((IInputElement)sender),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.MiddleButton,
                WheelDelta = e.Delta
            };

            if (ViewModel.MouseWheelCommand.CanExecute(cargs))
                ViewModel.MouseWheelCommand.Execute(cargs);

            ScrollToTile(ViewModel.MouseOverTile);
        }

        private void ViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
            {
                Location = e.GetPosition((IInputElement)sender),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.MiddleButton,
                WheelDelta = 0
            };

            if (ViewModel.MouseUpCommand.CanExecute(cargs))
                ViewModel.MouseUpCommand.Execute(cargs);
        }

        private void ViewportMouseEnter(object sender, MouseEventArgs e)
        {
            ViewModel.IsMouseContained = true;
        }

        private void ViewportMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.IsMouseContained = false;
        }

        private void ScrollToTile(Point tile)
        {
            var zoom = ViewModel.Zoom;
            var partView = (ScrollViewer)this.FindName("WorldScrollViewer");
            partView.ScrollToHorizontalOffset((tile.X * zoom) - (partView.ActualWidth / 2.0));
            partView.ScrollToVerticalOffset((tile.Y * zoom) - (partView.ActualHeight / 2.0));
        }

        private void RequestedScrollToTile(object sender, DataTransferEventArgs e)
        {
            this.ScrollToTile(ViewModel.RequestScrollTile);
        }
    }
}
