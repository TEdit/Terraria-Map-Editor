using System;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;

namespace TEdit.Terraria
{
    [Serializable]
    public class NPC : ObservableObject
    {
        private Vector2Int32 _home;
        private bool _isHomeless;
        private string _name;
        private Vector2 _position;
        private int _spriteId;

        private string _displayName;
        private int _townNpcVariationIndex;

        public string DisplayName
        {
            get { return _displayName; }
            set { Set(nameof(DisplayName), ref _displayName, value); }
        }

        public int SpriteId
        {
            get { return _spriteId; }
            set { Set(nameof(SpriteId), ref _spriteId, value); }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { Set(nameof(Position), ref _position, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public bool IsHomeless
        {
            get { return _isHomeless; }
            set { Set(nameof(IsHomeless), ref _isHomeless, value); }
        }

        public int TownNpcVariationIndex
        {
            get { return _townNpcVariationIndex; }
            set { Set(nameof(TownNpcVariationIndex), ref _townNpcVariationIndex, value); }
        }

        public Vector2Int32 Home
        {
            get { return _home; }
            set { Set(nameof(Home), ref _home, value); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
