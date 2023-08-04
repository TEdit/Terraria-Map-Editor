using TEdit.Common.Reactive;

namespace TEdit.Terraria;

public class TileEntityItem : ObservableObject
{
    private short _id;
    private byte _prefix;
    private short _stackSize;

    public short Id
    {
        get { return _id; }
        set
        {
            Set(nameof(Id), ref _id, value);
            if (value != 0 && StackSize == 0) { StackSize = 1; }
        }
    }

    public byte Prefix
    {
        get { return _prefix; }
        set { Set(nameof(Prefix), ref _prefix, value); }
    }

    public short StackSize
    {
        get { return _stackSize; }
        set
        {
            Set(nameof(StackSize), ref _stackSize, value);
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
}
