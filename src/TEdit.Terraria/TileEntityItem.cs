using System.Collections.Generic;
using TEdit.Common.IO;

namespace TEdit.Terraria;

public partial class TileEntityItem : ReactiveObject
{
    private short _id;
    private short _stackSize;

    [Reactive]
    private byte _prefix;

    // Mod item identity (null for vanilla items)
    private string _modName;
    public string ModName
    {
        get => _modName;
        set { this.RaiseAndSetIfChanged(ref _modName, value); this.RaisePropertyChanged(nameof(Name)); this.RaisePropertyChanged(nameof(IsModItem)); }
    }

    private string _modItemName;
    public string ModItemName
    {
        get => _modItemName;
        set { this.RaiseAndSetIfChanged(ref _modItemName, value); this.RaisePropertyChanged(nameof(Name)); }
    }

    // Mod prefix identity (null if vanilla or no prefix)
    public string ModPrefixMod { get; set; }
    public string ModPrefixName { get; set; }

    // Opaque per-item mod data and global data (preserved for round-trip)
    public TagCompound ModItemData { get; set; }
    public List<TagCompound> ModGlobalData { get; set; }

    public bool IsModItem => !string.IsNullOrEmpty(ModName) && ModName != "Terraria";

    public string Name
    {
        get
        {
            if (IsModItem)
                return $"{ModName}:{ModItemName}";
            return null;
        }
    }

    public short Id
    {
        get { return _id; }
        set
        {
            // Clear mod identity when setting a vanilla item
            _modName = null;
            _modItemName = null;
            ModPrefixMod = null;
            ModPrefixName = null;
            ModItemData = null;
            ModGlobalData = null;

            this.RaiseAndSetIfChanged(ref _id, value);
            if (value != 0 && StackSize == 0) { StackSize = 1; }
            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(IsModItem));
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

    public bool IsValid => (StackSize > 0 && Id > 0) || (IsModItem && StackSize > 0);

    public TileEntityItem Copy() => new TileEntityItem
    {
        _id = this._id,
        _prefix = this._prefix,
        _stackSize = this._stackSize,
        _modName = this._modName,
        _modItemName = this._modItemName,
        ModPrefixMod = this.ModPrefixMod,
        ModPrefixName = this.ModPrefixName,
        ModItemData = this.ModItemData,
        ModGlobalData = this.ModGlobalData,
    };

    public static implicit operator TileEntityItem(Item item)
    {
        return new TileEntityItem
        {
            _id = (short)item.NetId,
            Prefix = item.Prefix,
            _stackSize = (short)item.StackSize,
            _modName = item.ModName,
            _modItemName = item.ModItemName,
            ModPrefixMod = item.ModPrefixMod,
            ModPrefixName = item.ModPrefixName,
            ModItemData = item.ModItemData,
            ModGlobalData = item.ModGlobalData,
        };
    }

    public Item ToItem()
    {
        var item = new Item(StackSize, Id, Prefix);
        if (IsModItem)
        {
            item.ModName = _modName;
            item.ModItemName = _modItemName;
            item.ModPrefixMod = ModPrefixMod;
            item.ModPrefixName = ModPrefixName;
            item.ModItemData = ModItemData;
            item.ModGlobalData = ModGlobalData;
        }
        return item;
    }
}
