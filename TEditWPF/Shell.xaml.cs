using System;
using System.Collections.Generic;
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
using TEditWPF.ViewModels;
using TEditWPF.Views;

namespace TEditWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        
        private void MockLoadWorldandRender(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            if ((bool)ofd.ShowDialog())
            {
                var wf = TerrariaWorld.World.Load(ofd.FileName);
                var r = new RenderWorld.WorldRenderer();
                var img = r.RenderWorld(wf);
                var vm = (WorldViewModel) worldImageView1.DataContext;
                vm.World = wf;
                vm.WorldImage = img;
            }
        }
    }
}
