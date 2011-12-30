using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
