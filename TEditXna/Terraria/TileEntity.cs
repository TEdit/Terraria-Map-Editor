using System;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria
{
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

        public TileEntity Copy()
        {
            var frame = new TileEntity();
            frame.Type = Type;
            frame.PosX = PosX;
            frame.PosY = PosY;
            switch (Type)
            {
                case 0:
                    frame.Npc = Npc;
                    break;
                case 1:
                    frame.NetId = NetId;
                    frame.StackSize = StackSize;
                    frame.Prefix = Prefix;
                    break;
                case 2:
                    frame.LogicCheck = LogicCheck;
                    frame.On = On;
                    break;
            }
            return frame;
        }


    }
}
