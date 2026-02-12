using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Objects;

public class SpriteItemPreview : SpriteItem, INotifyPropertyChanged
{
    private WriteableBitmap _preview;
    private WriteableBitmap[] _biomePreviews;
    private int _selectedBiomeIndex;

    /// <summary>
    /// Default preview bitmap (for tiles without biome variants, or biome index 0).
    /// </summary>
    public WriteableBitmap Preview
    {
        get => _preview;
        set
        {
            if (_preview != value)
            {
                _preview = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPreview));
            }
        }
    }

    /// <summary>
    /// Array of preview bitmaps for each biome variant (null if tile has no biome variants).
    /// Index corresponds to BiomeVariants list in TileProperty.
    /// </summary>
    public WriteableBitmap[] BiomePreviews
    {
        get => _biomePreviews;
        set
        {
            if (_biomePreviews != value)
            {
                _biomePreviews = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPreview));
                OnPropertyChanged(nameof(HasBiomeVariants));
            }
        }
    }

    /// <summary>
    /// Currently selected biome variant index.
    /// </summary>
    public int SelectedBiomeIndex
    {
        get => _selectedBiomeIndex;
        set
        {
            if (_selectedBiomeIndex != value)
            {
                _selectedBiomeIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPreview));
            }
        }
    }

    /// <summary>
    /// Whether this sprite has biome variants available.
    /// </summary>
    public bool HasBiomeVariants => _biomePreviews != null && _biomePreviews.Length > 0;

    /// <summary>
    /// Gets the preview for the currently selected biome (or default if no biome variants).
    /// </summary>
    public WriteableBitmap CurrentPreview
    {
        get
        {
            if (_biomePreviews != null && _selectedBiomeIndex >= 0 && _selectedBiomeIndex < _biomePreviews.Length)
            {
                return _biomePreviews[_selectedBiomeIndex] ?? _preview;
            }
            return _preview;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
