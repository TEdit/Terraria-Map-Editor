/*
Copyright (c) 2024 RussDev7

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.IO;
using System.Reactive;
using System.Windows;
using TEdit.ViewModel;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for ImageToPixelartEditorView.xaml
/// </summary>
public partial class ImageToPixelartEditorView : FluentWindow
{
    public ImageToPixelartEditorViewModel ViewModel { get; }

    public ImageToPixelartEditorView(WorldViewModel worldViewModel)
    {
        InitializeComponent();

        ViewModel = new ImageToPixelartEditorViewModel(worldViewModel);
        DataContext = ViewModel;

        SetupInteractions();
    }

    private void SetupInteractions()
    {
        // Handle open file dialog
        ViewModel.OpenFileInteraction.RegisterHandler(interaction =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            var result = dialog.ShowDialog() == true ? dialog.FileName : null;
            interaction.SetOutput(result);
        });

        // Handle save image dialog
        ViewModel.SaveImageInteraction.RegisterHandler(interaction =>
        {
            using var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            saveFileDialog.Title = "Save Generated Pixelart";
            saveFileDialog.FileName = "ImageOut";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                interaction.SetOutput(saveFileDialog.FileName);
            }
            else
            {
                interaction.SetOutput(null);
            }
        });

        // Handle save schematic dialog
        ViewModel.SaveSchematicInteraction.RegisterHandler(interaction =>
        {
            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "TEdit Schematic File|*.TEditSch",
                Title = "Export Schematic File",
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics")
            };

            if (!Directory.Exists(sfd.InitialDirectory))
                Directory.CreateDirectory(sfd.InitialDirectory);

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                interaction.SetOutput(sfd.FileName);
            }
            else
            {
                interaction.SetOutput(null);
            }
        });

        // Handle color picker dialog
        ViewModel.PickColorInteraction.RegisterHandler(interaction =>
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                interaction.SetOutput(colorDialog.Color);
            }
            else
            {
                interaction.SetOutput(null);
            }
        });

        // Handle message dialog
        ViewModel.ShowMessageInteraction.RegisterHandler(interaction =>
        {
            MessageBox.Show(interaction.Input);
            interaction.SetOutput(Unit.Default);
        });

        // Handle close dialog
        ViewModel.CloseDialogInteraction.RegisterHandler(interaction =>
        {
            DialogResult = interaction.Input;
            Close();
            interaction.SetOutput(Unit.Default);
        });
    }

    #region Drag and Drop

    private void BackgroundImage1_Drop(object sender, DragEventArgs e)
    {
        var data = e.Data.GetData(DataFormats.FileDrop);
        if (data is string[] filenames && filenames.Length > 0)
        {
            ViewModel.LoadImageFromPath(filenames[0]);
        }
    }

    private void BackgroundImage1_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    #endregion
}
