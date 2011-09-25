using System;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    // Very basic stuff for most ObservableObjects
    [Serializable]
    public class OOProperty : ObservableObject
    {
        
        private int? _id;
        public int? ID
        {
            get { return _id; }
            set { StandardSet<int?>(ref _id, ref value, "ID"); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { StandardSet<string>(ref _name, ref value, "Name"); }
        }

        public override string ToString()
        {
            return Name;
        }

    }
}