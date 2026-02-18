using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Terraria.Player;

namespace TEdit.ViewModel;

/// <summary>
/// Design-time ViewModel providing sample player data for XAML designer preview.
/// </summary>
public class DesignTimePlayerEditorViewModel
{
    public PlayerCharacter? Player { get; }
    public string? PlayerFilePath { get; }
    public int SelectedSubTabIndex { get; }
    public string StatusText { get; }
    public WriteableBitmap? PlayerPreview { get; }
    
    public HslColorViewModel HairColorVm { get; }
    public HslColorViewModel SkinColorVm { get; }
    public HslColorViewModel EyeColorVm { get; }
    public HslColorViewModel ShirtColorVm { get; }
    public HslColorViewModel UnderShirtColorVm { get; }
    public HslColorViewModel PantsColorVm { get; }
    public HslColorViewModel ShoeColorVm { get; }

    public ObservableCollection<PlayerItem>? ActiveArmor { get; }
    public ObservableCollection<PlayerItem>? ActiveDye { get; }
    public ObservableCollection<EquipmentSlot>? EquipmentSlots { get; }
    public ObservableCollection<EquipmentSlot>? MiscSlots { get; }

    public List<SkinVariantOption>? SkinVariantOptions { get; }
    public SkinVariantOption? SelectedSkinVariant { get; }

    public DesignTimePlayerEditorViewModel()
    {
        // Create sample player
        Player = new PlayerCharacter
        {
            Name = "Designer",
            Difficulty = 0, // Classic
            StatLife = 100,
            StatLifeMax = 100,
            StatMana = 20,
            StatManaMax = 20,
            Appearance = new PlayerAppearance
            {
                Hair = 0,
                SkinVariant = 0,
                HairColor = new(1.0f, 0.54f, 0.0f),    // Orange
                SkinColor = new(1.0f, 0.80f, 0.60f),   // Skin tone
                EyeColor = new(0.39f, 0.58f, 0.93f),   // Cornflower blue
                ShirtColor = new(1.0f, 0.0f, 0.0f),    // Red
                UnderShirtColor = new(0.78f, 0.78f, 0.78f),  // Gray
                PantsColor = new(0.0f, 0.0f, 0.0f),    // Black
                ShoeColor = new(0.55f, 0.27f, 0.07f),  // Brown
            }
        };

        PlayerFilePath = "Designs/Sample Player.plr";
        SelectedSubTabIndex = 0;
        StatusText = "Design-time preview";
        
        // Create placeholder bitmap for preview
        PlayerPreview = CreatePlaceholderBitmap();
        
        // Initialize color VMs with sample colors
        HairColorVm = new HslColorViewModel("Hair") { Hue = 30, Saturation = 1.0, Lightness = 0.5 };
        SkinColorVm = new HslColorViewModel("Skin") { Hue = 25, Saturation = 0.7, Lightness = 0.7 };
        EyeColorVm = new HslColorViewModel("Eyes") { Hue = 220, Saturation = 1.0, Lightness = 0.5 };
        ShirtColorVm = new HslColorViewModel("Shirt") { Hue = 0, Saturation = 1.0, Lightness = 0.5 };
        UnderShirtColorVm = new HslColorViewModel("Undershirt") { Hue = 0, Saturation = 0.0, Lightness = 0.78 };
        PantsColorVm = new HslColorViewModel("Pants") { Hue = 0, Saturation = 0.0, Lightness = 0.0 };
        ShoeColorVm = new HslColorViewModel("Shoes") { Hue = 30, Saturation = 0.5, Lightness = 0.4 };

        // Sample equipment slots
        EquipmentSlots = new ObservableCollection<EquipmentSlot>();
        MiscSlots = new ObservableCollection<EquipmentSlot>();
        ActiveArmor = new ObservableCollection<PlayerItem>();
        ActiveDye = new ObservableCollection<PlayerItem>();

        // Sample skin variants (you can expand this as needed)
        SkinVariantOptions = new List<SkinVariantOption>
        {
            new SkinVariantOption { Index = 0, Name = "Default", Preview = null },
            new SkinVariantOption { Index = 1, Name = "Variant 1", Preview = null },
            new SkinVariantOption { Index = 2, Name = "Variant 2", Preview = null },
        };
        SelectedSkinVariant = SkinVariantOptions[0];
    }

    private WriteableBitmap CreatePlaceholderBitmap()
    {
        var bitmap = new WriteableBitmap(120, 168, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
        var pixels = new byte[120 * 168 * 4];

        // Fill with a light gray background with a simple pattern
        for (int y = 0; y < 168; y++)
        {
            for (int x = 0; x < 120; x++)
            {
                int index = (y * 120 + x) * 4;
                // Checkerboard pattern
                if ((x / 12 + y / 12) % 2 == 0)
                {
                    pixels[index] = 200;     // B
                    pixels[index + 1] = 200; // G
                    pixels[index + 2] = 200; // R
                }
                else
                {
                    pixels[index] = 220;     // B
                    pixels[index + 1] = 220; // G
                    pixels[index + 2] = 220; // R
                }
                pixels[index + 3] = 255;     // A
            }
        }

        bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, 120, 168), pixels, 120 * 4, 0);
        return bitmap;
    }
}
