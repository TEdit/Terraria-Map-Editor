using System;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ItemProperty : NamedBase
    {
        private string _itemType;
        public string ItemType
        {
            get { return _itemType; }
            set { SetProperty(ref _itemType, ref value, "ItemType"); }
        }
    }
}