using System.ComponentModel.Composition;
using TEdit.Common;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.RenderWorld
{
    [Export]
    public class MarkerLocations : ObservableObject
    {
        private static readonly PointInt32 MarkerOffset = new PointInt32(10, 34);

        public MarkerLocations()
        {
            
        }

        public void UpdateLocations(World world)
        {
            Spawn = world.Header.SpawnTile - MarkerOffset;
            Dungeon = world.Header.DungeonEntrance - MarkerOffset;
            for (int i = 0; i < World.MaxNpcs; i++)
            {
                if (world.Npcs[i] == null)
                    break;
                if (world.Npcs[i].Name != "")
                {
                    switch (world.Npcs[i].Name)
                    {
                        case "Guide":
                            Guide = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                        case "Nurse":
                            Nurse = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                        case "Merchant":
                            Merchant = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                        case "Arms Dealer":
                            ArmsDealer = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                        case "Dryad":
                            Dryad = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                        case "Demolitionist":
                            ExplosiveVendor = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                        case "Clothier":
                            Clothier = world.Npcs[i].HomeTile - MarkerOffset;
                            break;
                    }
                }
                else
                    break;
                
            }
        }

        private PointInt32 _Guide;
        public PointInt32 Guide
        {
            get { return this._Guide; }
            set
            {
                if (this._Guide != value)
                {
                    this._Guide = value;
                    this.RaisePropertyChanged("Guide");
                }
            }
        }


        private PointInt32 _Spawn;
        public PointInt32 Spawn
        {
            get { return this._Spawn; }
            set
            {
                if (this._Spawn != value)
                {
                    this._Spawn = value;
                    this.RaisePropertyChanged("Spawn");
                }
            }
        }

        private PointInt32 _Nurse;
        public PointInt32 Nurse
        {
            get { return this._Nurse; }
            set
            {
                if (this._Nurse != value)
                {
                    this._Nurse = value;
                    this.RaisePropertyChanged("Nurse");
                }
            }
        }

        private PointInt32 _Merchant;
        public PointInt32 Merchant
        {
            get { return this._Merchant; }
            set
            {
                if (this._Merchant != value)
                {
                    this._Merchant = value;
                    this.RaisePropertyChanged("Merchant");
                }
            }
        }

        private PointInt32 _ArmsDealer;
        public PointInt32 ArmsDealer
        {
            get { return this._ArmsDealer; }
            set
            {
                if (this._ArmsDealer != value)
                {
                    this._ArmsDealer = value;
                    this.RaisePropertyChanged("ArmsDealer");
                }
            }
        }

        private PointInt32 _ExplosiveVendor;
        public PointInt32 ExplosiveVendor
        {
            get { return this._ExplosiveVendor; }
            set
            {
                if (this._ExplosiveVendor != value)
                {
                    this._ExplosiveVendor = value;
                    this.RaisePropertyChanged("ExplosiveVendor");
                }
            }
        }

        private PointInt32 _Clothier;
        public PointInt32 Clothier
        {
            get { return this._Clothier; }
            set
            {
                if (this._Clothier != value)
                {
                    this._Clothier = value;
                    this.RaisePropertyChanged("Clothier");
                }
            }
        }

        private PointInt32 _Dryad;
        public PointInt32 Dryad
        {
            get { return this._Dryad; }
            set
            {
                if (this._Dryad != value)
                {
                    this._Dryad = value;
                    this.RaisePropertyChanged("Dryad");
                }
            }
        }

        private PointInt32 _Dungeon;
        public PointInt32 Dungeon
        {
            get { return this._Dungeon; }
            set
            {
                if (this._Dungeon != value)
                {
                    this._Dungeon = value;
                    this.RaisePropertyChanged("Dungeon");
                }
            }
        }








    }
}