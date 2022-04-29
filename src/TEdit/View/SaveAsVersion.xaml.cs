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
using GalaSoft.MvvmLight.Command;

namespace TEdit.UI.Xaml
{
    /// <summary>
    /// Interaction logic for SaveAsVersionGUI.xaml
    /// </summary>
    public partial class SaveAsVersionGUI : Window
    {
        // Using a DependencyProperty as the backing store for WorldVersion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WorldVersionProperty =
            DependencyProperty.Register("WorldVersion", typeof(uint), typeof(SaveAsVersionGUI), new PropertyMetadata((uint)0));

        private ICommand _saveAsCommand;


        public SaveAsVersionGUI()
        {
            InitializeComponent();
            DataContext = this;
        }

        public uint WorldVersion
        {
            get { return (uint)GetValue(WorldVersionProperty); }
            set { SetValue(WorldVersionProperty, value); }
        }

        public ICommand SaveAsVersionCommand
        {
            get { return _saveAsCommand ?? (_saveAsCommand = new RelayCommand<string>(SaveAsVersionCommandAction)); }
        }

        private void SaveAsVersionCommandAction(string gameVersion)
        {
            if (World.SaveConfiguration.GameVersionToSaveVersion.TryGetValue(gameVersion, out uint worldVersion))
            {
                WorldVersion = worldVersion;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                WorldVersion = World.CompatibleVersion;
                this.DialogResult = false;
                this.Close();
            }
        }
    }
}
