using System;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ItemProperty : TileItemBase
    {
        
        private string _type;
        public string Type
        {
            get { return _type; }
            set { StandardSet(ref _type, ref value, "Type"); }
        }

    }
}