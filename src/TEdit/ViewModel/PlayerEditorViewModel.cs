using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Properties;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.Terraria.Player;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class PlayerEditorViewModel
{
    [Reactive] private PlayerCharacter? _player;
    [Reactive] private string? _playerFilePath;
    [Reactive] private int _selectedSubTabIndex;
    [Reactive] private int _selectedLoadoutIndex;
    [Reactive] private string _statusText = "";

    // Appearance color ViewModels
    public HslColorViewModel HairColorVm { get; } = new("Hair");
    public HslColorViewModel SkinColorVm { get; } = new("Skin");
    public HslColorViewModel EyeColorVm { get; } = new("Eyes");
    public HslColorViewModel ShirtColorVm { get; } = new("Shirt");
    public HslColorViewModel UnderShirtColorVm { get; } = new("Undershirt");
    public HslColorViewModel PantsColorVm { get; } = new("Pants");
    public HslColorViewModel ShoeColorVm { get; } = new("Shoes");

    // Active loadout equipment (switched by radio buttons)
    [Reactive] private ObservableCollection<PlayerItem>? _activeArmor;
    [Reactive] private ObservableCollection<PlayerItem>? _activeDye;

    // Paired equipment slots for mannequin-style layout
    [Reactive] private ObservableCollection<EquipmentSlot>? _equipmentSlots;
    [Reactive] private ObservableCollection<EquipmentSlot>? _miscSlots;

    // Selected equipment slot (shared between equipment and misc lists)
    [Reactive] private EquipmentSlot? _selectedEquipmentSlot;

    // Player preview
    [Reactive] private WriteableBitmap? _playerPreview;

    // Skin variant selector
    [Reactive] private List<SkinVariantOption>? _skinVariantOptions;
    [Reactive] private SkinVariantOption? _selectedSkinVariant;

    private static readonly string DefaultPlayerDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "My Games", "Terraria", "Players");

    public PlayerEditorViewModel()
    {
        // Sync colors from VM back to model on change
        SetupColorSync(HairColorVm, p => p.Appearance.HairColor = HairColorVm.GetColor());
        SetupColorSync(SkinColorVm, p => p.Appearance.SkinColor = SkinColorVm.GetColor());
        SetupColorSync(EyeColorVm, p => p.Appearance.EyeColor = EyeColorVm.GetColor());
        SetupColorSync(ShirtColorVm, p => p.Appearance.ShirtColor = ShirtColorVm.GetColor());
        SetupColorSync(UnderShirtColorVm, p => p.Appearance.UnderShirtColor = UnderShirtColorVm.GetColor());
        SetupColorSync(PantsColorVm, p => p.Appearance.PantsColor = PantsColorVm.GetColor());
        SetupColorSync(ShoeColorVm, p => p.Appearance.ShoeColor = ShoeColorVm.GetColor());

        // When loadout index changes, switch active armor/dye
        this.WhenAnyValue(x => x.SelectedLoadoutIndex, x => x.Player)
            .Subscribe(_ => UpdateActiveLoadout());

        // Regenerate preview when colors or appearance change
        foreach (var colorVm in new[] { HairColorVm, SkinColorVm, EyeColorVm, ShirtColorVm, UnderShirtColorVm, PantsColorVm, ShoeColorVm })
        {
            colorVm.WhenAnyValue(x => x.Hue, x => x.Saturation, x => x.Lightness)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => RegeneratePreview());
        }

        // When skin variant selection changes, update the player model
        this.WhenAnyValue(x => x.SelectedSkinVariant)
            .Where(x => x != null)
            .Subscribe(option =>
            {
                if (Player != null && option != null)
                {
                    Player.Appearance.SkinVariant = (byte)option.Index;
                    RegeneratePreview();
                }
            });
    }

    /// <summary>
    /// Bakes skin variant preview images. Called after textures are loaded.
    /// </summary>
    public void BakeSkinVariantPreviews()
    {
        var textures = ViewModelLocator.WorldViewModel.Textures;
        if (textures == null || !textures.Valid) return;

        try
        {
            SkinVariantOptions = PlayerPreviewRenderer.BakeSkinVariantPreviews(textures);
        }
        catch
        {
            // Silently fail if textures aren't available
        }
    }

    private void SetupColorSync(HslColorViewModel colorVm, Action<PlayerCharacter> apply)
    {
        colorVm.WhenAnyValue(x => x.Hue, x => x.Saturation, x => x.Lightness)
            .Throttle(TimeSpan.FromMilliseconds(50))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (Player != null)
                    apply(Player);
            });
    }

    private void UpdateActiveLoadout()
    {
        if (Player == null) return;

        if (SelectedLoadoutIndex == Player.CurrentLoadoutIndex)
        {
            ActiveArmor = Player.Armor;
            ActiveDye = Player.Dye;
        }
        else if (SelectedLoadoutIndex >= 0 && SelectedLoadoutIndex < Player.Loadouts.Length)
        {
            ActiveArmor = Player.Loadouts[SelectedLoadoutIndex].Armor;
            ActiveDye = Player.Loadouts[SelectedLoadoutIndex].Dye;
        }

        BuildEquipmentSlots();
        BuildMiscSlots();
    }

    private void BuildEquipmentSlots()
    {
        if (ActiveArmor == null || ActiveDye == null) return;

        var slots = new ObservableCollection<EquipmentSlot>();

        // Armor[0-2] = Head/Body/Legs, paired with Dye[0-2]
        string[] armorLabels = ["Head", "Body", "Legs"];
        EquipmentSlotCategory[] armorCategories = [EquipmentSlotCategory.Head, EquipmentSlotCategory.Body, EquipmentSlotCategory.Legs];
        for (int i = 0; i < 3 && i < ActiveArmor.Count; i++)
        {
            slots.Add(new EquipmentSlot
            {
                Label = armorLabels[i],
                Item = ActiveArmor[i],
                Dye = i < ActiveDye.Count ? ActiveDye[i] : null,
                Category = armorCategories[i]
            });
        }

        // Armor[3-5] = Vanity Head/Body/Legs (share dyes with 0-2)
        string[] vanityArmorLabels = ["V.Head", "V.Body", "V.Legs"];
        for (int i = 0; i < 3 && i + 3 < ActiveArmor.Count; i++)
        {
            slots.Add(new EquipmentSlot
            {
                Label = vanityArmorLabels[i],
                Item = ActiveArmor[i + 3],
                Dye = i < ActiveDye.Count ? ActiveDye[i] : null,
                Category = armorCategories[i]
            });
        }

        // Armor[6-12] = Accessories, paired with Dye[3-9]
        for (int i = 0; i < 7 && i + 6 < ActiveArmor.Count; i++)
        {
            slots.Add(new EquipmentSlot
            {
                Label = $"Acc {i + 1}",
                Item = ActiveArmor[i + 6],
                Dye = i + 3 < ActiveDye.Count ? ActiveDye[i + 3] : null,
                Category = EquipmentSlotCategory.Accessory
            });
        }

        // Armor[13-19] = Vanity Accessories (share dyes with Acc slots)
        for (int i = 0; i < 7 && i + 13 < ActiveArmor.Count; i++)
        {
            slots.Add(new EquipmentSlot
            {
                Label = $"V.Acc {i + 1}",
                Item = ActiveArmor[i + 13],
                Dye = i + 3 < ActiveDye.Count ? ActiveDye[i + 3] : null,
                Category = EquipmentSlotCategory.Accessory
            });
        }

        EquipmentSlots = slots;
    }

    private void BuildMiscSlots()
    {
        if (Player == null) return;

        string[] miscLabels = ["Pet", "Light Pet", "Minecart", "Mount", "Hook"];
        var slots = new ObservableCollection<EquipmentSlot>();

        for (int i = 0; i < miscLabels.Length && i < Player.MiscEquips.Count; i++)
        {
            slots.Add(new EquipmentSlot
            {
                Label = miscLabels[i],
                Item = Player.MiscEquips[i],
                Dye = i < Player.MiscDyes.Count ? Player.MiscDyes[i] : null,
                Category = EquipmentSlotCategory.Misc
            });
        }

        MiscSlots = slots;
    }

    // Item clipboard for inventory copy/paste {quantity, prefix, itemId}
    private PlayerItem? _playerItemClipboard;

    [ReactiveCommand]
    private void CopyPlayerItem(PlayerItem item)
    {
        _playerItemClipboard = item?.Copy();
    }

    [ReactiveCommand]
    private void PastePlayerItem(object parameter)
    {
        if (_playerItemClipboard == null) return;

        if (parameter is IList selectedItems)
        {
            foreach (var obj in selectedItems)
            {
                if (obj is PlayerItem pi)
                    PasteToPlayerItem(pi);
            }
        }
        else if (parameter is PlayerItem item)
        {
            PasteToPlayerItem(item);
        }
    }

    private void PasteToPlayerItem(PlayerItem item)
    {
        if (_playerItemClipboard != null)
        {
            item.NetId = _playerItemClipboard.NetId;
            item.Prefix = _playerItemClipboard.Prefix;
            item.StackSize = _playerItemClipboard.StackSize;
        }
        else
        {
            item.NetId = 0;
        }
    }

    // Equipment clipboard for copy/paste {dye, prefix, itemId}
    private (PlayerItem item, PlayerItem? dye)? _equipmentClipboard;

    [ReactiveCommand]
    private void CopyEquipmentSlot(EquipmentSlot slot)
    {
        if (slot == null) return;
        _equipmentClipboard = (slot.Item.Copy(), slot.Dye?.Copy());
    }

    [ReactiveCommand]
    private void PasteEquipmentSlot(EquipmentSlot slot)
    {
        if (_equipmentClipboard == null || slot == null) return;
        var (item, dye) = _equipmentClipboard.Value;
        slot.Item.NetId = item.NetId;
        slot.Item.Prefix = item.Prefix;
        slot.Item.StackSize = item.StackSize;
        if (slot.Dye != null && dye != null)
        {
            slot.Dye.NetId = dye.NetId;
            slot.Dye.Prefix = dye.Prefix;
            slot.Dye.StackSize = dye.StackSize;
        }
    }

    [ReactiveCommand]
    private void PlayerItemSetToMaxStack(object parameter)
    {
        if (parameter is IList selectedItems)
        {
            foreach (var obj in selectedItems)
            {
                if (obj is PlayerItem pi)
                    SetPlayerItemMaxStack(pi);
            }
        }
        else if (parameter is PlayerItem item)
        {
            SetPlayerItemMaxStack(item);
        }
    }

    private static void SetPlayerItemMaxStack(PlayerItem item)
    {
        if (WorldConfiguration.ItemLookupTable.TryGetValue(item.NetId, out var props) && props.MaxStackSize > 0)
            item.StackSize = props.MaxStackSize;
        else
            item.StackSize = 9999;
    }

    [ReactiveCommand]
    private void LoadPlayer()
    {
        var dlg = new OpenFileDialog
        {
            Title = Language.player_editor_load,
            Filter = "Player Files (*.plr)|*.plr|All Files (*.*)|*.*",
            InitialDirectory = Directory.Exists(DefaultPlayerDir) ? DefaultPlayerDir : ""
        };

        if (dlg.ShowDialog() == true)
        {
            LoadPlayerFromFile(dlg.FileName);
        }
    }

    public void LoadPlayerFromFile(string path)
    {
        try
        {
            Player = PlayerFile.Load(path);
            PlayerFilePath = path;
            SyncFromPlayer();
            StatusText = $"Loaded: {Player.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load player file:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [ReactiveCommand]
    private void SavePlayer()
    {
        if (Player == null || string.IsNullOrEmpty(PlayerFilePath)) return;

        try
        {
            SyncToPlayer();
            PlayerFile.Save(PlayerFilePath, Player);
            StatusText = $"Saved: {Player.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save player file:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [ReactiveCommand]
    private void SavePlayerAs()
    {
        if (Player == null) return;

        var dlg = new SaveFileDialog
        {
            Title = Language.player_editor_save_as,
            Filter = "Player Files (*.plr)|*.plr|All Files (*.*)|*.*",
            InitialDirectory = Directory.Exists(DefaultPlayerDir) ? DefaultPlayerDir : "",
            FileName = Path.GetFileName(PlayerFilePath ?? "player.plr")
        };

        if (dlg.ShowDialog() == true)
        {
            PlayerFilePath = dlg.FileName;
            SavePlayer();
        }
    }

    private void SyncFromPlayer()
    {
        if (Player == null) return;

        HairColorVm.SetColor(Player.Appearance.HairColor);
        SkinColorVm.SetColor(Player.Appearance.SkinColor);
        EyeColorVm.SetColor(Player.Appearance.EyeColor);
        ShirtColorVm.SetColor(Player.Appearance.ShirtColor);
        UnderShirtColorVm.SetColor(Player.Appearance.UnderShirtColor);
        PantsColorVm.SetColor(Player.Appearance.PantsColor);
        ShoeColorVm.SetColor(Player.Appearance.ShoeColor);

        // Set skin variant selection
        if (SkinVariantOptions != null && Player.Appearance.SkinVariant < SkinVariantOptions.Count)
            SelectedSkinVariant = SkinVariantOptions[Player.Appearance.SkinVariant];

        SelectedLoadoutIndex = Player.CurrentLoadoutIndex;
        UpdateActiveLoadout();
        RegeneratePreview();
    }

    private void RegeneratePreview()
    {
        if (Player == null) return;

        var textures = ViewModelLocator.WorldViewModel.Textures;
        if (textures == null || !textures.Valid) return;

        try
        {
            PlayerPreview = PlayerPreviewRenderer.RenderPreview(Player, textures);
        }
        catch
        {
            // Silently fail if textures aren't available yet
        }
    }

    private void SyncToPlayer()
    {
        if (Player == null) return;

        Player.Appearance.HairColor = HairColorVm.GetColor();
        Player.Appearance.SkinColor = SkinColorVm.GetColor();
        Player.Appearance.EyeColor = EyeColorVm.GetColor();
        Player.Appearance.ShirtColor = ShirtColorVm.GetColor();
        Player.Appearance.UnderShirtColor = UnderShirtColorVm.GetColor();
        Player.Appearance.PantsColor = PantsColorVm.GetColor();
        Player.Appearance.ShoeColor = ShoeColorVm.GetColor();
    }

    public static string GetDifficultyName(byte difficulty) => difficulty switch
    {
        0 => "Classic",
        1 => "Mediumcore",
        2 => "Hardcore",
        3 => "Journey",
        _ => $"Unknown ({difficulty})"
    };
}
