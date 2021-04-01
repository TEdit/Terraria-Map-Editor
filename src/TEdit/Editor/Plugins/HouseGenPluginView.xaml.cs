using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32;
using TEdit.Terraria;
using TEdit.Editor.Clipboard;
using TEdit.ViewModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace TEdit.Editor.Plugins
{
    public class HouseGenTemplate
    {
        private readonly string _name;
        private readonly int _schematicId;
        private readonly HouseGenTemplateData _data;

        public string Name
        {
            get { return _name; }
        }

        public int SchematicID
        {
            get { return _schematicId; }
        }

        public HouseGenTemplateData Template
        {
            get { return _data; }
        }

        public HouseGenTemplate(int id, string name, HouseGenTemplateData template)
        {
            _schematicId = id;
            _name = name;
            _data = template;
        }
    }

    public class HouseGenTemplateData
    {
        public int Count
        {
            get;
            set;
        }

        public IList <Room> Rooms
        {
            get;
            set;
        }

        public IList <Roof> Roofs
        {
            get;
            set;
        }
    }

    public class Room
    {
        public string Name
        {
            get;
            set;
        }
        public int X
        {
            get;
            set;
        }
        public int Y
        {
            get;
            set;
        }
        public int Width
        {
            get;
            set;
        }
        public int Height
        {
            get;
            set;
        }
    }

    public class Roof
    {
        public string Name
        {
            get;
            set;
        }
        public int X
        {
            get;
            set;
        }
        public int Y
        {
            get;
            set;
        }
        public int Width
        {
            get;
            set;
        }
        public int Height
        {
            get;
            set;
        }
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

        private readonly IList <HouseGenTemplate> HouseGenTemplates;
        private readonly IList <ClipboardBuffer> HouseGenSchematics;

        private Color _backgroundColor = Color.FromNonPremultiplied(32, 32, 32, 255);
        
        ClipboardBuffer _generatedSchematic;
        Geometry.Primitives.Vector2Int32 _bufferSize;

        public HouseGenPluginView()
        {
            InitializeComponent();
            _wvm = null;
            _bufferSize = new(0,0);
            HouseGenTemplates = new List<HouseGenTemplate>();
            HouseGenSchematics = new List<ClipboardBuffer>();
        }

        private bool ImportTemplateSchematic()
        {
            try
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "TEdit House Gen Schematic File|*.TEditHGSch";
                ofd.DefaultExt = "TEdit House Gen Schematic File|*.TEditHGSch";
                ofd.Title = "Import TEdit House Gen Schematic File";
                ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
                if (!Directory.Exists(ofd.InitialDirectory))
                    Directory.CreateDirectory(ofd.InitialDirectory);
                ofd.Multiselect = false;
                if ((bool)ofd.ShowDialog())
                {
                    int schematic_id = HouseGenSchematics.Count;
                    string filename = Path.GetFileNameWithoutExtension(ofd.FileName);

                    //Schematic Loading
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(ImportTemplateSchematic));
                    HouseGenSchematics.Add(ClipboardBuffer.Load(ofd.FileName));

                    //Template Loading
                    string jsonValue = "";
                    using (var sr = new StreamReader(new FileStream(Path.GetDirectoryName(ofd.FileName) + "\\" + filename + ".json", FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        jsonValue = sr.ReadToEnd();
                    }

                    HouseGenTemplateData data = JsonConvert.DeserializeObject<HouseGenTemplateData>(jsonValue);
                    HouseGenTemplates.Add(new HouseGenTemplate(schematic_id, filename, data));


                    System.Windows.Controls.ComboBoxItem cbi = new();
                    cbi.Content = HouseGenTemplates[schematic_id].Name;
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

            if (HouseGenTemplates == null)
            {
                return false;
            }
            return true;
        }

        private void GenHouse()
        {
            
            Random rand = new();

            //Select house type
            int houseType = rand.Next(HouseGenTemplates.Count);

            ClipboardBuffer templateSchematic = HouseGenSchematics[HouseGenTemplates[houseType].SchematicID];
            HouseGenTemplateData templateData = HouseGenTemplates[houseType].Template;

            //Retrive buffer size.
            _bufferSize.X = templateSchematic.Size.X;
            _bufferSize.Y = templateSchematic.Size.Y / templateData.Count;

            _generatedSchematic = new(_bufferSize);

            int type;

            //Process Rooms
            for (int i = 0; i < templateData.Rooms.Count; i++)
            {
                type = rand.Next(templateData.Count);

                for (int x = 0; x < templateData.Rooms[i].Width; x++)
                {
                    for (int y = 0; y < templateData.Rooms[i].Height; y++)
                    {
                        _generatedSchematic.Tiles[x + templateData.Rooms[i].X, y + templateData.Rooms[i].Y] = (Tile)templateSchematic.Tiles[x + templateData.Rooms[i].X, y + templateData.Rooms[i].Y + (_bufferSize.Y * type)].Clone();
                    }
                }
            }

            //Process Roofs
            type = rand.Next(templateData.Count); 

            for (int i = 0; i < templateData.Roofs.Count; i++)
            {
                for (int x = 0; x < templateData.Roofs[i].Width; x++)
                {
                    for (int y = 0; y < templateData.Roofs[i].Height; y++)
                    {
                        _generatedSchematic.Tiles[x + templateData.Roofs[i].X, y + templateData.Roofs[i].Y] = (Tile)templateSchematic.Tiles[x + templateData.Roofs[i].X, y + templateData.Roofs[i].Y + (_bufferSize.Y * type)].Clone();
                    }
                }
            }

            byte roofColor = (byte)rand.Next(31);

            //Fill in any empty space of schematic outside of room definintions (empty space within bounds of roof or room should alredy be filled)
            for (int x2 = 0; x2 < _bufferSize.X; x2++)
            {
                for (int y2 = 0; y2 < _bufferSize.Y; y2++)
                {
                    if (_generatedSchematic.Tiles[x2, y2] == null) _generatedSchematic.Tiles[x2, y2] = new Tile();
                }
            }

            UpdatePreview();
            
        }

        private void UpdatePreview()
        {
            _generatedSchematic.RenderBuffer();
            PreviewImage.Source = _generatedSchematic.Preview;
            Copy.IsEnabled = true;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Copy.IsEnabled = false;            
            Hide();
        }

        private void GenButtonClick(object sender, RoutedEventArgs e)
        {
            GenHouse();
            UpdatePreview();
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
                GenHouse();
            }
        }
    }
}
