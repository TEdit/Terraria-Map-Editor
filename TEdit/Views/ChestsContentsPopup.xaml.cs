using System;
using System.Collections;
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
using TEdit.TerrariaWorld;

namespace TEdit.Views
{
    /// <summary>
    /// Interaction logic for ChestsContentsPopup.xaml
    /// </summary>
    public partial class ChestsContentsPopup : Popup
    {
        public ChestsContentsPopup(Chest chest)
        {
            InitializeComponent();
            ChestList.ItemsSource = chest.Items;
        }

        private void Popup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsOpen = false;
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            IsOpen = false;
        }
    }
}
