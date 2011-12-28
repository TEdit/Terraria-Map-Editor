using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
        private PixelMapManager _pixelMap;
        private ProgressChangedEventArgs _progress;
        private ICommand _saveAsCommand;
        private ICommand _saveCommand;
        private ICommand _setTool;
        private readonly IList<ITool> _tools = new ObservableCollection<ITool>();
        private readonly TilePicker _tilePicker = new TilePicker();
        private readonly BrushSettings _brush = new BrushSettings();
        private readonly MouseTile _mouseOverTile = new MouseTile();

        public WorldViewModel()
        {
            World.ProgressChanged += OnProgressChanged;
            Brush.BrushChanged += OnPreviewChanged;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            WindowTitle = string.Format("TEdit v{0} *ALPHA* {1}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                Path.GetFileName(_currentFile));
        }

        public event EventHandler PreviewChanged;
        private void PreviewChange()
        {
            OnPreviewChanged(this, new EventArgs());
        }
        protected virtual void OnPreviewChanged(object sender, EventArgs e)
        {
            if (PreviewChanged != null) PreviewChanged(sender, e);
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

        private string _windowTitle;


        public string WindowTitle
        {
            get { return _windowTitle; }
            set { Set("WindowTitle", ref _windowTitle, value); }
        }

        public BrushSettings Brush
        {
            get { return _brush; }
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

                PreviewChange();
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
            ActiveTool.MouseDown(e);
        }

        public void MouseUpTile(TileMouseState e)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;
            ActiveTool.MouseUp(e);
        }

        public void MouseMoveTile(TileMouseState e)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;
            ActiveTool.MouseMove(e);
        }

        private void OpenWorld()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Terrarial World File|*.wld|Terraria World Backup|*.bak|TEdit Backup File|*.TEdit";
            ofd.DefaultExt = "Terrarial World File|*.wld";
            ofd.Title = "Load Terraria World File";
            ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
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
            if ((bool)sfd.ShowDialog())
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
                .ContinueWith(t =>
                {
                    PixelMap = t.Result;
                    UpdateTitle();
                }, TaskFactoryHelper.UiTaskScheduler);
        }

        public void SetPixel(int x, int y, PaintMode? mode = null, bool? erase = null)
        {
            var curTile = CurrentWorld.Tiles[x, y];
            var curMode = mode ?? TilePicker.PaintMode;
            var isErase = erase ?? TilePicker.IsEraser;

            switch (curMode)
            {
                case PaintMode.Tile:
                    SetTile(curTile, isErase);
                    break;
                case PaintMode.Wall:
                    SetWall(curTile, isErase);
                    break;
                case PaintMode.TileAndWall:
                    SetTile(curTile, isErase);
                    SetWall(curTile, isErase);
                    break;
                case PaintMode.Wire:
                    SetPixelAutomatic(curTile, wire: !isErase);
                    break;
                case PaintMode.Liquid:
                    SetPixelAutomatic(curTile, liquid: isErase ? (byte)0 : (byte)255, isLava: TilePicker.IsLava);
                    break;
            }

            Color curBgColor = GetBackgroundColor(y);
            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
        }

        private void UpdateRenderWorld()
        {
            Task.Factory.StartNew(
                () =>
                    {
                        if (CurrentWorld != null)
                        {
                            for (int y = 0; y < CurrentWorld.TilesHigh; y++)
                            {
                                Color curBgColor = GetBackgroundColor(y);
                                OnProgressChanged(this, new ProgressChangedEventArgs(y.ProgressPercentage(CurrentWorld.TilesHigh), "Calculating Colors..."));
                                for (int x = 0; x < CurrentWorld.TilesWide; x++)
                                {
                                    PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
                                }
                            }
                        }
                    });
        }

        private bool _showWalls = true;
        private bool _showTiles = true;
        private bool _showLiquid = true;
        private bool _showWires = true;


        public bool ShowWires
        {
            get { return _showWires; }
            set
            {
                Set("ShowWires", ref _showWires, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowLiquid
        {
            get { return _showLiquid; }
            set
            {
                Set("ShowLiquid", ref _showLiquid, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowTiles
        {
            get { return _showTiles; }
            set
            {
                Set("ShowTiles", ref _showTiles, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowWalls
        {
            get { return _showWalls; }
            set
            {
                Set("ShowWalls", ref _showWalls, value);
                UpdateRenderWorld();
            }
        }

        private void SetWall(Tile curTile, bool erase)
        {
            if (TilePicker.WallMaskMode == MaskMode.Off ||
                (TilePicker.WallMaskMode == MaskMode.Tile && curTile.Wall == TilePicker.WallMask) ||
                (TilePicker.WallMaskMode == MaskMode.Empty && curTile.Wall == 0))
            {
                if (erase)
                    SetPixelAutomatic(curTile, wall: 0);
                else
                    SetPixelAutomatic(curTile, wall: TilePicker.Wall);
            }
        }

        private void SetTile(Tile curTile, bool erase)
        {
            if (TilePicker.TileMaskMode == MaskMode.Off ||
                (TilePicker.TileMaskMode == MaskMode.Tile && curTile.Type == TilePicker.TileMask && curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive))
            {
                if (erase)
                    SetPixelAutomatic(curTile, tile: -1);
                else
                    SetPixelAutomatic(curTile, tile: TilePicker.Tile);
            }
        }

        private void SetPixelAutomatic(Tile curTile,
            int? tile = null,
            int? wall = null,
            byte? liquid = null,
            bool? isLava = null,
            bool? wire = null,
            short? u = null,
            short? v = null)
        {
            // Set Tile Data

            if (u != null)
                curTile.U = (short)u;
            if (v != null)
                curTile.V = (short)v;

            if (tile != null)
            {
                if (tile == -1)
                {
                    curTile.Type = 0;
                    curTile.IsActive = false;
                }
                else
                {
                    curTile.Type = (byte)tile;
                    curTile.IsActive = true;
                }
            }

            if (wall != null)
                curTile.Wall = (byte)wall;

            if (liquid != null)
                curTile.Liquid = (byte)liquid;

            if (isLava != null)
                curTile.IsLava = (bool)isLava;

            if (wire != null)
                curTile.HasWire = (bool)wire;

            if (curTile.IsActive)
                if (World.TileProperties[curTile.Type].IsSolid)
                    curTile.Liquid = 0;

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
                        pixels.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
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