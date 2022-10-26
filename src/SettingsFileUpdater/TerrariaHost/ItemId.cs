using System.Threading;
using System.Reflection;
using Terraria.ID;

namespace SettingsFileUpdater.TerrariaHost
{
    //public static class ColorExt
    //{
    //    public static string ColorToHex(this Color color) => string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
    //}
    public class ItemId
    {
        public ItemId(int id, string name, string type)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }

        public bool IsFood { get; set; }

        public int Head { get; set; }
        public int Banner { get; set; }
        public int Body { get; set; }
        public int Legs { get; set; }
        public bool Accessory { get; set; }
        public bool Rack { get; set; }
    }
}