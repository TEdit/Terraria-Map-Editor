using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TEdit.Helper;
using TEdit.Terraria;


namespace TEdit.ViewModel;


public partial class BestiaryItem : ReactiveObject
{
    [Reactive]
    private string _name;
    private string _fullName;
    public string FullName
    {
        get => _fullName;
        set => this.RaiseAndSetIfChanged(ref _fullName, value);
    }

    private Int32 _defeated;
    public Int32 Defeated
    {
        get => _defeated;
        set
        {
            if (!CanKill) { value = 0; }
            this.RaiseAndSetIfChanged(ref _defeated, value);
        }
    }

    [Reactive]
    private bool _unlocked;

    private bool _near;
    public bool Near
    {
        get => _near;
        set
        {
            if (!CanNear) { value = false; }
            this.RaiseAndSetIfChanged(ref _near, value);
        }
    }

    private bool _talked;
    public bool Talked
    {
        get => _talked;
        set
        {
            if (!CanTalk) { value = false; }
            this.RaiseAndSetIfChanged(ref _talked, value);
        }
    }

    private bool _canTalk;
    public bool CanTalk
    {
        get => _canTalk;
        set
        {
            if (!value) { Talked = false; }
            this.RaiseAndSetIfChanged(ref _canTalk, value);
        }
    }

    private bool _canNear;
    public bool CanNear
    {
        get => _canNear;
        set
        {
            if (!value) { Near = false; }
            this.RaiseAndSetIfChanged(ref _canNear, value);
        }
    }

    private bool _canKill = true;
    public bool CanKill
    {
        get => _canKill;
        set
        {
            if (!value) { Defeated = 0; }
            this.RaiseAndSetIfChanged(ref _canKill, value);
        }
    }
}


public partial class BestiaryViewModel : ReactiveObject
{
    private readonly WorldViewModel _wvm;

    [Reactive]
    private string _name;

    public BestiaryViewModel()
    {
        _wvm = ViewModelLocator.WorldViewModel;
    }

    [Reactive]
    private ObservableCollection<BestiaryItem> _bestiaryData = new ObservableCollection<BestiaryItem>();

    [ReactiveCommand]
    private void UpdateKillTally()
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

    [ReactiveCommand]
    private void LoadBestiary()
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

            // determine display name (use FullName from npc data when available)
            string displayName = entity;
            if (WorldConfiguration.BestiaryData.NpcData.TryGetValue(entity, out var npcData) && !string.IsNullOrEmpty(npcData.FullName))
            {
                displayName = npcData.FullName;
            }

            BestiaryData.Add(new BestiaryItem
            {
                CanKill = WorldConfiguration.BestiaryData.BestiaryKilledIDs.Contains(entity),
                CanNear = WorldConfiguration.BestiaryData.BestiaryNearIDs.Contains(entity),
                CanTalk = WorldConfiguration.BestiaryData.BestiaryTalkedIDs.Contains(entity),
                Name = entity,
                FullName = displayName,
                Defeated = kills,
                Near = near,
                Talked = talked,
                Unlocked = kills > 0 || near || talked,

            });
        }
    }

    [ReactiveCommand]
    private void CompleteBestiary()
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
                bestiaryEdits.NPCKills[line] = (kills < 150) ? 150 : kills;
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

    [ReactiveCommand]
    private void ResetBestiary()
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
    /// Save Bestiary data back to world
    /// </summary>
    [ReactiveCommand]
    private void SaveBestiary()
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
}
