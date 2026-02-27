using System.Linq;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Player;

public partial class PlayerItem : ReactiveObject
{
    private int _netId;
    private int _stackSize;
    private byte _prefix;

    [Reactive]
    private bool _favorited;

    public int NetId
    {
        get => _netId;
        set
        {
            this.RaiseAndSetIfChanged(ref _netId, value);
            var items = WorldConfiguration.ItemProperties;
            ItemProperty match = null;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == _netId) { match = items[i]; break; }
            }
            _currentItemProperty = match;
            if (_netId == 0)
                StackSize = 0;
            else if (StackSize == 0)
                StackSize = 1;
            this.RaisePropertyChanged(nameof(Name));
        }
    }

    public int StackSize
    {
        get => _stackSize;
        set
        {
            int validValue = value;
            if (validValue < 0) validValue = 0;
            if (validValue > short.MaxValue) validValue = short.MaxValue;
            this.RaiseAndSetIfChanged(ref _stackSize, validValue);
        }
    }

    public byte Prefix
    {
        get => _prefix;
        set
        {
            this.RaiseAndSetIfChanged(ref _prefix, value);
            this.RaisePropertyChanged(nameof(PrefixName));
        }
    }

    private ItemProperty _currentItemProperty;

    public string Name => _currentItemProperty?.Name ?? "[empty]";

    public string PrefixName =>
        WorldConfiguration.ItemPrefix.Count > Prefix
            ? WorldConfiguration.ItemPrefix[Prefix]
            : "Unknown " + Prefix;

    public PlayerItem() { }

    public PlayerItem(int netId, int stackSize, byte prefix = 0, bool favorited = false)
    {
        StackSize = stackSize;
        NetId = stackSize > 0 ? netId : 0;
        Prefix = prefix;
        Favorited = favorited;
    }

    public Item ToItem() => new(StackSize, NetId, Prefix);

    public static PlayerItem FromItem(Item item) =>
        new(item.NetId, item.StackSize, item.Prefix);

    public PlayerItem Copy() =>
        new(_netId, _stackSize, _prefix, _favorited);

    public override string ToString()
    {
        if (StackSize > 0)
            return $"{Name}: {StackSize}";
        return Name;
    }
}
