using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using TEdit.Editor.Clipboard;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins
{
    public class HouseGenPlugin : BasePlugin
    {
        HouseGenPluginView view;

        public HouseGenPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Procedural House Generator";
        }

        public override void Execute()
        {
            if (view == null)
            {
                view = new();
                view.Owner = Application.Current.MainWindow;
                view.WorldViewModel = _wvm;
                view.DataContext = view;
                view.Show();
                _wvm.SelectedTabIndex = 3;
            }
            else
            {
                view.Show();
                _wvm.SelectedTabIndex = 3;
            }
        }

        public static HouseGenTemplate LoadTemplate(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);

            var shematic = ClipboardBuffer.Load(path);

            //Template Loading
            string jsonValue = "";
            using (var sr = new StreamReader(new FileStream(Path.GetDirectoryName(path) + "\\" + filename + ".json", FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                jsonValue = sr.ReadToEnd();
            }

            HouseGenTemplateData data = JsonConvert.DeserializeObject<HouseGenTemplateData>(jsonValue);

            return new HouseGenTemplate(filename, shematic, data);
        }

        public class HouseGenTemplate
        {
            private readonly string _name;
            private readonly HouseGenTemplateData _data;
            private readonly ClipboardBuffer _schematic;

            public string Name => _name;

            public HouseGenTemplateData Template => _data;

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
}
