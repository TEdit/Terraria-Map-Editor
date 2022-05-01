using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using TEdit.Helper;
using TEdit.Terraria;

namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for BestiaryEditor.xaml
    /// </summary>
    public partial class BestiaryEditor : Window
    {
        public String Entities { get; set; }
        public Int32 Defeated { get; set; }
        public bool Unlocked { get; set; }
        public bool Near { get; set; }
        public bool Talked { get; set; }

        World CurrentWorld;

        public BestiaryEditor(World world)
        {
            InitializeComponent();
            CurrentWorld = world;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e) // Save changes from current selection
        {
            Close();
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "This will completely replace your currently loaded world Bestiary and Kill Tally with a reset bestiary. Continue?",
                "Reset Bestiary?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            var world = CurrentWorld;

            // make a backup
            var bestiary = world.Bestiary.Copy(CurrentWorld.Version);
            var killTally = world.KilledMobs.ToArray();
            try
            {
                ErrorLogging.TelemetryClient?.TrackEvent(nameof(UnlockButtonClick));

                World.ResetBestiary(world);
            }
            catch (Exception ex)
            {
                world.Bestiary = bestiary;
                world.KilledMobs.Clear();
                world.KilledMobs.AddRange(killTally);
                MessageBox.Show($"Error resetting Bestiary data. Your current bestiary has been restored.\r\n{ex.Message}");

            }
            Close();
        }

        private void UnlockButtonClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "This will completely replace your currently loaded world Bestiary and Kill Tally with a completed bestiary. Continue?",
                "Complete Bestiary?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            var world = CurrentWorld;

            // make a backup
            var bestiary = world.Bestiary.Copy(CurrentWorld.Version);
            var killTally = world.KilledMobs.ToArray();
            try
            {
                ErrorLogging.TelemetryClient?.TrackEvent(nameof(UnlockButtonClick));

                World.CompleteBestiary(world);
            }
            catch (Exception ex)
            {
                world.Bestiary = bestiary;
                world.KilledMobs.Clear();
                world.KilledMobs.AddRange(killTally);
                MessageBox.Show($"Error completing Bestiary data. Your current bestiary has been restored.\r\n{ex.Message}");

            }
            Close();
        }

        // Fill Table Upon Load
        private void BestiaryReadout_Loaded(object sender, RoutedEventArgs e)
        {
            List<BestiaryEditor> BestiaryList = new List<BestiaryEditor>(); //Set a list that holds values of your class' type.

            foreach (string Ent in World.BestiaryKilledIDs) // Foreach Entity Killed
            {
                // Check if NPC is defined in worlds defeated NPCS
                if (CurrentWorld.Bestiary.NPCKills.ContainsKey(Ent))
                {
                    // Grab values for NPC
                    int value;
                    CurrentWorld.Bestiary.NPCKills.TryGetValue(Ent, out value);

                    // Check if NPC is defined in worlds near NPCS
                    bool near = false;
                    if (CurrentWorld.Bestiary.NPCNear.Contains(Ent))
                    { near = true; }

                    // Check if NPC is defined in worlds talked NPCS
                    bool talked = false;
                    if (CurrentWorld.Bestiary.NPCChat.Contains(Ent))
                    { talked = true; }

                    // Add Entree
                    BestiaryList.Add(new BestiaryEditor(null) { Entities = Ent, Defeated = value, Unlocked = true, Near = near, Talked = talked });
                }
                else
                {
                    // No value found, leave defualt
                    // Check if NPC is defined in worlds near NPCS
                    bool near = false;
                    if (CurrentWorld.Bestiary.NPCNear.Contains(Ent))
                    { near = true; }

                    // Check if NPC is defined in worlds talked NPCS
                    bool talked = false;
                    if (CurrentWorld.Bestiary.NPCChat.Contains(Ent))
                    { talked = true; }

                    // Add Entree
                    BestiaryList.Add(new BestiaryEditor(null) { Entities = Ent, Defeated = 0, Unlocked = false, Near = near, Talked = talked });
                }
            }

            //Add to the listview
            BestiaryReadout.ItemsSource = BestiaryList;
        }
    }
}
