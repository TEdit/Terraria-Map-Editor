using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Terraria.TModLoader;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class WorldExplorerViewModel
{
    private readonly WorldViewModel _worldViewModel;

    public WorldExplorerViewModel(WorldViewModel worldViewModel)
    {
        _worldViewModel = worldViewModel;

        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(FilteredWorlds)));
        this.WhenAnyValue(x => x.ShowModLoaderWorlds)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(FilteredWorlds)));
        this.WhenAnyValue(x => x.PreviewImage)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(HasPreview));
                this.RaisePropertyChanged(nameof(ShowPreviewPanel));
            });
        this.WhenAnyValue(x => x.IsPreviewLoading)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ShowPreviewPanel)));

        _ = RefreshAsync();
    }

    public ObservableCollection<WorldEntryViewModel> Worlds { get; } = [];

    [Reactive]
    private string _searchText = "";

    [Reactive]
    private WorldEntryViewModel _selectedWorld;

    [Reactive]
    private BackupEntryViewModel _selectedBackup;

    [Reactive]
    private bool _showModLoaderWorlds;

    [Reactive]
    private bool _isLoading;

    [Reactive]
    private ImageSource _previewImage;

    [Reactive]
    private bool _isPreviewLoading;

    [Reactive]
    private string _previewTitle;

    public bool HasPreview => PreviewImage != null;

    public bool ShowPreviewPanel => HasPreview || IsPreviewLoading;

    public static string PreviewCachePath => Path.Combine(WorldViewModel.TempPath, "previews");

    public IEnumerable<WorldEntryViewModel> FilteredWorlds
    {
        get
        {
            var worlds = Worlds.AsEnumerable();

            // Filter by toggle: Vanilla or tModLoader
            if (ShowModLoaderWorlds)
                worlds = worlds.Where(w => w.IsTModLoader);
            else
                worlds = worlds.Where(w => !w.IsTModLoader);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                worlds = worlds.Where(w =>
                    w.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    w.FilePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (w.CloudLabel != null && w.CloudLabel.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }
            return worlds;
        }
    }

    /// <summary>
    /// Whether the tModLoader tab has any worlds (used to show/hide the tab count badge).
    /// </summary>
    public int VanillaWorldCount => Worlds.Count(w => !w.IsTModLoader);
    public int ModLoaderWorldCount => Worlds.Count(w => w.IsTModLoader);

    public async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var pinnedWorlds = new HashSet<string>(
                UserSettingsService.Current.PinnedWorlds ?? [],
                StringComparer.OrdinalIgnoreCase);
            var recentWorlds = UserSettingsService.Current.RecentWorlds ?? [];

            // ── Phase 1: Fast file enumeration (no binary I/O) ──
            var worldEntries = new List<WorldEntryViewModel>();
            var backupEntries = new List<(string WorldName, BackupEntryViewModel Entry)>();
            var loadedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Enumerate local worlds folder
            string worldsPath = DependencyChecker.PathToWorlds;
            if (!string.IsNullOrEmpty(worldsPath) && Directory.Exists(worldsPath))
            {
                foreach (var file in Directory.GetFiles(worldsPath, "*.wld"))
                {
                    var fi = new FileInfo(file);
                    worldEntries.Add(new WorldEntryViewModel(fi, pinnedWorlds.Contains(file)));
                    loadedPaths.Add(file);
                }
            }

            // Enumerate Steam cloud worlds from ALL user profiles
            try
            {
                var cloudPaths = DependencyChecker.GetAllSteamCloudWorldPaths();
                foreach (var (userId, cloudWorldsDir) in cloudPaths)
                {
                    foreach (var file in Directory.GetFiles(cloudWorldsDir, "*.wld"))
                    {
                        if (loadedPaths.Contains(file)) continue;
                        var fi = new FileInfo(file);
                        worldEntries.Add(new WorldEntryViewModel(fi,
                            pinnedWorlds.Contains(file),
                            isCloudSave: true,
                            cloudUserId: userId));
                        loadedPaths.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
            }

            // Load worlds from tModLoader worlds folder
            string tmodWorldsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"My Games\Terraria\tModLoader\Worlds");
            if (Directory.Exists(tmodWorldsPath))
            {
                foreach (var file in Directory.GetFiles(tmodWorldsPath, "*.wld"))
                {
                    if (loadedPaths.Contains(file)) continue;
                    var fi = new FileInfo(file);
                    worldEntries.Add(new WorldEntryViewModel(fi, pinnedWorlds.Contains(file)));
                    loadedPaths.Add(file);
                }
            }

            // Enumerate recent worlds that aren't already in the list
            foreach (var recentPath in recentWorlds)
            {
                if (loadedPaths.Contains(recentPath)) continue;

                if (File.Exists(recentPath))
                {
                    var fi = new FileInfo(recentPath);
                    worldEntries.Add(new WorldEntryViewModel(fi, pinnedWorlds.Contains(recentPath), isRecent: true));
                    loadedPaths.Add(recentPath);
                }
                else
                {
                    worldEntries.Add(new WorldEntryViewModel(recentPath));
                }
            }

            // Collect backups (filename parsing only, no binary I/O)
            string backupPath = WorldViewModel.BackupPath;
            if (Directory.Exists(backupPath))
            {
                foreach (var file in Directory.GetFiles(backupPath, "*.wld"))
                {
                    string name = Path.GetFileName(file);
                    // Parse WorldName.YYYYMMDDHHMMSS.wld
                    int lastDot = name.LastIndexOf('.');
                    if (lastDot < 0) continue;
                    string nameWithoutExt = name.Substring(0, lastDot);
                    int timestampDot = nameWithoutExt.LastIndexOf('.');
                    if (timestampDot < 0) continue;

                    string worldName = nameWithoutExt.Substring(0, timestampDot);
                    string timestamp = nameWithoutExt.Substring(timestampDot + 1);
                    if (timestamp.Length != 14 || !timestamp.All(char.IsDigit)) continue;

                    backupEntries.Add((worldName, new BackupEntryViewModel(file)));
                }
            }

            // Collect autosaves
            string autoSavePath = WorldViewModel.AutoSavePath;
            if (Directory.Exists(autoSavePath))
            {
                foreach (var file in Directory.GetFiles(autoSavePath, "*.autosave"))
                {
                    string worldName = Path.GetFileNameWithoutExtension(file); // removes .autosave
                    backupEntries.Add((worldName, new BackupEntryViewModel(file)));
                }
            }

            // Match backups to worlds by filename stem (case-insensitive)
            // Note: before hydration, Title == filename stem, so matching works the same
            var worldsByKey = new Dictionary<string, WorldEntryViewModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var w in worldEntries)
            {
                worldsByKey.TryAdd(w.Title, w);
                if (!string.IsNullOrEmpty(w.FileName))
                {
                    var stem = Path.GetFileNameWithoutExtension(w.FileName);
                    worldsByKey.TryAdd(stem, w);
                }
                worldsByKey.TryAdd(w.Title.Replace(' ', '_'), w);
                worldsByKey.TryAdd(w.Title.Replace('_', ' '), w);
            }

            var orphanBackups = new Dictionary<string, List<BackupEntryViewModel>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (worldName, entry) in backupEntries)
            {
                if (worldsByKey.TryGetValue(worldName, out var world)
                    || worldsByKey.TryGetValue(worldName.Replace('_', ' '), out world)
                    || worldsByKey.TryGetValue(worldName.Replace(' ', '_'), out world))
                {
                    world.Backups.Add(entry);
                }
                else
                {
                    if (!orphanBackups.TryGetValue(worldName, out var list))
                    {
                        list = [];
                        orphanBackups[worldName] = list;
                    }
                    list.Add(entry);
                }
            }

            // Create placeholder entries for orphan backups
            foreach (var (worldName, entries) in orphanBackups)
            {
                var placeholder = new WorldEntryViewModel(worldName, isMissing: true);
                foreach (var entry in entries)
                    placeholder.Backups.Add(entry);
                worldEntries.Add(placeholder);
            }

            // Sort backup entries within each world (newest first)
            foreach (var world in worldEntries)
            {
                if (world.Backups.Count > 1)
                {
                    var sorted = world.Backups.OrderByDescending(e => e.Timestamp).ToList();
                    world.Backups.Clear();
                    foreach (var entry in sorted) world.Backups.Add(entry);
                }
            }

            // Sort worlds (by lastModified from FileInfo; favorites not known yet)
            worldEntries.Sort();

            // Populate UI immediately with placeholder entries
            Application.Current.Dispatcher.Invoke(() =>
            {
                Worlds.Clear();
                foreach (var w in worldEntries) Worlds.Add(w);
                this.RaisePropertyChanged(nameof(FilteredWorlds));
            });

            // ── Phase 2: Background header hydration ──
            // Collect entries that need hydration (non-missing, non-loaded)
            var toHydrate = worldEntries.Where(w => !w.IsMissing && !w.IsLoaded).ToList();

            await Task.Run(() =>
            {
                foreach (var entry in toHydrate)
                {
                    try
                    {
                        var header = World.ReadWorldHeader(entry.FilePath);
                        if (header != null)
                        {
                            Application.Current.Dispatcher.Invoke(() => entry.Hydrate(header));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogging.LogException(ex);
                    }
                }
            });

            // Re-sort after hydration (favorites are now known)
            Application.Current.Dispatcher.Invoke(() =>
            {
                var sorted = Worlds.OrderBy(w => w).ToList();
                Worlds.Clear();
                foreach (var w in sorted) Worlds.Add(w);
                this.RaisePropertyChanged(nameof(FilteredWorlds));
                this.RaisePropertyChanged(nameof(VanillaWorldCount));
                this.RaisePropertyChanged(nameof(ModLoaderWorldCount));

                // Auto-select tModLoader if there are modded worlds but no vanilla worlds
                if (VanillaWorldCount == 0 && ModLoaderWorldCount > 0)
                    ShowModLoaderWorlds = true;
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OpenWorld(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        // Track as recent if not in the standard worlds folder
        string worldsPath = DependencyChecker.PathToWorlds;
        if (!string.IsNullOrEmpty(worldsPath) &&
            !path.StartsWith(worldsPath, StringComparison.OrdinalIgnoreCase))
        {
            AddRecentWorld(path);
        }

        _worldViewModel.LoadWorld(path);
    }

    public void PinWorld(string path)
    {
        var pinned = UserSettingsService.Current.PinnedWorlds ?? new List<string>();
        if (!pinned.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            pinned.Add(path);
            UserSettingsService.Current.PinnedWorlds = pinned;
        }

        var entry = Worlds.FirstOrDefault(w => w.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (entry != null) entry.IsPinned = true;
    }

    public void UnpinWorld(string path)
    {
        var pinned = UserSettingsService.Current.PinnedWorlds ?? new List<string>();
        pinned.RemoveAll(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
        UserSettingsService.Current.PinnedWorlds = pinned;

        var entry = Worlds.FirstOrDefault(w => w.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (entry != null) entry.IsPinned = false;
    }

    public void ToggleFavorite(WorldEntryViewModel entry)
    {
        if (entry == null || entry.IsMissing || string.IsNullOrEmpty(entry.FilePath))
            return;

        try
        {
            bool newValue = !entry.IsFavorite;
            WorldHeaderPatcher.SetFavorite(entry.FilePath, newValue);
            entry.IsFavorite = newValue;
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public void DeleteBackup(BackupEntryViewModel backup)
    {
        if (backup == null) return;
        try
        {
            File.Delete(backup.FilePath);

            // Remove from the world's backup list
            foreach (var world in Worlds)
            {
                if (world.Backups.Remove(backup))
                    break;
            }

            SelectedBackup = null;
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public void CreateBackupNow(WorldEntryViewModel world)
    {
        if (world == null || world.IsMissing || string.IsNullOrEmpty(world.FilePath) || !File.Exists(world.FilePath))
            return;

        try
        {
            // Create Terraria .bak if it doesn't exist
            string bakPath = world.FilePath + ".bak";
            if (!File.Exists(bakPath))
            {
                File.Copy(world.FilePath, bakPath, false);
            }

            // Create TEdit timestamped backup
            string worldBaseName = Path.GetFileNameWithoutExtension(world.FilePath);
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string backupDest = Path.Combine(WorldViewModel.BackupPath, $"{worldBaseName}.{timestamp}.wld");

            if (!Directory.Exists(WorldViewModel.BackupPath))
                Directory.CreateDirectory(WorldViewModel.BackupPath);

            if (!File.Exists(backupDest))
            {
                File.Copy(world.FilePath, backupDest, false);
                world.Backups.Insert(0, new BackupEntryViewModel(backupDest));
            }

            world.RefreshBackupState();
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public async Task GeneratePreviewAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;

        IsPreviewLoading = true;
        PreviewTitle = Path.GetFileName(filePath);

        try
        {
            // Compute cache path
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(filePath));
            var hashStr = Convert.ToHexString(hash);
            var cachePath = Path.Combine(PreviewCachePath, $"{hashStr}.png");

            // Check cache
            if (File.Exists(cachePath))
            {
                var cached = new BitmapImage();
                cached.BeginInit();
                cached.CacheOption = BitmapCacheOption.OnLoad;
                cached.UriSource = new Uri(cachePath);
                cached.EndInit();
                cached.Freeze();
                PreviewImage = cached;
                return;
            }

            // Load world on background thread
            var (world, error) = await Task.Run(() => World.LoadWorld(filePath));

            if (error != null || world == null)
            {
                PreviewTitle = $"Error loading: {Path.GetFileName(filePath)}";
                return;
            }

            // Register mod properties on UI thread (ObservableCollections are UI-bound)
            // then render and save (WriteableBitmap requires UI thread too)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (world.TwldData != null)
                    TwldFile.RegisterModProperties(world.TwldData);

                var bmp = RenderMiniMap.Render(world, showBackground: true, targetWidth: 600, targetHeight: 200);
                Directory.CreateDirectory(PreviewCachePath);
                bmp.SavePng(cachePath);

                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.UriSource = new Uri(cachePath);
                result.EndInit();
                result.Freeze();
                PreviewImage = result;
            });
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
            PreviewTitle = $"Error: {ex.Message}";
        }
        finally
        {
            IsPreviewLoading = false;
        }
    }

    public void ClearPreview()
    {
        PreviewImage = null;
        PreviewTitle = null;
    }

    private void AddRecentWorld(string path)
    {
        var recent = UserSettingsService.Current.RecentWorlds ?? new List<string>();
        recent.RemoveAll(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
        recent.Insert(0, path);
        if (recent.Count > 20) recent.RemoveRange(20, recent.Count - 20);
        UserSettingsService.Current.RecentWorlds = recent;
    }
}
