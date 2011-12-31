using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Threading;
using Microsoft.Win32;
using TEditXNA.Terraria;
using TEditXNA.Terraria.Objects;
using TEditXna.Editor;
using TEditXna.Editor.Clipboard;
using TEditXna.Editor.Tools;
using TEditXna.Editor.Undo;
using TEditXna.Render;

namespace TEditXna.ViewModel
{
    public partial class WorldViewModel : ViewModelBase
    {
        private readonly BrushSettings _brush = new BrushSettings();
        private readonly MouseTile _mouseOverTile = new MouseTile();
        private readonly TilePicker _tilePicker = new TilePicker();
        private readonly IList<ITool> _tools = new ObservableCollection<ITool>();
        private readonly Selection _selection = new Selection();
        private readonly ClipboardManager _clipboard;
        private readonly UndoManager _undoManager;
        private ITool _activeTool;
        private string _currentFile;
        private World _currentWorld;
        private PixelMapManager _pixelMap;
        private ProgressChangedEventArgs _progress;
        private bool _showPoints = true;
        private bool _showLiquid = true;
        private bool _showTiles = true;
        private bool _showWalls = true;
        private bool _showWires = true;
        private string _windowTitle;
        private bool _showTextures = true;
        public bool[] CheckTiles;
        private bool _showGrid = true;
         

        public bool ShowGrid
        {
            get { return _showGrid; }
            set { Set("ShowGrid", ref _showGrid, value); }
        }
        public bool ShowTextures
        {
            get { return _showTextures; }
            set { Set("ShowTextures", ref _showTextures, value); }
        }
        private string _selectedPoint;
        private readonly ObservableCollection<string> _points = new ObservableCollection<string>();

        private Sprite _selectedSprite;


        public Sprite SelectedSprite
        {
            get { return _selectedSprite; }
            set
            {
                Set("SelectedSprite", ref _selectedSprite, value);
                PreviewChange();
            }
        }

        public ObservableCollection<string> Points
        {
            get { return _points; }
        }

        public string SelectedPoint
        {
            get { return _selectedPoint; }
            set { Set("SelectedPoint", ref _selectedPoint, value); }
        }

        public WorldViewModel()
        {
            _undoManager = new UndoManager(this);
            _clipboard = new ClipboardManager(this);
            World.ProgressChanged += OnProgressChanged;
            Brush.BrushChanged += OnPreviewChanged;
            UpdateTitle();
        }

        public UndoManager UndoManager
        {
            get { return _undoManager; }
        }

        public ClipboardManager Clipboard
        {
            get { return _clipboard; }
        }

        public Selection Selection
        {
            get { return _selection; }
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

        public bool ShowWires
        {
            get { return _showWires; }
            set
            {
                Set("ShowWires", ref _showWires, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowPoints
        {
            get { return _showPoints; }
            set { Set("ShowPoints", ref _showPoints, value); }
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

        private void UpdateTitle()
        {
            WindowTitle = string.Format("TEdit v{0} *BETA* {1}",
                                        Assembly.GetExecutingAssembly().GetName().Version,
                                        Path.GetFileName(_currentFile));
        }

        public event EventHandler PreviewChanged;

        public void PreviewChange()
        {
            OnPreviewChanged(this, new EventArgs());
        }

        protected virtual void OnPreviewChanged(object sender, EventArgs e)
        {
            if (PreviewChanged != null) PreviewChanged(sender, e);
        }

        private void SetActiveTool(ITool tool)
        {
            if (ActiveTool != tool)
            {
                if (tool.Name == "Paste" && !CanPaste())
                    return;

                if (ActiveTool != null)
                    ActiveTool.IsActive = false;

                ActiveTool = tool;
                tool.IsActive = true;

                PreviewChange();
            }
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

            CommandManager.InvalidateRequerySuggested();
        }

        public void MouseUpTile(TileMouseState e)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;
            ActiveTool.MouseUp(e);
            CommandManager.InvalidateRequerySuggested();
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
                LoadWorld(ofd.FileName);
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


        public void LoadWorld(string filename)
        {
            CurrentFile = filename;
            Task.Factory.StartNew(() => World.LoadWorld(filename))
                .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t =>
                                  {
                                      PixelMap = t.Result;
                                      UpdateTitle();
                                      Points.Clear();
                                      Points.Add("Spawn");
                                      Points.Add("Dungeon");
                                      foreach (var npc in CurrentWorld.NPCs)
                                      {
                                          Points.Add(npc.Name);
                                      }

                                  }, TaskFactoryHelper.UiTaskScheduler);
        }
    }
}