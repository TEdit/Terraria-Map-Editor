using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Objects;

public class SpriteItemPreview : SpriteItem, INotifyPropertyChanged
{
    private WriteableBitmap _preview;

    public WriteableBitmap Preview
    {
        get => _preview;
        set
        {
            if (_preview != value)
            {
                _preview = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
