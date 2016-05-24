using System;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria
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
         

        public string DisplayName
        {
            get { return _displayName; }
            set { Set("DisplayName", ref _displayName, value); }
        }

        public int SpriteId
        {
            get { return _spriteId; }
            set { Set("SpriteId", ref _spriteId, value); }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { Set("Position", ref _position, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        public bool IsHomeless
        {
            get { return _isHomeless; }
            set { Set("IsHomeless", ref _isHomeless, value); }
        }

        public Vector2Int32 Home
        {
            get { return _home; }
            set { Set("Home", ref _home, value); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}