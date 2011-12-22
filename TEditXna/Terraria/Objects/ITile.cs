using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TEditXNA.Terraria.Objects
{
    public interface ITile
    {
        Color Color { get; }
        int Id { get; }
        string Name { get; }
        WriteableBitmap Image { get; } 
    }
}