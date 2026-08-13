using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Terraria.DataModel;

namespace TEdit.Editor;

public partial class MorphToolOptions : ReactiveObject
{
    private MorphMode _mode = MorphMode.SafeConvert;

    public MorphMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;
            this.RaiseAndSetIfChanged(ref _mode, value);
            this.RaisePropertyChanged(nameof(TargetBiomes));
            if (!TargetBiomes.Contains(TargetBiome))
            {
                TargetBiome = TargetBiomes.FirstOrDefault() ?? string.Empty;
            }
        }
    }

    [Reactive]
    private string _targetBiome = "Purify";
    [Reactive]
    private int _mossType = 179;
    [Reactive]
    private bool _enableBaseTiles = true;
    [Reactive]
    private bool _enableEvilTiles = true;
    [Reactive]
    private bool _enableMoss = true;
    [Reactive]
    private bool _enableSprites = true;
    [Reactive]
    private bool _enableDecoSprites = true;

    public IReadOnlyList<string> TargetBiomes => Mode == MorphMode.GenerateBiome
        ? WorldConfiguration.DestructiveBiomes
        : WorldConfiguration.SafeBiomes;
}
