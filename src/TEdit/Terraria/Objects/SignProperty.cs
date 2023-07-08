using TEdit.Common.Reactive;
using TEdit.Geometry;
using TEdit.Common;

namespace TEdit.Terraria.Objects
{
    public class SignProperty : ObservableObject, ITile
    {
        public SignProperty()
        {
            Name = "UNKNOWN";
            Id = 0;
        }

        private int _id;
        private string _name;
        private Vector2Short _size;
        private int _signId;
        private Vector2Short _uV;
        private ushort _tileType;

        public int SignId
        {
            get { return _signId; }
            set { Set(nameof(SignId), ref _signId, value); }
        }

        public Vector2Short UV
        {
            get { return _uV; }
            set { Set(nameof(UV), ref _uV, value); }
        } 

        public ushort TileType
        {
            get { return _tileType; }
            set { Set(nameof(TileType), ref _tileType, value); }
        }

        public Vector2Short Size
        {
            get { return _size; }
            set { Set(nameof(Size), ref _size, value); }
        } 

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public TEditColor Color
        {
            get { return TEditColor.Transparent; }
        }

        public int Id
        {
            get { return _id; }
            set { Set(nameof(Id), ref _id, value); }
        }
    }
}
