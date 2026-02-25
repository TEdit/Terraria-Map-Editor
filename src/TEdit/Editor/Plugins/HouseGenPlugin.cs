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
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;

namespace TEdit.Editor.Plugins;

public partial class HouseGenPlugin : BasePlugin
{
    private HouseGenPluginView _view;
    private HouseGenViewModel _vm;

    public HouseGenPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "Procedural House Generator";
    }

    public override void Execute()
    {
        if (_view == null)
        {
            _view = new();
            _view.Owner = Application.Current.MainWindow;
            _vm = new(_wvm.Clipboard.LoadedBuffers);
            _view.DataContext = _vm;
            _view.Show();
            _wvm.SelectedTabIndex = (int)SidebarTab.Clipboard;
        }
        else
        {
            _view.Show();
            _wvm.SelectedTabIndex = (int)SidebarTab.Clipboard;
        }
    }

    public partial class HouseGenViewModel : ReactiveObject
    {
        private readonly Random _rand = new();

        private ObservableCollection<ClipboardBufferPreview> _clipboardManagerLoadedBuffers;

        private ObservableCollection<HouseGenTemplate> _templates;
        public ObservableCollection<HouseGenTemplate> HouseGenTemplates => _templates;

        [Reactive] private HouseGenTemplate _selectedTemplate;

        [Reactive] private ClipboardBufferPreview _generatedSchematic;

        public WriteableBitmap Preview => GeneratedSchematic?.Preview;

        private Vector2Int32 _generatedSchematicSize;
        public Vector2Int32 GeneratedSchematicSize
        {
            get { return GeneratedSchematicSize; }
        }

        private IObservable<bool> CanGenerate =>
            this.WhenAnyValue(x => x.SelectedTemplate).Select(t => t != null);

        private IObservable<bool> CanCopy =>
            this.WhenAnyValue(x => x.GeneratedSchematic).Select(s => s?.Preview != null);

        [ReactiveCommand]
        private void Import() => ImportTemplateSchematic();

        [ReactiveCommand(CanExecute = nameof(CanGenerate))]
        private void Generate() => GenerateFromTemplate(SelectedTemplate);

        [ReactiveCommand(CanExecute = nameof(CanCopy))]
        private void Copy() => _clipboardManagerLoadedBuffers.Add(GeneratedSchematic);

        public HouseGenViewModel(ObservableCollection<ClipboardBufferPreview> loadedBuffers)
        {
            _templates = new ObservableCollection<HouseGenTemplate>();
            _clipboardManagerLoadedBuffers = loadedBuffers;
            //TODO: Load all templates located in templates folder.   
        }

        private void ImportTemplateSchematic()
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

                    _templates.Add(LoadTemplate(ofd.FileName));
                    SelectedTemplate = _templates[_templates.Count - 1];
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateFromTemplate(HouseGenTemplate template)
        {

            //Retrive buffer size.
            _generatedSchematicSize.X = template.Schematic.Size.X;
            _generatedSchematicSize.Y = template.Schematic.Size.Y / template.Data.Count;

            var bufferData = new ClipboardBuffer(_generatedSchematicSize);

            int type;

            // Process Rooms
            for (int i = 0; i < template.Data.Rooms.Count; i++)
            {
                type = _rand.Next(template.Data.Count);

                Room room = template.Data.Rooms[i];

                for (int x = 0; x < room.Width; x++)
                {
                    for (int y = 0; y < room.Height; y++)
                    {
                        try
                        {
                            bufferData.Tiles[x + room.X, y + room.Y] =
                                (Tile)template.Schematic.Tiles[x + room.X, y + room.Y + (_generatedSchematicSize.Y * type)].Clone();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            ErrorLogging.LogDebug($"HouseGen IndexOutOfRange in {room.Name}: {e.Message}");
                            MessageBox.Show(e.Message + " Check JSON Data for " + room.Name, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            // Process Roofs
            // Generate the random roof value once for the whole "building"
            type = _rand.Next(template.Data.Count);

            for (int i = 0; i < template.Data.Roofs.Count; i++)
            {
                Roof roof = template.Data.Roofs[i];

                for (int x = 0; x < roof.Width; x++)
                {
                    for (int y = 0; y < roof.Height; y++)
                    {
                        try
                        {
                            bufferData.Tiles[x + roof.X, y + roof.Y] =
                                (Tile)template.Schematic.Tiles[x + roof.X, y + roof.Y + (_generatedSchematicSize.Y * type)].Clone();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            MessageBox.Show(e.Message + " Check JSON Data for " + roof.Name, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            //Fill in any empty space of schematic outside of room definintions (empty space within bounds of roof or room should already be filled)
            for (int x2 = 0; x2 < _generatedSchematicSize.X; x2++)
            {
                for (int y2 = 0; y2 < _generatedSchematicSize.Y; y2++)
                {
                    try
                    {
                        if (bufferData.Tiles[x2, y2] == null) { bufferData.Tiles[x2, y2] = new Tile(); }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        ErrorLogging.LogDebug($"HouseGen IndexOutOfRange (Count mismatch): {e.Message}");
                        MessageBox.Show(e.Message + " Check JSON Data for value 'Count' to make sure it matches with associated schematic.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            GeneratedSchematic = new ClipboardBufferPreview(bufferData);
        }

        private static HouseGenTemplate LoadTemplate(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);

            var schematic = ClipboardBuffer.Load(path);

            //Template Loading
            string jsonValue = "";
            using (var sr = new StreamReader(new FileStream(Path.GetDirectoryName(path) + "\\" + filename + ".json", FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                jsonValue = sr.ReadToEnd();
            }

            HouseGenTemplateData data = JsonConvert.DeserializeObject<HouseGenTemplateData>(jsonValue);

            return new HouseGenTemplate(filename, schematic, data);
        }
    }

    public class HouseGenTemplate
    {
        private readonly string _name;
        private readonly HouseGenTemplateData _data;
        private readonly ClipboardBuffer _schematic;

        public string Name => _name;
        public HouseGenTemplateData Data => _data;
        public ClipboardBuffer Schematic => _schematic;

        public HouseGenTemplate(string name, ClipboardBuffer schematic, HouseGenTemplateData template)
        {
            _schematic = schematic;
            _name = name;
            _data = template;
        }
    }

    public class HouseGenTemplateData
    {
        public int Count { get; set; }
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Roof> Roofs { get; set; } = new List<Roof>();
    }

    public class Room
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Roof
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
