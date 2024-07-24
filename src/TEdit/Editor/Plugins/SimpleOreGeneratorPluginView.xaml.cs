using System;
using System.Windows;

namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for SimpleOreGeneratorPluginView.xaml
    /// </summary>
    public partial class SimpleOreGeneratorPluginView : Window
    {
        public bool OnlySelection { get; private set; }
        public bool EnableUndo { get; private set; } = true;
        public bool IncludeAsh { get; private set; }

        // Tier 1
        public bool Tier1Both { get; private set; }
        public bool Copper { get; private set; }
        public bool Tin { get; private set; }
        public bool Tier1NA { get; private set; }

        // Tier 2
        public bool Tier2Both { get; private set; }
        public bool Iron { get; private set; }
        public bool Lead { get; private set; }
        public bool Tier2NA { get; private set; }

        // Tier 3
        public bool Tier3Both { get; private set; }
        public bool Silver { get; private set; }
        public bool Tungsten { get; private set; }
        public bool Tier3NA { get; private set; }

        // Tier 4
        public bool Tier4Both { get; private set; }
        public bool Gold { get; private set; }
        public bool Platinum { get; private set; }
        public bool Tier4NA { get; private set; }

        // Tier 5
        public bool Meteorite { get; private set; }

        // Tier 6
        public bool Tier6Both { get; private set; }
        public bool Demonite { get; private set; }
        public bool Crimtane { get; private set; }
        public bool Tier6NA { get; private set; }

        // Tier 7
        public bool Obsidian { get; private set; }

        // Tier 8
        public bool Hellstone { get; private set; }

        // Tier 9
        public bool Tier9Both { get; private set; }
        public bool Cobalt { get; private set; }
        public bool Palladium { get; private set; }
        public bool Tier9NA { get; private set; }

        // Tier 10
        public bool Tier10Both { get; private set; }
        public bool Mythril { get; private set; }
        public bool Orichalcum { get; private set; }
        public bool Tier10NA { get; private set; }

        // Tier 11
        public bool Tier11Both { get; private set; }
        public bool Adamantite { get; private set; }
        public bool Titanium { get; private set; }
        public bool Tier11NA { get; private set; }

        // Tier 12
        public bool Chlorophyte { get; private set; }

        // Tier 13
        public bool Luminite { get; private set; }

        public SimpleOreGeneratorPluginView(bool activeSelection)
        {
            InitializeComponent();
            if (activeSelection)
            {
                OnlyUseSelectionCheckBox.IsEnabled = true;
                OnlySelection = true;
            }
            else
            {
                OnlyUseSelectionCheckBox.IsEnabled = false;
                OnlySelection = false;
            }

            // Initialize radio buttons to default values.
            Tier1BothRadioButton.IsChecked = true;
            Tier2BothRadioButton.IsChecked = true;
            Tier3BothRadioButton.IsChecked = true;
            Tier4BothRadioButton.IsChecked = true;
            Tier6NARadioButton.IsChecked = true;
            Tier9NARadioButton.IsChecked = true;
            Tier10NARadioButton.IsChecked = true;
            Tier11NARadioButton.IsChecked = true;
            IncludeAshCheckBox.IsChecked = true;
            UndoCheckBox.IsChecked = true;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IncludeAsh = IncludeAshCheckBox.IsChecked ?? false;
                OnlySelection = OnlyUseSelectionCheckBox.IsChecked ?? false;
                EnableUndo = UndoCheckBox.IsChecked ?? false;

                // Tier 1
                Tier1Both = Tier1BothRadioButton.IsChecked ?? false;
                Copper = CopperRadioButton.IsChecked ?? false;
                Tin = TinRadioButton.IsChecked ?? false;
                Tier1NA = Tier1NARadioButton.IsChecked ?? false;

                // Tier 2
                Tier2Both = Tier2BothRadioButton.IsChecked ?? false;
                Iron = IronRadioButton.IsChecked ?? false;
                Lead = LeadRadioButton.IsChecked ?? false;
                Tier2NA = Tier2NARadioButton.IsChecked ?? false;

                // Tier 3
                Tier3Both = Tier3BothRadioButton.IsChecked ?? false;
                Silver = SilverRadioButton.IsChecked ?? false;
                Tungsten = TungstenRadioButton.IsChecked ?? false;
                Tier3NA = Tier3NARadioButton.IsChecked ?? false;

                // Tier 4
                Tier4Both = Tier4BothRadioButton.IsChecked ?? false;
                Gold = GoldRadioButton.IsChecked ?? false;
                Platinum = PlatinumRadioButton.IsChecked ?? false;
                Tier4NA = Tier4NARadioButton.IsChecked ?? false;

                // Tier 5
                Meteorite = MeteoriteCheckbox.IsChecked ?? false;

                // Tier 6
                Tier6Both = Tier6BothRadioButton.IsChecked ?? false;
                Demonite = DemoniteRadioButton.IsChecked ?? false;
                Crimtane = CrimtaneRadioButton.IsChecked ?? false;
                Tier6NA = Tier6NARadioButton.IsChecked ?? false;

                // Tier 7
                Obsidian = ObsidianCheckbox.IsChecked ?? false;

                // Tier 8
                Hellstone = HellstoneCheckbox.IsChecked ?? false;

                // Tier 9
                Tier9Both = Tier9BothRadioButton.IsChecked ?? false;
                Cobalt = CobaltRadioButton.IsChecked ?? false;
                Palladium = PalladiumRadioButton.IsChecked ?? false;
                Tier9NA = Tier9NARadioButton.IsChecked ?? false;

                // Tier 10
                Tier10Both = Tier10BothRadioButton.IsChecked ?? false;
                Mythril = MythrilRadioButton.IsChecked ?? false;
                Orichalcum = OrichalcumRadioButton.IsChecked ?? false;
                Tier10NA = Tier10NARadioButton.IsChecked ?? false;

                // Tier 11
                Tier11Both = Tier11BothRadioButton.IsChecked ?? false;
                Adamantite = AdamantiteRadioButton.IsChecked ?? false;
                Titanium = TitaniumRadioButton.IsChecked ?? false;
                Tier11NA = Tier11NARadioButton.IsChecked ?? false;

                // Tier 12
                Chlorophyte = ChlorophyteCheckbox.IsChecked ?? false;

                // Tier 13
                Luminite = LuminiteCheckbox.IsChecked ?? false;

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
