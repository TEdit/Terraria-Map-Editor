using System;
using System.Windows.Input;
using TEdit.Common.Reactive;
using TEdit.Common.Reactive.Command;
using TEdit.Terraria;
using TEdit.Utility;

namespace TEdit.ViewModel
{
    public class CreativePowersViewModel : ObservableObject
    {
        private readonly WorldViewModel _wvm;

        private bool _IsTimeFrozen = false;
        //private bool _IsGodMode = false;
        private bool _IsRainFrozen = false;
        private bool _IsWindFrozen = false;
        //private bool _IsIncreasePlacementRange = false;
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

            var spawnRate = powers.GetPowerFloat(CreativePowerId.setspawnrate);
            EnableSpawnRate = (spawnRate != null);
            SpawnRate = spawnRate ?? 0f;

            var timeSpeed = powers.GetPowerFloat(CreativePowerId.time_setspeed);
            EnableTimeSpeed = (timeSpeed != null);
            TimeSpeed = timeSpeed ?? 0f;

            var setdifficulty = powers.GetPowerFloat(CreativePowerId.setdifficulty);
            EnableDifficulty = (setdifficulty != null);
            Difficulty = setdifficulty ?? 0f;

            IsTimeFrozen = powers.GetPowerBool(CreativePowerId.time_setfrozen) ?? false;
            //IsGodMode = powers.GetPowerBool(CreativePowerId.godmode) ?? false;
            IsRainFrozen = powers.GetPowerBool(CreativePowerId.rain_setfrozen) ?? false;
            IsWindFrozen = powers.GetPowerBool(CreativePowerId.wind_setfrozen) ?? false;
            //IsIncreasePlacementRange = powers.GetPowerBool(CreativePowerId.increaseplacementrange) ?? false;
            IsBiomeSpreadFrozen = powers.GetPowerBool(CreativePowerId.biomespread_setfrozen) ?? false;
        }

        public void SaveToWorld()
        {
            var powers = _wvm?.CurrentWorld?.CreativePowers;
            if (powers == null) { return; }

            powers.SetPowerStateSafe(CreativePowerId.setspawnrate, value: EnableSpawnRate ? SpawnRate : null);
            powers.SetPowerStateSafe(CreativePowerId.time_setspeed, value: EnableTimeSpeed ? TimeSpeed : null);
            powers.SetPowerStateSafe(CreativePowerId.setdifficulty, value: EnableDifficulty ? Difficulty : null);
            powers.SetPowerStateSafe(CreativePowerId.time_setfrozen, isEnabled: IsTimeFrozen);
            //powers.SetPowerStateSafe(CreativePowerId.godmode, isEnabled: IsGodMode);
            powers.SetPowerStateSafe(CreativePowerId.rain_setfrozen, isEnabled: IsRainFrozen);
            powers.SetPowerStateSafe(CreativePowerId.wind_setfrozen, isEnabled: IsWindFrozen);
            //powers.SetPowerStateSafe(CreativePowerId.increaseplacementrange, isEnabled: IsIncreasePlacementRange);
            powers.SetPowerStateSafe(CreativePowerId.biomespread_setfrozen, isEnabled: IsBiomeSpreadFrozen);
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

        /// <summary>
        /// Two part mapping
        /// 0 - 0.5f maps to 0.1x spawn to 1x spawn rate
        /// 0.5f to 1f maps to 1x spawn to 10x spawn rate
        /// </summary>
        public float SpawnRate
        {
            get { return _SpawnRate; }
            set
            {
                Set(nameof(SpawnRate), ref _SpawnRate, value);
                RaisePropertyChanged(nameof(SpawnRateUI));
            }
        }

        public float SpawnRateUI => (SpawnRate < 0.5f) ? Calc.Remap(SpawnRate, 0.0f, 0.5f, 0.1f, 1f) : Calc.Remap(SpawnRate, 0.5f, 1f, 1f, 10f);

        /// <summary>
        /// Enemy difficulty
        /// 0: 3x master difficulty
        /// 0.3333333f: 2x expert difficulty
        /// 0.6666666f: 1x normal difficulty
        /// 1: 0.5x creative difficulty
        /// </summary>
        public float Difficulty
        {
            get { return _Difficulty; }
            set
            {
                Set(nameof(Difficulty), ref _Difficulty, value);
                RaisePropertyChanged(nameof(DifficultyUI));
            }
        }

        public float DifficultyUI => (float)System.Math.Round(
            ((double)this.Difficulty > 0.330000013113022 ?
            Calc.Remap(Difficulty, 0.33f, 1f, 1f, 3f) :
            Calc.Remap(Difficulty, 0.0f, 0.33f, 0.5f, 1f)) * 20) / 20f;


        /// <summary>
        /// 0 to 1.0f maps to 0x to 24x time rate
        /// </summary>
        public float TimeSpeed
        {
            get { return _TimeSpeed; }
            set
            {
                Set(nameof(TimeSpeed), ref _TimeSpeed, value);
                RaisePropertyChanged(nameof(TimeSpeedUI));
            }
        }

        public float TimeSpeedUI => (int)Math.Round((double)Calc.Remap(TimeSpeed, 0.0f, 1f, 1f, 24f));

        public bool IsBiomeSpreadFrozen
        {
            get { return _IsBiomeSpreadFrozen; }
            set { Set(nameof(IsBiomeSpreadFrozen), ref _IsBiomeSpreadFrozen, value); }
        }

        //public bool IsIncreasePlacementRange
        //{
        //    get { return _IsIncreasePlacementRange; }
        //    set { Set(nameof(IsIncreasePlacementRange), ref _IsIncreasePlacementRange, value); }
        //}
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

        //public bool IsGodMode
        //{
        //    get { return _IsGodMode; }
        //    set { Set(nameof(IsGodMode), ref _IsGodMode, value); }
        //}
        public bool IsTimeFrozen
        {
            get { return _IsTimeFrozen; }
            set { Set(nameof(IsTimeFrozen), ref _IsTimeFrozen, value); }
        }
    }
}
