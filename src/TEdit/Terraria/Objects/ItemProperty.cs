using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;

namespace TEdit.Terraria.Objects
{
    public class ItemProperty : ObservableObject, ITile
    {
        public ItemProperty()
        {
            Name = "UNKNOWN";
            Id = 0;
        }

        private int _maxStackSize;
        private int _id;
        private string _name;
        private float _scale;
        private Vector2Short _size;
        private Vector2Short _uV; 

        public Vector2Short UV
        {
            get { return _uV; }
            set { Set(nameof(UV), ref _uV, value); }
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

        public float Scale
        {
            get { return _scale; }
            set { Set(nameof(Scale), ref _scale, value); }
        }


        public bool IsFood
        {
            get { return _isFood; }
            set { Set(nameof(IsFood), ref _isFood, value); }
        }



        private WriteableBitmap _image;
        private bool _isFood;

        public WriteableBitmap Image
        {
            get { return _image; }
            set { Set(nameof(Image), ref _image, value); }
        }

        public Color Color
        {
            get { return Colors.Transparent; }
        }

        public int Id
        {
            get { return _id; }
            set { Set(nameof(Id), ref _id, value); }
        }

        public int MaxStackSize
        {
            get { return _maxStackSize; }
            set { Set(nameof(MaxStackSize), ref _maxStackSize, value); }
        } 
    }
}