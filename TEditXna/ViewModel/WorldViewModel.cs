using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Command;
using BCCL.MvvmLight.Threading;
using BCCL.Utility;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;
using TEditXna.Editor;
using TEditXna.Editor.Tools;
using TEditXna.Render;

namespace TEditXna.ViewModel
{
    public class WorldViewModel : ViewModelBase
    {
        private ITool _activeTool;
        private ICommand _closeApplication;
        private ICommand _commandOpenWorld;
        private string _currentFile;
        private World _currentWorld;
        private readonly MouseTile _mouseOverTile = new MouseTile();
        private PixelMapManager _pixelMap;
        private ProgressChangedEventArgs _progress;
        private ICommand _saveAsCommand;
        private ICommand _saveCommand;
        private readonly IList<ITool> _tools = new ObservableCollection<ITool>();
        private readonly TilePicker _tilePicker = new TilePicker();

        public WorldViewModel()
        {
            World.ProgressChanged += OnProgressChanged;
        }

        public MouseTile MouseOverTile
        {
            get { return _mouseOverTile; }
        }

        public string CurrentFile
        {
            get { return _currentFile; }
            set { Set("CurrentFile", ref _currentFile, value); }
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

        
        
        public TilePicker TilePicker
        {
            get { return _tilePicker; }
        }

        public ICommand CloseApplicationCommand
        {
            get { return _closeApplication ?? (_closeApplication = new RelayCommand(Application.Current.Shutdown)); }
        }

        public ICommand OpenCommand
        {
            get { return _commandOpenWorld ?? (_commandOpenWorld = new RelayCommand(OpenWorld)); }
        }

        public ICommand SaveAsCommand
        {
            get { return _saveAsCommand ?? (_saveAsCommand = new RelayCommand(SaveWorldAs)); }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(SaveWorld)); }
        }

        private ICommand _setTool;
         

        public ICommand SetTool
        {
            get { return _setTool ?? (_setTool = new RelayCommand<ITool>(SetActiveTool)); }
        }

        private void SetActiveTool(ITool tool)
        {
            if (ActiveTool != tool)
            {
                if (ActiveTool != null)
                    ActiveTool.IsActive = false;

                ActiveTool = tool;
                tool.IsActive = true;
            }
        }

        public IList<ITool> Tools
        {
            get { return _tools; }
        }

        public ITool ActiveTool
        {
            get { return _activeTool; }
            set { Set("ActiveTool", ref _activeTool, value); }
        }

        public PixelMapManager PixelMap
        {
            get { return _pixelMap; }
            set { Set("PixelMap", ref _pixelMap, value); }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => Progress = e);
        }

        public void MouseDownTile(TileMouseState e)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;
        }

        public void MouseUpTile(TileMouseState e)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;
        }

        public void MouseMoveTile(TileMouseState e)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;
        }

        private void OpenWorld()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Terrarial World File|*.wld|Terraria World Backup|*.bak|TEdit Backup File|*.TEdit";
            ofd.DefaultExt = "Terrarial World File|*.wld";
            ofd.Title = "Load Terraria World File";
            ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = false;
            if ((bool) ofd.ShowDialog())
            {
                CurrentFile = ofd.FileName;
                LoadWorld(CurrentFile);
            }
        }

        private void SaveWorld()
        {
            if (CurrentWorld == null)
                return;

            if (string.IsNullOrWhiteSpace(CurrentFile))
                SaveWorldAs();

            SaveWorldFile();
        }

        private void SaveWorldAs()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            if ((bool) sfd.ShowDialog())
            {
                CurrentFile = sfd.FileName;
                SaveWorldFile();
            }
        }

        private void SaveWorldFile()
        {
            Task.Factory.StartNew(() => CurrentWorld.Save(CurrentFile))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
        }


        private void LoadWorld(string filename)
        {
            Task.Factory.StartNew(() => World.LoadWorld(filename))
                .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t => PixelMap = t.Result, TaskFactoryHelper.UiTaskScheduler);
        }

        private PixelMapManager RenderEntireWorld()
        {
            var pixels = new PixelMapManager();
            pixels.InitializeBuffers(CurrentWorld.TilesWide, CurrentWorld.TilesHigh);
            if (CurrentWorld != null)
            {
                for (int y = 0; y < CurrentWorld.TilesHigh; y++)
                {
                    Color curBgColor = GetBackgroundColor(y);
                    OnProgressChanged(this, new ProgressChangedEventArgs(y.ProgressPercentage(CurrentWorld.TilesHigh), "Calculating Colors..."));
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
            if (y < 80)
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