using System;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class PressurePlate : ObservableObject
    {
        private int _x;
        private int _y;

        public int PosX
        {
            get { return _x; }
            set { Set("PosX", ref _x, value); }
        }
        
        public int PosY
        {
            get { return _y; }
            set { Set("PosY", ref _y, value); }
        }        
        
    }
}