using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using TEdit.ViewModels;

namespace TEdit.Views
{
    /// <summary>
    /// Interaction logic for NewWorldView.xaml
    /// </summary>
    public partial class NewWorldView : UserControl
    {
        public NewWorldView()
        {
            InitializeComponent();
        }

        [Import]
        public NewWorldViewModel ViewModel
        {
            get { return (NewWorldViewModel)DataContext; }
            set { DataContext = value; }
        }
    }
}
