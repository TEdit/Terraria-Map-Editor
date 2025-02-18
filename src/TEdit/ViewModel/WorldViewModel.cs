using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Common.Reactive;
using TEdit.Common.Reactive.Command;
using TEdit.Configuration;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Plugins;
using TEdit.Editor.Tools;
using TEdit.Editor.Undo;
using TEdit.Framework.Threading;
using TEdit.Geometry;
using TEdit.Properties;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.UI;
using TEdit.UI.Xaml;
using TEdit.Utility;
using TEdit.View.Popups;
using static TEdit.Terraria.CreativePowers;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Timer = System.Timers.Timer;

namespace TEdit.ViewModel;



public partial class WorldViewModel : ViewModelBase
{
    private readonly BrushSettings _brush = new BrushSettings();
    private readonly Stopwatch _loadTimer = new Stopwatch();
    private readonly MouseTile _mouseOverTile = new MouseTile();
    private readonly ObservableCollection<IPlugin> _plugins = new ObservableCollection<IPlugin>();
    private readonly ObservableCollection<string> _points = new ObservableCollection<string>();
    private readonly Timer _saveTimer = new Timer();
    private readonly Selection _selection = new Selection();
    private readonly MorphToolOptions _MorphToolOptions = new MorphToolOptions();
    private readonly ObservableCollection<ITool> _tools = new ObservableCollection<ITool>();
    private UndoManager _undoManager;
    public bool[] CheckTiles;
    private ITool _activeTool;
    private bool _checkUpdates;
    private string _currentFile;
    public static World _currentWorld;
    private ClipboardManager _clipboard;
    private bool _isAutoSaveEnabled = true;
    private ICommand _launchWikiCommand;
    private WriteableBitmap _minimapImage;
    private string _morphBiomeTarget;
    private PixelMapManager _pixelMap;
    private ProgressChangedEventArgs _progress;
    private Chest _selectedChest;
    private Item _selectedChestItem;
    private string _selectedPoint;
    private Sign _selectedSign;
    private SpriteItemPreview _selectedSpriteItem;
    private SpriteSheet _selectedSpriteSheet;
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
    private bool _showCoatings = true;
    private bool _showWalls = true;
    private bool _showActuators = true;
    private bool _showRedWires = true;
    private bool _showBlueWires = true;
    private bool _showGreenWires = true;
    private bool _showYellowWires = true;
    private bool _showAllWires = true;
    private bool _showWireTransparency = true;
    private string _spriteFilter;
    private ushort _spriteTileFilter;
    private ListCollectionView _spriteSheetView;
    private ListCollectionView _spriteStylesView;
    private ICommand _viewLogCommand;
    private ICommand _showNewsCommand;
    private string _windowTitle;
    private ICommand _checkUpdatesCommand;




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




        World.ProgressChanged += OnProgressChanged;
        Brush.BrushChanged += OnPreviewChanged;
        UpdateTitle();

        InitSpriteViews();

        _saveTimer.AutoReset = true;
        _saveTimer.Elapsed += SaveTimerTick;
        // 3 minute save timer
        _saveTimer.Interval = 3 * 60 * 1000;

        // Test File Association and command line
        if (Application.Current.Properties["OpenFile"] != null)
        {
            string filename = Application.Current.Properties["OpenFile"].ToString();
            LoadWorld(filename);
        }
    }

    public void InitSpriteViews()
    {
        _spriteSheetView = (ListCollectionView)CollectionViewSource.GetDefaultView(WorldConfiguration.Sprites2);
        _spriteSheetView.Filter = o =>
        {

            if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

            var sprite = (SpriteSheet)o;


            string[] _spriteFilterSplit = _spriteFilter.Split(new char[] { '/', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (sprite.Tile.ToString().StartsWith(_spriteFilter)) return true;

            foreach (string _spriteWord in _spriteFilterSplit)
            {
                if (sprite.Name == _spriteWord) return true;
                if (sprite.Name != null && sprite.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                foreach (var style in sprite.Styles)
                {
                    if (style.Name != null && style.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
            }
            return false;
        };

        _spriteStylesView = (ListCollectionView)CollectionViewSource.GetDefaultView(new ObservableCollection<SpriteItemPreview>(WorldConfiguration.Sprites2.SelectMany(s => s.Styles).Select(s => (SpriteItemPreview)s).ToList()));
        _spriteStylesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        _spriteStylesView.Filter = (o) =>
        {
            var sprite = (SpriteItemPreview)o;

            if (_spriteTileFilter <= 0 && string.IsNullOrWhiteSpace(_spriteFilter)) { return false; }

            if (_spriteTileFilter > 0 && sprite.Tile != _spriteTileFilter) return false;
            if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

            string[] _spriteFilterSplit = _spriteFilter.Split(new char[] { '/', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string _spriteWord in _spriteFilterSplit)
            {
                if (sprite.Name == _spriteWord) return true;
                if (sprite.Name != null && sprite.Name.IndexOf(_spriteWord, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        };
    }

    public WriteableBitmap MinimapImage
    {
        get { return _minimapImage; }
        set { Set(nameof(MinimapImage), ref _minimapImage, value); }
    }

    public ListCollectionView SpriteSheetView => _spriteSheetView;

    public ListCollectionView SpriteStylesView => _spriteStylesView;

    public string SpriteFilter
    {
        get { return _spriteFilter; }
        set
        {
            Set(nameof(SpriteFilter), ref _spriteFilter, value);
            SpriteSheetView.Refresh();
            SpriteStylesView.Refresh();
        }
    }

    public ushort SpriteTileFilter
    {
        get { return _spriteTileFilter; }
        set
        {
            Set(nameof(SpriteFilter), ref _spriteTileFilter, value);
            SpriteStylesView.Refresh();
        }
    }

    public SpriteItemPreview SelectedSpriteItem
    {
        get { return _selectedSpriteItem; }
        set
        {
            Set("SelectedSpriteItem", ref _selectedSpriteItem, value);
            PreviewChange();
        }
    }

    public SpriteSheet SelectedSpriteSheet
    {
        get { return _selectedSpriteSheet; }
        set
        {
            Set("SelectedSpriteSheet", ref _selectedSpriteSheet, value);

            if (value == null) { SpriteTileFilter = 0; }
            else { SpriteTileFilter = value.Tile; }

            if (value?.Styles != null && value.Styles.Count > 0)
            {
                SelectedSpriteItem = value.Styles.First() as SpriteItemPreview;
            }

            if (ActiveTool is not SpriteTool2)
            {
                SetActiveTool(Tools.FirstOrDefault(t => t is SpriteTool2));
            }

            PreviewChange();
        }
    }


    public ICommand LaunchWikiCommand
    {
        get { return _launchWikiCommand ??= new RelayCommand(() => LaunchUrl("http://github.com/BinaryConstruct/Terraria-Map-Editor/wiki")); }
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

    public string MorphBiomeTarget
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
            try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }
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
        set { Set(nameof(Clipboard), ref _clipboard, value); }
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

    private ClipboardManager _clipboardManager;
    public World CurrentWorld
    {
        get { return _currentWorld; }
        set
        {
            Set(nameof(CurrentWorld), ref _currentWorld, value);

            if (value != null)
            {

                WorldEditor?.Dispose();

                var rb = new RenderBlender(CurrentWorld, TilePicker);

                NotifyTileChanged updateTiles = (x, y, width, height) =>
                {
                    UpdateRenderPixel(x, y);
                    //UpdateRenderRegion(new RectangleInt32(x, y, width, height));
                    rb.UpdateTile(x, y, width, height);
                };

                _undoManager = new UndoManager(CurrentWorld, updateTiles, UpdateMinimap);

                var undo = new UndoManagerWrapper(UndoManager);

                // Preserve clipboard between world loads.
                if (_clipboardManager == null)
                {
                    Clipboard = new ClipboardManager(Selection, undo, rb.UpdateTile);
                }
                else
                {
                    Clipboard = new ClipboardManager(Selection, undo, rb.UpdateTile);

                    // Add all previously loaded buffers to the new clipboard.
                    foreach (var buffer in _clipboardManager.LoadedBuffers)
                    {
                        Clipboard.LoadedBuffers.Add(buffer);
                    }
                
                    // Preserve the current active buffer if it's not null.
                    if (_clipboardManager.Buffer != null)
                    {
                        Clipboard.Buffer = _clipboardManager.Buffer;
                    }
                }
                _clipboardManager = Clipboard;

                WorldEditor = new WorldEditor(TilePicker, CurrentWorld, Selection, undo, updateTiles);
            }
            else
            {
                WorldEditor?.Dispose();
                WorldEditor = null;
            }
        }
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

    public MorphToolOptions MorphToolOptions => _MorphToolOptions;

    public TilePicker TilePicker { get; } = new TilePicker();

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
    
    public bool ShowAllWires
    {
        get { return _showAllWires; }
        set
        {
            Set(nameof(ShowAllWires), ref _showAllWires, value);
            ToggleWireStates(_showAllWires);
            UpdateRenderWorld();
        }
    }
    
    public void ToggleWireStates(bool state)
    {
        ShowRedWires = state;
        ShowBlueWires = state;
        ShowGreenWires = state;
        ShowYellowWires = state;
    }
    
    public bool ShowWireTransparency
    {
        get { return _showWireTransparency; }
        set
        {
            Set(nameof(ShowWireTransparency), ref _showWireTransparency, value);
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

    public bool ShowCoatings
    {
        get { return _showCoatings; }
        set
        {
            Set(nameof(ShowCoatings), ref _showCoatings, value);
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
    public ICommand ShowNewsCommand
    {
        get { return _showNewsCommand ??= new RelayCommand(ShowNewsDialog); }
    }

    public ICommand CheckUpdatesCommand
    {
        get { return _checkUpdatesCommand ??= new RelayCommand(async () => await CheckVersion(false)); }
    }

    public ICommand ViewLogCommand
    {
        get { return _viewLogCommand ??= new RelayCommand(ViewLog); }
    }

    public bool RealisticColors
    {
        get { return Settings.Default.RealisticColors; }
        set
        {
            RaisePropertyChanged(nameof(RealisticColors), Settings.Default.RealisticColors, value);
            Settings.Default.RealisticColors = value;
            try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }
            MessageBox.Show(Properties.Language.messagebox_restartrequired, Properties.Language.messagebox_restartrequired, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public bool CheckUpdates
    {
        get { return _checkUpdates; }
        set
        {
            Set(nameof(CheckUpdates), ref _checkUpdates, value);
            Settings.Default.CheckUpdates = value;
            try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }
        }
    }

    public float _textureVisibilityZoomLevel = Settings.Default.TextureVisibilityZoomLevel;
    public float TextureVisibilityZoomLevel
    {
        get => _textureVisibilityZoomLevel;
        set
        {
            value = (float)Math.Floor(MathHelper.Clamp(value, 3, 64));
            if (Set(nameof(TextureVisibilityZoomLevel), ref _textureVisibilityZoomLevel, value))
            {
                Settings.Default.TextureVisibilityZoomLevel = value;
                try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }
            }
        }
    }

    private bool _showNews = Settings.Default.ShowNews;

    public bool ShowNews
    {
        get { return _showNews; }
        set
        {
            if (Set(nameof(EnableTelemetry), ref _showNews, value))
            {
                Settings.Default.ShowNews = value;
                try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }
            }
        }
    }


    private bool _enableTelemetry = Settings.Default.Telemetry != 0;

    public bool EnableTelemetry
    {
        get { return _enableTelemetry; }
        set
        {
            if (Set(nameof(EnableTelemetry), ref _enableTelemetry, value))
            {
                Settings.Default.Telemetry = value ? 1 : 0;
                try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }
                ErrorLogging.InitializeTelemetry();
            }
        }
    }

    private void UpdateMinimap()
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

    private void ShowNewsDialog()
    {
        var w = new NotificationsWindow();
        w.Owner = Application.Current.MainWindow;
        w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        w.ShowDialog();
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

    public async Task CheckVersion(bool auto = true)
    {
        bool isOutdated = false;

        const string versionRegex = @"""tag_name"":\s?""(?<version>[^\""]*)""";
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0");
                string githubReleases = await client.GetStringAsync("https://api.github.com/repos/TEdit/Terraria-map-Editor/releases");
                var versions = Regex.Match(githubReleases, versionRegex);

                var githubVersion = Semver.SemVersion.Parse(versions?.Groups?[1].Value, Semver.SemVersionStyles.Any);
                var appVersion = App.Version;

                isOutdated = appVersion.ComparePrecedenceTo(githubVersion) < 0;
            }
        }
        catch (Exception)
        {
            MessageBox.Show("Unable to check version.", "Update Check Failed");
        }



        if (isOutdated)
        {
#if !DEBUG
            if (MessageBox.Show("You are using an outdated version of TEdit. Do you wish to download the update?", "Update?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start("http://www.binaryconstruct.com/downloads/");
                }
                catch { }
            }
#else
            MessageBox.Show("This is a debug build, version checking disabled.", "Update");
#endif

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
        get { return _analyzeWorldSaveCommand ??= new RelayCommand(AnalyzeWorldSave); }
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
            Editor.WorldAnalysis.AnalyzeWorld(CurrentWorld, sfd.FileName);

        }
    }

    /// <summary>
    /// Relay command to execute AnalizeWorld.
    /// </summary>
    public ICommand AnalyzeWorldCommand
    {
        get { return _analyzeWorldCommand ??= new RelayCommand(AnalyzeWorld); }
    }

    private void AnalyzeWorld()
    {
        WorldAnalysis = Editor.WorldAnalysis.AnalyzeWorld(CurrentWorld);
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
        get { return _tallyCountCommand ??= new RelayCommand(GetTallyCount); }
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
        if (CurrentWorld == null) return;

        if (e.Location != MouseOverTile.MouseState.Location)
            MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

        MouseOverTile.MouseState = e;
        ActiveTool?.MouseDown(e);

        CommandManager.InvalidateRequerySuggested();
    }

    public void MouseUpTile(TileMouseState e)
    {
        if (CurrentWorld == null) return;

        if (e.Location != MouseOverTile.MouseState.Location)
            MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

        MouseOverTile.MouseState = e;
        ActiveTool?.MouseUp(e);
        CommandManager.InvalidateRequerySuggested();
    }

    public void MouseMoveTile(TileMouseState e)
    {
        if (CurrentWorld == null) return;

        if (e.Location.X >= 0 && e.Location.Y >= 0 && e.Location.X < CurrentWorld.TilesWide && e.Location.Y < CurrentWorld.TilesHigh)
        {
            if (e.Location != MouseOverTile.MouseState.Location)
                MouseOverTile.Tile = CurrentWorld.Tiles[e.Location.X, e.Location.Y];

            MouseOverTile.MouseState = e;

            ActiveTool?.MouseMove(e);
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
        // Define the bool for prompting ore generation plugin
        bool generateOres = false;

        // Open the dialog for creating a new world and check if user clicked 'OK'
        var nwDialog = new NewWorldView();
        if ((bool)nwDialog.ShowDialog())
        {
            // Reset and start the load timer for performance tracking
            _loadTimer.Reset();
            _loadTimer.Start();
            _saveTimer.Stop(); // Stop the save timer as we are generating a new world

            // Start a new task for world generation
            Task.Factory.StartNew(() =>
            {
                // Retrieve the new world settings from the dialog
                World w = nwDialog.NewWorld;

                // Report progress at the beginning of world generation
                OnProgressChanged(w, new ProgressChangedEventArgs(100, "Generating World..."));

                // Generate a new seed if not provided
                w.Seed = w.Seed;
                if (string.IsNullOrEmpty(w.Seed))
                {
                    w.Seed = (new Random()).Next(0, int.MaxValue).ToString(); // Generate a random seed
                }

                // Initialize random number generator with the seed's hash code
                Random rand = new(w.Seed.GetHashCode());
                PerlinNoise perlinNoise = new(rand.Next()); // Initialize Perlin noise with a random value

                // Set world properties
                w.SpawnX = (int)(w.TilesWide / 2); // Center spawn position horizontally
                w.SpawnY = (int)Math.Max(0, w.GroundLevel - 10); // Set spawn position vertically with a slight offset
                w.GroundLevel = (int)w.GroundLevel; // Ensure ground level is an integer
                w.RockLevel = (int)w.RockLevel; // Ensure rock level is an integer
                w.BottomWorld = w.TilesHigh * 16; // Calculate the bottom of the world
                w.RightWorld = w.TilesWide * 16; // Calculate the right edge of the world
                w.Tiles = new Tile[w.TilesWide, w.TilesHigh]; // Initialize the world tile array

                // Extract custom settings for world generation
                int hillSize = (int)w.HillSize;
                bool generateGrass = w.GenerateGrass;
                bool generateWalls = w.GenerateWalls;
                bool generateCaves = w.GenerateCaves;
                int cavePreset = w.CavePresetIndex;
                bool surfaceCaves = w.SurfaceCaves;
                double caveNoise = w.CaveNoise;
                double caveMultiplier = w.CaveMultiplier;
                double caveDensity = w.CaveDensity;
                bool generateUnderworld = w.GenerateUnderworld;
                bool generateAsh = w.GenerateAsh;
                bool generateLava = w.GenerateLava;
                double underworldRoofNoise = w.UnderworldRoofNoise;
                double underworldFloorNoise = w.UnderworldFloorNoise;
                double underworldLavaNoise = w.UnderworldLavaNoise;
                generateOres = w.GenerateOres;

                // Generate hills in the world
                GenerateHills(w, perlinNoise, generateUnderworld, generateWalls);

                // Cleanup generation: Add grass to the top layers and remove exposed surface walls
                // if (generateGrass || generateWalls)
                CleanupGeneration(w, generateGrass, generateWalls);

                // Generate caves in the world
                if (generateCaves)
                    GenerateCaves(w, rand, caveNoise, caveMultiplier, caveDensity, surfaceCaves, generateUnderworld, generateGrass, generateWalls);

                // Generate underworld features if specified
                if (generateUnderworld && generateAsh)
                    GenerateUnderworld(w, rand, generateLava, underworldRoofNoise, underworldFloorNoise, underworldLavaNoise);

                return w; // Return the generated world
            })
            .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler) // Set the current world after generation
            .ContinueWith(t => RenderEntireWorld()) // Render the entire world after setting it
            .ContinueWith(t =>
            {
                // Update UI elements after world generation and rendering
                CurrentFile = null;
                PixelMap = t.Result; // Set the pixel map for the world
                UpdateTitle(); // Update the window title with the current world name
                Points.Clear(); // Clear previous points
                Points.Add("Spawn"); // Add default points to the list
                Points.Add("Dungeon");
                foreach (NPC npc in CurrentWorld.NPCs)
                {
                    Points.Add(npc.Name); // Add NPC names to points
                }
                MinimapImage = RenderMiniMap.Render(CurrentWorld); // Render and set the minimap image
                _loadTimer.Stop(); // Stop the load timer
                OnProgressChanged(this, new ProgressChangedEventArgs(0, $"World loaded in {_loadTimer.Elapsed.TotalSeconds} seconds.")); // Report completion
                _saveTimer.Start(); // Restart the save timer
            }, TaskFactoryHelper.UiTaskScheduler)
            .ContinueWith(t =>
            {
                // Launch the ore generation plugin after completion
                if (generateOres)
                {
                    // OnProgressChanged(this, new ProgressChangedEventArgs(0, "Launched OreGen Plugin")); // Report launch completion

                    var orePlugin = new SimpleOreGeneratorPlugin(this);
                    orePlugin.Execute();
                }
            }, TaskFactoryHelper.UiTaskScheduler); // Ensure UI updates are performed on the UI thread
        }
    }

    #region Hill Generation

    private void GenerateHills(World w, PerlinNoise perlinNoise, bool generateUnderworld, bool generateWalls)
    {
        // Hill generation
        for (int y = 0; y < w.TilesHigh; y++) // Iterate through all y levels
        {
            OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, w.TilesHigh), "Generating Hills..."));

            for (int x = 0; x < w.TilesWide; x++)
            {
                double hillHeight = w.GroundLevel - perlinNoise.Noise(x * 0.01, 0) * w.HillSize;

                // Flip y-axis calculation
                int flippedY = w.TilesHigh - 1 - y;

                if (flippedY >= hillHeight)
                {
                    if (generateUnderworld && flippedY >= w.TilesHigh - 200)
                    {
                        // Set the tile to inactive (air) in the underworld area
                        w.Tiles[x, flippedY] = new Tile
                        {
                            IsActive = false
                        };
                    }
                    else
                    {
                        // Determine tile type based on depth
                        Tile.TileType tileType = (flippedY < w.RockLevel) ? Tile.TileType.DirtBlock : Tile.TileType.StoneBlock;
                        w.Tiles[x, flippedY] = new Tile
                        {
                            IsActive = true,
                            Type = (ushort)tileType // Encapsulated tile types
                        };

                        // Check to skip walls
                        if (generateWalls)
                        {
                            // Add walls based on depth, skipping the topmost layer of dirt
                            if (flippedY < w.RockLevel && flippedY != (int)hillHeight)
                            {
                                w.Tiles[x, flippedY].Wall = (ushort)Tile.WallType.DirtWall;
                            }
                            else if (flippedY >= w.RockLevel)
                            {
                                w.Tiles[x, flippedY].Wall = (ushort)Tile.WallType.StoneWall;
                            }
                        }
                    }
                }
                else
                {
                    w.Tiles[x, flippedY] = new Tile { IsActive = false }; // Air
                }
            }
        }
    }
    #endregion

    #region Cleanup Generation

    private void CleanupGeneration(World w, bool generateGrass, bool generateWalls)
    {
        // Variable to store the lowest topmost layer across all columns
        int minHillY = -1;

        // Iterate through each column to find the lowest topmost layer
        for (int x = 0; x < w.TilesWide; x++)
        {
            int topmostLayer = -1; // Reset for each column

            // Determine the topmost active layer for each column
            for (int y = 0; y < w.TilesHigh; y++)
            {
                if (w.Tiles[x, y].IsActive)
                {
                    topmostLayer = y;
                    break; // Stop at the first active tile found from the top
                }
            }

            // Continue only if a valid topmost layer is found
            if (topmostLayer >= 0 && topmostLayer < w.TilesHigh)
            {
                // Update the minimum hill Y value if the topmost layer is above the current minHillY and below the rock level
                if (topmostLayer > minHillY && topmostLayer < w.RockLevel)
                {
                    minHillY = topmostLayer;
                }

                // Place grass on the topmost dirt layer and continue down
                if (generateGrass)
                {
                    for (int y = topmostLayer; y < w.TilesHigh; y++)
                    {
                        // Place grass function
                        if (w.Tiles[x, y].Type == (ushort)Tile.TileType.DirtBlock) // Ensure not to place grass on stone
                        {
                            w.Tiles[x, y] = new Tile
                            {
                                IsActive = true,
                                Type = (ushort)Tile.TileType.GrassBlock
                            };

                            // Check surrounding tiles for air
                            bool leftIsAir = (x - 1 >= 0) && !w.Tiles[x - 1, y].IsActive;
                            bool rightIsAir = (x + 1 < w.TilesWide) && !w.Tiles[x + 1, y].IsActive;

                            // Continue placing grass only if either side has air
                            if (!leftIsAir && !rightIsAir)
                            {
                                break; // Stop if both sides are not air
                            }
                        }
                    }
                }

                // Remove top exposed walls function
                if (generateWalls)
                {
                    for (int y = topmostLayer; y < w.TilesHigh; y++)
                    {
                        w.Tiles[x, y].Wall = (ushort)Tile.WallType.Sky; // Remove wall by setting it to a default state

                        // Check surrounding tiles for air
                        bool leftIsAir = (x - 1 >= 0) && !w.Tiles[x - 1, y].IsActive;
                        bool rightIsAir = (x + 1 < w.TilesWide) && !w.Tiles[x + 1, y].IsActive;

                        // Further remove walls on adjacent tiles
                        if (x - 1 >= 0)
                            w.Tiles[x - 1, y].Wall = (ushort)Tile.WallType.Sky;
                        if (x + 1 < w.TilesWide)
                            w.Tiles[x + 1, y].Wall = (ushort)Tile.WallType.Sky;

                        // Continue removing walls only if either side has air
                        if (!leftIsAir && !rightIsAir)
                        {
                            break; // Stop if both sides are not air
                        }
                    }
                }
            }
        }

        // Set the ground level of the world to the lowest topmost layer found
        w.GroundLevel = minHillY;
    }
    #endregion

    #region Cave Generation

    private void GenerateCaves(World w, Random rand, double caveNoise, double caveMultiplier, double caveDensity, bool surfaceCaves, bool skipUnderworld, bool generateGrass, bool generateWalls)
    {
        OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(0, w.TilesHigh), "Generating Caves..."));

        int width = w.TilesWide;
        int height = w.TilesHigh;

        // Generate initial cave map based on Perlin noise
        double[,] caveMap = new double[width, height];
        double noiseScale = caveNoise; // Keep noise scale independent of density
        double noiseThreshold = caveMultiplier * caveDensity; // Adjust threshold based on caveDensity

        // Initialize PerlinNoise object
        PerlinNoise perlinNoise = new(rand.Next()); // Use a random seed

        for (int y = 0; y < height; y++)
        {
            OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, w.TilesHigh), "Generating Cave Noise..."));

            for (int x = 0; x < width; x++)
            {
                double nx = x * noiseScale;
                double ny = y * noiseScale;
                caveMap[x, y] = perlinNoise.Noise(nx, ny); // Use Perlin noise
            }
        }

        // Cellular automata for cave refinement
        for (int iteration = 0; iteration < 5; iteration++) // Number of iterations
        {
            double[,] newCaveMap = new double[width, height];

            for (int y = 1; y < height - 1; y++)
            {
                OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, w.TilesHigh), "Refining Caves..."));

                for (int x = 1; x < width - 1; x++)
                {
                    int activeNeighbors = CountActiveNeighbors(caveMap, x, y);
                    if (caveMap[x, y] > noiseThreshold)
                    {
                        newCaveMap[x, y] = 1.0; // Cave
                    }
                    else
                    {
                        newCaveMap[x, y] = activeNeighbors >= 4 ? 1.0 : 0.0; // Cave or wall
                    }
                }
            }

            caveMap = newCaveMap;
        }

        // Determine the topmost layer of dirt
        double topDirtLayer = w.GroundLevel + perlinNoise.Noise(0, 0) * w.HillSize;

        // Apply cave map to world with adjustable cave size
        for (int y = 0; y < height; y++)
        {
            OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, w.TilesHigh), "Placing Caves..."));

            if (!surfaceCaves && y < w.RockLevel) continue; // Skip tiles above ground level
            if (skipUnderworld && y > height - 200) continue; // Skip tiles in underworld

            for (int x = 0; x < width; x++)
            {
                if (caveMap[x, y] > 0.5)
                {
                    for (int dy = (int)-caveMultiplier; dy <= (int)caveMultiplier; dy++)
                    {
                        for (int dx = (int)-caveMultiplier; dx <= (int)caveMultiplier; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                w.Tiles[nx, ny].IsActive = false; // Hollow out the tile

                                // Add grass.
                                if (generateGrass && w.Tiles[nx, ny].Type != (ulong)Tile.TileType.GrassBlock)
                                {
                                    w.Tiles[nx, ny].Type = (ushort)Tile.TileType.DirtBlock;
                                }

                                // Check to skip walls
                                if (generateWalls && w.Tiles[nx, ny].IsActive)
                                {
                                    // Skip walls on the very top layer of dirt
                                    if (y < w.RockLevel && y != topDirtLayer)
                                    {
                                        w.Tiles[nx, ny].Wall = (ushort)Tile.WallType.DirtWall;
                                    }
                                    // Rock level
                                    else if (y >= w.RockLevel)
                                    {
                                        w.Tiles[nx, ny].Wall = (ushort)Tile.WallType.StoneWall;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private int CountActiveNeighbors(double[,] caveMap, int x, int y)
    {
        int count = 0;

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                if (x + dx >= 0 && x + dx < caveMap.GetLength(0) && y + dy >= 0 && y + dy < caveMap.GetLength(1))
                {
                    if (caveMap[x + dx, y + dy] > 0.5)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
    #endregion

    #region Underworld Generation

    private void GenerateUnderworld(World w, Random rand, bool generateLava, double underworldRoofNoise, double underworldFloorNoise, double underworldLavaNoise)
    {
        int width = w.TilesWide;
        int height = w.TilesHigh;
        int underworldHeight = 200; // Define the height of the underworld
        int underworldStart = height - underworldHeight; // Starting y-coordinate for the underworld

        double groundNoiseScale = underworldFloorNoise; // Scale for ground Perlin noise
        double roofNoiseScale = underworldRoofNoise; // Scale for roof Perlin noise
        double lavaNoiseScale = underworldLavaNoise; // Scale for lava patches

        // Manually set the base levels for the ground and roof
        int roofBaseLevel = underworldStart + 20;   // 20 tiles up from the bottom
        int groundBaseLevel = height - 80; // 80 tiles up from the bottom

        PerlinNoise groundNoise = new(rand.Next());
        PerlinNoise roofNoise = new(rand.Next());
        PerlinNoise lavaNoise = new(rand.Next());

        // Iterate through the bottom 200 tiles to generate the underworld
        for (int y = underworldStart; y < height; y++)
        {
            OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, height), "Generating Underworld..."));

            for (int x = 0; x < width; x++)
            {
                // Calculate ground height with a rugged, mostly flat surface
                double groundHeight = groundBaseLevel - groundNoise.Noise(x * groundNoiseScale, 0) * 10; // Adjust '10' for ruggedness

                // Calculate roof height with a more spiked appearance
                double roofHeight = roofBaseLevel - roofNoise.Noise(x * roofNoiseScale, 0) * 15; // Adjust '15' for spike sharpness

                // Determine tile type based on position and Perlin noise
                if (y >= groundHeight && y < height)
                {
                    // Ground layer
                    w.Tiles[x, y] = new Tile
                    {
                        IsActive = true,
                        Type = (ushort)Tile.TileType.AshBlock // Use StoneBlock for underworld ground
                    };

                    // Add lava in patches based on Perlin noise
                    double lavaValue = lavaNoise.Noise(x * lavaNoiseScale, y * lavaNoiseScale);
                    if (generateLava && y >= groundBaseLevel + 2 && lavaValue > 0.1) // Adjust threshold for lava patch size
                    {
                        w.Tiles[x, y].LiquidType = LiquidType.Lava; // Lava type
                        w.Tiles[x, y].LiquidAmount = 255; // Full lava amount
                    }
                }
                else if (y < groundHeight && y >= roofHeight)
                {
                    // Air layer between ground and roof
                    w.Tiles[x, y] = new Tile { IsActive = false };
                }
                else if (y < roofHeight)
                {
                    // Roof layer
                    w.Tiles[x, y] = new Tile
                    {
                        IsActive = true,
                        Type = (ushort)Tile.TileType.AshBlock // Use StoneBlock for underworld roof
                    };
                }
            }
        }
    }
    #endregion

    #region Worldgen Perlin Noise

    // This class implements Perlin noise for procedural generation of terrain or textures.
    public class PerlinNoise
    {
        private readonly int[] _permutation;

        // Default permutation table used in the generation of noise.
        private static readonly int[] _defaultPermutation = {
                151,160,137,91,90,15,
                131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
                8,99,37,240,21,10,23,190, 6,148,247,120,234,75,0,26,197,62,94,252,
                219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,
                168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
                60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54, 65,25,63,161,
                1,216,80,73,209,76,132,187,208, 89,18,169,200,196,135,130,116,188,159,
                86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,5,202,38,147,
                118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,
                170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,
                97,228,251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,
                249,14,239,107,49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,
                50,45,127, 4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,
                195,78,66,215,61,156,180
            };

        // Constructor initializes the permutation table with a seed.
        public PerlinNoise(int seed)
        {
            _permutation = new int[512];
            Random rand = new(seed);

            // Copy default permutation to the start of the permutation array.
            for (int i = 0; i < 256; i++)
                _permutation[i] = _defaultPermutation[i];

            // Shuffle the permutation table with the given seed.
            for (int i = 0; i < 256; i++)
            {
                int j = rand.Next(256);
                int swap = _permutation[i];
                _permutation[i] = _permutation[j];
                _permutation[j] = swap;
            }

            // Duplicate the permutation array to handle wrap-around at edges.
            for (int i = 0; i < 256; i++)
                _permutation[256 + i] = _permutation[i];
        }

        // Computes the Perlin noise value for the given coordinates (x, y).
        public double Noise(double x, double y)
        {
            // Determine grid cell coordinates.
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;

            // Relative coordinates within the grid cell.
            x -= Math.Floor(x);
            y -= Math.Floor(y);

            // Fade functions to smooth the coordinate values.
            double u = Fade(x);
            double v = Fade(y);

            // Hash coordinates to determine which gradient vectors to use.
            int A = (_permutation[X] + Y) & 255;
            int B = (_permutation[X + 1] + Y) & 255;

            // Perform bilinear interpolation between gradient vectors.
            return Lerp(v, Lerp(u, Grad(_permutation[A], x, y),
                                   Grad(_permutation[B], x - 1, y)),
                           Lerp(u, Grad(_permutation[A + 1], x, y - 1),
                                   Grad(_permutation[B + 1], x - 1, y - 1)));
        }

        // Fade function to smooth the coordinate values.
        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        // Linear interpolation function.
        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        // Gradient function that calculates the dot product between a gradient vector and the coordinate.
        private static double Grad(int hash, double x, double y)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : 0;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
    #endregion

    private void SaveWorld()
    {
        if (CurrentWorld == null) return;

        if (string.IsNullOrWhiteSpace(CurrentFile))
            SaveWorldAs();
        else
            SaveWorldFile();
    }

    private void SaveWorldAsVersion()
    {
        if (CurrentWorld == null) return;

        var w = new SaveAsVersionGUI();
        w.Owner = Application.Current.MainWindow;
        w.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var sfd = new SaveFileDialog();
        sfd.Filter = "Terraria World File|*.wld";
        sfd.Title = "Save World As";
        sfd.InitialDirectory = DependencyChecker.PathToWorlds;
        sfd.FileName = Path.GetFileName(CurrentFile) ?? string.Join("-", CurrentWorld.Title.Split(Path.GetInvalidFileNameChars()));

        bool pickVersion = (bool)w.ShowDialog();
        uint version = w.WorldVersion;

        if (pickVersion && (bool)sfd.ShowDialog())
        {
            CurrentFile = sfd.FileName;
            SaveWorldFile(version);
        }
    }

    private void SaveWorldAs()
    {
        if (CurrentWorld == null) return;

        var sfd = new SaveFileDialog();
        sfd.Filter =
            "Terraria World File|*.wld|" +
            string.Join("|", WorldConfiguration.SaveConfiguration.SaveVersions.Values.Reverse().Select(vers => $"Terraria {vers.GameVersion}|*.wld"));

        sfd.Title = "Save World As";
        sfd.InitialDirectory = DependencyChecker.PathToWorlds;
        sfd.FileName = Path.GetFileName(CurrentFile) ?? string.Join("-", CurrentWorld.Title.Split(Path.GetInvalidFileNameChars()));
        if ((bool)sfd.ShowDialog())
        {
            CurrentFile = sfd.FileName;

            if (sfd.FilterIndex > 0)
            {
                try
                {
                    var name = sfd.Filter.ToString()
                        .Replace("Terraria World File|", "")
                        .Replace("*.wld|Terraria v", "")
                        .Replace("*.wld", "")
                        .Split('|')[sfd.FilterIndex - 1];

                    if (WorldConfiguration.SaveConfiguration.GameVersionToSaveVersion.TryGetValue(name, out uint versionOverride))
                    {
                        SaveWorldFile(versionOverride);
                        return;
                    }
                }
                catch (Exception)
                {
                    // fall back to default save
                }

                SaveWorldFile();
            }
        }
    }

    private void SaveWorldFile(uint version = 0)
    {
        if (CurrentWorld == null)
            return;
        if (CurrentWorld.LastSave < File.GetLastWriteTimeUtc(CurrentFile))
        {
            MessageBoxResult overwrite = MessageBox.Show(_currentWorld.Title + " was externally modified since your last save.\r\nDo you wish to overwrite?", "World Modified", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (overwrite.Equals(MessageBoxResult.Cancel))
                return;
        }

        SaveWorldThreaded(CurrentFile, version);
    }

    private void SaveWorldThreaded(string filename, uint version = 0)
    {
        Task.Factory.StartNew(async () =>
        {
            ErrorLogging.TelemetryClient?.TrackEvent(nameof(SaveWorldThreaded));
            try
            {
                OnProgressChanged(CurrentWorld, new ProgressChangedEventArgs(0, "Validating World..."));
                // await CurrentWorld.ValidateAsync();
            }
            catch (ArgumentOutOfRangeException err)
            {
                string msg = "There is a problem in your world.\r\n" + $"{err.ParamName}\r\n" + $"This world may not open in Terraria\r\n" + "Would you like to save anyways??\r\n";
                if (MessageBox.Show(msg, "World Error", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    return;
            }
            catch (Exception ex)
            {
                string msg = "There is a problem in your world.\r\n" + $"{ex.Message}\r\n" + "This world may not open in Terraria\r\n" + "Would you like to save anyways??\r\n";
                if (MessageBox.Show(msg, "World Error", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    return;
            }

            await World.SaveAsync(CurrentWorld, filename, versionOverride: (int)version, progress: new Progress<ProgressChangedEventArgs>(e =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => OnProgressChanged(CurrentWorld, e));
            }));
        }).ContinueWith(t => CommandManager.InvalidateRequerySuggested(), TaskFactoryHelper.UiTaskScheduler);
    }

    public void ReloadWorld()
    {
        // perform validations.
        if (CurrentWorld == null)
        {
            MessageBox.Show("No opened world loaded for reloading.", "World File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        else
        {
            // Prompt for world loading.
            if (MessageBox.Show("Unsaved work will be lost!", "Reload Current World?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return; // if no, abort.

            // Load world.
            LoadWorld(CurrentFile);
        }
    }

    public void LoadWorld(string filename)
    {
        _loadTimer.Reset();
        _loadTimer.Start();
        _saveTimer.Stop();
        CurrentFile = filename;
        //CurrentWorld = null;
        GC.WaitForFullGCComplete();

        Task.Factory.StartNew(() =>
        {
            // perform validations
            var validation = World.ValidateWorldFile(filename);
            if (!validation.IsValid)
            {
                //ErrorLogging.LogException(err);
                string msg =
                    "There was an error reading the world file.\r\n" +
                    "This is usually caused by a corrupt save file or a world version newer than supported.\r\n\r\n" +
                    $"TEdit v{TEdit.App.Version}\r\n" +
                    $"TEdit Max World: {WorldConfiguration.CompatibleVersion}\r\n" +
                    $"Current World: {validation.Version}\r\n\r\n" +
                    "Do you wish to force it to load anyway?\r\n\r\n" +
                    "WARNING: This may have unexpected results including corrupt world files and program crashes.\r\n\r\n" +
                    $"The error is :\r\n{validation.Message}";

                // there is no recovering here, so just show the message and return (aka abort)
                MessageBox.Show(msg, "World File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (validation.IsLegacy && validation.IsTModLoader)
            {
                string message = $"You are loading a legacy TModLoader world version: {validation.Version}.\r\n" +
                    $"1. Editing legacy files is a BETA feature.\r\n" +
                    $"2. Editing modded worlds is unsupported.\r\n" +
                    "Please make a backup as you may experience world file corruption.\r\n" +
                    "Do you wish to continue?";

                if (MessageBox.Show(message, "Convert File?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    // if no, abort
                    return null;
                }
            }
            else if (validation.IsLegacy)
            {
                // this has been around forever, removing "beta" warning for now 
                // string message = $"You are loading a legacy world version: {validation.Version}.\r\n" +
                //     $"Editing legacy files is a BETA feature.\r\n" +
                //     "Please make a backup as you may experience world file corruption.\r\n" +
                //     "Do you wish to continue?";
                // 
                // if (MessageBox.Show(message, "Convert File?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                // {
                //     return;
                // }
            }
            else if (validation.IsTModLoader)
            {
                string message = $"You are loading a TModLoader world." +
                    $"Editing modded worlds is unsupported.\r\n" +
                    "Please make a backup as you may experience world file corruption.\r\n" +
                    "Do you wish to continue?";

                if (MessageBox.Show(message, "Load Mod World?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return null;
                }
            }


            var (world, error) = World.LoadWorld(filename);
            if (error != null)
            {
                string msg =
                "There was an error reading the world file.\r\n" +
                "This is usually caused by a corrupt save file or a world version newer than supported.\r\n\r\n" +
                $"TEdit v{TEdit.App.Version}\r\n" +
                $"TEdit Max World: {WorldConfiguration.CompatibleVersion}\r\n" +
                $"Current World: {validation.Version}\r\n\r\n" +
                "Do you wish to force it to load anyway?\r\n\r\n" +
                "WARNING: This may have unexpected results including corrupt world files and program crashes.\r\n\r\n" +
                $"The error is :\r\n{error.Message}";

                if (MessageBox.Show(msg, "Load Invalid World?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return null;
                }
            }

            return world;
        })
        .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler)
        .ContinueWith(t => RenderEntireWorld())
        .ContinueWith(t =>
        {
            try
            {
                if (CurrentWorld != null)
                {
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(LoadWorld));

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
