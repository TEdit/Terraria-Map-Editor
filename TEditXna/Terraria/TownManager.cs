using System;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class TownManager : ObservableObject
    {
        private Vector2Int32 _home;
        private int _npcId;

        public int NpcId
        {
            get { return _npcId; }
            set { Set("NpcId", ref _npcId, value); }
        }

        public Vector2Int32 Home
        {
            get { return _home; }
            set { Set("Home", ref _home, value); }
        }
    }
}
