namespace TEdit.Terraria;

public partial class TileEntityItem : ReactiveObject
{
    private short _id;
    private short _stackSize;

    [Reactive]
    private byte _prefix;

    public short Id
    {
        get { return _id; }
        set
        {
            this.RaiseAndSetIfChanged(ref _id, value);
            if (value != 0 && StackSize == 0) { StackSize = 1; }
        }
    }

    public short StackSize
    {
        get { return _stackSize; }
        set
        {
            this.RaiseAndSetIfChanged(ref _stackSize, value);
            if (value == 0 && Id != 0)
            {
                Id = 0;
                Prefix = 0;
            }
        }
    }

    public bool IsValid => StackSize > 0 && Id > 0;

    public TileEntityItem Copy() => new TileEntityItem
    {
        Id = this.Id,
        Prefix = this.Prefix,
        StackSize = this.StackSize
    };

    public static implicit operator TileEntityItem(Item item)
    {
        return new TileEntityItem
        {
            Id = (short)item.NetId,
            Prefix = item.Prefix,
            StackSize = (short)item.StackSize
        };
    }

    public Item ToItem()
    {
        return new Item(StackSize, Id, Prefix);
    }
}
