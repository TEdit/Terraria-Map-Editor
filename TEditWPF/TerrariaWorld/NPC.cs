// -----------------------------------------------------------------------
// <copyright file="NPC.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------



namespace TEditWPF.TerrariaWorld
{
    using TEditWPF.Common;
    using TEditWPF.TerrariaWorld.Structures;

    public class NPC : ObservableObject
    {
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

        private bool _IsHomeless;
        public bool IsHomeless
        {
            get { return this._IsHomeless; }
            set
            {
                if (this._IsHomeless != value)
                {
                    this._IsHomeless = value;
                    this.RaisePropertyChanged("IsHomeless");
                }
            }
        }

        private PointInt32 _HomeTile;
        public PointInt32 HomeTile
        {
            get { return this._HomeTile; }
            set
            {

                if (this._HomeTile != value)
                {
                    this._HomeTile = value;
                    this.RaisePropertyChanged("HomeTile");
                }
            }
        }

        private PointFloat _Position;
        public PointFloat Position
        {
            get { return this._Position; }
            set
            {
                if (this._Position != value)
                {
                    this._Position = value;
                    this.RaisePropertyChanged("Position");
                }
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
