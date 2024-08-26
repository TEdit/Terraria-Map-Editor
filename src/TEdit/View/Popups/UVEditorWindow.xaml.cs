using System.Collections.Generic;
using System.Windows;
using System;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.ViewModel;

namespace TEdit.View.Popups
{
    public partial class UVEditorWindow : Window
    {
        private readonly List<Tuple<Tile, Vector2Int32>> UVEditorTileList;
        private readonly List<Tuple<Tile, Vector2Int32>> UVEditorTileListOriginal;
        private readonly WorldViewModel _wvm;

        // Constructor to initialize the window with given tiles and ViewModel.
        public UVEditorWindow(List<Tuple<Tile, Vector2Int32>> tiles, WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            InitializeComponent();
            UVEditorTileList = tiles;

            // Clone initial tile list for restoration.
            UVEditorTileListOriginal = CloneTileList(tiles);

            // Save the orignal states.
            SaveFramesToUndo();

            // Display current UV coordinates.
            DisplaySelectedFrames();
        }

        #region Controls

        // Handles Set Values button click to set all values to a manual state.
        private void SetValuesButton_Click(object sender, RoutedEventArgs e)
        {
            // Amount to move in UV directions.
            int uAmount = int.Parse(UTextBoxManual.Text);
            int vAmount = int.Parse(VTextBoxManual.Text);

            // Set each tile to the manual values.
            foreach (var tileTuple in UVEditorTileList)
            {
                tileTuple.Item1.U = (short)uAmount;
                tileTuple.Item1.V = (short)vAmount;
            }

            // Update display to reflect restored state.
            DisplaySelectedFrames();
        }

        // Handles button clicks to move UV coordinates.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Amount to move in UV directions.
            int uAmount = int.Parse(UTextBox.Text);
            int vAmount = int.Parse(VTextBox.Text);

            if (sender == UpButton)
            {
                MoveV(-vAmount); // Move UV coordinates up (negative V direction).
            }
            else if (sender == DownButton)
            {
                MoveV(vAmount); // Move UV coordinates down (positive V direction).
            }
            else if (sender == LeftButton)
            {
                MoveU(-uAmount); // Move UV coordinates left (negative U direction).
            }
            else if (sender == RightButton)
            {
                MoveU(uAmount); // Move UV coordinates right (positive U direction).
            }
        }

        // Handles OK button click to save changes and close the window.
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Save current state for undo functionality.
            SaveFramesToUndo();

            // Close the editor window.
            Close();
        }

        // Handles Restore button click to revert changes to original state.
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore the original tile values.
            for (int i = 0; i < UVEditorTileList.Count; i++)
            {
                UVEditorTileList[i].Item1.U = UVEditorTileListOriginal[i].Item1.U;
                UVEditorTileList[i].Item1.V = UVEditorTileListOriginal[i].Item1.V;
            }

            // Update display to reflect restored state.
            DisplaySelectedFrames();
        }

        // Handles Cancel button click to discard changes and close the window.
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore original tile states.
            RestoreButton_Click(sender, e);

            // Close the editor window.
            Close();
        }
        #endregion

        #region Data Functions

        // Updates the display with the current UV coordinates of selected tiles.
        private void DisplaySelectedFrames()
        {
            // Clear the current display.
            SelectedFramesTextBox.Clear();

            // Illiterate through each tile and update the textbox.
            foreach (var tileTuple in UVEditorTileList)
            {
                var tile = tileTuple.Item1;

                // Check if tile is empty. If empty, skip its entree.
                if (tile.Type <= 0)
                    continue;

                SelectedFramesTextBox.AppendText($"({tile.U}, {tile.V}){Environment.NewLine}");
            }
        }

        // Moves UV coordinates in the U direction by a specified amount.
        private void MoveU(int amount)
        {
            foreach (var tileTuple in UVEditorTileList)
            {
                tileTuple.Item1.U += (short)amount;
            }
            DisplaySelectedFrames();
        }

        // Moves UV coordinates in the V direction by a specified amount.
        private void MoveV(int amount)
        {
            foreach (var tileTuple in UVEditorTileList)
            {
                tileTuple.Item1.V += (short)amount;
            }
            DisplaySelectedFrames();
        }
        #endregion

        #region Clone Function

        // Creates a deep copy of the tile list for undo functionality.
        private List<Tuple<Tile, Vector2Int32>> CloneTileList(List<Tuple<Tile, Vector2Int32>> tiles)
        {
            var clonedList = new List<Tuple<Tile, Vector2Int32>>();
            foreach (var tileTuple in tiles)
            {
                // Create a new Tile object with the same properties.
                var clonedTile = new Tile
                {
                    U = tileTuple.Item1.U,
                    V = tileTuple.Item1.V,
                    IsActive = tileTuple.Item1.IsActive
                };

                // Add the cloned Tile and its corresponding Vector2Int32 to the list.
                clonedList.Add(new Tuple<Tile, Vector2Int32>(clonedTile, tileTuple.Item2));
            }
            return clonedList;
        }
        #endregion

        #region Undo Manager

        // Saves the current state of tile UV coordinates for undo functionality.
        private void SaveFramesToUndo()
        {
            // Illiterate through each tile and save its new location.
            foreach (var tileTuple in UVEditorTileList)
            {
                var location = tileTuple.Item2;
                _wvm.UndoManager.SaveTile(location.X, location.Y);
            }

            // Save undo operation.
            _wvm.UndoManager.SaveUndo();
        }
        #endregion
    }
}
