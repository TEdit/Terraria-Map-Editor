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

    public async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var pinnedWorlds = new HashSet<string>(
                UserSettingsService.Current.PinnedWorlds ?? [],
                StringComparer.OrdinalIgnoreCase);
            var recentWorlds = UserSettingsService.Current.RecentWorlds ?? [];

            var worldEntries = new List<WorldEntryViewModel>();
            var backupEntries = new List<(string WorldName, BackupEntryViewModel Entry)>();

            await Task.Run(() =>
            {
                // Load worlds from Terraria worlds folder
                string worldsPath = DependencyChecker.PathToWorlds;
                var loadedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(worldsPath) && Directory.Exists(worldsPath))
                {
                    foreach (var file in Directory.GetFiles(worldsPath, "*.wld"))
                    {
                        var header = World.ReadWorldHeader(file);
                        if (header != null)
                        {
                            worldEntries.Add(new WorldEntryViewModel(header, pinnedWorlds.Contains(file)));
                            loadedPaths.Add(file);
                        }
                    }
                }

                // Load Steam cloud worlds from ALL user profiles
                try
                {
                    var cloudPaths = DependencyChecker.GetAllSteamCloudWorldPaths();
                    foreach (var (userId, cloudWorldsDir) in cloudPaths)
                    {
                        foreach (var file in Directory.GetFiles(cloudWorldsDir, "*.wld"))
                        {
                            if (loadedPaths.Contains(file)) continue;
                            var header = World.ReadWorldHeader(file);
                            if (header != null)
                            {
                                worldEntries.Add(new WorldEntryViewModel(header,
                                    pinnedWorlds.Contains(file),
                                    isCloudSave: true,
                                    cloudUserId: userId));
                                loadedPaths.Add(file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogException(ex);
                }

                // Load recent worlds that aren't already in the list
                var existingPaths = loadedPaths;
                foreach (var recentPath in recentWorlds)
                {
                    if (existingPaths.Contains(recentPath)) continue;

                    if (File.Exists(recentPath))
                    {
                        var header = World.ReadWorldHeader(recentPath);
                        if (header != null)
                        {
                            worldEntries.Add(new WorldEntryViewModel(header, pinnedWorlds.Contains(recentPath), isRecent: true));
                        }
                    }
                    else
                    {
                        worldEntries.Add(new WorldEntryViewModel(recentPath));
                    }
                }

                // Load backups
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

                // Load autosaves
                string autoSavePath = WorldViewModel.AutoSavePath;
                if (Directory.Exists(autoSavePath))
                {
                    foreach (var file in Directory.GetFiles(autoSavePath, "*.autosave"))
                    {
                        string worldName = Path.GetFileNameWithoutExtension(file); // removes .autosave
                        backupEntries.Add((worldName, new BackupEntryViewModel(file)));
                    }
                }
            });

            // Match backups to worlds by title or filename stem (case-insensitive)
            // Backup filenames use underscores where world titles use spaces,
            // e.g. backup "Brewery_of_Tungsten2" should match world title "Brewery of Tungsten2"
            var worldsByKey = new Dictionary<string, WorldEntryViewModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var w in worldEntries)
            {
                // Index by title
                worldsByKey.TryAdd(w.Title, w);
                // Also index by filename stem (e.g. "Brewery_of_Tungsten2")
                if (!string.IsNullOrEmpty(w.FileName))
                {
                    var stem = Path.GetFileNameWithoutExtension(w.FileName);
                    worldsByKey.TryAdd(stem, w);
                }
                // Also index by title with spaces replaced by underscores and vice versa
                worldsByKey.TryAdd(w.Title.Replace(' ', '_'), w);
                worldsByKey.TryAdd(w.Title.Replace('_', ' '), w);
            }

            var orphanBackups = new Dictionary<string, List<BackupEntryViewModel>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (worldName, entry) in backupEntries)
            {
                // Try exact match, then underscore↔space normalization
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

            // Sort worlds
            worldEntries.Sort();

            // Update UI collections on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                Worlds.Clear();
                foreach (var w in worldEntries) Worlds.Add(w);

                this.RaisePropertyChanged(nameof(FilteredWorlds));
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

            // Render and save on UI thread (WriteableBitmap requires it)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
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
