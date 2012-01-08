using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Threading;
using Microsoft.Win32;
using TEditXNA.Terraria;
using TEditXNA.Terraria.Objects;
using TEditXna.Editor;
using TEditXna.Editor.Clipboard;
using TEditXna.Editor.Plugins;
using TEditXna.Editor.Tools;
using TEditXna.Editor.Undo;
using TEditXna.Render;
using TEditXna.View.Popups;

namespace TEditXna.ViewModel
{
    public partial class WorldViewModel : ViewModelBase
    {
        private readonly BrushSettings _brush = new BrushSettings();
        private readonly MouseTile _mouseOverTile = new MouseTile();
        private readonly TilePicker _tilePicker = new TilePicker();
        private readonly ObservableCollection<ITool> _tools = new ObservableCollection<ITool>();
        private readonly ObservableCollection<IPlugin> _plugins = new ObservableCollection<IPlugin>();
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
        private MorphBiome _morphBiomeTarget;

        private ICollectionView _spritesView;
        public ICollectionView SpritesView
        {
            get { return _spritesView; }
            set
            {
                Set("SpritesView", ref _spritesView, value);

            }
        }

        private string _spriteFilter;
        public string SpriteFilter
        {
            get { return _spriteFilter; }
            set
            {
                Set("SpriteFilter", ref _spriteFilter, value);

                SpritesView.Refresh();
            }
        }

        private Chest _selectedChest;
        private Sign _selectedSign;
        private int _selectedTabIndex;


        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set("SelectedTabIndex", ref _selectedTabIndex, value); }
        }

        public Sign SelectedSign
        {
            get { return _selectedSign; }
            set
            {
                Set("SelectedSign", ref _selectedSign, value);
                SelectedTabIndex = 3;
            }
        }
        public Chest SelectedChest
        {
            get { return _selectedChest; }
            set
            {
                Set("SelectedChest", ref _selectedChest, value);
                SelectedTabIndex = 3;
            }
        }

        public ObservableCollection<IPlugin> Plugins
        {
            get { return _plugins; }
        }
        public MorphBiome MorphBiomeTarget
        {
            get { return _morphBiomeTarget; }
            set { Set("MorphBiomeTarget", ref _morphBiomeTarget, value); }
        }

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

            _spriteFilter = string.Empty;
            _spritesView = CollectionViewSource.GetDefaultView(World.Sprites);
            _spritesView.Filter = o =>
            {
                var sprite = o as Sprite;
                if (sprite == null || string.IsNullOrWhiteSpace(sprite.TileName))
                    return false;

                return sprite.TileName.IndexOf(_spriteFilter, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                       sprite.Name.IndexOf(_spriteFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
            };
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

        public ObservableCollection<ITool> Tools
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

        private void NewWorld()
        {
            NewWorldView nwDialog = new NewWorldView();
            if ((bool)nwDialog.ShowDialog())
            {
                _loadTimer.Reset();
                _loadTimer.Start();
                Task.Factory.StartNew(() =>
                                          {
                                              var w = nwDialog.NewWorld;
                                              w.SpawnX = w.TilesWide / 2;
                                              w.SpawnY = (int)Math.Max(0, w.GroundLevel - 10);
                                              w.GroundLevel = (int)w.GroundLevel;
                                              w.RockLevel = (int)w.RockLevel;
                                              w.BottomWorld = w.TilesHigh * 16;
                                              w.RightWorld = w.TilesWide * 16;
                                              w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
                                              Tile cloneTile = new Tile();
                                              for (int y = 0; y < w.TilesHigh; y++)
                                              {
                                                  OnProgressChanged(w, new ProgressChangedEventArgs(BCCL.Utility.Calc.ProgressPercentage(y, w.TilesHigh), "Generating World..."));

                                                  if (y == (int)w.GroundLevel - 10)
                                                      cloneTile = new Tile { HasWire = false, IsActive = true, IsLava = false, Liquid = 0, Type = 2, U = -1, V = -1, Wall = 2 };
                                                  if (y == (int)w.GroundLevel - 9)
                                                      cloneTile = new Tile { HasWire = false, IsActive = true, IsLava = false, Liquid = 0, Type = 0, U = -1, V = -1, Wall = 2 };
                                                  else if (y == (int)w.GroundLevel + 1)
                                                      cloneTile = new Tile { HasWire = false, IsActive = true, IsLava = false, Liquid = 0, Type = 0, U = -1, V = -1, Wall = 0 };
                                                  else if (y == (int)w.RockLevel)
                                                      cloneTile = new Tile { HasWire = false, IsActive = true, IsLava = false, Liquid = 0, Type = 1, U = -1, V = -1, Wall = 0 };
                                                  else if (y == w.TilesHigh - 182)
                                                      cloneTile = new Tile();
                                                  for (int x = 0; x < w.TilesWide; x++)
                                                  {
                                                      w.Tiles[x, y] = (Tile)cloneTile.Clone();
                                                  }
                                              }
                                              return w;
                                          })
                .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t =>
                                  {
                                      CurrentFile = null;
                                      PixelMap = t.Result;
                                      UpdateTitle();
                                      Points.Clear();
                                      Points.Add("Spawn");
                                      Points.Add("Dungeon");
                                      foreach (var npc in CurrentWorld.NPCs)
                                      {
                                          Points.Add(npc.Name);
                                      }
                                      _loadTimer.Stop();
                                      OnProgressChanged(this, new ProgressChangedEventArgs(0, string.Format("World loaded in {0} seconds.", _loadTimer.Elapsed.TotalSeconds)));
                                  }, TaskFactoryHelper.UiTaskScheduler);

            }
        }

        private void SaveWorld()
        {
            if (CurrentWorld == null)
                return;

            if (string.IsNullOrWhiteSpace(CurrentFile))
                SaveWorldAs();
            else
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

        Stopwatch _loadTimer = new Stopwatch();
        public void LoadWorld(string filename)
        {
            _loadTimer.Reset();
            _loadTimer.Start();
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
                    _loadTimer.Stop();
                    OnProgressChanged(this, new ProgressChangedEventArgs(0, string.Format("World loaded in {0} seconds.", _loadTimer.Elapsed.TotalSeconds)));
                }, TaskFactoryHelper.UiTaskScheduler);

        }
    }
}