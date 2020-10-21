using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TEdit.MvvmLight.Threading;
using TEdit.Geometry.Primitives;
using TEdit.Utility;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Plugins;
using TEdit.Editor.Tools;
using TEdit.Editor.Undo;
using TEdit.Properties;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.View.Popups;
using Application = System.Windows.Application;
using DispatcherHelper = GalaSoft.MvvmLight.Threading.DispatcherHelper;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.Linq;

namespace TEdit.ViewModel
{
    public partial class WorldViewModel : ViewModelBase
    {
        private readonly BrushSettings _brush = new BrushSettings();
        private readonly ClipboardManager _clipboard;
        private readonly Stopwatch _loadTimer = new Stopwatch();
        private readonly MouseTile _mouseOverTile = new MouseTile();
        private readonly ObservableCollection<IPlugin> _plugins = new ObservableCollection<IPlugin>();
        private readonly ObservableCollection<string> _points = new ObservableCollection<string>();
        private readonly Timer _saveTimer = new Timer();
        private readonly Selection _selection = new Selection();
        private readonly TilePicker _tilePicker = new TilePicker();
        private readonly ObservableCollection<ITool> _tools = new ObservableCollection<ITool>();
        private readonly UndoManager _undoManager;
        public bool[] CheckTiles;
        private ITool _activeTool;
        private bool _checkUpdates;
        private string _currentFile;
        private World _currentWorld;
        private bool _isAutoSaveEnabled = true;
        private ICommand _launchWikiCommand;
        private WriteableBitmap _minimapImage;
        private MorphBiome _morphBiomeTarget;
        private PixelMapManager _pixelMap;
        private ProgressChangedEventArgs _progress;
        private Chest _selectedChest;
        private Item _selectedChestItem;
        private string _selectedPoint;
        private Sign _selectedSign;
        private Sprite _selectedSprite;
        private KeyValuePair<int, SpriteSub> _selectedSprite2;
        private SpriteFull _selectedSpriteTile2;
        private Vector2Int32 _selectedXmas;
        private int _selectedXmasStar;
        private int _selectedXmasGarland;
        private int _selectedXmasBulb;
        private int _selectedXmasLight;

        private int _selectedTabIndex;
        private int _selectedSpecialTile = -1;
        private bool _showGrid = true;
        private bool _showLiquid = true;
        private bool _showPoints = true;
        private bool _showTextures = true;
        private bool _showTiles = true;
        private bool _showWalls = true;
        private bool _showActuators = true;
        private bool _showRedWires = true;
        private bool _showBlueWires = true;
        private bool _showGreenWires = true;
        private bool _showYellowWires = true;
        private string _spriteFilter;
        private ListCollectionView _spritesView;
        private ListCollectionView _spritesView2;
        private ListCollectionView _spriteStylesView;
        private ICommand _viewLogCommand;
        private string _windowTitle;
        private ICommand _checkUpdatesCommand;


        public ICommand CheckUpdatesCommand
        {
            get { return _checkUpdatesCommand ?? (_checkUpdatesCommand = new RelayCommand<bool>(CheckVersion)); }
        }

        static WorldViewModel()
        {
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
        }

        public WorldViewModel()
        {
            if (IsInDesignModeStatic)
            {
                return;
            }

            CheckUpdates = Settings.Default.CheckUpdates;

            if (CheckUpdates)
                CheckVersion();


            IsAutoSaveEnabled = Settings.Default.Autosave;

            _undoManager = new UndoManager(this);
            _clipboard = new ClipboardManager(this);
            World.ProgressChanged += OnProgressChanged;
            Brush.BrushChanged += OnPreviewChanged;
            UpdateTitle();

            _spriteFilter = string.Empty;
            _spritesView = (ListCollectionView)CollectionViewSource.GetDefaultView(World.Sprites);
            _spritesView.Filter = o =>
            {
                if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

                var sprite = (Sprite)o;

                string[] _spriteFilterSplit = _spriteFilter.Split('/');
                foreach (string _spriteWord in _spriteFilterSplit)
                {
                    if (sprite.TileName == _spriteWord) return true;
                    if (sprite.Name == _spriteWord) return true;
                    if (sprite.TileName != null && sprite.TileName.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                    if (sprite.Name != null && sprite.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }

                if (sprite.TileName == _spriteFilter) return true;
                if (sprite.Name == _spriteFilter) return true;
                if (sprite.TileName != null && sprite.TileName.IndexOf(_spriteFilter, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (sprite.Name != null && sprite.Name.IndexOf(_spriteFilter, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                return false;
            };

            _spritesView2 = (ListCollectionView)CollectionViewSource.GetDefaultView(World.Sprites2);
            _spritesView2.Filter = o =>
            {
                if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

                var sprite = (SpriteFull)o;

                string[] _spriteFilterSplit = _spriteFilter.Split(new char[] { '/', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (sprite.Tile.ToString().StartsWith(_spriteFilter)) return true;

                foreach (string _spriteWord in _spriteFilterSplit)
                {
                    if (sprite.Name == _spriteWord) return true;
                    if (sprite.Name != null && sprite.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                    foreach (var style in sprite.Styles)
                    {
                        if (style.Value.Name != null && style.Value.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                    }
                }
                return false;
            };

            UpdateSpriteStyleView();

            _saveTimer.AutoReset = true;
            _saveTimer.Elapsed += SaveTimerTick;
            // 3 minute save timer
            _saveTimer.Interval = 3 * 60 * 1000;

            _undoManager.Redid += UpdateMinimap;
            _undoManager.Undid += UpdateMinimap;
            _undoManager.UndoSaved += UpdateMinimap;

            // Test File Association and command line
            if (Application.Current.Properties["OpenFile"] != null)
            {
                string filename = Application.Current.Properties["OpenFile"].ToString();
                LoadWorld(filename);
            }
        }

        private void UpdateSpriteStyleView()
        {
            if (SelectedSpriteTile2 == null) { return; }
            SpriteStylesView = (ListCollectionView)CollectionViewSource.GetDefaultView(new ObservableCollection<KeyValuePair<int, SpriteSub>>(SelectedSpriteTile2.Styles.ToList()));
            SpriteStylesView.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
            SpriteStylesView.Filter = (o) =>
            {
                if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

                var sprite = (KeyValuePair<int, SpriteSub>)o;
                string[] _spriteFilterSplit = _spriteFilter.Split(new char[] { '/', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string _spriteWord in _spriteFilterSplit)
                {
                    if (sprite.Value.Name == _spriteWord) return true;
                    if (sprite.Value.Name != null && sprite.Value.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
                return false;
            };
        }

        public WriteableBitmap MinimapImage
        {
            get { return _minimapImage; }
            set { Set(nameof(MinimapImage), ref _minimapImage, value); }
        }

        public ListCollectionView SpritesView
        {
            get { return _spritesView; }
            set { Set(nameof(SpritesView), ref _spritesView, value); }
        }

        public ListCollectionView SpritesView2
        {
            get { return _spritesView2; }
            set { Set(nameof(SpritesView), ref _spritesView2, value); }
        }

        public ListCollectionView SpriteStylesView
        {
            get { return _spriteStylesView; }
            set { Set(nameof(SpriteStylesView), ref _spriteStylesView, value); }
        }

        public string SpriteFilter
        {
            get { return _spriteFilter; }
            set
            {
                Set(nameof(SpriteFilter), ref _spriteFilter, value);

                SpritesView2.Refresh();
                SpriteStylesView?.Refresh();
            }
        }


        public ICommand LaunchWikiCommand
        {
            get { return _launchWikiCommand ?? (_launchWikiCommand = new RelayCommand(() => LaunchUrl("http://github.com/BinaryConstruct/Terraria-Map-Editor/wiki"))); }
        }

        /* SBLogic - catch exception if browser can't be launched */
        public static void LaunchUrl(string url)
        {
            System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.None;
            try
            {
                Process.Start(url);
            }
            catch
            {
                result = System.Windows.Forms.MessageBox.Show("Unable to open external browser.  Copy to clipboard?", "Link Error", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Exclamation);
            }

            // Just in case
            try
            {
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Windows.Clipboard.SetText(url);
                }
            }
            catch { }
        }


        public static string TempPath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TEdit"); }
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set(nameof(SelectedTabIndex), ref _selectedTabIndex, value); }
        }

        public int SelectedSpecialTile
        {
            get { return _selectedSpecialTile; }
            set { Set(nameof(SelectedSpecialTile), ref _selectedSpecialTile, value); }
        }

        public Vector2Int32 SelectedXmas
        {
            get { return _selectedXmas; }
            set
            {
                Set(nameof(SelectedXmas), ref _selectedXmas, value);
                SelectedTabIndex = 1;
                SelectedSpecialTile = 10;
            }
        }

        public int SelectedXmasStar
        {
            get { return _selectedXmasStar; }
            set { Set(nameof(SelectedXmasStar), ref _selectedXmasStar, value); }
        }

        public int SelectedXmasGarland
        {
            get { return _selectedXmasGarland; }
            set { Set(nameof(SelectedXmasGarland), ref _selectedXmasGarland, value); }
        }

        public int SelectedXmasBulb
        {
            get { return _selectedXmasBulb; }
            set { Set(nameof(SelectedXmasBulb), ref _selectedXmasBulb, value); }
        }

        public int SelectedXmasLight
        {
            get { return _selectedXmasLight; }
            set { Set(nameof(SelectedXmasLight), ref _selectedXmasLight, value); }
        }

        public Sign SelectedSign
        {
            get { return _selectedSign; }
            set
            {
                Set(nameof(SelectedSign), ref _selectedSign, value);
                SelectedTabIndex = 1;
                SelectedSpecialTile = 11;
            }
        }

        public Chest SelectedChest
        {
            get { return _selectedChest; }
            set
            {
                Set(nameof(SelectedChest), ref _selectedChest, value);
                SelectedTabIndex = 1;
                SelectedSpecialTile = 12;
            }
        }

        public TileEntity SelectedTileEntity
        {
            get { return _selectedTileEntity; }
            set
            {
                Set(nameof(SelectedTileEntity), ref _selectedTileEntity, value);
                SelectedTabIndex = 1;
                SelectedSpecialTile = (int)value?.EntityType;
            }
        }

        public ObservableCollection<IPlugin> Plugins
        {
            get { return _plugins; }
        }

        public MorphBiome MorphBiomeTarget
        {
            get { return _morphBiomeTarget; }
            set { Set(nameof(MorphBiomeTarget), ref _morphBiomeTarget, value); }
        }

        public bool IsAutoSaveEnabled
        {
            get { return _isAutoSaveEnabled; }
            set
            {
                Set(nameof(IsAutoSaveEnabled), ref _isAutoSaveEnabled, value);
                Settings.Default.Autosave = _isAutoSaveEnabled;
                Settings.Default.Save();
            }
        }

        public bool ShowGrid
        {
            get { return _showGrid; }
            set { Set(nameof(ShowGrid), ref _showGrid, value); }
        }

        public bool ShowTextures
        {
            get { return _showTextures; }
            set { Set(nameof(ShowTextures), ref _showTextures, value); }
        }

        public KeyValuePair<int, SpriteSub> SelectedSprite2
        {
            get { return _selectedSprite2; }
            set
            {
                Set("SelectedSprite2", ref _selectedSprite2, value);
                PreviewChange();
            }
        }

        public SpriteFull SelectedSpriteTile2
        {
            get { return _selectedSpriteTile2; }
            set
            {
                Set("SelectedSpriteTile2", ref _selectedSpriteTile2, value);
                UpdateSpriteStyleView();
                if (value?.Styles != null && value.Styles.Count > 0)
                {
                    SelectedSprite2 = value.Styles.First();
                }
                PreviewChange();
            }
        }

        public Sprite SelectedSprite
        {
            get { return _selectedSprite; }
            set
            {
                Set(nameof(SelectedSprite), ref _selectedSprite, value);
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
            set { Set(nameof(SelectedPoint), ref _selectedPoint, value); }
        }


        public Item SelectedChestItem
        {
            get { return _selectedChestItem; }
            set { Set(nameof(SelectedChestItem), ref _selectedChestItem, value); }
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
            set { Set(nameof(CurrentFile), ref _currentFile, value); }
        }

        public World CurrentWorld
        {
            get { return _currentWorld; }
            set { Set(nameof(CurrentWorld), ref _currentWorld, value); }
        }

        public ProgressChangedEventArgs Progress
        {
            get { return _progress; }
            set { Set(nameof(Progress), ref _progress, value); }
        }

        public string WindowTitle
        {
            get { return _windowTitle; }
            set { Set(nameof(WindowTitle), ref _windowTitle, value); }
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
            set { Set(nameof(ActiveTool), ref _activeTool, value); }
        }

        public PixelMapManager PixelMap
        {
            get { return _pixelMap; }
            set { Set(nameof(PixelMap), ref _pixelMap, value); }
        }

        public bool ShowRedWires
        {
            get { return _showRedWires; }
            set
            {
                Set(nameof(ShowRedWires), ref _showRedWires, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowBlueWires
        {
            get { return _showBlueWires; }
            set
            {
                Set(nameof(ShowBlueWires), ref _showBlueWires, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowGreenWires
        {
            get { return _showGreenWires; }
            set
            {
                Set(nameof(ShowGreenWires), ref _showGreenWires, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowYellowWires
        {
            get { return _showYellowWires; }
            set
            {
                Set(nameof(ShowYellowWires), ref _showYellowWires, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowActuators
        {
            get { return _showActuators; }
            set
            {
                Set(nameof(ShowActuators), ref _showActuators, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowPoints
        {
            get { return _showPoints; }
            set { Set(nameof(ShowPoints), ref _showPoints, value); }
        }

        public bool ShowLiquid
        {
            get { return _showLiquid; }
            set
            {
                Set(nameof(ShowLiquid), ref _showLiquid, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowTiles
        {
            get { return _showTiles; }
            set
            {
                Set(nameof(ShowTiles), ref _showTiles, value);
                UpdateRenderWorld();
            }
        }

        public bool ShowWalls
        {
            get { return _showWalls; }
            set
            {
                Set(nameof(ShowWalls), ref _showWalls, value);
                UpdateRenderWorld();
            }
        }


        public ICommand ViewLogCommand
        {
            get { return _viewLogCommand ?? (_viewLogCommand = new RelayCommand(ViewLog)); }
        }

        public bool CheckUpdates
        {
            get { return _checkUpdates; }
            set
            {
                Set(nameof(CheckUpdates), ref _checkUpdates, value);
                Settings.Default.CheckUpdates = value;
                Settings.Default.Save();
            }
        }

        private bool _enableTelemetry = Settings.Default.Telemetry != 0;

        public bool EnableTelemetry
        {
            get { return _enableTelemetry; }
            set
            {
                RaisePropertyChanged();
                Set(nameof(EnableTelemetry), ref _enableTelemetry, value);
                Settings.Default.Telemetry = value ? 1 : 0;
                Settings.Default.Save();
                ErrorLogging.InitializeTelemetry();
            }
        }

        private void UpdateMinimap(object sender, EventArgs eventArgs)
        {
            if (CurrentWorld != null)
            {
                if (MinimapImage != null)
                    RenderMiniMap.UpdateMinimap(CurrentWorld, ref _minimapImage);
            }
        }

        private void SaveTimerTick(object sender, ElapsedEventArgs e)
        {
            if (IsAutoSaveEnabled)
            {
                if (!string.IsNullOrWhiteSpace(CurrentFile))
                    SaveWorldThreaded(Path.Combine(TempPath, Path.GetFileNameWithoutExtension(CurrentFile) + ".autosave"));
                else
                    SaveWorldThreaded(Path.Combine(TempPath, "newworld.autosave"));
            }
        }

        private void ViewLog()
        {
            ErrorLogging.ViewLog();
        }

        private void UpdateTitle()
        {


            WindowTitle =
                $"TEdit v{App.Version} {Path.GetFileName(_currentFile)}";
        }

        public async void CheckVersion(bool auto = true)
        {
            bool isoutofdate = false;

            const string versionRegex = @"""tag_name"":\s?""(?<version>[^\""]*)""";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0");
                    string githubReleases = await client.GetStringAsync("https://api.github.com/repos/TEdit/Terraria-map-Editor/releases");
                    var versions = Regex.Match(githubReleases, versionRegex);

                    isoutofdate = versions.Success && (App.Version != versions?.Groups?[1].Value);

                    // ignore revision, build should be enough
                    // if ((revis != -1) && (revis > App.Version.ProductPrivatePart)) return true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to check version.", "Update Check Failed");
            }


            if (isoutofdate && MessageBox.Show("You are using an outdated version of TEdit. Do you wish to download the update?", "Update?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start("http://www.binaryconstruct.com/downloads/");
                }
                catch { }

            }
            else if (!auto)
            {
                MessageBox.Show("TEdit is up to date.", "Update");
            }

        }

        private ICommand _analyzeWorldCommand;
        private ICommand _analyzeWorldSaveCommand;
        private ICommand _tallyCountCommand;

        /// <summary>
        /// Relay command to execute AnalyzeWorldSave.
        /// </summary>
        public ICommand AnalyzeWorldSaveCommand
        {
            get { return _analyzeWorldSaveCommand ?? (_analyzeWorldSaveCommand = new RelayCommand(AnalyzeWorldSave)); }
        }

        private void AnalyzeWorldSave()
        {
            if (CurrentWorld == null) return;
            var sfd = new SaveFileDialog();
            sfd.DefaultExt = "Text File|*.txt";
            sfd.Filter = "Text Files|*.txt";
            sfd.FileName = CurrentWorld.Title + " Analysis.txt";
            sfd.Title = "Save world analysis.";
            sfd.OverwritePrompt = true;
            if (sfd.ShowDialog() == true)
            {
                TEdit.Terraria.WorldAnalysis.AnalyzeWorld(CurrentWorld, sfd.FileName);

            }
        }

        /// <summary>
        /// Relay command to execute AnalizeWorld.
        /// </summary>
        public ICommand AnalyzeWorldCommand
        {
            get { return _analyzeWorldCommand ?? (_analyzeWorldCommand = new RelayCommand(AnalyzeWorld)); }
        }

        private void AnalyzeWorld()
        {
            WorldAnalysis = TEdit.Terraria.WorldAnalysis.AnalyzeWorld(CurrentWorld);
        }

        private string _worldAnalysis;


        public string WorldAnalysis
        {
            get { return _worldAnalysis; }
            set { Set(nameof(WorldAnalysis), ref _worldAnalysis, value); }
        }

        /* SBLogic - Relay command to execute KillTally */

        public ICommand LoadTallyCommand
        {
            get { return _tallyCountCommand ?? (_tallyCountCommand = new RelayCommand(GetTallyCount)); }
        }

        private void GetTallyCount()
        {
            TallyCount = KillTally.LoadTally(CurrentWorld);
        }

        private string _tallyCount;
        private TileEntity _selectedTileEntity;

        public string TallyCount
        {
            get { return _tallyCount; }
            set { Set(nameof(TallyCount), ref _tallyCount, value); }
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

                if (tool.Name.StartsWith("Sprite"))
                {
                    SelectedTabIndex = 2;
                }

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
            if (e.Location.X >= 0 && e.Location.Y >= 0 && e.Location.X < CurrentWorld.TilesWide && e.Location.Y < CurrentWorld.TilesHigh)
            {
                if (e.Location != MouseOverTile.MouseState.Location)
                    MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

                MouseOverTile.MouseState = e;

                ActiveTool.MouseMove(e);
            }
        }

        private void OpenWorld()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Terraria World File|*.wld|Terraria World Backup|*.bak|TEdit Backup File|*.TEdit";
            ofd.DefaultExt = "Terraria World File|*.wld";
            ofd.Title = "Load Terraria World File";
            ofd.InitialDirectory = DependencyChecker.PathToWorlds;
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                LoadWorld(ofd.FileName);
            }
        }

        private void NewWorld()
        {
            var nwDialog = new NewWorldView();
            if ((bool)nwDialog.ShowDialog())
            {
                _loadTimer.Reset();
                _loadTimer.Start();
                _saveTimer.Stop();
                Task.Factory.StartNew(() =>
                {
                    World w = nwDialog.NewWorld;
                    w.Seed = (new Random()).Next(0, int.MaxValue).ToString();
                    w.SpawnX = w.TilesWide / 2;
                    w.SpawnY = (int)Math.Max(0, w.GroundLevel - 10);
                    w.GroundLevel = (int)w.GroundLevel;
                    w.RockLevel = (int)w.RockLevel;
                    w.BottomWorld = w.TilesHigh * 16;
                    w.RightWorld = w.TilesWide * 16;
                    w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
                    var cloneTile = new Tile();
                    for (int y = 0; y < w.TilesHigh; y++)
                    {
                        OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, w.TilesHigh), "Generating World..."));

                        if (y == (int)w.GroundLevel - 10)
                            cloneTile = new Tile { WireRed = false, IsActive = true, LiquidType = LiquidType.None, LiquidAmount = 0, Type = 2, U = -1, V = -1, Wall = 2 };
                        if (y == (int)w.GroundLevel - 9)
                            cloneTile = new Tile { WireRed = false, IsActive = true, LiquidType = LiquidType.None, LiquidAmount = 0, Type = 0, U = -1, V = -1, Wall = 2 };
                        else if (y == (int)w.GroundLevel + 1)
                            cloneTile = new Tile { WireRed = false, IsActive = true, LiquidType = LiquidType.None, LiquidAmount = 0, Type = 0, U = -1, V = -1, Wall = 0 };
                        else if (y == (int)w.RockLevel)
                            cloneTile = new Tile { WireRed = false, IsActive = true, LiquidType = LiquidType.None, LiquidAmount = 0, Type = 1, U = -1, V = -1, Wall = 0 };
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
                        foreach (NPC npc in CurrentWorld.NPCs)
                        {
                            Points.Add(npc.Name);
                        }
                        MinimapImage = RenderMiniMap.Render(CurrentWorld);
                        _loadTimer.Stop();
                        OnProgressChanged(this, new ProgressChangedEventArgs(0,
                            $"World loaded in {_loadTimer.Elapsed.TotalSeconds} seconds."));
                        _saveTimer.Start();
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
            sfd.InitialDirectory = DependencyChecker.PathToWorlds;
            if ((bool)sfd.ShowDialog())
            {
                CurrentFile = sfd.FileName;
                SaveWorldFile();
            }
        }

        private void SaveWorldFile()
        {
            if (CurrentWorld.LastSave < File.GetLastWriteTimeUtc(CurrentFile))
            {
                MessageBoxResult overwrite = MessageBox.Show(_currentWorld.Title + " was externally modified since your last save.\r\nDo you wish to overwrite?", "World Modified", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (overwrite.Equals(MessageBoxResult.Cancel))
                    return;
            }
            SaveWorldThreaded(CurrentFile);
        }

        private void SaveWorldThreaded(string filename)
        {
            Task.Factory.StartNew(() => World.Save(CurrentWorld, filename))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
        }

        public void LoadWorld(string filename)
        {
            _loadTimer.Reset();
            _loadTimer.Start();
            _saveTimer.Stop();
            CurrentFile = filename;
            CurrentWorld = null;
            GC.WaitForFullGCComplete();

            Task.Factory.StartNew(() => World.LoadWorld(filename))
                .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t =>
                {
                    try
                    {
                        if (CurrentWorld != null)
                        {
                            ErrorLogging.TelemetryClient.TrackEvent(nameof(LoadWorld));

                            PixelMap = t.Result;
                            UpdateTitle();
                            Points.Clear();
                            Points.Add("Spawn");
                            Points.Add("Dungeon");
                            foreach (NPC npc in CurrentWorld.NPCs)
                            {
                                Points.Add(npc.Name);
                            }
                            MinimapImage = RenderMiniMap.Render(CurrentWorld);
                            _loadTimer.Stop();
                            OnProgressChanged(this, new ProgressChangedEventArgs(0,
                                $"World loaded in {_loadTimer.Elapsed.TotalSeconds} seconds."));
                            _saveTimer.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogging.LogException(ex);
                    }
                    finally
                    {

                        _loadTimer.Stop();
                    }

                }, TaskFactoryHelper.UiTaskScheduler);
        }
    }
}
