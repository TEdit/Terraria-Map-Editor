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

namespace TEditXna.View
{
    /// <summary>
    /// Interaction logic for KillTallyView.xaml
    /// </summary>
    public partial class SpecialTileView : UserControl
    {
        public SpecialTileView()
        {
            InitializeComponent();

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
