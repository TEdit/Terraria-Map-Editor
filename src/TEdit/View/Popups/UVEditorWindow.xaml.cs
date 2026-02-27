using System.Collections.Generic;
using System.Windows;
using System;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

namespace TEdit.View.Popups
{
    public partial class UVEditorWindow : FluentWindow
    {
        private readonly List<Vector2Int32> _positions;
        private readonly List<Tile> _originalTiles;
        private readonly WorldViewModel _wvm;

        // Constructor to initialize the window with given tiles and ViewModel.
        public UVEditorWindow(List<Vector2Int32> positions, WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            InitializeComponent();
            _positions = positions;

            // Clone initial tile values for restoration.
            _originalTiles = new List<Tile>(positions.Count);
            foreach (var pos in positions)
            {
                _originalTiles.Add(_wvm.CurrentWorld.Tiles[pos.X, pos.Y]);
            }

            // Save the original states.
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
            foreach (var pos in _positions)
            {
                _wvm.CurrentWorld.Tiles[pos.X, pos.Y].U = (short)uAmount;
                _wvm.CurrentWorld.Tiles[pos.X, pos.Y].V = (short)vAmount;
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
            for (int i = 0; i < _positions.Count; i++)
            {
                var pos = _positions[i];
                _wvm.CurrentWorld.Tiles[pos.X, pos.Y].U = _originalTiles[i].U;
                _wvm.CurrentWorld.Tiles[pos.X, pos.Y].V = _originalTiles[i].V;
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

            foreach (var pos in _positions)
            {
                ref var tile = ref _wvm.CurrentWorld.Tiles[pos.X, pos.Y];

                // Check if tile is empty. If empty, skip its entry.
                if (tile.Type <= 0)
                    continue;

                SelectedFramesTextBox.AppendText($"({tile.U}, {tile.V}){Environment.NewLine}");
            }
        }

        // Moves UV coordinates in the U direction by a specified amount.
        private void MoveU(int amount)
        {
            foreach (var pos in _positions)
            {
                _wvm.CurrentWorld.Tiles[pos.X, pos.Y].U += (short)amount;
            }
            DisplaySelectedFrames();
        }

        // Moves UV coordinates in the V direction by a specified amount.
        private void MoveV(int amount)
        {
            foreach (var pos in _positions)
            {
                _wvm.CurrentWorld.Tiles[pos.X, pos.Y].V += (short)amount;
            }
            DisplaySelectedFrames();
        }
        #endregion

        #region Undo Manager

        // Saves the current state of tile UV coordinates for undo functionality.
        private void SaveFramesToUndo()
        {
            foreach (var pos in _positions)
            {
                _wvm.UndoManager.SaveTile(pos.X, pos.Y);
            }

            // Save undo operation.
            _wvm.UndoManager.SaveUndo();
        }
        #endregion
    }
}
