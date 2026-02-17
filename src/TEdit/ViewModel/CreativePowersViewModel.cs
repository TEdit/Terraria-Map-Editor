using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Utility;

namespace TEdit.ViewModel
{
    public partial class CreativePowersViewModel : ReactiveObject
    {
        private readonly WorldViewModel _wvm;

        public CreativePowersViewModel()
        {
            _wvm = ViewModelLocator.WorldViewModel;

            this.WhenAnyValue(x => x.SpawnRate)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SpawnRateUI)));

            this.WhenAnyValue(x => x.Difficulty)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(DifficultyUI)));

            this.WhenAnyValue(x => x.TimeSpeed)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(TimeSpeedUI)));
        }

        [ReactiveCommand]
        private void SavePowers() => SaveToWorld();

        [ReactiveCommand]
        private void LoadPowers() => LoadFromWorld();

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
            IsRainFrozen = powers.GetPowerBool(CreativePowerId.rain_setfrozen) ?? false;
            IsWindFrozen = powers.GetPowerBool(CreativePowerId.wind_setfrozen) ?? false;
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
            powers.SetPowerStateSafe(CreativePowerId.rain_setfrozen, isEnabled: IsRainFrozen);
            powers.SetPowerStateSafe(CreativePowerId.wind_setfrozen, isEnabled: IsWindFrozen);
            powers.SetPowerStateSafe(CreativePowerId.biomespread_setfrozen, isEnabled: IsBiomeSpreadFrozen);
        }

        [Reactive] private bool _enableSpawnRate;
        [Reactive] private bool _enableDifficulty;
        [Reactive] private bool _enableTimeSpeed;

        /// <summary>
        /// Two part mapping
        /// 0 - 0.5f maps to 0.1x spawn to 1x spawn rate
        /// 0.5f to 1f maps to 1x spawn to 10x spawn rate
        /// </summary>
        [Reactive] private float _spawnRate;

        public float SpawnRateUI => (SpawnRate < 0.5f) ? Calc.Remap(SpawnRate, 0.0f, 0.5f, 0.1f, 1f) : Calc.Remap(SpawnRate, 0.5f, 1f, 1f, 10f);

        /// <summary>
        /// Enemy difficulty
        /// 0: 3x master difficulty
        /// 0.3333333f: 2x expert difficulty
        /// 0.6666666f: 1x normal difficulty
        /// 1: 0.5x creative difficulty
        /// </summary>
        [Reactive] private float _difficulty;

        public float DifficultyUI => (float)System.Math.Round(
            ((double)this.Difficulty > 0.330000013113022 ?
            Calc.Remap(Difficulty, 0.33f, 1f, 1f, 3f) :
            Calc.Remap(Difficulty, 0.0f, 0.33f, 0.5f, 1f)) * 20) / 20f;

        /// <summary>
        /// 0 to 1.0f maps to 0x to 24x time rate
        /// </summary>
        [Reactive] private float _timeSpeed;

        public float TimeSpeedUI => (int)Math.Round((double)Calc.Remap(TimeSpeed, 0.0f, 1f, 1f, 24f));

        [Reactive] private bool _isBiomeSpreadFrozen;
        [Reactive] private bool _isWindFrozen;
        [Reactive] private bool _isRainFrozen;
        [Reactive] private bool _isTimeFrozen;
    }
}
