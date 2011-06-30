using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TEdit.TerrariaWorld;

namespace TEdit.Views
{
    /// <summary>
    /// Interaction logic for ChestsContentsPopup.xaml
    /// </summary>
    public partial class SignPopup : Popup
    {
        public SignPopup(Sign sign)
        {
            InitializeComponent();
            DataContext = sign;
        }

        private void Popup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //IsOpen = false;
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            //IsOpen = false;
        }

        private void ClosePopup(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void ValidateLines(object sender, KeyEventArgs e)
        {
            // Limit to 10 lines
            var tb = sender as TextBox;

            if (tb != null)
            {
                if (e.Key == Key.Enter)
                {
                    if (tb.LineCount > 9)
                    {
                        e.Handled = true;
                    }
                }
            }
        }
    }
}