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
        private string _Name;
        private int _StackSize;

        public Item()
        {
            StackSize = 0;
            Name = "[empty]";
        }

        public Item(int stackSize, string name)
        {
            StackSize = stackSize;
            Name = stackSize > 0 ? name : "[empty]";
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
                        Name = "[empty]";
                }
            }
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                    RaisePropertyChanged("IsVisible");

                    if (_Name == "[empty]")
                        StackSize = 0;
                }
            }
        }

        public ObservableCollection<ItemProperty> ValidItems
        {
            get { return WorldSettings.Items; }
        }

        public Visibility IsVisible
        {
            get { return Name == "[empty]" ? Visibility.Collapsed : Visibility.Visible; }
        }

        public override string ToString()
        {
            if (StackSize > 0)
                return string.Format("{0}: {1}", Name, StackSize);

            return Name;
        }
    }
}