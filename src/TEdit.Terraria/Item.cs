using System;
using System.Linq;
using TEdit.Common.Reactive;
using TEdit.Configuration;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria;

public class Item : ObservableObject
{
    private const int MaxStackSize = Int16.MaxValue;

    private int _stackSize;
    private byte _prefix;
    private int _netId;


    public int NetId
    {
        get { return _netId; }
        set
        {
            Set(nameof(NetId), ref _netId, value);
            _currentItemProperty = WorldConfiguration.ItemProperties.FirstOrDefault(x => x.Id == _netId);
            if (_netId == 0)
                StackSize = 0;
            else
            {
                if (StackSize == 0)
                    StackSize = 1;
            }
            RaisePropertyChanged("Name");
        }
    }

    public void SetFromName(string name)
    {
        var curItem = WorldConfiguration.ItemProperties.FirstOrDefault(x => x.Name == name);
        NetId = curItem.Id;
        if (NetId != 0)
            StackSize = 1;
    }

    public string Name
    {
        get { return GetName(); }
    }

    public string PrefixName
    {
        get { return WorldConfiguration.ItemPrefix.Count > Prefix ? WorldConfiguration.ItemPrefix[Prefix] : "Unknown " + Prefix.ToString(); }
    }

    public string GetName()
    {
        if (_currentItemProperty != null)
            return _currentItemProperty.Name;

        return "[empty]";
    }

    public byte Prefix
    {
        get { return _prefix; }
        set { Set(nameof(Prefix), ref _prefix, value); RaisePropertyChanged("PrefixName"); }
    }

    public Item()
    {
        StackSize = 0;
        NetId = 0;
    }

    public Item(int stackSize, int netId)
    {
        StackSize = stackSize;
        NetId = stackSize > 0 ? netId : 0;
    }

    public Item(int stackSize, int netId, byte prefix)
    {
        StackSize = stackSize;
        NetId = stackSize > 0 ? netId : 0;
        Prefix = prefix;
    }

    public static implicit operator Item(TileEntityItem tileEntityItem)
    {
        return new Item(tileEntityItem.StackSize, tileEntityItem.Id, tileEntityItem.Prefix);
    }

    public TileEntityItem ToTileEntityItem()
    {
        return new TileEntityItem
        {
            Id = (short)NetId,
            Prefix = Prefix,
            StackSize = (short)StackSize
        };
    }

    private ItemProperty _currentItemProperty;
    public int StackSize
    {
        get { return _stackSize; }
        set
        {
            int max = MaxStackSize;
            if (_currentItemProperty != null && _currentItemProperty.MaxStackSize > 0)
            {
                max = _currentItemProperty.MaxStackSize;
            }

            int validValue = value;
            if (validValue > max)
                validValue = max;
            if (validValue < 0)
                validValue = 0;

            Set(nameof(StackSize), ref _stackSize, validValue);
        }
    }

    public Item Copy()
    {
        return new Item(_stackSize, _netId) { Prefix = _prefix };
    }

    //public Visibility IsVisible
    //{
    //    get { return _netId == 0 ? Visibility.Collapsed : Visibility.Visible; }
    //}

    public override string ToString()
    {
        if (StackSize > 0)
            return $"{_currentItemProperty?.Name}: {StackSize}";

        return _currentItemProperty?.Name;
    }
}
