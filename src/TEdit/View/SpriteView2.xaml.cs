using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Terraria.Objects;
using System.Collections.Generic;
using System;

namespace TEdit.View
{
    /// <summary>
    /// Interaction logic for SpriteView.xaml
    /// </summary>
    public partial class SpriteView2 : UserControl
    {
        private WorldViewModel _wvm;

        public SpriteView2()
        {
            InitializeComponent();
            _wvm = ViewModelLocator.WorldViewModel;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }
        }
    }
}
