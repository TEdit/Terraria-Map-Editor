using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace TEdit.Terraria;

public partial class TileEntity : ReactiveObject
{
    public static TileEntity CreateForTile(Tile curTile, int x, int y, int id)
    {
        TileEntity TE = new TileEntity();
        TE.PosX = (short)x;
        TE.PosY = (short)y;
        TE.Id = id;
        switch (curTile.Type)
        {
            case (int)TileType.TrainingDummy:
                TE.Type = (byte)TileEntityType.TrainingDummy;
                TE.Npc = -1;
                break;
            case (int)TileType.ItemFrame:
                TE.Type = (byte)TileEntityType.ItemFrame;
                TE.NetId = 0;
                TE.Prefix = 0;
                TE.StackSize = 0;
                break;
            case (int)TileType.LogicSensor:
                TE.Type = (byte)TileEntityType.LogicSensor;
                TE.On = false;
                TE.LogicCheck = (byte)(curTile.V / 18 + 1);
                break;
            case (int)TileType.MannequinLegacy:
            case (int)TileType.WomannequinLegacy:
            case (int)TileType.DisplayDoll:
                TE.Type = (byte)TileEntityType.DisplayDoll;
                TE.Items = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 9));
                TE.Dyes = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 9));
                TE.Misc = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 1));
                TE.Pose = 0;
                break;
            case (int)TileType.WeaponRackLegacy:
            case (int)TileType.WeaponRack:
                TE.Type = (byte)TileEntityType.WeaponRack;
                TE.NetId = 0;
                TE.Prefix = 0;
                TE.StackSize = 0;
                break;
            case (int)TileType.HatRack:
                TE.Type = (byte)TileEntityType.HatRack;
                TE.Items = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 2));
                TE.Dyes = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), 2));
                break;
            case (int)TileType.FoodPlatter:
                TE.Type = (byte)TileEntityType.FoodPlatter;
                TE.NetId = 0;
                TE.Prefix = 0;
                TE.StackSize = 0;
                break;
            case (int)TileType.TeleportationPylon:
                TE.Type = (byte)TileEntityType.TeleportationPylon;
                break;
            case (int)TileType.DeadCellsDisplayJar:
                TE.Type = (byte)TileEntityType.DeadCellsDisplayJar;
                TE.NetId = 0;
                TE.Prefix = 0;
                TE.StackSize = 0;
                break;
            case (int)TileType.CritterAnchor:
                TE.Type = (byte)TileEntityType.CritterAnchor;
                TE.NetId = (int)LeashedCritters.NormalButterfly1;
                break;
            case (int)TileType.KiteAnchor:
                TE.Type = (byte)TileEntityType.KiteAnchor;
                TE.NetId = (int)LeashedKites.KiteBunny;
                break;
        }
        return TE;
    }

    private byte _type;

    [Reactive]
    private int _id;

    [Reactive]
    private Int16 _posX;

    [Reactive]
    private Int16 _posY;

    //data for when this is a dummy
    [Reactive]
    private Int16 _npc;

    //data for this is a item frame
    [Reactive]
    private int _netId;

    [Reactive]
    private byte _prefix;

    [Reactive]
    private Int16 _stackSize;

    //data for Logic Sensor
    [Reactive]
    private byte _logicCheck;

    [Reactive]
    private bool _on;

    // display doll
    public ObservableCollection<TileEntityItem> Items { get; set; } = new ObservableCollection<TileEntityItem>();
    public ObservableCollection<TileEntityItem> Dyes { get; set; } = new ObservableCollection<TileEntityItem>();
    public ObservableCollection<TileEntityItem> Misc { get; set; } = new ObservableCollection<TileEntityItem>();

    [Reactive]
    private byte _pose;

    public byte Type
    {
        get { return _type; }
        set
        {
            this.RaiseAndSetIfChanged(ref _type, value);
            this.RaisePropertyChanged(nameof(EntityType));
        }
    }

    // linked prop
    public DisplayDollPoseID DisplayDollPose
    {
        get { return (DisplayDollPoseID)_pose; }
        set
        {
            Pose = (byte)value;
            this.RaisePropertyChanged(nameof(DisplayDollPose));
        }
    }

    // linked prop
    public TileEntityType EntityType
    {
        get { return (TileEntityType)_type; }
        set
        {
            Type = (byte)value;
        }
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
                case TileEntityType.DeadCellsDisplayJar:
                    return TileType.DeadCellsDisplayJar;
                case TileEntityType.KiteAnchor:
                    return TileType.KiteAnchor;
                case TileEntityType.CritterAnchor:
                    return TileType.CritterAnchor;
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
            case TileEntityType.DeadCellsDisplayJar:
                break;
            case TileEntityType.KiteAnchor:
            case TileEntityType.CritterAnchor:
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


    public void Save(BinaryWriter bw, uint version)
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
                SaveDisplayDoll(bw, version);
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
            case TileEntityType.DeadCellsDisplayJar:
                SaveStack(bw);
                break;
            case TileEntityType.CritterAnchor:
            case TileEntityType.KiteAnchor:
                bw.Write((short)NetId); // aka item type
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
                LoadDisplayDoll(r, version);
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
            case TileEntityType.DeadCellsDisplayJar:
                LoadStack(r);
                break;
            case TileEntityType.CritterAnchor:
            case TileEntityType.KiteAnchor:
                NetId = r.ReadInt16(); // aka itemtype
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

    private void LoadDisplayDoll(BinaryReader r, uint version)
    {
        var itemSlots = (BitsByte)r.ReadByte();
        var dyeSlots = (BitsByte)r.ReadByte();

        if (version >= 307)
            _pose = r.ReadByte();

        BitsByte extraSlots = (BitsByte)(byte)0;
        if (version >= 308)
            extraSlots = (BitsByte)r.ReadByte();

        bool v311 = false;
        if (version == 311)
        {
            v311 = extraSlots[1];
            extraSlots[1] = false;
        }

        // Determine how many slots to process
        int maxSlots = (version >= 308) ? 9 : 8;

        Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Range(0, 9).Select(_ => new TileEntityItem()));
        Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Range(0, 9).Select(_ => new TileEntityItem()));
        Misc = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(Enumerable.Range(0, 1).Select(_ => new TileEntityItem()));

        // Read items - first 8 bits from itemSlots, 9th bit from extraSlots[1]
        for (int i = 0; i < maxSlots; i++)
        {
            bool hasItem = (i < 8) ? itemSlots[i] : extraSlots[1];
            if (hasItem)
            {
                Items[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }

        // Read dyes - first 8 bits from dyeSlots, 9th bit from extraSlots[2]
        for (int i = 0; i < maxSlots; i++)
        {
            bool hasDye = (i < 8) ? dyeSlots[i] : extraSlots[2];
            if (hasDye)
            {
                Dyes[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }

        // Read misc items (only in version 308+, extraSlots[0] is for Misc[0])
        for (int i = 0; i < Misc.Count; i++)
        {
            if (extraSlots[i])
            {
                Misc[i] = new TileEntityItem
                {
                    Id = r.ReadInt16(),
                    Prefix = r.ReadByte(),
                    StackSize = r.ReadInt16(),
                };
            }
        }

        // Version 311 special bug handling
        if (v311)
        {
            TileEntityItem item = Items[8];
            item.Id = r.ReadInt16();
            item.Prefix = r.ReadByte();
            item.StackSize = r.ReadInt16();
        }
    }

    private void SaveDisplayDoll(BinaryWriter w, uint gameVersion)
    {
        var itemSlots = new BitsByte();
        var dyeSlots = new BitsByte();

        // First 8 item slots
        for (int i = 0; i < 8; i++)
        {
            itemSlots[i] = Items[i]?.IsValid ?? false;
        }

        // First 8 dye slots
        for (int i = 0; i < 8; i++)
        {
            dyeSlots[i] = Dyes[i]?.IsValid ?? false;
        }

        // Write first two bytes (always present)
        w.Write((byte)itemSlots);
        w.Write((byte)dyeSlots);

        // Version 307+: write pose
        if (gameVersion >= 307)
        {
            w.Write(this._pose);
        }

        // Version 308+: write extra slots byte
        if (gameVersion >= 308)
        {
            var extraSlots = new BitsByte();
            extraSlots[0] = Misc[0]?.IsValid ?? false;  // Misc[0]
            extraSlots[1] = Items[8]?.IsValid ?? false; // 9th item slot
            extraSlots[2] = Dyes[8]?.IsValid ?? false;  // 9th dye slot

            w.Write((byte)extraSlots);
        }

        // Determine how many slots to write
        int maxSlots = (gameVersion >= 308) ? 9 : 8;

        // Write item data
        for (int i = 0; i < maxSlots; i++)
        {
            if (Items[i]?.IsValid ?? false)
            {
                w.Write(Items[i].Id);
                w.Write(Items[i].Prefix);
                w.Write(Items[i].StackSize);
            }
        }

        // Write dye data
        for (int i = 0; i < maxSlots; i++)
        {
            if (Dyes[i]?.IsValid ?? false)
            {
                w.Write(Dyes[i].Id);
                w.Write(Dyes[i].Prefix);
                w.Write(Dyes[i].StackSize);
            }
        }

        // Version 308+: write misc data
        if (gameVersion >= 308)
        {
            for (int i = 0; i < Misc.Count; i++)
            {
                if (Misc[i]?.IsValid ?? false)
                {
                    w.Write(Misc[i].Id);
                    w.Write(Misc[i].Prefix);
                    w.Write(Misc[i].StackSize);
                }
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
        frame.Pose = Pose;
        frame.NetId = NetId;
        frame.StackSize = StackSize;
        frame.Prefix = Prefix;

        frame.LogicCheck = LogicCheck;
        frame.On = On;



        if (this.Items.Count > 0)
        {
            int itemsCount = this.Items.Count;

            frame.Items = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), itemsCount));
            for (int i = 0; i < itemsCount; i++)
            {
                frame.Items[i] = Items[i]?.Copy();
            }
        }

        if (this.Dyes.Count > 0)
        {
            int dyesCount = this.Dyes.Count;

            frame.Dyes = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), dyesCount));
            for (int i = 0; i < dyesCount; i++)
            {
                frame.Dyes[i] = Dyes[i]?.Copy();
            }
        }

        if (this.Misc.Count > 0)
        {
            var miscCount = this.Misc.Count;
            frame.Misc = new ObservableCollection<TileEntityItem>(Enumerable.Repeat(new TileEntityItem(), miscCount));
            for (int i = 0; i < miscCount; i++)
            {
                frame.Misc[i] = Misc[i]?.Copy();
            }
        }

        return frame;
    }

    // WPF binding properties. Only needed since each slot can be something different.
    public TileEntityItem Item0 { get { return (Items.Count > 0) ? Items[0] : null; } set { if (Items.Count > 0) { Items[0] = value; this.RaisePropertyChanged(nameof(Item0)); } } }
    public TileEntityItem Item1 { get { return (Items.Count > 1) ? Items[1] : null; } set { if (Items.Count > 1) { Items[1] = value; this.RaisePropertyChanged(nameof(Item1)); } } }
    public TileEntityItem Item2 { get { return (Items.Count > 2) ? Items[2] : null; } set { if (Items.Count > 2) { Items[2] = value; this.RaisePropertyChanged(nameof(Item2)); } } }
    public TileEntityItem Item3 { get { return (Items.Count > 3) ? Items[3] : null; } set { if (Items.Count > 3) { Items[3] = value; this.RaisePropertyChanged(nameof(Item3)); } } }
    public TileEntityItem Item4 { get { return (Items.Count > 4) ? Items[4] : null; } set { if (Items.Count > 4) { Items[4] = value; this.RaisePropertyChanged(nameof(Item4)); } } }
    public TileEntityItem Item5 { get { return (Items.Count > 5) ? Items[5] : null; } set { if (Items.Count > 5) { Items[5] = value; this.RaisePropertyChanged(nameof(Item5)); } } }
    public TileEntityItem Item6 { get { return (Items.Count > 6) ? Items[6] : null; } set { if (Items.Count > 6) { Items[6] = value; this.RaisePropertyChanged(nameof(Item6)); } } }
    public TileEntityItem Item7 { get { return (Items.Count > 7) ? Items[7] : null; } set { if (Items.Count > 7) { Items[7] = value; this.RaisePropertyChanged(nameof(Item7)); } } }

    public TileEntityItem Dye0 { get { return (Dyes.Count > 0) ? Dyes[0] : null; } set { if (Dyes.Count > 0) { Dyes[0] = value; this.RaisePropertyChanged(nameof(Dye0)); } } }
    public TileEntityItem Dye1 { get { return (Dyes.Count > 1) ? Dyes[1] : null; } set { if (Dyes.Count > 1) { Dyes[1] = value; this.RaisePropertyChanged(nameof(Dye1)); } } }
    public TileEntityItem Dye2 { get { return (Dyes.Count > 2) ? Dyes[2] : null; } set { if (Dyes.Count > 2) { Dyes[2] = value; this.RaisePropertyChanged(nameof(Dye2)); } } }
    public TileEntityItem Dye3 { get { return (Dyes.Count > 3) ? Dyes[3] : null; } set { if (Dyes.Count > 3) { Dyes[3] = value; this.RaisePropertyChanged(nameof(Dye3)); } } }
    public TileEntityItem Dye4 { get { return (Dyes.Count > 4) ? Dyes[4] : null; } set { if (Dyes.Count > 4) { Dyes[4] = value; this.RaisePropertyChanged(nameof(Dye4)); } } }
    public TileEntityItem Dye5 { get { return (Dyes.Count > 5) ? Dyes[5] : null; } set { if (Dyes.Count > 5) { Dyes[5] = value; this.RaisePropertyChanged(nameof(Dye5)); } } }
    public TileEntityItem Dye6 { get { return (Dyes.Count > 6) ? Dyes[6] : null; } set { if (Dyes.Count > 6) { Dyes[6] = value; this.RaisePropertyChanged(nameof(Dye6)); } } }
    public TileEntityItem Dye7 { get { return (Dyes.Count > 7) ? Dyes[7] : null; } set { if (Dyes.Count > 7) { Dyes[7] = value; this.RaisePropertyChanged(nameof(Dye7)); } } }

    public TileEntityItem WeaponDye { get { return (Dyes.Count > 8) ? Dyes[8] : null; } set { if (Dyes.Count > 8) { Dyes[8] = value; this.RaisePropertyChanged(nameof(WeaponDye)); } } }
    public TileEntityItem Weapon { get { return (Items.Count > 8) ? Items[8] : null; } set { if (Items.Count > 8) { Items[8] = value; this.RaisePropertyChanged(nameof(Weapon)); } } }
    public TileEntityItem Mount { get { return (Misc.Count > 0) ? Misc[0] : null; } set { if (Misc.Count > 0) { Misc[0] = value; this.RaisePropertyChanged(nameof(Mount)); } } }
}
