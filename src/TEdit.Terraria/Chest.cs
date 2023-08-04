using System.Collections.ObjectModel;
using TEdit.Common.Reactive;

namespace TEdit.Terraria;

public class Chest : ObservableObject
{
    public static int MaxItems = 40; 
    public static int LegacyLimit = 1000;

    public Chest()
    {
        for (int i = 0; i < MaxItems; i++)
        {
            _items.Add(new Item());
        }
    }
    public Chest(int x, int y)
        : this()
    {
        _x = x;
        _y = y;
    }

    public Chest(int x, int y, string name)
        : this()
    {
        _x = x;
        _y = y;
        _name = name;
    }


    private int _x;
    private int _y;

    private string _name = string.Empty;
    private int _chestId = -1;

    

    public string Name
    {
        get { return _name; }
        set { Set(nameof(Name), ref _name, value); }
    }
    public int Y
    {
        get { return _y; }
        set { Set(nameof(Y), ref _y, value); }
    }

    public int X
    {
        get { return _x; }
        set { Set(nameof(X), ref _x, value); }
    }

    private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
    public ObservableCollection<Item> Items
    {
        get { return _items; }
    }

    public Chest Copy()
    {
        var chest = new Chest(_x, _y);
        chest.Name = Name;
        //chest.Items.Clear();
        for (int i = 0; i < MaxItems; i++)
        {
            if (Items.Count > i)
                chest.Items[i] = Items[i].Copy();
            else
            {
                chest.Items[i] = new Item();
            }
        }

        return chest;
    }

    public override string ToString()
    {
        return $"[Chest: ({X},{Y})]";
    }
}
