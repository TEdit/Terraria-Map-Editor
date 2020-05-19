using System;
using System.Collections.ObjectModel;
using System.IO;
using GalaSoft.MvvmLight;
using TEdit.Terraria;

namespace TEdit.Terraria
{
    public class TileEntityItem : ObservableObject
    {
        private short _id;
        private byte _prefix;
        private short _stackSize;

        public short Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public byte Prefix
        {
            get { return _prefix; }
            set { Set("Prefix", ref _prefix, value); }
        }

        public short StackSize
        {
            get { return _stackSize; }
            set { Set("StackSize", ref _stackSize, value); }
        }

        public bool IsValid => StackSize > 0 && Id > 0;

        public TileEntityItem Copy() => new TileEntityItem
        {
            Id = this.Id,
            Prefix = this.Prefix,
            StackSize = this.StackSize
        };
    }

    [Serializable]
    public class TileEntity : ObservableObject
    {
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
            set { Set("Type", ref _type, value); }
        }

        public int Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public Int16 PosX
        {
            get { return _x; }
            set { Set("PosX", ref _x, value); }
        }

        public Int16 PosY
        {
            get { return _y; }
            set { Set("PosY", ref _y, value); }
        }

        public Int16 Npc
        {
            get { return _npc; }
            set { Set("Npc", ref _npc, value); }
        }


        public int NetId
        {
            get { return _netId; }
            set { Set("NetId", ref _netId, value); }
        }

        public byte Prefix
        {
            get { return _prefix; }
            set { Set("Prefix", ref _prefix, value); }
        }

        public Int16 StackSize
        {
            get { return _stackSize; }
            set { Set("StackSize", ref _stackSize, value); }
        }

        public byte LogicCheck
        {
            get { return _logicCheck; }
            set { Set("LogicCheck", ref _logicCheck, value); }
        }

        public bool On
        {
            get { return _on; }
            set { Set("On", ref _on, value); }
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(Type);
            bw.Write(Id);
            bw.Write(PosX);
            bw.Write(PosY);
            switch (Type)
            {
                case 0: //it is a dummy
                    bw.Write(Npc);
                    break;
                case 1: //it is a item frame
                    bw.Write((Int16)NetId);
                    bw.Write(Prefix);
                    bw.Write(StackSize);
                    break;
                case 2: //it is a logic sensor
                    bw.Write(LogicCheck);
                    bw.Write(On);
                    break;
                case 3: // display doll
                    SaveSlots(bw, 8);
                    break;
                case 4: // weapons rack 
                    bw.Write((Int16)NetId);
                    bw.Write(Prefix);
                    bw.Write(StackSize);
                    break;
                case 5: // hat rack 
                    SaveSlots(bw, 2);
                    break;
                case 6: // food platter
                    bw.Write((Int16)NetId);
                    bw.Write(Prefix);
                    bw.Write(StackSize);
                    break;
                case 7: // teleportation pylon

                    break;
            }
        }

        public void Load(BinaryReader r, uint version)
        {
            Type = r.ReadByte();
            Id = r.ReadInt32();
            PosX = r.ReadInt16();
            PosY = r.ReadInt16();
            switch (Type)
            {
                case 0: //it is a dummy
                    Npc = r.ReadInt16();
                    break;
                case 1: //it is a item frame
                    NetId = (int)r.ReadInt16();
                    Prefix = r.ReadByte();
                    StackSize = r.ReadInt16();
                    break;
                case 2: //it is a logic sensor
                    LogicCheck = r.ReadByte();
                    On = r.ReadBoolean();
                    break;
                case 3: // display doll
                    LoadSlots(r, 8);
                    break;
                case 4: // weapons rack 
                    NetId = (int)r.ReadInt16();
                    Prefix = r.ReadByte();
                    StackSize = r.ReadInt16();
                    break;
                case 5: // hat rack 
                    LoadSlots(r, 2);
                    break;
                case 6: // food platter
                    NetId = (int)r.ReadInt16();
                    Prefix = r.ReadByte();
                    StackSize = r.ReadInt16();
                    break;
                case 7: // teleportation pylon

                    break;
            }
        }

        private void LoadSlots(BinaryReader r, byte slots)
        {
            var itemSlots = (BitsByte)r.ReadByte();
            var dyeSlots = (BitsByte)r.ReadByte();
            Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(new TileEntityItem[slots]);
            Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(new TileEntityItem[slots]);
            for (int i = 0; i < slots; i++)
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
            for (int i = 0; i < slots; i++)
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

        private void SaveSlots(BinaryWriter w, byte slots)
        {
            var items = new BitsByte();
            var dyes = new BitsByte();
            for (int i = 0; i < slots; i++)
            {
                items[i] = Items[0]?.IsValid ?? false;
            }
            for (int i = 0; i < slots; i++)
            {
                items[i] = Dyes[0]?.IsValid ?? false;
            }

            w.Write((byte)items);
            w.Write((byte)dyes);

            for (int i = 0; i < 8; i++)
            {
                if (Items[0]?.IsValid ?? false)
                {
                    w.Write(Items[0].Id);
                    w.Write(Items[0].Prefix);
                    w.Write(Items[0].StackSize);
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (Dyes[0]?.IsValid ?? false)
                {
                    w.Write(Dyes[0].Id);
                    w.Write(Dyes[0].Prefix);
                    w.Write(Dyes[0].StackSize);
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

            if (this.Items.Count > 0)
            {
                frame.Items = new ObservableCollection<TileEntityItem>(new TileEntityItem[Items.Count]);
                for (int i = 0; i < Items.Count; i++)
                {
                    frame.Items[i] = Items[i].Copy();
                }
            }

            if (this.Dyes.Count > 0)
            {
                frame.Dyes = new ObservableCollection<TileEntityItem>(new TileEntityItem[Dyes.Count]);
                for (int i = 0; i < Dyes.Count; i++)
                {
                    frame.Dyes[i] = Dyes[i].Copy();
                }
            }

            return frame;
        }


    }
}
