using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using TEdit.Helper;
using TEdit.Terraria;

namespace TEdit.ViewModel
{

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

    public class CreativePowersViewModel : ObservableObject
    {
        private readonly WorldViewModel _wvm;

        private bool _IsTimeFrozen = false;
        private bool _IsGodMode = false;
        private bool _IsRainFrozen = false;
        private bool _IsWindFrozen = false;
        private bool _IsIncreasePlacementRange = false;
        private bool _IsBiomeSpreadFrozen = false;

        private float _TimeSpeed = 0f;
        private float _Difficulty = 0f;
        private float _SpawnRate = 0f;

        private bool _EnableTimeSpeed = false;
        private bool _EnableDifficulty = false;
        private bool _EnableSpawnRate = false;

        private ICommand _SavePowersCommand;
        private ICommand _LoadPowersCommand;

        public ICommand SavePowersCommand
        {
            get { return _SavePowersCommand ??= new RelayCommand(() => SaveToWorld()); }
        }

        public ICommand LoadPowersCommand
        {
            get { return _LoadPowersCommand ??= new RelayCommand(() => LoadFromWorld()); }
        }

        public CreativePowersViewModel()
        {
            _wvm = ViewModelLocator.WorldViewModel;
        }


        public void LoadFromWorld()
        {
            var powers = _wvm?.CurrentWorld?.CreativePowers;
            if (powers == null) { return; }

            var spawnRate = powers.GetPowerFloat(CreativePowers.CreativePowerId.setspawnrate);
            EnableSpawnRate = (spawnRate != null);
            SpawnRate = spawnRate ?? 0f;

            var timeSpeed = powers.GetPowerFloat(CreativePowers.CreativePowerId.time_setspeed);
            EnableTimeSpeed = (timeSpeed != null);
            TimeSpeed = timeSpeed ?? 0f;

            var setdifficulty = powers.GetPowerFloat(CreativePowers.CreativePowerId.setdifficulty);
            EnableDifficulty = (setdifficulty != null);
            Difficulty = setdifficulty ?? 0f;

            IsTimeFrozen = powers.GetPowerBool(CreativePowers.CreativePowerId.time_setfrozen) ?? false;
            IsGodMode = powers.GetPowerBool(CreativePowers.CreativePowerId.godmode) ?? false;
            IsRainFrozen = powers.GetPowerBool(CreativePowers.CreativePowerId.rain_setfrozen) ?? false;
            IsWindFrozen = powers.GetPowerBool(CreativePowers.CreativePowerId.wind_setfrozen) ?? false;
            IsIncreasePlacementRange = powers.GetPowerBool(CreativePowers.CreativePowerId.increaseplacementrange) ?? false;
            IsBiomeSpreadFrozen = powers.GetPowerBool(CreativePowers.CreativePowerId.biomespread_setfrozen) ?? false;
        }

        public void SaveToWorld()
        {
            var powers = _wvm?.CurrentWorld?.CreativePowers;
            if (powers == null) { return; }

            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.setspawnrate, value: EnableSpawnRate ? SpawnRate : null);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.time_setspeed, value: EnableTimeSpeed ? TimeSpeed : null);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.setdifficulty, value: EnableDifficulty ? Difficulty : null);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.time_setfrozen, isEnabled: IsTimeFrozen);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.godmode, isEnabled: IsGodMode);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.rain_setfrozen, isEnabled: IsRainFrozen);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.wind_setfrozen, isEnabled: IsWindFrozen);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.increaseplacementrange, isEnabled: IsIncreasePlacementRange);
            powers.SetPowerStateSafe(CreativePowers.CreativePowerId.biomespread_setfrozen, isEnabled: IsBiomeSpreadFrozen);
        }

        public bool EnableSpawnRate
        {
            get { return _EnableSpawnRate; }
            set { Set(nameof(EnableSpawnRate), ref _EnableSpawnRate, value); }
        }
        public bool EnableDifficulty
        {
            get { return _EnableDifficulty; }
            set { Set(nameof(EnableDifficulty), ref _EnableDifficulty, value); }
        }
        public bool EnableTimeSpeed
        {
            get { return _EnableTimeSpeed; }
            set { Set(nameof(EnableTimeSpeed), ref _EnableTimeSpeed, value); }
        }

        public float SpawnRate
        {
            get { return _SpawnRate; }
            set { Set(nameof(SpawnRate), ref _SpawnRate, value); }
        }
        public float Difficulty
        {
            get { return _Difficulty; }
            set { Set(nameof(Difficulty), ref _Difficulty, value); }
        }

        public float TimeSpeed
        {
            get { return _TimeSpeed; }
            set { Set(nameof(TimeSpeed), ref _TimeSpeed, value); }
        }

        public bool IsBiomeSpreadFrozen
        {
            get { return _IsBiomeSpreadFrozen; }
            set { Set(nameof(IsBiomeSpreadFrozen), ref _IsBiomeSpreadFrozen, value); }
        }

        public bool IsIncreasePlacementRange
        {
            get { return _IsIncreasePlacementRange; }
            set { Set(nameof(IsIncreasePlacementRange), ref _IsIncreasePlacementRange, value); }
        }
        public bool IsWindFrozen
        {
            get { return _IsWindFrozen; }
            set { Set(nameof(IsWindFrozen), ref _IsWindFrozen, value); }
        }

        public bool IsRainFrozen
        {
            get { return _IsRainFrozen; }
            set { Set(nameof(IsRainFrozen), ref _IsRainFrozen, value); }
        }
        public bool IsGodMode
        {
            get { return _IsGodMode; }
            set { Set(nameof(IsGodMode), ref _IsGodMode, value); }
        }
        public bool IsTimeFrozen
        {
            get { return _IsTimeFrozen; }
            set { Set(nameof(IsTimeFrozen), ref _IsTimeFrozen, value); }
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

                foreach (string line in World.BestiaryData.BestiaryKilledIDs)
                {
                    // Prevent writing to values already 50 or over.
                    _wvm.CurrentWorld.Bestiary.NPCKills.TryGetValue(line, out var kills);
                    bestiaryEdits.NPCKills[line] = (kills < 50) ? 50 : kills;
                }
                foreach (string line in World.BestiaryData.BestiaryTalkedIDs)
                {
                    bestiaryEdits.NPCChat.Add(line);
                }
                foreach (string line in World.BestiaryData.BestiaryNearIDs)
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

            foreach (string entity in World.BestiaryData.BestiaryKilledIDs
                                            .Union(World.BestiaryData.BestiaryNearIDs)
                                            .Union(World.BestiaryData.BestiaryTalkedIDs)
                                            .Distinct()
                                            .OrderBy(e => e))
            {
                _wvm.CurrentWorld.Bestiary.NPCKills.TryGetValue(entity, out var kills);
                bool near = _wvm.CurrentWorld.Bestiary.NPCNear.Contains(entity);
                bool talked = _wvm.CurrentWorld.Bestiary.NPCChat.Contains(entity);

                BestiaryData.Add(new BestiaryItem
                {
                    CanKill = World.BestiaryData.BestiaryKilledIDs.Contains(entity),
                    CanNear = World.BestiaryData.BestiaryNearIDs.Contains(entity),
                    CanTalk = World.BestiaryData.BestiaryTalkedIDs.Contains(entity),
                    Name = entity,
                    Defeated = kills,
                    Near = near,
                    Talked = talked,
                    Unlocked = kills > 0 || near || talked,

                });
            }
        }

    }
}
