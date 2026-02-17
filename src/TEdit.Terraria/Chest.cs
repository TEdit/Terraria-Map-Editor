using System.Collections.ObjectModel;

namespace TEdit.Terraria;

public partial class Chest : ReactiveObject
{
    public static int LegacyMaxItems = 40;
    public static int LegacyLimit = 1000;

    public Chest()
    {
        for (int i = 0; i < LegacyMaxItems; i++)
        {
            Items.Add(new Item());
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

    [Reactive]
    private int _x;

    [Reactive]
    private int _y;

    [Reactive]
    private int _maxItems = 40;

    [Reactive]
    private string _name = string.Empty;

    private int _chestId = -1;

    public ObservableCollection<Item> Items { get; } = [];


    public Chest Copy()
    {
        var chest = new Chest(_x, _y);
        chest.Name = Name;
        //chest.Items.Clear();
        for (int i = 0; i < LegacyMaxItems; i++)
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
