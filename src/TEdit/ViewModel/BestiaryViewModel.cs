using TEdit.Common.Reactive;
using TEdit.Common.Reactive.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using TEdit.Helper;
using TEdit.Terraria;
using TEdit.Configuration;

namespace TEdit.ViewModel;


public class BestiaryItem : ObservableObject
{
    private string _name;
    private Int32 _defeated;
    private bool _unlocked;
    private bool _near;
    private bool _talked;

    private bool _canTalk;
    private bool _canNear;
    private bool _canKill = true;

    public string Name { get => _name; set => Set(nameof(Name), ref _name, value); }
    public Int32 Defeated
    {
        get => _defeated;
        set
        {
            if (!CanKill) { value = 0; }
            Set(nameof(Defeated), ref _defeated, value);
        }
    }
    public bool Unlocked { get => _unlocked; set => Set(nameof(Unlocked), ref _unlocked, value); }
    public bool Near
    {
        get => _near;
        set
        {
            if (!CanNear) { value = false; }
            Set(nameof(Near), ref _near, value);
        }
    }
    public bool Talked
    {
        get => _talked;
        set
        {
            if (!CanTalk) { value = false; }
            Set(nameof(Talked), ref _talked, value);
        }
    }

    public bool CanTalk
    {
        get => _canTalk;
        set
        {
            if (!value) { Talked = false; }
            Set(nameof(CanTalk), ref _canTalk, value);
        }
    }
    public bool CanNear
    {
        get => _canNear;
        set
        {
            if (!value) { Near = false; }
            Set(nameof(CanNear), ref _canNear, value);
        }
    }
    public bool CanKill
    {
        get => _canKill;
        set
        {
            if (!value) { Defeated = 0; }
            Set(nameof(CanKill), ref _canKill, value);
        }
    }
}


public class BestiaryViewModel : ObservableObject
{
    private readonly WorldViewModel _wvm;
    private string _name;
    public BestiaryViewModel()
    {
        _wvm = ViewModelLocator.WorldViewModel;
    }

    private ObservableCollection<BestiaryItem> _bestiaryData = new ObservableCollection<BestiaryItem>();
    public ObservableCollection<BestiaryItem> BestiaryData { get => _bestiaryData; set => Set(nameof(BestiaryData), ref _bestiaryData, value); }


    private ICommand _completeBestiaryCommand;
    private ICommand _saveBestiaryCommand;
    private ICommand _resetBestiaryCommand;
    private ICommand _loadBestiaryCommand;
    private ICommand _updateKillTallyCommand;
    public ICommand UpdateKillTallyCommand
    {
        get { return _updateKillTallyCommand ??= new RelayCommand<bool>(o => UpdateKillTally()); }
    }

    public ICommand LoadBestiaryCommand
    {
        get { return _loadBestiaryCommand ??= new RelayCommand<bool>(o => LoadBestiary()); }
    }
    public ICommand CompleteBestiaryCommand
    {
        get { return _completeBestiaryCommand ??= new RelayCommand<bool>(o => CompleteBestiary()); }
    }
    public ICommand ResetBestiaryCommand
    {
        get { return _resetBestiaryCommand ??= new RelayCommand<bool>(o => ResetBestiary()); }
    }
    public ICommand SaveBestiaryCommand
    {
        get { return _saveBestiaryCommand ??= new RelayCommand<bool>(o => SaveBestiary()); }
    }

    public string Name { get => _name; set => Set(nameof(Name), ref _name, value); }

    public void UpdateKillTally()
    {
        if (_wvm.CurrentWorld == null) { return; }

        // clear out kill tally
        for (int i = 0; i < _wvm.CurrentWorld.KilledMobs.Count; i++) {
            _wvm.CurrentWorld.KilledMobs[i] = 0;
        }

        foreach (var item in BestiaryData)
        {
            var bestiaryId = item.Name;
            if (WorldConfiguration.BestiaryData.NpcData.TryGetValue(bestiaryId, out var npcData) &&
                _wvm.CurrentWorld.KilledMobs.Count > npcData.BannerId &&
                npcData.BannerId >= 0)
            {
                _wvm.CurrentWorld.KilledMobs[npcData.BannerId] += item.Defeated;
            }
        }
    }

    public void CompleteBestiary()
    {
        if (_wvm.CurrentWorld == null) { return; }

        if (MessageBox.Show(
            "This will completely replace your currently loaded world Bestiary and Kill Tally with a completed bestiary. Continue?",
            "Complete Bestiary?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.Yes) != MessageBoxResult.Yes)
            return;

        // make a backup
        var bestiary = _wvm.CurrentWorld.Bestiary.Copy(_wvm.CurrentWorld.Version);
        var killTally = _wvm.CurrentWorld.KilledMobs.ToArray();
        try
        {
            ErrorLogging.TelemetryClient?.TrackEvent(nameof(CompleteBestiary));
            var bestiaryEdits = new Bestiary();

            foreach (string line in WorldConfiguration.BestiaryData.BestiaryKilledIDs)
            {
                // Prevent writing to values already 50 or over.
                _wvm.CurrentWorld.Bestiary.NPCKills.TryGetValue(line, out var kills);
                bestiaryEdits.NPCKills[line] = (kills < 50) ? 50 : kills;
            }
            foreach (string line in WorldConfiguration.BestiaryData.BestiaryTalkedIDs)
            {
                bestiaryEdits.NPCChat.Add(line);
            }
            foreach (string line in WorldConfiguration.BestiaryData.BestiaryNearIDs)
            {
                bestiaryEdits.NPCNear.Add(line);
            }

            _wvm.CurrentWorld.Bestiary = bestiaryEdits;
            LoadBestiary();
        }
        catch (Exception ex)
        {
            // revert to backup
            _wvm.CurrentWorld.Bestiary = bestiary;
            _wvm.CurrentWorld.KilledMobs.Clear();
            _wvm.CurrentWorld.KilledMobs.AddRange(killTally);
            MessageBox.Show($"Error completing Bestiary data. Your current bestiary has been restored.\r\n{ex.Message}");
        }
    }

    /// <summary>
    /// Save Bestiary data back to world
    /// </summary>
    public void SaveBestiary()
    {
        if (_wvm.CurrentWorld == null) { return; }

        if (MessageBox.Show(
            "Are you sure you wish to save Bestiary changes?",
            "Save Bestiary?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.Yes) != MessageBoxResult.Yes)
            return;

        // make a backup
        var bestiary = _wvm.CurrentWorld.Bestiary.Copy(_wvm.CurrentWorld.Version);
        var killTally = _wvm.CurrentWorld.KilledMobs.ToArray();
        try
        {
            ErrorLogging.TelemetryClient?.TrackEvent(nameof(SaveBestiary));

            var bestiaryEdits = new Bestiary();

            foreach (var line in BestiaryData)
            {
                if (line.Defeated > 0) { bestiaryEdits.NPCKills[line.Name] = line.Defeated; }
                if (line.Talked) { bestiaryEdits.NPCChat.Add(line.Name); }
                if (line.Near) { bestiaryEdits.NPCNear.Add(line.Name); }
            }
            _wvm.CurrentWorld.Bestiary = bestiaryEdits;
            LoadBestiary();
        }
        catch (Exception ex)
        {
            // revert to backup
            _wvm.CurrentWorld.Bestiary = bestiary;
            _wvm.CurrentWorld.KilledMobs.Clear();
            _wvm.CurrentWorld.KilledMobs.AddRange(killTally);
            MessageBox.Show($"Error completing Bestiary data. Your current bestiary has been restored.\r\n{ex.Message}");
        }
    }

    /// <summary>
    /// Reset the Bestiary
    /// </summary>
    public void ResetBestiary()
    {
        if (_wvm.CurrentWorld == null) { return; }

        if (MessageBox.Show(
            "This will completely replace your currently loaded world Bestiary and Kill Tally with a reset bestiary. Continue?",
            "Reset Bestiary?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.Yes) != MessageBoxResult.Yes)
            return;

        // make a backup
        var bestiary = _wvm.CurrentWorld.Bestiary.Copy(_wvm.CurrentWorld.Version);
        var killTally = _wvm.CurrentWorld.KilledMobs.ToArray();
        try
        {
            ErrorLogging.TelemetryClient?.TrackEvent(nameof(ResetBestiary));
            var bestiaryEdits = new Bestiary();
            _wvm.CurrentWorld.Bestiary = bestiaryEdits;
            _wvm.CurrentWorld.KilledMobs.Clear();
            LoadBestiary();
        }
        catch (Exception ex)
        {
            // revert to backup
            _wvm.CurrentWorld.Bestiary = bestiary;
            _wvm.CurrentWorld.KilledMobs.Clear();
            _wvm.CurrentWorld.KilledMobs.AddRange(killTally);
            MessageBox.Show($"Error completing Bestiary data. Your current bestiary has been restored.\r\n{ex.Message}");
        }
    }


    /// <summary>
    /// Load Bestiary from World Data
    /// </summary>
    public void LoadBestiary()
    {
        if (_wvm.CurrentWorld == null) { return; }

        BestiaryData.Clear();

        foreach (string entity in WorldConfiguration.BestiaryData.BestiaryKilledIDs
                                        .Union(WorldConfiguration.BestiaryData.BestiaryNearIDs)
                                        .Union(WorldConfiguration.BestiaryData.BestiaryTalkedIDs)
                                        .Distinct()
                                        .OrderBy(e => e))
        {
            _wvm.CurrentWorld.Bestiary.NPCKills.TryGetValue(entity, out var kills);
            bool near = _wvm.CurrentWorld.Bestiary.NPCNear.Contains(entity);
            bool talked = _wvm.CurrentWorld.Bestiary.NPCChat.Contains(entity);

            BestiaryData.Add(new BestiaryItem
            {
                CanKill = WorldConfiguration.BestiaryData.BestiaryKilledIDs.Contains(entity),
                CanNear = WorldConfiguration.BestiaryData.BestiaryNearIDs.Contains(entity),
                CanTalk = WorldConfiguration.BestiaryData.BestiaryTalkedIDs.Contains(entity),
                Name = entity,
                Defeated = kills,
                Near = near,
                Talked = talked,
                Unlocked = kills > 0 || near || talked,

            });
        }
    }

}
