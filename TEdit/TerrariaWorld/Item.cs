using System;
using System.Collections.ObjectModel;
using System.Windows;
using TEdit.Common;
using TEdit.RenderWorld;

namespace TEdit.TerrariaWorld
{
    [Serializable]
    public class Item : ObservableObject
    {
        private string _itemName;
        private int _StackSize;

        public Item()
        {
            StackSize = 0;
            ItemName = "[empty]";
        }

        public Item(int stackSize, string itemName)
        {
            StackSize = stackSize;
            ItemName = stackSize > 0 ? itemName : "[empty]";
        }

        public int StackSize
        {
            get { return _StackSize; }
            set
            {
                int validValue = value;
                if (validValue > 255)
                    validValue = 255;
                if (validValue < 0)
                    validValue = 0;

                if (_StackSize != validValue)
                {
                    _StackSize = validValue;
                    RaisePropertyChanged("StackSize");
                    if (_StackSize == 0)
                        ItemName = "[empty]";
                }
            }
        }

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    RaisePropertyChanged("ItemName");
                    RaisePropertyChanged("IsVisible");

                    if (_itemName == "[empty]")
                        StackSize = 0;
                }
            }
        }

        public ObservableCollection<string> ValidItems
        {
            get { return WorldSettings.ItemNames; }
        }

        public Visibility IsVisible
        {
            get { return ItemName == "[empty]" ? Visibility.Collapsed : Visibility.Visible; }
        }

        public override string ToString()
        {
            if (StackSize > 0)
                return string.Format("{0}: {1}", ItemName, StackSize);

            return ItemName;
        }
    }
}