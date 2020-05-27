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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TEdit.ViewModel;

namespace TEdit.View
{
    /// <summary>
    /// Interaction logic for Sponsor.xaml
    /// </summary>
    public partial class Sponsor : UserControl
    {
        public Sponsor()
        {
            InitializeComponent();
        }

        private void PatreonButtonClick(object sender, RoutedEventArgs e)
        {
            WorldViewModel.LaunchUrl("https://www.patreon.com/bePatron?u=2278324");
        }

        private void GithubButtonClick(object sender, RoutedEventArgs e)
        {
            WorldViewModel.LaunchUrl("https://github.com/TEdit/Terraria-Map-Editor");
        }

        private void TwitterButtonClick(object sender, RoutedEventArgs e)
        {
            WorldViewModel.LaunchUrl("https://twitter.com/binaryconstruct");
        }
    }
}
