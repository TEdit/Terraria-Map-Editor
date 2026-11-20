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
using TEdit.Render;
using TEdit.Reactive;
using TEdit.Terraria;

namespace TEdit.ViewModel;

public partial class BannerViewModel : ReactiveObject
{
    private readonly WorldViewModel _wvm;

    public BannerViewModel()
    {
        _wvm = ViewModelLocator.WorldViewModel;

        BannerData = new ObservableCollection<BannerItem>();
        Categories = new ObservableCollection<string> { AllCategories };

        BannersView = CollectionViewSource.GetDefaultView(BannerData);
        BannersView.Filter = FilterBanners;
        BannersView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BannerItem.LocalizedCategory)));

        this.WhenAnyValue(x => x.FilterText, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOnMainThread()
            .Subscribe(_ => BannersView.Refresh());

        // Auto-populate banners when a world is opened
        _wvm.WhenAnyValue(x => x.CurrentWorld)
            .Where(w => w != null)
            .ObserveOnMainThread()
            .Subscribe(_ => PopulateBanners());

        NpcPreviewCache.PreviewsLoaded += OnNpcPreviewsLoaded;
    }

    private void OnNpcPreviewsLoaded()
    {
        foreach (var item in BannerData)
        {
            item.NotifyPreviewChanged();
        }
    }

    [Reactive]
    private ObservableCollection<BannerItem> _bannerData;

    [Reactive]
    private string _filterText = "";

    /// <summary>"All" filter label, localized like the categories.</summary>
    private static readonly string AllCategories = LocalizeCategory("All");

    [Reactive]
    private string _selectedCategory = AllCategories;

    public ObservableCollection<string> Categories { get; }

    public ICollectionView BannersView { get; }

    /// <summary>Falls back to the raw value when the locale has no entry.</summary>
    private static string LocalizeCategory(string category) =>
        string.IsNullOrEmpty(category) ? category : UiLabelResolver.ResolveBestiaryCategory(category) ?? category;

    private bool FilterBanners(object obj)
    {
        if (obj is not BannerItem item) return false;

        if (!string.IsNullOrEmpty(SelectedCategory) &&
            SelectedCategory != AllCategories &&
            !string.Equals(item.LocalizedCategory, SelectedCategory, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(FilterText) &&
            !item.LocalizedName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.LocalizedCategory.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
            !item.NpcId.ToString().Contains(FilterText, StringComparison.Ordinal) &&
            !item.BestiaryIndex.ToString().Contains(FilterText, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    public void ApplySort(string propertyName)
    {
        var direction = ListSortDirection.Ascending;

        if (BannersView.SortDescriptions.Count > 0 &&
            BannersView.SortDescriptions[0].PropertyName == propertyName)
        {
            direction = BannersView.SortDescriptions[0].Direction == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }

        BannersView.SortDescriptions.Clear();
        BannersView.SortDescriptions.Add(new SortDescription(propertyName, direction));
    }

    /// <summary>
    /// Populates the banner list from the current world data.
    /// Called automatically when a world is loaded.
    /// </summary>
    private void PopulateBanners()
    {
        using var operation = _wvm.BeginWorldOperation("PopulateBanners", mutation: false);
        if (operation == null) return;
        if (_wvm.CurrentWorld == null) return;

        BannerData.Clear();
        Categories.Clear();
        Categories.Add(AllCategories);

        // Build lookup: bannerId -> first NPC data
        var npcByBanner = WorldConfiguration.BestiaryData.NpcById.Values
            .Where(n => n.BannerId >= 0)
            .GroupBy(n => n.BannerId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var kvp in WorldConfiguration.TallyNames.OrderBy(k => k.Key))
        {
            int tallyIndex = kvp.Key;
            string bannerName = kvp.Value;
            string displayName = WorldConfiguration.TallyLocalizedNames.TryGetValue(tallyIndex, out var localizedBanner) ? localizedBanner : bannerName;

            // Get category, NPC ID, and bestiary index from bestiary data
            string category = "";
            int npcId = 0;
            int bestiaryIndex = 0;
            if (npcByBanner.TryGetValue(tallyIndex, out var npcData))
            {
                category = npcData.Category ?? "";
                npcId = npcData.Id;
                bestiaryIndex = npcData.BestiaryDisplayIndex;
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
                LocalizedName = displayName,
                Category = string.IsNullOrEmpty(category) ? "Other" : category,
                LocalizedCategory = LocalizeCategory(string.IsNullOrEmpty(category) ? "Other" : category),
                NpcId = npcId,
                BestiaryIndex = bestiaryIndex,
                Kills = kills,
                Count = bannerCount,
            });
        }

        // Populate category filter list
        foreach (var cat in BannerData.Select(b => b.LocalizedCategory).Distinct().OrderBy(c => c))
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
        using var operation = _wvm.BeginWorldOperation("ImportBanners");
        if (operation == null) return;
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_import_banners_title,
            Properties.Language.dialog_import_banners_prompt);

        if (!confirm)
            return;

        var ofd = new OpenFileDialog
        {
            Filter = "Terraria World File|*.wld",
            DefaultExt = "Terraria World File|*.wld",
            Title = Properties.Language.dialog_import_banners,
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
            await App.DialogService.ShowExceptionAsync(string.Format(Properties.Language.dialog_import_banners_error, ofd.FileName, ex.Message));
        }
    }

    [ReactiveCommand]
    private async Task SaveBanners()
    {
        using var operation = _wvm.BeginWorldOperation("SaveBanners");
        if (operation == null) return;
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_save_banners_title,
            Properties.Language.dialog_save_banners_prompt);

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
            await App.DialogService.ShowExceptionAsync(string.Format(Properties.Language.dialog_save_banners_error, ex.Message));
        }
    }

    [ReactiveCommand]
    private async Task MaxOutBanners()
    {
        using var operation = _wvm.BeginWorldOperation("MaxOutBanners");
        if (operation == null) return;
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_max_banners_title,
            Properties.Language.dialog_max_banners_prompt);

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
        using var operation = _wvm.BeginWorldOperation("ResetBanners");
        if (operation == null) return;
        if (_wvm.CurrentWorld == null) return;

        var confirm = await App.DialogService.ShowConfirmationAsync(
            Properties.Language.dialog_reset_banners_title,
            Properties.Language.dialog_reset_banners_prompt);

        if (!confirm)
            return;

        foreach (var item in BannerData)
        {
            item.Count = 0;
        }
    }
}
