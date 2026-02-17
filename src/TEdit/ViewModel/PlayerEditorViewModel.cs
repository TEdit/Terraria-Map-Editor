using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Properties;
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

        SelectedLoadoutIndex = Player.CurrentLoadoutIndex;
        UpdateActiveLoadout();
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
