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
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Plugins;
using TEdit.Editor.Tools;
using TEdit.Editor.Undo;
using TEdit.Framework.Threading;
using TEdit.Geometry;
using TEdit.Configuration;
using TEdit.Properties;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.UI;
using TEdit.UI.Xaml;
using TEdit.Utility;
using TEdit.View.Popups;
using TEdit.UI.Xaml.Dialog;
using static TEdit.Terraria.CreativePowers;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Timer = System.Timers.Timer;
using static TEdit.Terraria.WorldConfiguration;

namespace TEdit.ViewModel;



public partial class WorldViewModel : ReactiveObject
{
    private readonly BrushSettings _brush = new BrushSettings();
    private readonly Stopwatch _loadTimer = new Stopwatch();
    private readonly MouseTile _mouseOverTile = new MouseTile();
    private readonly ObservableCollection<IPlugin> _plugins = new ObservableCollection<IPlugin>();
    private readonly ObservableCollection<string> _points = new ObservableCollection<string>();
    private readonly ObservableCollection<NpcListItem> _allNpcs = new ObservableCollection<NpcListItem>();
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
    private bool _showBackgrounds = true;
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
    private string _windowTitle;
    private int? _selectedBiomeVariantIndex; // null = Auto (follows cursor), 0+ = manual selection
    private int _currentAutoBiomeIndex; // Tracks biome at cursor position
    private int _lastSavedUndoIndex = 0;
    private bool _hasUnsavedPropertyChanges = false;
    private int _lastUserSavedUndoIndex = 0;
    private bool _hasUnsavedUserPropertyChanges = false;

    // Style preview collections for world properties comboboxes
    public ObservableCollection<StylePreviewItem> TreeStylePreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> TreeTopPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> ForestBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> SnowBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> JungleBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> CorruptionBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> CrimsonBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> HallowBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> DesertBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> OceanBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> MushroomBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> UnderworldBgPreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> CaveStylePreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> IceBackStylePreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> JungleBackStylePreviews { get; } = new();
    public ObservableCollection<StylePreviewItem> HellBackStylePreviews { get; } = new();

    /// <summary>
    /// Action to export all textures to PNG files. Set by WorldRenderXna when textures are loaded.
    /// Only available in DEBUG builds.
    /// </summary>
    public Action? ExportTexturesAction { get; set; }

    static WorldViewModel()
    {
        if (!Directory.Exists(TempPath))
        {
            Directory.CreateDirectory(TempPath);
        }
    }

    public WorldViewModel()
    {
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) { return; }

        CheckUpdates = UserSettingsService.Current.CheckUpdates;

        if (CheckUpdates)
            CheckVersion();


        IsAutoSaveEnabled = UserSettingsService.Current.Autosave;

        World.ProgressChanged += OnProgressChanged;
        Brush.BrushChanged += OnPreviewChanged;
        UpdateTitle();

        // Build sprites from tile config (Frames data), not from textures
        BuildSpritesFromConfig();

        InitSpriteViews();

        _saveTimer.AutoReset = true;
        _saveTimer.Elapsed += SaveTimerTick;
        // 3 minute save timer
        _saveTimer.Interval = 3 * 60 * 1000;

        // Populate NPC list from static config (world-specific state synced on load)
        RefreshAllNpcs();

        // Test File Association and command line
        if (Application.Current.Properties["OpenFile"] != null)
        {
            string filename = Application.Current.Properties["OpenFile"].ToString();
            LoadWorld(filename);
        }
    }

    /// <summary>
    /// Build SpriteSheet objects from TileProperty.Frames config data (not from textures).
    /// Preview images are set later when textures are loaded.
    /// </summary>
    public void BuildSpritesFromConfig()
    {
        lock (WorldConfiguration.Sprites2Lock)
        {
            WorldConfiguration.Sprites2.Clear();

            // Debug: Log how many framed tiles we have
            var framedTiles = WorldConfiguration.TileProperties.Where(t => t.IsFramed).ToList();
            var framedWithFrames = framedTiles.Where(t => t.Frames != null && t.Frames.Count > 0).ToList();
            ErrorLogging.Log($"BuildSpritesFromConfig: {framedTiles.Count} framed tiles, {framedWithFrames.Count} with frames data");

            if (framedWithFrames.Count == 0 && framedTiles.Count > 0)
            {
                // Log first few framed tiles to see their state
                foreach (var t in framedTiles.Take(5))
                {
                    ErrorLogging.Log($"  Tile {t.Id} ({t.Name}): IsFramed={t.IsFramed}, Frames={t.Frames?.Count ?? -1}");
                }
            }

            foreach (var tile in WorldConfiguration.TileProperties.Where(t => t.IsFramed && t.Frames != null && t.Frames.Count > 0))
            {
                var sprite = new SpriteSheet
                {
                    Tile = (ushort)tile.Id,
                    Name = tile.Name,
                    SizeTiles = tile.FrameSize,
                    SizePixelsRender = tile.TextureGrid,
                    SizePixelsInterval = tile.TextureGrid + tile.FrameGap,
                    IsAnimated = tile.IsAnimated
                };

                // Add a SpriteItemPreview for each frame from config
                int styleIndex = 0;
                foreach (var frame in tile.Frames)
                {
                    var spriteItem = new SpriteItemPreview
                    {
                        Tile = sprite.Tile,
                        Style = styleIndex++,
                        Name = frame.ToString(),
                        UV = frame.UV,
                        SizeTiles = frame.Size.X > 0 && frame.Size.Y > 0 ? frame.Size : tile.FrameSize[0],
                        SizePixelsInterval = sprite.SizePixelsInterval,
                        Anchor = frame.Anchor,
                        StyleColor = frame.Color.A > 0 ? frame.Color : tile.Color,
                        Preview = null // Set later when textures load
                    };

                    // Set custom preview config for special tiles
                    ConfigureSpecialTilePreview(spriteItem, tile, frame);

                    sprite.Styles.Add(spriteItem);
                }

                WorldConfiguration.Sprites2.Add(sprite);
            }
        }

        ErrorLogging.Log($"BuildSpritesFromConfig: {WorldConfiguration.Sprites2.Count} sprite sheets created from config");
    }

    /// <summary>
    /// Configure custom preview settings for special tiles that need non-standard preview rendering.
    /// </summary>
    private static void ConfigureSpecialTilePreview(SpriteItemPreview spriteItem, TileProperty tile, FrameProperty frame)
    {
        // DeadCellsDisplayJar (tile 698) - texture has 3 layers stacked vertically
        // Y=0: Main jar (36x44px), Y=46: Foreground border, Y=92: Background glow
        // Preview should only show the main jar layer (Y=0)
        // Tile UV.X values are 0, 18, 36 for variants 0, 1, 2
        // But jar texture has variants at X=0, 38, 76 (each 38px apart)
        if (tile.Id == (int)TileType.DeadCellsDisplayJar)
        {
            int variant = frame.UV.X / 18;  // 0, 18, 36 -> 0, 1, 2
            int sourceX = variant * 38;      // 0, 1, 2 -> 0, 38, 76
            spriteItem.PreviewConfig = new PreviewConfig
            {
                TextureType = PreviewTextureType.Tile,
                SourceRect = new System.Drawing.Rectangle(sourceX, 0, 36, 44),
                Offset = new Geometry.Vector2Short(-10, 0) // Jar renders with -10px X offset
            };
        }
        // Sleeping Digtoise (tile 751) - 56x46 texture with 7 animation frames
        // Only render from anchor tile (frameX == 0 && frameY == 0)
        // Center 56x46 sprite over 32x32 tile area: offset -12px X, -7px Y
        else if (tile.Id == 751)
        {
            spriteItem.PreviewConfig = new PreviewConfig
            {
                TextureType = PreviewTextureType.Tile,
                SourceRect = new System.Drawing.Rectangle(0, 0, 56, 46),
                Offset = new Geometry.Vector2Short(-12, -7)
            };
        }
        // Chillet Egg (tile 752) - 36x38 texture
        // Only render from anchor tile (frameX == 0 && frameY == 0)
        // Offset: -2px X, +5px Y
        else if (tile.Id == 752)
        {
            spriteItem.PreviewConfig = new PreviewConfig
            {
                TextureType = PreviewTextureType.Tile,
                SourceRect = new System.Drawing.Rectangle(0, 0, 36, 38),
                Offset = new Geometry.Vector2Short(-2, 2)
            };
        }
        // Tree tiles (tile 5) - identify tree tops and branches logic:
        // frameY >= 198 AND frameX >= 22 triggers tree foliage rendering
        // frameX == 22: tree top (uses Tree_Tops texture)
        // frameX == 44: left branch (uses Tree_Branches texture)
        // frameX == 66: right branch (uses Tree_Branches texture)
        // Variant index = (frameY - 198) / 22 gives 0, 1, or 2 for variants A, B, C
        else if (tile.Id == 5)
        {
            int frameX = frame.UV.X;
            int frameY = frame.UV.Y;

            // Tree foliage: frameY >= 198 AND frameX >= 22
            if (frameY >= 198 && frameX >= 22)
            {
                // Calculate variant index from frameY (0, 1, or 2)
                int variant = (frameY - 198) / 22;

                if (frameX == 22)
                {
                    // Tree top - use Tree_Tops_0 texture
                    // Variants are arranged horizontally at 82px intervals (80px width + 2px gap)
                    // Offset: Bottom anchor - center X, anchor Y at bottom of tile
                    // X = (16-80)/2 = -32, Y = (16-80) = -64
                    spriteItem.PreviewConfig = new PreviewConfig
                    {
                        TextureType = PreviewTextureType.TreeTops,
                        TextureStyle = 0, // Forest tree style
                        SourceRect = new System.Drawing.Rectangle(variant * 82, 0, 80, 80),
                        Offset = new Geometry.Vector2Short(-32, -64)
                    };
                }
                else if (frameX == 44)
                {
                    // Left branch - use Tree_Branches_0 texture, left side (X=0)
                    // Variants are arranged vertically at 42px intervals (40px height + 2px gap)
                    // Offset: Right anchor - anchor X at right of tile, center Y
                    // X = (16-40) = -24, Y = (16-40)/2 = -12
                    spriteItem.PreviewConfig = new PreviewConfig
                    {
                        TextureType = PreviewTextureType.TreeBranch,
                        TextureStyle = 0,
                        SourceRect = new System.Drawing.Rectangle(0, variant * 42, 40, 40),
                        Offset = new Geometry.Vector2Short(-24, -12)
                    };
                }
                else if (frameX == 66)
                {
                    // Right branch - use Tree_Branches_0 texture, right side (X=42)
                    // Variants are arranged vertically at 42px intervals (40px height + 2px gap)
                    // Offset: Left anchor - anchor X at left of tile, center Y
                    // X = 0, Y = (16-40)/2 = -12
                    spriteItem.PreviewConfig = new PreviewConfig
                    {
                        TextureType = PreviewTextureType.TreeBranch,
                        TextureStyle = 0,
                        SourceRect = new System.Drawing.Rectangle(42, variant * 42, 40, 40),
                        Offset = new Geometry.Vector2Short(0, -12)
                    };
                }
            }
            else
            {
                // Tree trunk tiles - use Tiles_5_{treeType} texture based on biome
                // SourceRect uses the tile's UV coordinates
                spriteItem.PreviewConfig = new PreviewConfig
                {
                    TextureType = PreviewTextureType.Tree,
                    TextureStyle = -1, // Default forest tree, will be overridden dynamically
                    SourceRect = new System.Drawing.Rectangle(frameX, frameY, 16, 16),
                    Offset = new Geometry.Vector2Short(0, 0)
                };
            }
        }
        // Palm trees (tile 323) - similar to regular trees but uses sand-based biomes
        // Palm tops: U >= 88 && U <= 132 (use Tree_Tops_15)
        // Palm trunks: everything else (use Tiles_323)
        else if (tile.Id == 323)
        {
            int frameX = frame.UV.X;
            int frameY = frame.UV.Y;

            if (frameX >= 88 && frameX <= 132)
            {
                // Palm tree top - use Tree_Tops_15 texture
                // source.X = frame variant based on U, source.Y = treeType * 82
                int frameVariant = (frameX - 88) / 22; // 0, 1, or 2
                spriteItem.PreviewConfig = new PreviewConfig
                {
                    TextureType = PreviewTextureType.PalmTreeTop,
                    TextureStyle = 0, // Will be determined dynamically for source.Y
                    SourceRect = new System.Drawing.Rectangle(frameVariant * 82, 0, 80, 80),
                    Offset = new Geometry.Vector2Short(-32, -64) // Bottom anchor
                };
            }
            else
            {
                // Palm tree trunk - use Tiles_323 texture
                // source.Y is replaced by treeType * 22 (not added)
                // Frame size is 20x20 (textureGrid), srcRect.X is preserved for horizontal position
                spriteItem.PreviewConfig = new PreviewConfig
                {
                    TextureType = PreviewTextureType.PalmTree,
                    TextureStyle = 0, // Will be determined dynamically
                    SourceRect = new System.Drawing.Rectangle(frameX, 0, 20, 20), // Y=0 will be replaced by palmType * 22
                    Offset = new Geometry.Vector2Short(-2, -2) // Adjust for 20x20 frame vs 16x16 tile
                };
            }
        }
        // Gem Trees (583-589), Vanity Trees (596, 616), Ash Tree (634)
        // Same foliage detection as regular trees but with fixed tree style indices
        else if (tile.Id >= 583 && tile.Id <= 589)
        {
            // Gem trees: style = tileId - 583 + 22 (indices 22-28)
            int treeStyle = tile.Id - 583 + 22;
            ConfigureTreePreview(spriteItem, frame.UV.X, frame.UV.Y, treeStyle);
        }
        else if (tile.Id == 596 || tile.Id == 616)
        {
            // Vanity trees: 596 Sakura = 29, 616 Willow = 30
            int treeStyle = tile.Id == 596 ? 29 : 30;
            ConfigureTreePreview(spriteItem, frame.UV.X, frame.UV.Y, treeStyle);
        }
        else if (tile.Id == 634)
        {
            // Ash tree: style = 31
            ConfigureTreePreview(spriteItem, frame.UV.X, frame.UV.Y, 31);
        }
    }

    private static void ConfigureTreePreview(SpriteItem spriteItem, int frameX, int frameY, int treeStyle)
    {
        // Tree foliage: frameY >= 198 AND frameX >= 22
        if (frameY >= 198 && frameX >= 22)
        {
            // Calculate variant index from frameY (0, 1, or 2)
            int variant = (frameY - 198) / 22;

            if (frameX == 22)
            {
                // Tree top - use Tree_Tops texture
                // Dimensions vary by tree style:
                // - Gem trees (22-28), Ash tree (31): 116x96
                // - Vanity trees (29-30): 118x96
                // - Default (normal trees): 80x80
                int topWidth, topHeight;
                if (treeStyle >= 22 && treeStyle <= 28 || treeStyle == 31)
                {
                    topWidth = 116;
                    topHeight = 96;
                }
                else if (treeStyle == 29 || treeStyle == 30)
                {
                    topWidth = 118;
                    topHeight = 96;
                }
                else
                {
                    topWidth = 80;
                    topHeight = 80;
                }

                spriteItem.PreviewConfig = new PreviewConfig
                {
                    TextureType = PreviewTextureType.TreeTops,
                    TextureStyle = treeStyle,
                    SourceRect = new System.Drawing.Rectangle(variant * (topWidth + 2), 0, topWidth, topHeight),
                    Offset = new Geometry.Vector2Short((short)(-topWidth / 2), (short)(-topHeight + 16))
                };
            }
            else if (frameX == 44)
            {
                // Left branch - use Tree_Branches texture
                spriteItem.PreviewConfig = new PreviewConfig
                {
                    TextureType = PreviewTextureType.TreeBranch,
                    TextureStyle = treeStyle,
                    SourceRect = new System.Drawing.Rectangle(0, variant * 42, 40, 40),
                    Offset = new Geometry.Vector2Short(-24, -12)
                };
            }
            else if (frameX == 66)
            {
                // Right branch - use Tree_Branches texture
                spriteItem.PreviewConfig = new PreviewConfig
                {
                    TextureType = PreviewTextureType.TreeBranch,
                    TextureStyle = treeStyle,
                    SourceRect = new System.Drawing.Rectangle(42, variant * 42, 40, 40),
                    Offset = new Geometry.Vector2Short(0, -12)
                };
            }
        }
        else
        {
            // Tree trunk tiles - use the tile's own texture
            spriteItem.PreviewConfig = new PreviewConfig
            {
                TextureType = PreviewTextureType.Tile,
                SourceRect = new System.Drawing.Rectangle(frameX, frameY, 20, 20),
                Offset = new Geometry.Vector2Short(-2, -2)
            };
        }
    }

    public void InitSpriteViews()
    {
        int spriteCount;
        lock (WorldConfiguration.Sprites2Lock)
        {
            spriteCount = WorldConfiguration.Sprites2.Count;
        }
        ErrorLogging.Log($"InitSpriteViews: {spriteCount} sprites loaded");

        _spriteSheetView = (ListCollectionView)CollectionViewSource.GetDefaultView(WorldConfiguration.Sprites2);
        _spriteSheetView.Filter = o =>
        {
            if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

            var sprite = (SpriteSheet)o;
            var filter = _spriteFilter.Trim();

            // Exact tile ID match (if filter is purely numeric)
            if (ushort.TryParse(filter, out ushort tileId))
            {
                if (sprite.Tile == tileId) return true;
            }

            // Split by delimiters for multi-term search (added space delimiter)
            string[] filterTerms = filter.Split(new char[] { '/', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string term in filterTerms)
            {
                // Tile ID prefix match (for searching "5" to find 50, 51, etc.)
                if (sprite.Tile.ToString().StartsWith(term)) return true;

                // Case-insensitive name match
                if (sprite.Name?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                // Match against any child style name
                foreach (var style in sprite.Styles)
                {
                    if (style.Name?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
            }
            return false;
        };

        List<SpriteItemPreview> styles;
        lock (WorldConfiguration.Sprites2Lock)
        {
            styles = WorldConfiguration.Sprites2.SelectMany(s => s.Styles).Select(s => (SpriteItemPreview)s).ToList();
        }
        _spriteStylesView = (ListCollectionView)CollectionViewSource.GetDefaultView(new ObservableCollection<SpriteItemPreview>(styles));
        _spriteStylesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        _spriteStylesView.Filter = (o) =>
        {
            var sprite = (SpriteItemPreview)o;

            // If no tile filter AND no text filter, hide everything
            if (_spriteTileFilter <= 0 && string.IsNullOrWhiteSpace(_spriteFilter)) return false;

            // Must pass tile filter if one is set
            if (_spriteTileFilter > 0 && sprite.Tile != _spriteTileFilter) return false;

            // If tile filter matches and no text filter, show all styles for this tile
            if (_spriteTileFilter > 0 && sprite.Tile == _spriteTileFilter)
            {
                if (string.IsNullOrWhiteSpace(_spriteFilter)) return true;

                // If text filter exists, check if PARENT sprite sheet name matches
                // This fixes the "tree styles hidden" bug - show all styles when parent matches search
                var parentSheet = WorldConfiguration.Sprites2.FirstOrDefault(s => s.Tile == sprite.Tile);
                if (parentSheet != null)
                {
                    string[] filterTerms = _spriteFilter.Split(new char[] { '/', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string term in filterTerms)
                    {
                        // If parent name matches, show all its styles
                        if (parentSheet.Name?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                        // Also check if tile ID matches
                        if (parentSheet.Tile.ToString() == term.Trim())
                            return true;
                    }
                }

                // Otherwise, filter by style's own name
                return MatchesSpriteFilter(sprite.Name, _spriteFilter);
            }

            // No tile filter - only show if text filter matches style name or tile ID
            return MatchesSpriteFilter(sprite.Name, _spriteFilter) ||
                   sprite.Tile.ToString() == _spriteFilter.Trim();
        };

        // Notify UI that the views have been recreated
        this.RaisePropertyChanged(nameof(SpriteSheetView));
        this.RaisePropertyChanged(nameof(SpriteStylesView));
    }

    /// <summary>
    /// Helper method to check if a name matches the sprite filter terms.
    /// </summary>
    private bool MatchesSpriteFilter(string name, string filter)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(filter))
            return false;

        string[] terms = filter.Split(new char[] { '/', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string term in terms)
        {
            if (name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    public WriteableBitmap MinimapImage
    {
        get { return _minimapImage; }
        set { this.RaiseAndSetIfChanged(ref _minimapImage, value); }
    }

    public ListCollectionView SpriteSheetView => _spriteSheetView;

    public ListCollectionView SpriteStylesView => _spriteStylesView;

    public string SpriteFilter
    {
        get { return _spriteFilter; }
        set
        {
            this.RaiseAndSetIfChanged(ref _spriteFilter, value);
            SpriteSheetView.Refresh();
            SpriteStylesView.Refresh();
        }
    }

    public ushort SpriteTileFilter
    {
        get { return _spriteTileFilter; }
        set
        {
            this.RaiseAndSetIfChanged(ref _spriteTileFilter, value);
            SpriteStylesView.Refresh();
        }
    }

    private int _spriteThumbnailSize = UserSettingsService.Current.SpriteThumbnailSize;
    public int SpriteThumbnailSize
    {
        get { return _spriteThumbnailSize; }
        set
        {
            var clamped = Math.Clamp(value, 16, 128);
            if (this.RaiseAndSetIfChanged(ref _spriteThumbnailSize, clamped) != clamped)
            {
                UserSettingsService.Current.SpriteThumbnailSize = clamped;
            }
        }
    }

    public SpriteItemPreview SelectedSpriteItem
    {
        get { return _selectedSpriteItem; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSpriteItem, value);
            // Sync biome index to the new sprite item
            if (value != null)
            {
                value.SelectedBiomeIndex = SelectedBiomeVariantIndex;
            }
            PreviewChange();
        }
    }

    public SpriteSheet SelectedSpriteSheet
    {
        get { return _selectedSpriteSheet; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSpriteSheet, value);

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
            UpdateBiomeVariants();
        }
    }

    /// <summary>
    /// Whether the currently selected tile has biome variants available.
    /// </summary>
    public bool HasBiomeVariants
    {
        get
        {
            if (_selectedSpriteSheet == null) return false;
            var tileProperty = WorldConfiguration.TileProperties.FirstOrDefault(t => t.Id == _selectedSpriteSheet.Tile);
            return tileProperty?.BiomeVariants?.Count > 0;
        }
    }

    /// <summary>
    /// List of biome variant names for the dropdown (includes "Auto" at index 0).
    /// </summary>
    public List<string> BiomeVariantNames
    {
        get
        {
            var names = new List<string>();
            if (_selectedSpriteSheet == null) return names;

            var tileProperty = WorldConfiguration.TileProperties.FirstOrDefault(t => t.Id == _selectedSpriteSheet.Tile);
            if (tileProperty?.BiomeVariants != null)
            {
                foreach (var variant in tileProperty.BiomeVariants)
                {
                    names.Add(variant.Name);
                }
            }
            return names;
        }
    }

    /// <summary>
    /// Selected biome variant index. When IsBiomeAutoMode is true, this follows CurrentAutoBiomeIndex.
    /// </summary>
    public int SelectedBiomeVariantIndex
    {
        get => _selectedBiomeVariantIndex ?? _currentAutoBiomeIndex;
        set
        {
            if (_selectedBiomeVariantIndex != value)
            {
                _selectedBiomeVariantIndex = value;
                this.RaisePropertyChanged();
                UpdateSpriteItemBiomeIndex(value);
            }
        }
    }

    /// <summary>
    /// Whether biome selection follows cursor position automatically.
    /// </summary>
    public bool IsBiomeAutoMode
    {
        get => _selectedBiomeVariantIndex == null;
        set
        {
            if (value)
            {
                _selectedBiomeVariantIndex = null;
                this.RaisePropertyChanged(nameof(SelectedBiomeVariantIndex));
            }
            else
            {
                _selectedBiomeVariantIndex = _currentAutoBiomeIndex;
            }
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Updates the auto-detected biome index based on cursor position.
    /// Called from mouse move handler.
    /// </summary>
    public void UpdateAutoBiomeIndex(int biomeIndex)
    {
        if (_currentAutoBiomeIndex != biomeIndex)
        {
            _currentAutoBiomeIndex = biomeIndex;
            if (IsBiomeAutoMode)
            {
                this.RaisePropertyChanged(nameof(SelectedBiomeVariantIndex));
                UpdateSpriteItemBiomeIndex(biomeIndex);
            }
        }
    }

    private void UpdateBiomeVariants()
    {
        this.RaisePropertyChanged(nameof(HasBiomeVariants));
        this.RaisePropertyChanged(nameof(BiomeVariantNames));
        this.RaisePropertyChanged(nameof(SelectedBiomeVariantIndex));
    }

    private void UpdateSpriteItemBiomeIndex(int biomeIndex)
    {
        // Update ALL sprite items in the current sheet, not just the selected one
        if (_selectedSpriteSheet?.Styles != null)
        {
            foreach (var style in _selectedSpriteSheet.Styles)
            {
                if (style is SpriteItemPreview preview)
                {
                    preview.SelectedBiomeIndex = biomeIndex;
                }
            }
            // Refresh the collection view to update the UI
            SpriteStylesView?.Refresh();
        }
    }

    [ReactiveCommand]
    private void LaunchWiki() => LaunchUrl("http://github.com/BinaryConstruct/Terraria-Map-Editor/wiki");

    /* SBLogic - catch exception if browser can't be launched */
    public static void LaunchUrl(string url)
    {
        DialogResponse result = DialogResponse.None;
        try
        {
            Process.Start(url);
        }
        catch
        {
            result = App.DialogService.ShowMessage(
                "Unable to open external browser. Copy to clipboard?",
                "Link Error",
                DialogButton.YesNo,
                DialogImage.Exclamation);
        }

        // Just in case
        try
        {
            if (result == DialogResponse.Yes)
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
        set { this.RaiseAndSetIfChanged(ref _selectedTabIndex, value); }
    }

    public int SelectedSpecialTile
    {
        get { return _selectedSpecialTile; }
        set { this.RaiseAndSetIfChanged(ref _selectedSpecialTile, value); }
    }

    public Vector2Int32 SelectedXmas
    {
        get { return _selectedXmas; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedXmas, value);
            SelectedTabIndex = 1;
            SelectedSpecialTile = 11;
        }
    }

    public int SelectedXmasStar
    {
        get { return _selectedXmasStar; }
        set { this.RaiseAndSetIfChanged(ref _selectedXmasStar, value); }
    }

    public int SelectedXmasGarland
    {
        get { return _selectedXmasGarland; }
        set { this.RaiseAndSetIfChanged(ref _selectedXmasGarland, value); }
    }

    public int SelectedXmasBulb
    {
        get { return _selectedXmasBulb; }
        set { this.RaiseAndSetIfChanged(ref _selectedXmasBulb, value); }
    }

    public int SelectedXmasLight
    {
        get { return _selectedXmasLight; }
        set { this.RaiseAndSetIfChanged(ref _selectedXmasLight, value); }
    }

    public Sign SelectedSign
    {
        get { return _selectedSign; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSign, value);
            SelectedTabIndex = 1;
            SelectedSpecialTile = 12;
        }
    }

    public Chest SelectedChest
    {
        get { return _selectedChest; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedChest, value);
            SelectedTabIndex = 1;
            SelectedSpecialTile = 13;
        }
    }

    public TileEntity SelectedTileEntity
    {
        get { return _selectedTileEntity; }
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTileEntity, value);
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
        set { this.RaiseAndSetIfChanged(ref _morphBiomeTarget, value); }
    }

    public bool IsAutoSaveEnabled
    {
        get { return _isAutoSaveEnabled; }
        set
        {
            this.RaiseAndSetIfChanged(ref _isAutoSaveEnabled, value);
            UserSettingsService.Current.Autosave = _isAutoSaveEnabled;
        }
    }

    public bool HasUnsavedChanges
    {
        get
        {
            // Check if undo index has changed since last autosave (tile edits)
            if (_undoManager != null && _undoManager.CurrentIndex != _lastSavedUndoIndex)
                return true;

            // Check if World properties have changed since last autosave
            return _hasUnsavedPropertyChanges;
        }
    }

    public bool HasUnsavedUserChanges
    {
        get
        {
            // Check if undo index has changed since last manual save (tile edits)
            if (_undoManager != null && _undoManager.CurrentIndex != _lastUserSavedUndoIndex)
                return true;

            // Check if World properties have changed since last manual save
            return _hasUnsavedUserPropertyChanges;
        }
    }

    public bool ShowGrid
    {
        get { return _showGrid; }
        set { this.RaiseAndSetIfChanged(ref _showGrid, value); }
    }

    public bool ShowTextures
    {
        get { return _showTextures; }
        set { this.RaiseAndSetIfChanged(ref _showTextures, value); }
    }

    public ObservableCollection<string> Points
    {
        get { return _points; }
    }

    public ObservableCollection<NpcListItem> AllNpcs
    {
        get { return _allNpcs; }
    }

    private ICollectionView _allNpcsView;
    public ICollectionView AllNpcsView
    {
        get
        {
            if (_allNpcsView == null)
            {
                _allNpcsView = CollectionViewSource.GetDefaultView(_allNpcs);
                _allNpcsView.SortDescriptions.Add(new SortDescription(nameof(NpcListItem.IsOnMap), ListSortDirection.Descending));
                _allNpcsView.SortDescriptions.Add(new SortDescription(nameof(NpcListItem.DefaultName), ListSortDirection.Ascending));
            }
            return _allNpcsView;
        }
    }

    public string SelectedPoint
    {
        get { return _selectedPoint; }
        set { this.RaiseAndSetIfChanged(ref _selectedPoint, value); }
    }

    private void RefreshPoints()
    {
        Points.Clear();
        Points.Add("Spawn");
        Points.Add("Dungeon");

        if (CurrentWorld?.TeamBasedSpawnsSeed == true)
        {
            for (int i = 0; i < World.TeamNames.Length; i++)
            {
                Points.Add($"Team {World.TeamNames[i]}");
            }
        }

        if (CurrentWorld != null)
        {
            foreach (NPC npc in CurrentWorld.NPCs)
            {
                Points.Add(npc.Name);
            }
        }

        RefreshAllNpcs();
    }

    private void RefreshAllNpcs()
    {
        if (_allNpcs.Count == 0)
        {
            foreach (var kvp in WorldConfiguration.NpcNames.OrderBy(x => x.Value))
            {
                WorldConfiguration.NpcById.TryGetValue(kvp.Key, out var npcData);
                _allNpcs.Add(new NpcListItem(
                    kvp.Key, kvp.Value,
                    npcData?.Variants,
                    npcData?.CanShimmer ?? false));
            }
        }

        foreach (var item in _allNpcs)
        {
            item.World = CurrentWorld;
            item.WorldNpc = CurrentWorld?.NPCs.FirstOrDefault(n => n.SpriteId == item.SpriteId);
        }

        AllNpcsView.Refresh();
    }


    public Item SelectedChestItem
    {
        get { return _selectedChestItem; }
        set { this.RaiseAndSetIfChanged(ref _selectedChestItem, value); }
    }

    public UndoManager UndoManager
    {
        get { return _undoManager; }
    }

    public ClipboardManager Clipboard
    {
        get { return _clipboard; }
        set { this.RaiseAndSetIfChanged(ref _clipboard, value); }
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
        set { this.RaiseAndSetIfChanged(ref _currentFile, value); }
    }

    private ClipboardManager _clipboardManager;
    public World CurrentWorld
    {
        get { return _currentWorld; }
        set
        {
            // Unsubscribe from old world
            if (_currentWorld != null)
            {
                _currentWorld.PropertyChanged -= OnWorldPropertyChanged;
            }

            this.RaiseAndSetIfChanged(ref _currentWorld, value);

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

                // Reset both autosave and user save tracking
                _lastSavedUndoIndex = 0;
                _hasUnsavedPropertyChanges = false;
                _lastUserSavedUndoIndex = 0;
                _hasUnsavedUserPropertyChanges = false;

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

                // Subscribe to new world property changes
                _currentWorld.PropertyChanged += OnWorldPropertyChanged;
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
        set { this.RaiseAndSetIfChanged(ref _progress, value); }
    }

    public string WindowTitle
    {
        get { return _windowTitle; }
        set { this.RaiseAndSetIfChanged(ref _windowTitle, value); }
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
        set { this.RaiseAndSetIfChanged(ref _activeTool, value); }
    }

    public PixelMapManager PixelMap
    {
        get { return _pixelMap; }
        set { this.RaiseAndSetIfChanged(ref _pixelMap, value); }
    }

    public bool ShowRedWires
    {
        get { return _showRedWires; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showRedWires, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowBlueWires
    {
        get { return _showBlueWires; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showBlueWires, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowGreenWires
    {
        get { return _showGreenWires; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showGreenWires, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowYellowWires
    {
        get { return _showYellowWires; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showYellowWires, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowAllWires
    {
        get { return _showAllWires; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showAllWires, value);
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
            this.RaiseAndSetIfChanged(ref _showWireTransparency, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowActuators
    {
        get { return _showActuators; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showActuators, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowPoints
    {
        get { return _showPoints; }
        set { this.RaiseAndSetIfChanged(ref _showPoints, value); }
    }

    public bool ShowLiquid
    {
        get { return _showLiquid; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showLiquid, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowCoatings
    {
        get { return _showCoatings; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showCoatings, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowTiles
    {
        get { return _showTiles; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showTiles, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowWalls
    {
        get { return _showWalls; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showWalls, value);
            UpdateRenderWorld();
        }
    }

    public bool ShowBackgrounds
    {
        get { return _showBackgrounds; }
        set
        {
            this.RaiseAndSetIfChanged(ref _showBackgrounds, value);
            UpdateRenderWorld();
        }
    }

    [ReactiveCommand]
    private async Task CheckUpdatesAsync() => await CheckVersion(false);

    [ReactiveCommand]
    private void ViewLog() => ErrorLogging.ViewLog();

    public bool RealisticColors
    {
        get { return UserSettingsService.Current.RealisticColors; }
        set
        {
            this.RaisePropertyChanged(nameof(RealisticColors));
            UserSettingsService.Current.RealisticColors = value;
            App.DialogService.ShowMessage(
                Properties.Language.messagebox_restartrequired,
                Properties.Language.messagebox_restartrequired,
                DialogButton.OK,
                DialogImage.Information);
        }
    }

    public bool CheckUpdates
    {
        get { return _checkUpdates; }
        set
        {
            this.RaiseAndSetIfChanged(ref _checkUpdates, value);
            UserSettingsService.Current.CheckUpdates = value;
        }
    }

    public float _textureVisibilityZoomLevel = UserSettingsService.Current.TextureVisibilityZoomLevel;
    public float TextureVisibilityZoomLevel
    {
        get => _textureVisibilityZoomLevel;
        set
        {
            value = (float)Math.Floor(MathHelper.Clamp(value, 3, 64));
            if (this.RaiseAndSetIfChanged(ref _textureVisibilityZoomLevel, value) != value)
            {
                UserSettingsService.Current.TextureVisibilityZoomLevel = value;
            }
        }
    }

    private bool _showNews = UserSettingsService.Current.ShowNews;

    public bool ShowNews
    {
        get { return _showNews; }
        set
        {
            if (this.RaiseAndSetIfChanged(ref _showNews, value) != value)
            {
                UserSettingsService.Current.ShowNews = value;
            }
        }
    }


    private bool _enableTelemetry = UserSettingsService.Current.Telemetry != 0;

    public bool EnableTelemetry
    {
        get { return _enableTelemetry; }
        set
        {
            if (this.RaiseAndSetIfChanged(ref _enableTelemetry, value) != value)
            {
                UserSettingsService.Current.Telemetry = value ? 1 : 0;
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
        UpdateTitle();
    }

    private void SaveTimerTick(object sender, ElapsedEventArgs e)
    {
        if (IsAutoSaveEnabled && HasUnsavedChanges)
        {
            if (!string.IsNullOrWhiteSpace(CurrentFile))
                SaveWorldThreaded(Path.Combine(TempPath, Path.GetFileNameWithoutExtension(CurrentFile) + ".autosave.tmp"), GetSaveVersion_MaxConfig());
            else
                SaveWorldThreaded(Path.Combine(TempPath, "newworld.autosave.tmp"), GetSaveVersion_MaxConfig());
        }
    }

    private void OnWorldPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Ignore metadata properties that are updated during save operations
        if (e.PropertyName == nameof(World.LastSave) ||
            e.PropertyName == nameof(World.FileRevision))
            return;

        // Mark as having unsaved changes for both autosave and user save tracking
        _hasUnsavedPropertyChanges = true;
        _hasUnsavedUserPropertyChanges = true;
        this.RaisePropertyChanged(nameof(HasUnsavedChanges));
        this.RaisePropertyChanged(nameof(HasUnsavedUserChanges));
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        string fileName = Path.GetFileName(_currentFile);
        string dirtyIndicator = HasUnsavedUserChanges ? "*" : "";
        WindowTitle = $"TEdit v{App.Version} {fileName}{dirtyIndicator}";
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
            App.DialogService.ShowMessage("Unable to check version.", "Update Check Failed", DialogButton.OK, DialogImage.Warning);
        }



        if (isOutdated)
        {
#if !DEBUG
            var updateResult = App.DialogService.ShowMessage(
                "You are using an outdated version of TEdit. Do you wish to download the update?",
                "Update?",
                DialogButton.YesNo,
                DialogImage.Question);

            if (updateResult == DialogResponse.Yes)
            {
                try
                {
                    Process.Start("http://www.binaryconstruct.com/downloads/");
                }
                catch { }
            }
#endif
        }
        else if (!auto)
        {
            App.SnackbarService.ShowSuccess("TEdit is up to date.", "Update");
        }
    }

    [ReactiveCommand]
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

    [ReactiveCommand]
    private void AnalyzeWorld()
    {
        WorldAnalysis = Editor.WorldAnalysis.AnalyzeWorld(CurrentWorld);
    }

    private string _worldAnalysis;


    public string WorldAnalysis
    {
        get { return _worldAnalysis; }
        set { this.RaiseAndSetIfChanged(ref _worldAnalysis, value); }
    }

    /* SBLogic - Relay command to execute KillTally */

    [ReactiveCommand]
    private void LoadTally()
    {
        TallyCount = KillTally.LoadTally(CurrentWorld);
    }

    [ReactiveCommand]
    private void EditBestiary()
    {
        SelectedTabIndex = 6; // Navigate to Bestiary tab
    }

    private string _tallyCount;
    private TileEntity _selectedTileEntity;

    public string TallyCount
    {
        get { return _tallyCount; }
        set { this.RaiseAndSetIfChanged(ref _tallyCount, value); }
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

    internal void SetActiveTool(ITool tool)
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

    [ReactiveCommand]
    private void NewWorld()
    {
        // Define the bool for prompting ore generation plugin
        bool generateOres = false;

        // Open the dialog for creating a new world and check if user clicked 'OK'
        var nwDialog = new NewWorldView();
        nwDialog.Owner = Application.Current.MainWindow;
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
                RefreshPoints();
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
            SaveWorldFile(GetSaveVersion_MaxConfig(version)); // Clamp to the max config version.
        }
    }

    private void SaveWorldAs()
    {
        if (CurrentWorld == null) return;

        // Build "Save As" targets from gameVersionToSaveVersion keys.
        // Sort descending so newest versions appear first.
        var versionKeys = WorldConfiguration.SaveConfiguration?.GameVersionToSaveVersion?.Keys
            ?.Select(v => v.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .OrderByDescending(v => v, DottedVersionComparer.Instance)
            .ToList()
            ?? [];

        var sfd = new SaveFileDialog
        {
            // FilterIndex:
            // 1 = "Terraria World File"
            // 2 = first game version entry
            // 3 = second game version entry
            // ...
            Filter =
                "Terraria World File|*.wld|" +
                string.Join("|", versionKeys.Select(v => $"Terraria v{v}|*.wld")),

            Title = "Save World As",
            InitialDirectory = DependencyChecker.PathToWorlds,
            FileName = Path.GetFileName(CurrentFile) ?? string.Join("-", CurrentWorld.Title.Split(Path.GetInvalidFileNameChars()))
        };

        if ((bool)sfd.ShowDialog())
        {
            CurrentFile = sfd.FileName;

            // If they picked a specific Terraria version filter, use that gameVersion -> saveVersion mapping.
            if (sfd.FilterIndex > 1)
            {
                try
                {
                    int idx = sfd.FilterIndex - 2; // map FilterIndex to versionKeys index

                    if (idx >= 0 && idx < versionKeys.Count)
                    {
                        string selectedGameVersion = versionKeys[idx];

                        if (WorldConfiguration.SaveConfiguration.GameVersionToSaveVersion.TryGetValue(selectedGameVersion, out uint versionOverride))
                        {
                            SaveWorldFile(versionOverride);
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    // fall through to default
                }
            }

            // Maintain the existing world version.
            // This is also the fallback for parsing failures.
            // SaveWorldFile(CurrentWorld.Version);
            SaveWorldFile(GetSaveVersion_MaxConfig());
        }
    }

    private void SaveWorldFile(uint version = 0)
    {
        if (CurrentWorld == null)
            return;
        if (CurrentWorld.LastSave < File.GetLastWriteTimeUtc(CurrentFile))
        {
            var overwriteResult = App.DialogService.ShowMessage(
                _currentWorld.Title + " was externally modified since your last save.\r\nDo you wish to overwrite?",
                "World Modified",
                DialogButton.OKCancel,
                DialogImage.Warning);

            if (overwriteResult == DialogResponse.Cancel)
                return;
        }

        SaveWorldThreaded(CurrentFile, GetSaveVersion_MaxConfig(version));
    }

    private void SaveWorldThreaded(string filename, uint version = 0)
    {
        Task.Factory.StartNew(async () =>
        {
            try
            {
                OnProgressChanged(CurrentWorld, new ProgressChangedEventArgs(0, "Validating World..."));
                // await CurrentWorld.ValidateAsync();
            }
            catch (ArgumentOutOfRangeException err)
            {
                string msg = "There is a problem in your world.\r\n" + $"{err.ParamName}\r\n" + $"This world may not open in Terraria\r\n" + "Would you like to save anyways??\r\n";
                var saveResult = App.DialogService.ShowMessage(msg, "World Error", DialogButton.YesNo, DialogImage.Error);
                if (saveResult != DialogResponse.Yes)
                    return;
            }
            catch (Exception ex)
            {
                string msg = "There is a problem in your world.\r\n" + $"{ex.Message}\r\n" + "This world may not open in Terraria\r\n" + "Would you like to save anyways??\r\n";
                var saveResult = App.DialogService.ShowMessage(msg, "World Error", DialogButton.YesNo, DialogImage.Error);
                if (saveResult != DialogResponse.Yes)
                    return;
            }

            await World.SaveAsync(CurrentWorld, filename, versionOverride: (int)version, progress: new Progress<ProgressChangedEventArgs>(e =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => OnProgressChanged(CurrentWorld, e));
            }));
        }).Unwrap().ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully && _undoManager != null)
            {
                bool isAutosave = filename.EndsWith(".autosave.tmp", StringComparison.OrdinalIgnoreCase);

                if (isAutosave)
                {
                    // Autosave: atomically rename temp file to final autosave file
                    try
                    {
                        string finalPath = filename.Substring(0, filename.Length - 4); // Remove ".tmp"

                        // Use File.Move with overwrite for atomic replacement (requires .NET Core 3.0+)
                        if (File.Exists(finalPath))
                            File.Delete(finalPath);
                        File.Move(filename, finalPath);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogging.LogException(ex);
                    }

                    // Only reset autosave tracking, keep user save tracking
                    _lastSavedUndoIndex = _undoManager.CurrentIndex;
                    _hasUnsavedPropertyChanges = false;
                    this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                }
                else
                {
                    // Manual save: reset both autosave and user save tracking
                    _lastSavedUndoIndex = _undoManager.CurrentIndex;
                    _hasUnsavedPropertyChanges = false;
                    _lastUserSavedUndoIndex = _undoManager.CurrentIndex;
                    _hasUnsavedUserPropertyChanges = false;
                    this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                    this.RaisePropertyChanged(nameof(HasUnsavedUserChanges));
                    UpdateTitle();
                }
            }
            CommandManager.InvalidateRequerySuggested();
        }, TaskFactoryHelper.UiTaskScheduler);
    }

    private uint GetSaveVersion_MaxConfig(uint requested = 0)
    {
        // Make sure config is loaded (safe even if already initialized).
        WorldConfiguration.Initialize();

        uint max = WorldConfiguration.CompatibleVersion;
        if (max == 0) return requested; // ultra-defensive

        // If caller didn't request a version, default to MAX config version.
        uint v = (requested == 0) ? max : requested;

        // Never allow saving above config max.
        if (v > max) v = max;

        return v;
    }

    public void ReloadWorld()
    {
        // perform validations.
        if (CurrentWorld == null)
        {
            App.DialogService.ShowMessage("No opened world loaded for reloading.", "World File Error", DialogButton.OK, DialogImage.Error);
            return;
        }
        else
        {
            // Prompt for world loading.
            var reloadResult = App.DialogService.ShowMessage("Unsaved work will be lost!", "Reload Current World?", DialogButton.YesNo, DialogImage.Question);
            if (reloadResult == DialogResponse.No)
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

            if (validation.IsPreeminent)
            {
                string message =
                        $"This world version is NEWER than supported by TEdit's config.\r\n\r\n" +
                        $"World version: {validation.Version}\r\n" +
                        $"Max supported: {WorldConfiguration.CompatibleVersion}\r\n\r\n" +
                        $"TEdit will fall back to config version {WorldConfiguration.CompatibleVersion} " +
                        $"(missing newer tiles/walls/etc may cause issues).\r\n\r\n" +
                        $"Do you want to attempt to load anyway?";

                var newerResult = App.DialogService.ShowMessage(message, "Newer World Version", DialogButton.YesNo, DialogImage.Warning);
                if (newerResult == DialogResponse.No)
                {
                    return null;
                }

                // Apply only after user accepts.
                WorldConfiguration.ApplyForWorldVersion(validation.Version, out _);
            }
            else if (validation.IsCorrupt)
            {
                // The world file contains all-zeros (corrupt).
                string msg =
                    "The world file appears to be empty or corrupt (all bytes are zero).\r\n" +
                    "This file cannot be recovered as no data exists.\r\n\r\n" +
                    "What can I do?\r\n" +
                    "1. Restore a previously made backup (.bak, .bak2).\r\n" +
                    "2. Restore a previously made manual backup.\r\n" +
                    "3. Restore a previously created TEdit checkpoint (.TEdit).\r\n" +
                    "4. Restore a backup via windows file history (if previously enabled).";

                App.DialogService.ShowMessage(msg, "Corrupt World File", DialogButton.OK, DialogImage.Error);
                return null;
            }
            else if (!validation.IsValid)
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
                App.DialogService.ShowMessage(msg, "World File Error", DialogButton.OK, DialogImage.Error);
                return null;
            }

            if (validation.IsLegacy && validation.IsTModLoader)
            {
                string message = $"You are loading a legacy TModLoader world version: {validation.Version}.\r\n" +
                    $"1. Editing legacy files is a BETA feature.\r\n" +
                    $"2. Editing modded worlds is unsupported.\r\n" +
                    "Please make a backup as you may experience world file corruption.\r\n" +
                    "Do you wish to continue?";

                var tmodResult = App.DialogService.ShowMessage(message, "Convert File?", DialogButton.YesNo, DialogImage.Question);
                if (tmodResult == DialogResponse.No)
                {
                    // if no, abort
                    return null;
                }
            }
            else if (validation.IsLegacy)
            {
                // Reworked "IsLegacy" to be versions < 1.0.
                string message = $"You are loading a legacy world version: {validation.Version}.\r\n" +
                    $"Editing legacy files could cause unexpected results.\r\n" +
                    "Please make a backup as you may experience world file corruption.\r\n" +
                    "Do you wish to continue?";

                var legacyResult = App.DialogService.ShowMessage(message, "Convert File?", DialogButton.YesNo, DialogImage.Question);
                if (legacyResult == DialogResponse.No)
                {
                    return null;
                }
            }
            else if (validation.IsTModLoader)
            {
                string message = $"You are loading a TModLoader world." +
                    $"Editing modded worlds is unsupported.\r\n" +
                    "Please make a backup as you may experience world file corruption.\r\n" +
                    "Do you wish to continue?";

                var modResult = App.DialogService.ShowMessage(message, "Load Mod World?", DialogButton.YesNo, DialogImage.Question);
                if (modResult == DialogResponse.No)
                {
                    return null;
                }
            }

            // Create a single backup of the original world file (if it doesn't already exist)
            // This preserves the unedited state when first opening a world
            try
            {
                string backupPath = filename + ".TEdit";
                if (!File.Exists(backupPath) && File.Exists(filename))
                {
                    File.Copy(filename, backupPath, false);
                }

                // Clean up old timestamped backup files
                FileMaintenance.CleanupOldWorldBackups(filename);
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
                // Continue loading even if backup fails
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

                var invalidResult = App.DialogService.ShowMessage(msg, "Load Invalid World?", DialogButton.YesNo, DialogImage.Question);
                if (invalidResult == DialogResponse.No)
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
                    PixelMap = t.Result;
                    UpdateTitle();
                    RefreshPoints();
                    MinimapImage = RenderMiniMap.Render(CurrentWorld);
                    _loadTimer.Stop();

                    // Reset both autosave and user save tracking after world load
                    _lastSavedUndoIndex = _undoManager?.CurrentIndex ?? 0;
                    _hasUnsavedPropertyChanges = false;
                    _lastUserSavedUndoIndex = _undoManager?.CurrentIndex ?? 0;
                    _hasUnsavedUserPropertyChanges = false;
                    this.RaisePropertyChanged(nameof(HasUnsavedChanges));
                    this.RaisePropertyChanged(nameof(HasUnsavedUserChanges));
                    UpdateTitle();

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
