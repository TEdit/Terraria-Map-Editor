using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Command;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;
using TEditXna.Render;

namespace TEditXna.ViewModel
{
    public class WorldViewModel : ViewModelBase
    {
        public class MouseTile : ObservableObject
        {
            private Tile _tile;
            private string _tileName;
            private string _wallName;

            public string WallName
            {
                get { return _wallName; }
                set { Set("WallName", ref _wallName, value); }
            }

            public string TileName
            {
                get { return _tileName; }
                set { Set("TileName", ref _tileName, value); }
            }

            public Tile Tile
            {
                get { return _tile; }
                set
                {
                    Set("Tile", ref _tile, value);
                    TileName = World.TileProperties[_tile.Type].Name;
                    WallName = World.WallProperties[_tile.Wall].Name;
                }
            }
        }

        public WorldViewModel()
        {
            World.ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BCCL.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => Progress = e);
        }

        private ProgressChangedEventArgs _progress;
        private World _currentWorld;

        private Vector2Int32 _mouseOverTileLocation;
        private MouseTile _mouseOverTile;


        public MouseTile MouseOverTile
        {
            get { return _mouseOverTile; }
            set { Set("MouseOverTile", ref _mouseOverTile, value); }
        }
        public Vector2Int32 MouseOverTileLocation
        {
            get { return _mouseOverTileLocation; }
            set
            {
                Set("MouseOverTileLocation", ref _mouseOverTileLocation, value);
                if (MouseOverTile == null) MouseOverTile = new MouseTile();
                MouseOverTile.Tile = CurrentWorld.Tiles[_mouseOverTileLocation.X, _mouseOverTileLocation.Y];
            }
        }

        public World CurrentWorld
        {
            get { return _currentWorld; }
            set { Set("CurrentWorld", ref _currentWorld, value); }
        }

        public ProgressChangedEventArgs Progress
        {
            get { return _progress; }
            set { Set("Progress", ref _progress, value); }
        }

        private ICommand _commandOpenWorld;

        private ICommand _closeApplication; 
        public ICommand CloseApplication
        {
            get { return _closeApplication ?? (_closeApplication = new RelayCommand(Application.Current.Shutdown)); }
        }
        public ICommand CommandOpenWorld
        {
            get { return _commandOpenWorld ?? (_commandOpenWorld = new RelayCommand(OpenWorld, CanOpenWorld)); }
        }

        private bool CanOpenWorld()
        {
            return true;
        }

        private void OpenWorld()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Terrarial World File|*.wld|Terraria World Backup|*.bak|TEdit Backup File|*.TEdit";
            ofd.DefaultExt = "Terrarial World File|*.wld";
            ofd.Title = "Load Terraria World File";
            ofd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                LoadWorld(ofd.FileName);
            }
        }

        private void LoadWorld(string filename)
        {
            Task.Factory.StartNew(() => World.LoadWorld(filename))
                .ContinueWith(t => this.CurrentWorld = t.Result, BCCL.MvvmLight.Threading.TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t => this.PixelMap = t.Result, BCCL.MvvmLight.Threading.TaskFactoryHelper.UiTaskScheduler);
        }

        private PixelMapManager _pixelMap;


        public PixelMapManager PixelMap
        {
            get { return _pixelMap; }
            set { Set("PixelMap", ref _pixelMap, value); }
        }
        private PixelMapManager RenderEntireWorld()
        {
            var pixels = new PixelMapManager(100, 100);
            pixels.InitializeBuffers(CurrentWorld.TilesWide, CurrentWorld.TilesHigh);
            if (CurrentWorld != null)
            {
                for (int y = 0; y < CurrentWorld.TilesHigh; y++)
                {
                    var curBgColor = GetBackgroundColor(y);
                    OnProgressChanged(this, new ProgressChangedEventArgs(BCCL.Utility.Calc.ProgressPercentage(y, CurrentWorld.TilesHigh), "Calculating Colors..."));
                    for (int x = 0; x < CurrentWorld.TilesWide; x++)
                    {

                        pixels.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor));
                    }
                }
            }
            return pixels;
        }


        public Color GetBackgroundColor(int y)
        {
            if (y < 20)
                return World.GlobalColors["Space"];
            if (y > CurrentWorld.TilesHigh - 192)
                return World.GlobalColors["Hell"];
            if (y > CurrentWorld.RockLevel)
                return World.GlobalColors["Rock"];
            if (y > CurrentWorld.GroundLevel)
                return World.GlobalColors["Earth"];

            return World.GlobalColors["Sky"];
        }
    }
}