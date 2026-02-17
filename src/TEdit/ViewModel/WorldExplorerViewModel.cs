using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.Utility;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class WorldExplorerViewModel
{
    private readonly WorldViewModel _worldViewModel;

    public WorldExplorerViewModel(WorldViewModel worldViewModel)
    {
        _worldViewModel = worldViewModel;
        _ = RefreshAsync();
    }

    public ObservableCollection<WorldEntryViewModel> Worlds { get; } = [];
    public ObservableCollection<BackupWorldGroupViewModel> BackupGroups { get; } = [];

    [Reactive]
    private int _selectedTabIndex;

    [Reactive]
    private string _searchText = "";

    [Reactive]
    private WorldEntryViewModel _selectedWorld;

    [Reactive]
    private BackupEntryViewModel _selectedBackup;

    [Reactive]
    private bool _isLoading;

    public IEnumerable<WorldEntryViewModel> FilteredWorlds
    {
        get
        {
            var worlds = Worlds.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                worlds = worlds.Where(w =>
                    w.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    w.FilePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            return worlds;
        }
    }

    public IEnumerable<BackupWorldGroupViewModel> FilteredBackupGroups
    {
        get
        {
            var groups = BackupGroups.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                groups = groups.Where(g =>
                    g.WorldName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            return groups;
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
            var backupGroups = new Dictionary<string, BackupWorldGroupViewModel>(StringComparer.OrdinalIgnoreCase);

            // Load worlds from Terraria worlds folder
            await Task.Run(() =>
            {
                string worldsPath = DependencyChecker.PathToWorlds;
                if (!string.IsNullOrEmpty(worldsPath) && Directory.Exists(worldsPath))
                {
                    foreach (var file in Directory.GetFiles(worldsPath, "*.wld"))
                    {
                        var header = World.ReadWorldHeader(file);
                        if (header != null)
                        {
                            worldEntries.Add(new WorldEntryViewModel(header, pinnedWorlds.Contains(file)));
                        }
                    }
                }

                // Load recent worlds that aren't already in the list
                var existingPaths = new HashSet<string>(worldEntries.Select(w => w.FilePath), StringComparer.OrdinalIgnoreCase);
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

                        if (!backupGroups.TryGetValue(worldName, out var group))
                        {
                            group = new BackupWorldGroupViewModel(worldName);
                            backupGroups[worldName] = group;
                        }
                        group.Entries.Add(new BackupEntryViewModel(file));
                    }
                }

                // Load autosaves
                string autoSavePath = WorldViewModel.AutoSavePath;
                if (Directory.Exists(autoSavePath))
                {
                    foreach (var file in Directory.GetFiles(autoSavePath, "*.autosave"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file); // removes .autosave
                        if (!backupGroups.TryGetValue(name, out var group))
                        {
                            group = new BackupWorldGroupViewModel(name);
                            backupGroups[name] = group;
                        }
                        group.Entries.Add(new BackupEntryViewModel(file));
                    }
                }
            });

            // Sort worlds
            worldEntries.Sort();

            // Sort backup entries within groups (newest first)
            foreach (var group in backupGroups.Values)
            {
                var sorted = group.Entries.OrderByDescending(e => e.Timestamp).ToList();
                group.Entries.Clear();
                foreach (var entry in sorted) group.Entries.Add(entry);
            }

            // Update UI collections on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                Worlds.Clear();
                foreach (var w in worldEntries) Worlds.Add(w);

                BackupGroups.Clear();
                foreach (var g in backupGroups.Values.OrderBy(g => g.WorldName))
                    BackupGroups.Add(g);

                this.RaisePropertyChanged(nameof(FilteredWorlds));
                this.RaisePropertyChanged(nameof(FilteredBackupGroups));
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

    public void DeleteBackup(BackupEntryViewModel backup)
    {
        if (backup == null) return;
        try
        {
            File.Delete(backup.FilePath);

            // Remove from group
            foreach (var group in BackupGroups)
            {
                if (group.Entries.Remove(backup))
                {
                    if (group.Entries.Count == 0)
                        BackupGroups.Remove(group);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
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
