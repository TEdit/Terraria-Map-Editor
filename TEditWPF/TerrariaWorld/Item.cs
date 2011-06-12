namespace TEditWPF.TerrariaWorld
{
    using TEditWPF.Common;

    public class Item : ObservableObject
    {
        public Item()
        {
            this.StackSize = 0;
            this.Name = "[empty]";
        }

        public Item(int stackSize, string name)
        {
            this.StackSize = stackSize;
            this.Name = stackSize > 0 ? name : "[empty]";
        }

        private int _StackSize;
        public int StackSize
        {
            get { return this._StackSize; }
            set
            {
                if (this._StackSize != value)
                {
                    this._StackSize = value;
                    this.RaisePropertyChanged("StackSize");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        public override string ToString()
        {
            if (this.StackSize > 0)
                return string.Format("{0}: {1}", this.Name, this.StackSize);

            return this.Name;
        }
    }
}
