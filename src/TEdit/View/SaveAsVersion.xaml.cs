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

        // 1.2.0
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(71, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.2.1
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(72, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.3.0
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(156, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.3.2
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(177, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.3.3
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(187, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.3.4
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(187, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.3.5
        private void Button_Click_7(object sender, RoutedEventArgs e)
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

        // 1.4.0.5
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(228, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.4.1.1
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(233, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.4.2.1
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveVersion(234, WorldViewModel._currentWorld, sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
            }
            this.Close();
        }

        // 1.4.2.3
        private void Button_Click_11(object sender, RoutedEventArgs e)
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
