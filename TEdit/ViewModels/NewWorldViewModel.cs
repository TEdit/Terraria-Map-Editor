using System.ComponentModel.Composition;
using TEdit.Common;

namespace TEdit.ViewModels
{
    [Export]
    public class NewWorldViewModel : ObservableObject
    {
        private int _Width;
        public int Width
        {
            get { return this._Width; }
            set
            {
                if (this._Width != value)
                {
                    this._Width = value;
                    this.RaisePropertyChanged("Width");
                }
            }
        }

        private int _Height;
        public int Height
        {
            get { return this._Height; }
            set
            {
                if (this._Height != value)
                {
                    this._Height = value;
                    this.RaisePropertyChanged("Height");
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
    }
}