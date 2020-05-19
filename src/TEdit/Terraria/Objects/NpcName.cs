using System.Linq;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria.Objects
{
    public class NpcName : ObservableObject
    {
        private string _name;
        private int _id;

        public NpcName(int id, string name)
        {
            _id = id;
            _name = name;
        }

        public int Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        public string Character
        {
            get
            {
                if (World.NpcIds.ContainsValue(_id))
                    return World.NpcIds.FirstOrDefault(c => c.Value == _id).Key;
                return string.Empty;
            }
        }
    }
}