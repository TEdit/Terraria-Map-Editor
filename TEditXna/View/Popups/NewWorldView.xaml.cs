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
using System.Windows.Shapes;
using TEditXNA.Terraria;

namespace TEditXna.View.Popups
{
    /// <summary>
    /// Interaction logic for NewWorldView.xaml
    /// </summary>
    public partial class NewWorldView : Window
    {
        private readonly World _newWorld;
        public NewWorldView()
        {
            InitializeComponent();
            _newWorld = new World(1200, 4300, "TEdit World");
            _newWorld.Version = World.CompatibleVersion;
            _newWorld.GroundLevel = 350;
            _newWorld.RockLevel = 480;
            _newWorld.ResetTime();
            AddCharNames();
            this.DataContext = NewWorld;
        }

        private void AddCharNames()
        {
            _newWorld.CharacterNames.Add(17, "Harold");
            _newWorld.CharacterNames.Add(18, "Molly");
            _newWorld.CharacterNames.Add(19, "Dominique");
            _newWorld.CharacterNames.Add(20, "Felicitae");
            _newWorld.CharacterNames.Add(22, "Steve");
            _newWorld.CharacterNames.Add(54, "Fitz");
            _newWorld.CharacterNames.Add(38, "Gimut");
            _newWorld.CharacterNames.Add(107, "Knogs");
            _newWorld.CharacterNames.Add(108, "Fizban");
            _newWorld.CharacterNames.Add(124, "Nancy");
        }

        public World NewWorld
        {
            get { return _newWorld; }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
