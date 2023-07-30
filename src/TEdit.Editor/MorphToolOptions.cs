using TEdit.Common.Reactive;

namespace TEdit.Editor;

public class MorphToolOptions : ObservableObject
{
    private string _TargetBiome = "Purify";
    private int _MossType = 179;
    private bool _EnableBaseTiles = true;
    private bool _EnableEvilTiles = true;
    private bool _EnableMoss = true;
    private bool _EnableSprites = true;

    public int MossType
    {
        get { return _MossType; }
        set { Set(nameof(MossType), ref _MossType, value); }
    }

    public bool EnableSprites
    {
        get { return _EnableSprites; }
        set { Set(nameof(EnableSprites), ref _EnableSprites, value); }
    }

    public bool EnableMoss
    {
        get { return _EnableMoss; }
        set { Set(nameof(EnableMoss), ref _EnableMoss, value); }
    }

    public bool EnableEvilTiles
    {
        get { return _EnableEvilTiles; }
        set { Set(nameof(EnableEvilTiles), ref _EnableEvilTiles, value); }
    }

    public bool EnableBaseTiles
    {
        get { return _EnableBaseTiles; }
        set { Set(nameof(EnableBaseTiles), ref _EnableBaseTiles, value); }
    }

    public string TargetBiome
    {
        get { return _TargetBiome; }
        set { Set(nameof(TargetBiome), ref _TargetBiome, value); }
    }
}
