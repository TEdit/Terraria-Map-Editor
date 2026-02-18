using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TEdit.Terraria;

namespace TEdit.ViewModel;

public partial class BannerViewModel : ReactiveObject
{
    private readonly WorldViewModel _wvm;

    public BannerViewModel()
    {
        _wvm = ViewModelLocator.WorldViewModel;

        BannerData = new ObservableCollection<BannerItem>();
        Categories = new ObservableCollection<string> { "All" };

        BannersView = CollectionViewSource.GetDefaultView(BannerData);
        BannersView.Filter = FilterBanners;
        BannersView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BannerItem.Category)));

        this.WhenAnyValue(x => x.FilterText, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => BannersView.Refresh());

        // Auto-populate banners when a world is opened
        _wvm.WhenAnyValue(x => x.CurrentWorld)
            .Where(w => w != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => PopulateBanners());
    }

    [Reactive]
    private ObservableCollection<BannerItem> _bannerData;

    [Reactive]
    private string _filterText = "";

    [Reactive]
    private string _selectedCategory = "All";

    public ObservableCollection<string> Categories { get; }

    public ICollectionView BannersView { get; }

    private bool FilterBanners(object obj)
    {
        if (obj is not BannerItem item) return false;

        if (!string.IsNullOrEmpty(SelectedCategory) &&
            SelectedCategory != "All" &&
            !string.Equals(item.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(FilterText) &&
            !item.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Populates the banner list from the current world data.
    /// Called automatically when a world is loaded.
    /// </summary>
    private void PopulateBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        BannerData.Clear();
        Categories.Clear();
        Categories.Add("All");

        // Build lookup: bannerId -> first NPC data
        var npcByBanner = WorldConfiguration.BestiaryData.NpcById.Values
            .Where(n => n.BannerId >= 0)
            .GroupBy(n => n.BannerId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var kvp in WorldConfiguration.TallyNames.OrderBy(k => k.Key))
        {
            int tallyIndex = kvp.Key;
            string bannerName = kvp.Value;

            // Get category and NPC ID from bestiary data
            string category = "";
            int npcId = 0;
            if (npcByBanner.TryGetValue(tallyIndex, out var npcData))
            {
                category = npcData.Category ?? "";
                npcId = npcData.Id;
            }

            // Read current values from world
            ushort bannerCount = 0;
            if (tallyIndex >= 0 && tallyIndex < _wvm.CurrentWorld.ClaimableBanners.Count)
            {
                bannerCount = _wvm.CurrentWorld.ClaimableBanners[tallyIndex];
            }

            int kills = 0;
            if (tallyIndex >= 0 && tallyIndex < _wvm.CurrentWorld.KilledMobs.Count)
            {
                kills = _wvm.CurrentWorld.KilledMobs[tallyIndex];
            }

            BannerData.Add(new BannerItem
            {
                TallyIndex = tallyIndex,
                Name = bannerName,
                Category = string.IsNullOrEmpty(category) ? "Other" : category,
                NpcId = npcId,
                Kills = kills,
                Count = bannerCount,
            });
        }

        // Populate category filter list
        foreach (var cat in BannerData.Select(b => b.Category).Distinct().OrderBy(c => c))
        {
            if (!Categories.Contains(cat))
                Categories.Add(cat);
        }

        BannersView.Refresh();
    }

    /// <summary>
    /// Import banner data from a different world file.
    /// </summary>
    [ReactiveCommand]
    private async Task ImportBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            "Import Banners?",
            "This will replace your current banner counts with data from the selected world file. Continue?");

        if (!confirm)
            return;

        var ofd = new OpenFileDialog
        {
            Filter = "Terraria World File|*.wld",
            DefaultExt = "Terraria World File|*.wld",
            Title = "Import Banners from World File",
            InitialDirectory = DependencyChecker.PathToWorlds,
            Multiselect = false,
        };

        if (ofd.ShowDialog() != true) return;

        // Backup current state
        var backup = _wvm.CurrentWorld.ClaimableBanners.ToArray();
        try
        {
            World.ImportBanners(_wvm.CurrentWorld, ofd.FileName);
            PopulateBanners();
        }
        catch (Exception ex)
        {
            // Restore backup
            for (int i = 0; i < backup.Length && i < _wvm.CurrentWorld.ClaimableBanners.Count; i++)
            {
                _wvm.CurrentWorld.ClaimableBanners[i] = backup[i];
            }
            await App.DialogService.ShowExceptionAsync($"Error importing banner data from {ofd.FileName}. Your current banners have been restored.\r\n{ex.Message}");
        }
    }

    [ReactiveCommand]
    private async Task SaveBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            "Save Banners?",
            "Save banner changes back to the world?");

        if (!confirm)
            return;

        // Backup
        var backup = _wvm.CurrentWorld.ClaimableBanners.ToArray();
        try
        {
            foreach (var item in BannerData)
            {
                if (item.TallyIndex >= 0 && item.TallyIndex < _wvm.CurrentWorld.ClaimableBanners.Count)
                {
                    _wvm.CurrentWorld.ClaimableBanners[item.TallyIndex] = item.Count;
                }
            }
        }
        catch (Exception ex)
        {
            // Restore backup
            for (int i = 0; i < backup.Length && i < _wvm.CurrentWorld.ClaimableBanners.Count; i++)
            {
                _wvm.CurrentWorld.ClaimableBanners[i] = backup[i];
            }
            await App.DialogService.ShowExceptionAsync($"Error saving banner data. Changes have been restored.\r\n{ex.Message}");
        }
    }

    [ReactiveCommand]
    private async Task MaxOutBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            "Max Out Banners?",
            "Set all banner counts to maximum (9999)?");

        if (!confirm)
            return;

        foreach (var item in BannerData)
        {
            item.Count = 9999;
        }
    }

    [ReactiveCommand]
    private async Task ResetBanners()
    {
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            "Reset Banners?",
            "Reset all banner counts to zero?");

        if (!confirm)
            return;

        foreach (var item in BannerData)
        {
            item.Count = 0;
        }
    }
}
