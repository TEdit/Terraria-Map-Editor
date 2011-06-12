using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TEditWPF.Infrastructure;

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

        private void ViewportMouseMove(object sender, MouseEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
            {
                Location = e.GetPosition(this),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.RightButton,
                WheelDelta = 0
            };

            if (ViewModel.MouseMoveCommand.CanExecute(cargs))
                ViewModel.MouseMoveCommand.Execute(cargs);
        }

        private void ViewportMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
                                          {
                                              Location = e.GetPosition(this),
                                              LeftButton = e.LeftButton,
                                              RightButton = e.RightButton,
                                              MiddleButton = e.RightButton,
                                              WheelDelta = 0
                                          };

            if (ViewModel.MouseDownCommand.CanExecute(cargs))
                ViewModel.MouseDownCommand.Execute(cargs);
        }

        private void ViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
            {
                Location = e.GetPosition(this),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.RightButton,
                WheelDelta = e.Delta
            };

            if (ViewModel.MouseWheelCommand.CanExecute(cargs))
                ViewModel.MouseWheelCommand.Execute(cargs);
        }

        private void ViewportMouseUp(object sender, MouseButtonEventArgs e)
        {
            var cargs = new CustomMouseEventArgs()
            {
                Location = e.GetPosition(this),
                LeftButton = e.LeftButton,
                RightButton = e.RightButton,
                MiddleButton = e.RightButton,
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

        private void ViewportSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.ViewportWidth = e.NewSize.Width;
            ViewModel.ViewportHeight = e.NewSize.Height;
        }
    }
}
