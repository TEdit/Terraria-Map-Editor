using System.Windows.Controls.Primitives;
using System.Windows.Input;
using TEdit.TerrariaWorld;

namespace TEdit.Views
{
    /// <summary>
    /// Interaction logic for ChestsContentsPopup.xaml
    /// </summary>
    public partial class ChestEditorPopup : Popup
    {
        public ChestEditorPopup(Chest chest)
        {
            InitializeComponent();
            this.DataContext = chest.Items;
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