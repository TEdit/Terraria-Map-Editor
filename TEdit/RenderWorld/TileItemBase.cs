using System;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    // Very basic stuff for most ObservableObjects
    [Serializable]
    public class TileItemBase : ObservableObject
    {
        
        protected internal int? _id;
        public virtual int? ID
        {
            get { return _id; }
            set { StandardSet(ref _id, ref value, "ID"); }
        }

        protected internal string _name;
        public virtual string Name
        {
            get { return _name; }
            set { StandardSet(ref _name, ref value, "Name"); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}