using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TEdit.ViewModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TEdit.ViewModel;
using Microsoft.Win32;
using TEdit.Terraria;
using TEdit.MvvmLight.Threading;

namespace TEdit.UI.Xaml
{
    /// <summary>
    /// Interaction logic for SaveAsVersionGUI.xaml
    /// </summary>
    public partial class SaveAsVersionGUI : Window
    {
        public SaveAsVersionGUI()
        {
            InitializeComponent();
        }

        // Export To 1.3.5.3
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(192, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // Export To Current Version (1.4.2.3)
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(238, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }
    }
}
