using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using TEdit.Helper;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.View
{
    /// <summary>
    /// Interaction logic for BestiaryEditor.xaml
    /// </summary>
    public partial class BestiaryEditor : UserControl
    {
        private BestiaryViewModel _vm;

        public BestiaryEditor()
        {
            InitializeComponent();
            _vm = ViewModelLocator.GetBestiaryViewModel();
            DataContext = _vm;
        }
    }
}
