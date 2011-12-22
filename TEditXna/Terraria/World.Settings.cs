using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria.Objects;

namespace TEditXNA.Terraria
{
    public partial class World
    {
        public static readonly Dictionary<string, Color> _globalColors = new Dictionary<string, Color>();
        public static readonly IList<ItemProperty> _itemProperties = new ObservableCollection<ItemProperty>();
        public static readonly IList<TileProperty> _tileProperties = new ObservableCollection<TileProperty>();
        public static readonly IList<WallProperty> _wallProperties = new ObservableCollection<WallProperty>();
        static World()
        {
            LoadObjectDbXml("Terraria.Object.DB.Xml");
        }

        private static void LoadObjectDbXml(string file)
        {
            throw new System.NotImplementedException();
        }

        public static Dictionary<string, Color> GlobalColors
        {
            get { return _globalColors; }
        }


        public static IList<TileProperty> TileProperties
        {
            get { return _tileProperties; }
        }


        public static IList<WallProperty> WallProperties
        {
            get { return _wallProperties; }
        }


        public static IList<ItemProperty> ItemProperties
        {
            get { return _itemProperties; }
        }
    }
}