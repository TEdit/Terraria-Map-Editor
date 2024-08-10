using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TEdit.Terraria;
using TEdit.Configuration;
using TEdit.Terraria.Objects;

namespace TEdit.View.Popups
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
            _newWorld = new World(1200, 4200, "TEdit World");
            _newWorld.Version = WorldConfiguration.CompatibleVersion;
            _newWorld.GroundLevel = 350;
            _newWorld.RockLevel = 480;

            // Custom world generation //

            _newWorld.GenerateGrass = true;
            _newWorld.GenerateWalls = true;
            _newWorld.CaveNoise = 0.08;
            _newWorld.CaveMultiplier = 0.02;
            _newWorld.CaveDensity = 3.00;
            _newWorld.CavePresets = new ObservableCollection<string> { "Normal", "Large", "labyrinth" };
            _newWorld.CavePresetIndex = 0;
            _newWorld.GenerateAsh = true;
            _newWorld.GenerateLava = true;
            _newWorld.UnderworldRoofNoise = 0.15;
            _newWorld.UnderworldFloorNoise = 0.05;
            _newWorld.UnderworldLavaNoise = 0.10;

            // End custom world generation //

            _newWorld.ResetTime();
            _newWorld.CreationTime = System.DateTime.Now.ToBinary();
            AddCharNames();
            DataContext = NewWorld;
        }

        private void AddCharNames()
        {
            _newWorld.CharacterNames.Add(new NpcName(17, "Harold"));
            _newWorld.CharacterNames.Add(new NpcName(18, "Molly"));
            _newWorld.CharacterNames.Add(new NpcName(19, "Dominique"));
            _newWorld.CharacterNames.Add(new NpcName(20, "Felicitae"));
            _newWorld.CharacterNames.Add(new NpcName(22, "Steve"));
            _newWorld.CharacterNames.Add(new NpcName(54, "Fitz"));
            _newWorld.CharacterNames.Add(new NpcName(38, "Gimut"));
            _newWorld.CharacterNames.Add(new NpcName(107, "Knogs"));
            _newWorld.CharacterNames.Add(new NpcName(108, "Fizban"));
            _newWorld.CharacterNames.Add(new NpcName(124, "Nancy"));

            // New for 1.2
            _newWorld.CharacterNames.Add(new NpcName(160, "Truffle"));
            _newWorld.CharacterNames.Add(new NpcName(178, "Steampunker"));
            _newWorld.CharacterNames.Add(new NpcName(207, "Dye Trader"));
            _newWorld.CharacterNames.Add(new NpcName(208, "Party Girl"));
            _newWorld.CharacterNames.Add(new NpcName(209, "Cyborg"));
            _newWorld.CharacterNames.Add(new NpcName(227, "Painter"));
            _newWorld.CharacterNames.Add(new NpcName(228, "Witch Doctor"));
            _newWorld.CharacterNames.Add(new NpcName(229, "Pirate"));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_newWorld.CavePresetIndex)
            {
                case 0:
                    _newWorld.CaveNoise = 0.08;
                    _newWorld.CaveMultiplier = 0.02;
                    _newWorld.CaveDensity = 3.00;
                    break;
                case 1:
                    _newWorld.CaveNoise = 0.1;
                    _newWorld.CaveMultiplier = 0.03;
                    _newWorld.CaveDensity = 2.5;
                    break;
                case 2:
                    _newWorld.CaveNoise = 0.12;
                    _newWorld.CaveMultiplier = 0.04;
                    _newWorld.CaveDensity = 2.0;
                    break;
                default:
                    _newWorld.CaveNoise = 0.08;
                    _newWorld.CaveMultiplier = 0.02;
                    _newWorld.CaveDensity = 3.00;
                    break;
            }
        }

        public World NewWorld
        {
            get { return _newWorld; }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
		
	    private void ToggleWorldGenerationClick(object sender, RoutedEventArgs e)
        {
            // Check if the property is currently visible.
            if (WorldGeneration.Visibility == Visibility.Collapsed)
            {
                // Change the window size.
                NewWorldWindow.Height = 592;

                // Toggle the property state.
                WorldGeneration.Visibility = Visibility.Visible;

                // Change button name.
                ToggleWorldGeneration.Content = "↑ Collapse World Generation ↑";
            }
            else if (WorldGeneration.Visibility == Visibility.Visible)
            {
                // Change the window size.
                NewWorldWindow.Height = 250;

                // Toggle the property state.
                WorldGeneration.Visibility = Visibility.Collapsed;

                // Change button name.
                ToggleWorldGeneration.Content = "↓ Expand World Generation ↓";
            }
        }
    }
}
