using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TEditXNA.Terraria;

namespace TEditXna.View.Popups
{
    /// <summary>
    /// Interaction logic for SignPopup.xaml
    /// </summary>
    public partial class SignPopup : Popup
    {
        public SignPopup(Sign sign)
        {
            InitializeComponent();
            DataContext = sign;
        }

        public void OpenSign(Sign sign)
        {
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
