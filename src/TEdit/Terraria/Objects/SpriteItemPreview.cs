using System;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TEdit.Terraria.Objects;

public partial class SpriteItemPreview : SpriteItem
{
    /// <summary>
    /// Default preview bitmap (for tiles without biome variants, or biome index 0).
    /// </summary>
    [Reactive]
    private WriteableBitmap _preview;

    /// <summary>
    /// Array of preview bitmaps for each biome variant (null if tile has no biome variants).
    /// Index corresponds to BiomeVariants list in TileProperty.
    /// </summary>
    [Reactive]
    private WriteableBitmap[] _biomePreviews;

    /// <summary>
    /// Currently selected biome variant index.
    /// </summary>
    [Reactive]
    private int _selectedBiomeIndex;

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

    public SpriteItemPreview()
    {
        // Subscribe to changes that affect computed properties
        this.WhenAnyValue(x => x.Preview, x => x.BiomePreviews, x => x.SelectedBiomeIndex)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(HasBiomeVariants));
                this.RaisePropertyChanged(nameof(CurrentPreview));
            });
    }
}
