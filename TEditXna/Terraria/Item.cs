using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BCCL.MvvmLight;
using TEditXNA.Terraria.Objects;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Item : ObservableObject
    {
        private static IList<string> _validItems = new ObservableCollection<string>();
        public static IList<string> ValidItems
        {
            get
            {
                if (_validItems.Count <= 0)
                    _validItems = World.ItemProperties.Select(x => x.Name).ToList();
                
                return _validItems;
            }
        }


        private string _itemName;
        private int _stackSize;
        private byte _prefix;
         

        public byte Prefix
        {
            get { return _prefix; }
            set { Set("Prefix", ref _prefix, value); }
        }

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

        private ItemProperty _currentItemProperty;
        public int StackSize
        {
            get { return _stackSize; }
            set
            {
                int maxSize = 255;
                if (_currentItemProperty != null)
                {
                    maxSize = _currentItemProperty.MaxStackSize;
                }

                int validValue = value;
                if (validValue > maxSize)
                    validValue = maxSize;
                if (validValue < 0)
                    validValue = 0;

                Set("StackSize", ref _stackSize, validValue);
            }
        }

        public string ItemName
        {
            get { return _itemName; }
            set
            {
                Set("StackSize", ref _itemName, value);
                _currentItemProperty = World.ItemProperties.FirstOrDefault(x => x.Name == _itemName);
                if (_itemName == "[empty]")
                    StackSize = 0;
            }
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