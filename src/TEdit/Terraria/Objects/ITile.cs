using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;

namespace TEdit.Terraria.Objects
{
    public interface ITile
    {
        TEditColor Color { get; }
        int Id { get; }
        string Name { get; }
    }
}
