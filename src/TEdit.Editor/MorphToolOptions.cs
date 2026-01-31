using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TEdit.Editor;

public partial class MorphToolOptions : ReactiveObject
{
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
}
