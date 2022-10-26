using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using TEdit.Terraria;

namespace TEdit.ViewModel
{
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
}
