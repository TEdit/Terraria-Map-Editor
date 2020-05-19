using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TEditXNA.Terraria;

namespace TEditXna.View.Popups
{
    /// <summary>
    /// Interaction logic for ChestsContentsPopup.xaml
    /// </summary>
    public partial class ChestPopup : Popup
    {
        public ChestPopup(Chest chest)
        {
            InitializeComponent();
            DataContext = chest.Items;
        }

        public void OpenChest(Chest chest)
        {
            DataContext = chest.Items;
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
    }
}
