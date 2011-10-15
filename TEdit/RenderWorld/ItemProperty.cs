using System;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ItemProperty : XMLBase
    {
        private byte _maxStack = 1;
        public byte MaxStack
        {
            get { return _maxStack; }
            set { SetProperty(ref _maxStack, ref value, "MaxStack"); }
        }

        private string _description = String.Empty;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, ref value, "Description"); }
        }

        private string _description2 = String.Empty;
        public string Description2
        {
            get { return _description2; }
            set { SetProperty(ref _description2, ref value, "Description2"); }
        }

        // TODO: Add extra properties as they are needed for TEdit //

    }
}