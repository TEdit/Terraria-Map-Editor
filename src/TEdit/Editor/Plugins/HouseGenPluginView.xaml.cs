/* 
Copyright (c) 2021 ReconditeDeity
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using TEdit.Terraria;
using TEdit.Editor.Clipboard;
using TEdit.Geometry.Primitives;
using TEdit.ViewModel;
using Newtonsoft.Json;
using static TEdit.Editor.Plugins.HouseGenPlugin;

namespace TEdit.Editor.Plugins
{

    public class HouseGenViewModel
    {


    }

    /// <summary>
    /// Interaction logic for HouseGenPluginView.xaml
    /// </summary>
    public partial class HouseGenPluginView : Window
    {
        private WorldViewModel _wvm;

        public WorldViewModel WorldViewModel
        {
            get { return _wvm; }
            set { _wvm = value; }
        }

        private readonly IList<HouseGenTemplate> HouseGenTemplates;

        ClipboardBuffer _generatedSchematic;
        Vector2Int32 _generatedSchematicSize;

        public HouseGenPluginView()
        {
            InitializeComponent();
            _wvm = null;
            _generatedSchematicSize = new(0, 0);
            HouseGenTemplates = new List<HouseGenTemplate>();
        }

        private bool ImportTemplateSchematic()
        {
            try
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "TEdit House Gen Schematic File|*.TEditHGSch";
                ofd.DefaultExt = "TEdit House Gen Schematic File|*.TEditHGSch";
                ofd.Title = "Import TEdit House Gen Schematic File";
                ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics\Generator");
                if (!Directory.Exists(ofd.InitialDirectory)) { Directory.CreateDirectory(ofd.InitialDirectory); }
                ofd.Multiselect = false;
                if ((bool)ofd.ShowDialog())
                {
                    string filename = Path.GetFileNameWithoutExtension(ofd.FileName);

                    //Schematic Loading
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(ImportTemplateSchematic));

                    var template = HouseGenPlugin.LoadTemplate(ofd.FileName);
                    HouseGenTemplates.Add(template);

                    System.Windows.Controls.ComboBoxItem cbi = new();
                    cbi.Content = template.Name;
                    SelectedTemplate.Items.Add(cbi);
                    SelectedTemplate.SelectedItem = cbi;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return GenHouse();
        }

        private bool GenHouse()
        {

            Random rand = new();

            //Select house type
            int houseType = SelectedTemplate.SelectedIndex;
            var selectedItem = HouseGenTemplates[houseType];

            var templateData = selectedItem.Template;
            var templateSchematic = selectedItem.Schematic;

            //Retrive buffer size.
            _generatedSchematicSize.X = templateSchematic.Size.X;
            _generatedSchematicSize.Y = templateSchematic.Size.Y / templateData.Count;

            _generatedSchematic = new(_generatedSchematicSize);

            int type;

            // Process Rooms
            for (int i = 0; i < templateData.Rooms.Count; i++)
            {
                type = rand.Next(templateData.Count);

                Room room = templateData.Rooms[i];

                for (int x = 0; x < room.Width; x++)
                {
                    for (int y = 0; y < room.Height; y++)
                    {
                        try
                        {
                            _generatedSchematic.Tiles[x + room.X, y + room.Y] =
                                (Tile)templateSchematic.Tiles[x + room.X, y + room.Y + (_generatedSchematicSize.Y * type)].Clone();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            MessageBox.Show(e.Message + " Check JSON Data for " + room.Name, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
            }

            // Process Roofs
            // Generate the random roof value once for the whole "building"
            type = rand.Next(templateData.Count);

            for (int i = 0; i < templateData.Roofs.Count; i++)
            {
                Roof roof = templateData.Roofs[i];

                for (int x = 0; x < roof.Width; x++)
                {
                    for (int y = 0; y < roof.Height; y++)
                    {
                        try
                        {
                            _generatedSchematic.Tiles[x + roof.X, y + roof.Y] =
                                (Tile)templateSchematic.Tiles[x + roof.X, y + roof.Y + (_generatedSchematicSize.Y * type)].Clone();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            MessageBox.Show(e.Message + " Check JSON Data for " + roof.Name, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
            }

            byte roofColor = (byte)rand.Next(31);

            //Fill in any empty space of schematic outside of room definintions (empty space within bounds of roof or room should already be filled)
            for (int x2 = 0; x2 < _generatedSchematicSize.X; x2++)
            {
                for (int y2 = 0; y2 < _generatedSchematicSize.Y; y2++)
                {
                    try
                    {
                        if (_generatedSchematic.Tiles[x2, y2] == null) { _generatedSchematic.Tiles[x2, y2] = new Tile(); }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        MessageBox.Show(e.Message + " Check JSON Data for value 'Count' to make sure it matches with associated schematic.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private void UpdatePreview()
        {
            _generatedSchematic.RenderBuffer();
            PreviewImage.Source = _generatedSchematic.Preview;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; //Prevent window from being closed, hide it instead.
            Hide();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void GenButtonClick(object sender, RoutedEventArgs e)
        {
            if (GenHouse())
            {
                UpdatePreview();
                Copy.IsEnabled = true;
            }
        }

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            _wvm.Clipboard.LoadedBuffers.Add(_generatedSchematic);
        }

        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            if (ImportTemplateSchematic())
            {
                Generate.IsEnabled = true;
            }
        }

    }
}
