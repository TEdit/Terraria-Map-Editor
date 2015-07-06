using System;
using TEdit.Geometry.Primitives;
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
        private Int16 _itemNetId;
        private byte _prefix;
        private Int16 _stack;

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


        public Int16 ItemNetId
        {
            get { return _itemNetId; }
            set { Set("ItemNetId", ref _itemNetId, value); }
        }

        public byte Prefix
        {
            get { return _prefix; }
            set { Set("Prefix", ref _prefix, value); }
        }

        public Int16 Stack
        {
            get { return _stack; }
            set { Set("Stack", ref _stack, value); }
        }
    }
}