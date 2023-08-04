using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TEdit.Common.Reactive;
using TEdit.Configuration;

namespace TEdit.Terraria;

public class TileEntity : ObservableObject
{
    public static TileEntity CreateForTile(Tile curTile, int x, int y, int id)
    {
        TileEntity TE = new TileEntity();
        TE.PosX = (short)x;
        TE.PosY = (short)y;
        TE.Id = id;
        if (curTile.Type == (int)TileType.TrainingDummy)
        {
            TE.Type = 0;
            TE.Npc = -1;
        }
        else if (curTile.Type == (int)TileType.ItemFrame)
        {
            TE.Type = 1;
            TE.NetId = 0;
            TE.Prefix = 0;
            TE.StackSize = 0;
        }
        else if (curTile.Type == (int)TileType.LogicSensor)
        {
            TE.Type = 2;
            TE.On = false;
            TE.LogicCheck = (byte)(curTile.V / 18 + 1);
        }
        else if (curTile.Type == (int)TileType.MannequinLegacy || curTile.Type == (int)TileType.WomannequinLegacy || curTile.Type == (int)TileType.DisplayDoll)
        {
            TE.Type = 3;
            TE.Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 8));
            TE.Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 8));
        }
        else if (curTile.Type == (int)TileType.WeaponRackLegacy || curTile.Type == (int)TileType.WeaponRack)
        {
            TE.Type = 4;
            TE.NetId = 0;
            TE.Prefix = 0;
            TE.StackSize = 0;
        }
        else if (curTile.Type == (int)TileType.HatRack)
        {
            TE.Type = 5;
            TE.Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 2));
            TE.Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 2));
        }
        else if (curTile.Type == (int)TileType.FoodPlatter)
        {
            TE.Type = 6;
            TE.NetId = 0;
            TE.Prefix = 0;
            TE.StackSize = 0;
        }
        else if (curTile.Type == (int)TileType.TeleportationPylon)
        {
            TE.Type = 7;
        }
        return TE;
    }

    private byte _type;
    private int _id;
    private Int16 _x;
    private Int16 _y;

    //data for when this is a dummy
    private Int16 _npc;

    //data for this is a item frame
    private int _netId;
    private byte _prefix;
    private Int16 _stackSize;

    //data for Logic Sensor
    private byte _logicCheck;
    private bool _on;

    public ObservableCollection<TileEntityItem> Items { get; set; } = new ObservableCollection<TileEntityItem>();
    public ObservableCollection<TileEntityItem> Dyes { get; set; } = new ObservableCollection<TileEntityItem>();

    public byte Type
    {
        get { return _type; }
        set
        {
            Set(nameof(Type), ref _type, value);
            RaisePropertyChanged(nameof(EntityType));
        }
    }

    public int Id
    {
        get { return _id; }
        set { Set(nameof(Id), ref _id, value); }
    }

    public TileEntityType EntityType
    {
        get { return (TileEntityType)_type; }
        set
        {
            Set(nameof(Type), ref _type, (byte)value);
            RaisePropertyChanged(nameof(EntityType));
        }
    }

    public Int16 PosX
    {
        get { return _x; }
        set { Set(nameof(PosX), ref _x, value); }
    }

    public Int16 PosY
    {
        get { return _y; }
        set { Set(nameof(PosY), ref _y, value); }
    }

    public Int16 Npc
    {
        get { return _npc; }
        set { Set(nameof(Npc), ref _npc, value); }
    }

    public int NetId
    {
        get { return _netId; }
        set { Set(nameof(NetId), ref _netId, value); }
    }

    public byte Prefix
    {
        get { return _prefix; }
        set { Set(nameof(Prefix), ref _prefix, value); }
    }

    public Int16 StackSize
    {
        get { return _stackSize; }
        set { Set(nameof(StackSize), ref _stackSize, value); }
    }

    public byte LogicCheck
    {
        get { return _logicCheck; }
        set { Set(nameof(LogicCheck), ref _logicCheck, value); }
    }

    public bool On
    {
        get { return _on; }
        set { Set(nameof(On), ref _on, value); }
    }

    public TileType TileType
    {
        get
        {
            switch (EntityType)
            {
                case TileEntityType.TrainingDummy:
                    return TileType.TrainingDummy;
                case TileEntityType.ItemFrame:
                    return TileType.ItemFrame;
                case TileEntityType.LogicSensor:
                    return TileType.LogicSensor;
                case TileEntityType.DisplayDoll:
                    return TileType.DisplayDoll;
                case TileEntityType.WeaponRack:
                    return TileType.WeaponRack;
                case TileEntityType.HatRack:
                    return TileType.HatRack;
                case TileEntityType.FoodPlatter:
                    return TileType.FoodPlatter;
                case TileEntityType.TeleportationPylon:
                    return TileType.TeleportationPylon;
                default:
                    return 0;
            }
        }
    }

    private static void AddEntityToWorld(TileEntity te, World world)
    {
        // remove existing tile entity entry
        var existingEntity = world.TileEntities.FirstOrDefault(existing => existing.PosX == te.PosX && existing.PosY == te.PosY);
        if (existingEntity != null)
        {
            world.TileEntities.Remove(existingEntity);
            te.Id = existingEntity.Id;
        }
        else if (world.TileEntities.Count > 0)
        {
            te.Id = world.TileEntities.Max(entity => entity.Id) + 1;
        }
        else
        {
            te.Id = 0;
        }

        // add new tile entity entry
        world.TileEntities.Add(te);
    }

    private static void PostAddEntityToWorld(TileEntity te, World world)
    {
        // custom logic per type
        switch (te.EntityType)
        {
            case TileEntityType.TrainingDummy:
                break;
            case TileEntityType.ItemFrame:
                break;
            case TileEntityType.LogicSensor:
                break;
            case TileEntityType.DisplayDoll:
                break;
            case TileEntityType.WeaponRack:
                break;
            case TileEntityType.HatRack:
                break;
            case TileEntityType.FoodPlatter:
                break;
            case TileEntityType.TeleportationPylon:
                break;
        }
    }

    public static void PlaceEntity(TileEntity te, World world)
    {
        if (te == null) return;
        if (world == null) return;

        AddEntityToWorld(te, world);
        PostAddEntityToWorld(te, world);
    }


    public void Save(BinaryWriter bw)
    {
        bw.Write(Type);
        bw.Write(Id);
        bw.Write(PosX);
        bw.Write(PosY);
        switch (EntityType)
        {
            case TileEntityType.TrainingDummy: //it is a dummy
                bw.Write(Npc);
                break;
            case TileEntityType.ItemFrame: //it is a item frame
                SaveStack(bw);
                break;
            case TileEntityType.LogicSensor: //it is a logic sensor
                bw.Write(LogicCheck);
                bw.Write(On);
                break;
            case TileEntityType.DisplayDoll: // display doll
                SaveDisplayDoll(bw);
                break;
            case TileEntityType.WeaponRack: // weapons rack 
                SaveStack(bw);
                break;
            case TileEntityType.HatRack: // hat rack 
                SaveHatRack(bw);
                break;
            case TileEntityType.FoodPlatter: // food platter
                SaveStack(bw);
                break;
            case TileEntityType.TeleportationPylon: // teleportation pylon

                break;
        }
    }

    public void Load(BinaryReader r, uint version)
    {
        Type = r.ReadByte();
        Id = r.ReadInt32();
        PosX = r.ReadInt16();
        PosY = r.ReadInt16();
        switch (EntityType)
        {
            case TileEntityType.TrainingDummy: //it is a dummy

                Npc = r.ReadInt16();
                break;
            case TileEntityType.ItemFrame: //it is a item frame
                LoadStack(r);
                break;
            case TileEntityType.LogicSensor: //it is a logic sensor
                LogicCheck = r.ReadByte();
                On = r.ReadBoolean();
                break;
            case TileEntityType.DisplayDoll: // display doll
                LoadDisplayDoll(r);
                break;
            case TileEntityType.WeaponRack: // weapons rack 
                LoadStack(r);
                break;
            case TileEntityType.HatRack: // hat rack 
                LoadHatRack(r);
                break;
            case TileEntityType.FoodPlatter: // food platter
                LoadStack(r);

                break;
            case TileEntityType.TeleportationPylon: // teleportation pylon

                break;
        }
    }

    private void SaveStack(BinaryWriter w)
    {
        w.Write((short)NetId);
        w.Write((byte)Prefix);
        w.Write((short)StackSize);
    }

    private void LoadStack(BinaryReader r)
    {
        NetId = (int)r.ReadInt16();
        Prefix = r.ReadByte();
        StackSize = r.ReadInt16();
    }

    private void LoadHatRack(BinaryReader r)
    {
        byte numSlots = 2;
        var slots = (BitsByte)r.ReadByte();
        Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), numSlots));
        Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), numSlots));
        for (int i = 0; i < numSlots; i++)
        {
            if (slots[i])
            {
                Items[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }
        for (int i = 0; i < numSlots; i++)
        {
            if (slots[i + 2])
            {
                Dyes[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }
    }

    private void SaveHatRack(BinaryWriter w)
    {
        byte numSlots = 2;
        var slots = new BitsByte();
        for (int i = 0; i < numSlots; i++)
        {
            slots[i] = Items[i]?.IsValid ?? false;
        }
        for (int i = 0; i < numSlots; i++)
        {
            slots[i + 2] = Dyes[i]?.IsValid ?? false;
        }

        w.Write((byte)slots);

        for (int i = 0; i < numSlots; i++)
        {
            if (slots[i])
            {
                w.Write(Items[i].Id);
                w.Write(Items[i].Prefix);
                w.Write(Items[i].StackSize);
            }
        }
        for (int i = 0; i < numSlots; i++)
        {
            if (slots[i + 2])
            {
                w.Write(Dyes[i].Id);
                w.Write(Dyes[i].Prefix);
                w.Write(Dyes[i].StackSize);
            }
        }
    }

    private void LoadDisplayDoll(BinaryReader r)
    {
        byte numSlots = 8;
        var itemSlots = (BitsByte)r.ReadByte();
        var dyeSlots = (BitsByte)r.ReadByte();
        Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), numSlots));
        Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), numSlots));
        for (int i = 0; i < numSlots; i++)
        {
            if (itemSlots[i])
            {
                Items[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }
        for (int i = 0; i < numSlots; i++)
        {
            if (dyeSlots[i])
            {
                Dyes[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }
    }

    private void SaveDisplayDoll(BinaryWriter w)
    {
        byte numSlots = 8;
        var items = new BitsByte();
        var dyes = new BitsByte();
        for (int i = 0; i < numSlots; i++)
        {
            items[i] = Items[i]?.IsValid ?? false;
        }
        for (int i = 0; i < numSlots; i++)
        {
            dyes[i] = Dyes[i]?.IsValid ?? false;
        }

        w.Write((byte)items);
        w.Write((byte)dyes);

        for (int i = 0; i < numSlots; i++)
        {
            if (items[i])
            {
                w.Write(Items[i].Id);
                w.Write(Items[i].Prefix);
                w.Write(Items[i].StackSize);
            }
        }
        for (int i = 0; i < numSlots; i++)
        {
            if (dyes[i])
            {
                w.Write(Dyes[i].Id);
                w.Write(Dyes[i].Prefix);
                w.Write(Dyes[i].StackSize);
            }
        }
    }

    public TileEntity Copy()
    {
        var frame = new TileEntity();
        frame.Type = Type;
        frame.PosX = PosX;
        frame.PosY = PosY;

        frame.Npc = Npc;

        frame.NetId = NetId;
        frame.StackSize = StackSize;
        frame.Prefix = Prefix;

        frame.LogicCheck = LogicCheck;
        frame.On = On;

        int slots = frame.EntityType == TileEntityType.DisplayDoll ? 8 : frame.EntityType == TileEntityType.HatRack ? 2 : 0;

        if (this.Items.Count > 0)
        {
            frame.Items = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), slots));
            for (int i = 0; i < Items.Count; i++)
            {
                frame.Items[i] = Items[i]?.Copy();
            }
        }

        if (this.Dyes.Count > 0)
        {
            frame.Dyes = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), slots));
            for (int i = 0; i < Dyes.Count; i++)
            {
                frame.Dyes[i] = Dyes[i]?.Copy();
            }
        }

        return frame;
    }

    // WPF binding properties. Only needed since each slog can be something different.
    public TileEntityItem Item0 { get { return (Items.Count > 0) ? Items[0] : null; } set { if (Items.Count > 0) { Items[0] = value; RaisePropertyChanged(nameof(Item0)); } } }
    public TileEntityItem Item1 { get { return (Items.Count > 1) ? Items[1] : null; } set { if (Items.Count > 1) { Items[1] = value; RaisePropertyChanged(nameof(Item1)); } } }
    public TileEntityItem Item2 { get { return (Items.Count > 2) ? Items[2] : null; } set { if (Items.Count > 2) { Items[2] = value; RaisePropertyChanged(nameof(Item2)); } } }
    public TileEntityItem Item3 { get { return (Items.Count > 3) ? Items[3] : null; } set { if (Items.Count > 3) { Items[3] = value; RaisePropertyChanged(nameof(Item3)); } } }
    public TileEntityItem Item4 { get { return (Items.Count > 4) ? Items[4] : null; } set { if (Items.Count > 4) { Items[4] = value; RaisePropertyChanged(nameof(Item4)); } } }
    public TileEntityItem Item5 { get { return (Items.Count > 5) ? Items[5] : null; } set { if (Items.Count > 5) { Items[5] = value; RaisePropertyChanged(nameof(Item5)); } } }
    public TileEntityItem Item6 { get { return (Items.Count > 6) ? Items[6] : null; } set { if (Items.Count > 6) { Items[6] = value; RaisePropertyChanged(nameof(Item6)); } } }
    public TileEntityItem Item7 { get { return (Items.Count > 7) ? Items[7] : null; } set { if (Items.Count > 7) { Items[7] = value; RaisePropertyChanged(nameof(Item7)); } } }

    public TileEntityItem Dye0 { get { return (Dyes.Count > 0) ? Dyes[0] : null; } set { if (Dyes.Count > 0) { Dyes[0] = value; RaisePropertyChanged(nameof(Dye0)); } } }
    public TileEntityItem Dye1 { get { return (Dyes.Count > 1) ? Dyes[1] : null; } set { if (Dyes.Count > 1) { Dyes[1] = value; RaisePropertyChanged(nameof(Dye1)); } } }
    public TileEntityItem Dye2 { get { return (Dyes.Count > 2) ? Dyes[2] : null; } set { if (Dyes.Count > 2) { Dyes[2] = value; RaisePropertyChanged(nameof(Dye2)); } } }
    public TileEntityItem Dye3 { get { return (Dyes.Count > 3) ? Dyes[3] : null; } set { if (Dyes.Count > 3) { Dyes[3] = value; RaisePropertyChanged(nameof(Dye3)); } } }
    public TileEntityItem Dye4 { get { return (Dyes.Count > 4) ? Dyes[4] : null; } set { if (Dyes.Count > 4) { Dyes[4] = value; RaisePropertyChanged(nameof(Dye4)); } } }
    public TileEntityItem Dye5 { get { return (Dyes.Count > 5) ? Dyes[5] : null; } set { if (Dyes.Count > 5) { Dyes[5] = value; RaisePropertyChanged(nameof(Dye5)); } } }
    public TileEntityItem Dye6 { get { return (Dyes.Count > 6) ? Dyes[6] : null; } set { if (Dyes.Count > 6) { Dyes[6] = value; RaisePropertyChanged(nameof(Dye6)); } } }
    public TileEntityItem Dye7 { get { return (Dyes.Count > 7) ? Dyes[7] : null; } set { if (Dyes.Count > 7) { Dyes[7] = value; RaisePropertyChanged(nameof(Dye7)); } } }
}
