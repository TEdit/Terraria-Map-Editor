using System;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    // Very basic stuff for most ObservableObjects
    [Serializable]
    public class NamedBase : ObservableObject
    {
        private short _id;
        public short ID
        {
            get { return _id; }
            set { SetProperty(ref _id, ref value, "ID"); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, ref value, "Name"); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}