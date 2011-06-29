using System.Windows;
using TEdit.Common;

namespace TEdit.TerrariaWorld
{
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
                if (_StackSize != value)
                {
                    _StackSize = value;
                    RaisePropertyChanged("StackSize");
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
                }
            }
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