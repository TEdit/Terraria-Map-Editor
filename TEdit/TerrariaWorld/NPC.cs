// -----------------------------------------------------------------------
// <copyright file="NPC.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------


using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.TerrariaWorld
{
    public class NPC : ObservableObject
    {
        private PointInt32 _HomeTile;
        private bool _IsHomeless;
        private string _Name;
        private PointFloat _Position;

        public string ImagePath
        {
            get { return "../Images/Overlays/npc_" + _Name.ToLower().Replace(' ', '_') + ".png"; }
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
                    RaisePropertyChanged("ImagePath");
                }
            }
        }

        public bool IsHomeless
        {
            get { return _IsHomeless; }
            set
            {
                if (_IsHomeless != value)
                {
                    _IsHomeless = value;
                    RaisePropertyChanged("IsHomeless");
                }
            }
        }

        public PointInt32 HomeTile
        {
            get { return _HomeTile; }
            set
            {
                if (_HomeTile != value)
                {
                    _HomeTile = value;
                    RaisePropertyChanged("HomeTile");
                }
            }
        }

        public PointFloat Position
        {
            get { return _Position; }
            set
            {
                if (_Position != value)
                {
                    _Position = value;
                    RaisePropertyChanged("Position");
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}