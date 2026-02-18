using System.Windows.Media.Imaging;

namespace TEdit.ViewModel;

public class SkinVariantOption
{
    public int Index { get; init; }
    public string Name { get; init; } = "";
    public BitmapSource? Preview { get; init; }
}
