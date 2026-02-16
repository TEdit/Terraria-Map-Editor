using Microsoft.Win32;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Plugins;
using TEdit.Editor.Tools;
using TEdit.Framework.Events;
using TEdit.Geometry;
using TEdit.Helper;
using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.UI.Xaml.Dialog;

namespace TEdit.ViewModel;

public partial class WorldViewModel
{



    public event EventHandler<EventArgs<bool>> RequestZoomEvent;
    public event EventHandler<EventArgs<bool>> RequestPanEvent;
    public event EventHandler<ScrollEventArgs> RequestScrollEvent;

    protected virtual void OnRequestZoom(object sender, EventArgs<bool> e)
    {
        RequestZoomEvent?.Invoke(sender, e);
    }

    protected virtual void OnRequestPan(object sender, EventArgs<bool> e)
    {
        RequestPanEvent?.Invoke(sender, e);
    }

    protected virtual void OnRequestScroll(object sender, ScrollEventArgs e)
    {
        RequestScrollEvent?.Invoke(sender, e);
    }

    [ReactiveCommand]
    private void ClearSpriteSelection()
    {
        SpriteFilter = string.Empty;
        SelectedSpriteSheet = null;
        SpriteStylesView.Refresh();
    }

    [ReactiveCommand]
    private void RequestPan(bool value) => OnRequestPan(this, new EventArgs<bool>(value));

    [ReactiveCommand]
    private void RequestZoom(bool value) => OnRequestZoom(this, new EventArgs<bool>(value));

    [ReactiveCommand]
    private void RequestScroll(ScrollEventArgs args) => OnRequestScroll(this, new ScrollEventArgs(args.Direction, args.Amount));

    [ReactiveCommand]
    private void NpcAdd(int npcId)
    {
        if (CurrentWorld == null || !WorldConfiguration.NpcNames.ContainsKey(npcId))
            return;

        string name = WorldConfiguration.NpcNames[npcId];
        if (CurrentWorld.NPCs.Any(n => n.SpriteId == npcId))
            return;

        var spawn = new Vector2Int32(CurrentWorld.SpawnX, CurrentWorld.SpawnY);
        var npc = new NPC
        {
            Home = spawn,
            IsHomeless = true,
            DisplayName = name,
            Name = name,
            Position = new Vector2FloatObservable(spawn.X * 16, spawn.Y * 16),
            SpriteId = npcId
        };
        CurrentWorld.NPCs.Add(npc);
        Points.Add(name);

        var listItem = AllNpcs.FirstOrDefault(i => i.SpriteId == npcId);
        if (listItem != null)
            listItem.WorldNpc = npc;

        AllNpcsView.Refresh();
        ActivateNpcPointTool(name);
    }

    [ReactiveCommand]
    private void NpcRemove(int npcId)
    {
        if (CurrentWorld == null)
            return;

        var npc = CurrentWorld.NPCs.FirstOrDefault(n => n.SpriteId == npcId);
        if (npc == null)
            return;

        CurrentWorld.NPCs.Remove(npc);
        Points.Remove(npc.Name);

        var listItem = AllNpcs.FirstOrDefault(i => i.SpriteId == npcId);
        if (listItem != null)
            listItem.WorldNpc = null;

        AllNpcsView.Refresh();
    }

    [ReactiveCommand]
    private void NpcSelect(NpcListItem item)
    {
        if (item?.IsOnMap == true)
        {
            ActivateNpcPointTool(item.WorldNpc.Name);
        }
    }

    [ReactiveCommand]
    private void NpcGoTo(NpcListItem item)
    {
        if (item?.IsOnMap != true) return;

        int tileX = (int)(item.WorldNpc.Position.X / 16);
        int tileY = (int)(item.WorldNpc.Position.Y / 16);
        ZoomFocus?.Invoke(tileX, tileY);
    }

    private void ActivateNpcPointTool(string npcName)
    {
        var pointTool = Tools.FirstOrDefault(t => t is PointTool);
        if (pointTool != null)
        {
            SetActiveTool(pointTool);
            SelectedPoint = npcName;
        }
    }

    [ReactiveCommand]
    private void SaveSign(bool save) => ExecuteSaveSign(save);

    [ReactiveCommand]
    private void SaveXmas(bool save) => SaveXmasTree(save);

    [ReactiveCommand]
    private void SaveTileEntity(bool save) => ExecuteSaveTileEntity(save);

    [ReactiveCommand]
    private Task ImportBestiary() => ImportKillsAndBestiaryAsync();

    private void ExecuteSaveTileEntity(bool save)
    {
        if (save)
        {
            if (SelectedTileEntity != null)
            {
                if (SelectedTileEntity.NetId != 0 && SelectedTileEntity.StackSize == 0) { SelectedTileEntity.StackSize = 1; }
                foreach (var item in SelectedTileEntity.Items)
                {
                    if (item.Id != 0 && item.StackSize == 0)
                    {
                        item.StackSize = 1;
                    }
                }
                foreach (var item in SelectedTileEntity.Dyes)
                {
                    if (item.Id != 0 && item.StackSize == 0)
                    {
                        item.StackSize = 1;
                    }
                }
                TileEntity.PlaceEntity(SelectedTileEntity, this.CurrentWorld);
            }
        }
        else
        {
            SelectedSpecialTile = -1;
        }
    }

    private void SaveXmasTree(bool save)
    {
        if (save)
        {
            if (SelectedXmas != null)
            {
                int tree = SelectedXmasStar;
                tree += (SelectedXmasGarland << 3);
                tree += (SelectedXmasBulb << 6);
                tree += (SelectedXmasLight << 10);
                CurrentWorld.Tiles[SelectedXmas.X, SelectedXmas.Y].V = (short)tree;
            }
        }
        else
        {
            SelectedSpecialTile = -1;
        }
    }

    private void ExecuteSaveSign(bool save)
    {
        if (save)
        {
            if (SelectedSign != null)
            {
                var worldSign = CurrentWorld.GetSignAtTile(SelectedSign.X, SelectedSign.Y);
                if (worldSign != null)
                {
                    worldSign.Text = SelectedSign.Text;
                }
            }
        }
        else
        {
            SelectedSign = null;
            SelectedSpecialTile = -1;
        }
    }

    [ReactiveCommand]
    private void SaveChest(bool save)
    {
        if (save)
        {
            if (SelectedChest != null)
            {
                var worldChest = CurrentWorld.GetChestAtTile(SelectedChest.X, SelectedChest.Y);
                if (worldChest != null)
                {
                    int index = CurrentWorld.Chests.IndexOf(worldChest);
                    CurrentWorld.Chests[index] = SelectedChest.Copy();
                }
                //SelectedChest = null;
            }
        }
        else
        {
            SelectedChest = null;
            SelectedSpecialTile = -1;
        }
    }

    [ReactiveCommand]
    private async Task Update()
    {
        try
        {
            var updateService = new Services.UpdateService(UserSettingsService.Current.UpdateChannel);

            if (!updateService.IsInstalled)
            {
                // Not a Velopack install (portable zip user) — open download page
                var process = new System.Diagnostics.ProcessStartInfo("https://github.com/TEdit/Terraria-Map-Editor/releases");
                process.UseShellExecute = true;
                System.Diagnostics.Process.Start(process);
                return;
            }

            App.SnackbarService.ShowInfo("Checking for updates...", "Update");

            bool downloaded = await updateService.CheckAndDownloadAsync();
            if (downloaded)
            {
                var result = await App.DialogService.ShowMessageAsync(
                    "A new version of TEdit has been downloaded. Restart now to apply the update?",
                    "Update Available",
                    UI.Xaml.Dialog.DialogButton.YesNo,
                    UI.Xaml.Dialog.DialogImage.Question);

                if (result == UI.Xaml.Dialog.DialogResponse.Yes)
                {
                    updateService.ApplyAndRestart();
                }
            }
            else
            {
                App.SnackbarService.ShowSuccess("TEdit is up to date.", "Update");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
            App.SnackbarService.ShowWarning("Update check failed.", "Update");
        }
    }

    [ReactiveCommand]
    private void RunPlugin(IPlugin plugin)
    {
        plugin.Execute();
    }

    // Command wrappers for methods defined in other files
    [ReactiveCommand]
    private void Open() => OpenWorld();

    [ReactiveCommand]
    private Task Save() => SaveWorldAsync();

    [ReactiveCommand]
    private Task SaveAs() => SaveWorldAsAsync();

    [ReactiveCommand]
    private Task SaveAsVersion() => SaveWorldAsVersionAsync();

    [ReactiveCommand]
    private Task Reload() => ReloadWorldAsync();

    [ReactiveCommand]
    private void Copy() => EditCopy();

    [ReactiveCommand]
    private void Paste() => EditPaste();

    [ReactiveCommand]
    private void Delete() => EditDelete();

    [ReactiveCommand]
    private Task Crop() => CropWorldAsync();

    [ReactiveCommand]
    private void Expand() => ExpandWorld();

    [ReactiveCommand]
    private void Redo() => UndoManager?.Redo();

    [ReactiveCommand]
    private void Undo() => UndoManager?.Undo();

    private bool CanCopy()
    {
        return _selection.IsActive;
    }

    private bool CanPaste()
    {
        return Clipboard != null && _clipboard.Buffer != null;
    }

    [ReactiveCommand]
    private void CloseApplication() => Application.Current.Shutdown();

    [ReactiveCommand]
    private void SetTool(ITool tool) => SetActiveTool(tool);

    [ReactiveCommand]
    private async Task SetLanguage(LanguageSelection language)
    {
        CurrentLanguage = language;
        UserSettingsService.Current.Language = language;

        var result = await App.DialogService.ShowMessageAsync(
            $"Language changed to {language}. Do you wish to restart now?",
            "Restart to change language",
            DialogButton.YesNo,
            DialogImage.Question);

        if (result == DialogResponse.Yes)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }
    }

    private LanguageSelection _currentLanguage;

    public LanguageSelection CurrentLanguage
    {
        get { return _currentLanguage; }
        set
        {
            this.RaiseAndSetIfChanged(ref _currentLanguage, value);
            UpdateRenderWorld();
        }
    }

    // Chest Commands

    [ReactiveCommand]
    private void CopyChestItem(object container) => CopyChestItems(container);

    [ReactiveCommand]
    private void PasteChestItem(object parameter) => PasteChestItems(parameter);

    [ReactiveCommand]
    private void ChestItemSetToMaxStack(object container) => ChestItemMaxStack(container);

    private Item _chestItemClipboard;

    private void CopyChestItems(object container)
    {
        if (container is Item item)
        {
            CopyChestItem(item);
        }
        else if (container is TileEntity te)
        {
            if (te.EntityType == TileEntityType.ItemFrame &&
                te.StackSize > 0 &&
                te.NetId != 0)
            {
                var frameItem = new Item(te.StackSize, te.NetId, te.Prefix);

                CopyChestItem(frameItem);
            }
        }
    }

    private void CopyChestItem(Item item)
    {
        _chestItemClipboard = item?.Copy();
    }

    private void PasteChestItems(object parameter)
    {
        if (parameter is System.Collections.IList selectedItems)
        {
            foreach (var obj in selectedItems)
            {
                if (obj is Item item)
                {
                    PasteChestItem(item);
                }
            }
        }
        else if (parameter is Item item)
        {
            PasteChestItem(item);
        }
        else if (parameter is TileEntity te)
        {
            if (te.EntityType == TileEntityType.ItemFrame)
            {
                if (_chestItemClipboard != null)
                {
                    te.NetId = _chestItemClipboard.NetId;
                    te.Prefix = _chestItemClipboard.Prefix;
                    te.StackSize = (short)_chestItemClipboard.StackSize;
                }
                else
                {
                    te.NetId = 0;
                    te.Prefix = 0;
                    te.StackSize = 0;
                }
            }
        }
    }

    private void PasteChestItem(Item item)
    {
        if (_chestItemClipboard != null)
        {
            item.NetId = _chestItemClipboard.NetId;
            item.Prefix = _chestItemClipboard.Prefix;
            item.StackSize = _chestItemClipboard.StackSize;
        }
        else
        {
            item.NetId = 0;
        }
    }

    private void ChestItemMaxStack(object container)
    {
        if (container == null) return;

        if (container is System.Collections.IList selectedItems)
        {
            foreach (var obj in selectedItems)
            {
                if (obj is Item listItem)
                    SetItemMaxStack(listItem);
            }
        }
        else if (container is Item item)
        {
            SetItemMaxStack(item);
        }
        else if (container is TileEntity te && te.EntityType == TileEntityType.ItemFrame)
        {
            var teItem = new Item(te.StackSize, te.NetId, te.Prefix);
            SetItemMaxStack(teItem);
            te.StackSize = (short)teItem.StackSize;
        }
    }

    private static void SetItemMaxStack(Item item)
    {
        if (WorldConfiguration.ItemLookupTable.TryGetValue(item.NetId, out var props) && props.MaxStackSize > 0)
        {
            item.StackSize = props.MaxStackSize;
        }
        else
        {
            item.StackSize = 9999;
        }
    }

    #region Clipboard

    [ReactiveCommand]
    private void ClipboardSetActive(ClipboardBufferPreview item) => ActivateBuffer(item);

    [ReactiveCommand]
    private void RemoveSchematic(ClipboardBufferPreview buffer) => _clipboard?.Remove(buffer);

    [ReactiveCommand]
    private Task ExportSchematic(ClipboardBufferPreview buffer) => ExportSchematicAsync(buffer);

    [ReactiveCommand]
    private Task ImportSchematic(bool isFalseColor) => ImportSchematicAsync(isFalseColor);

    [ReactiveCommand]
    private void EmptyClipboard() => _clipboard?.ClearBuffers();

    [ReactiveCommand]
    private void ClipboardFlipX(ClipboardBufferPreview buffer)
    {
        _clipboard.FlipX(buffer);
        this.PreviewChange();
    }

    [ReactiveCommand]
    private void ClipboardFlipY(ClipboardBufferPreview buffer)
    {
        _clipboard.FlipY(buffer);
        this.PreviewChange();
    }

    [ReactiveCommand]
    private void ClipboardRotate(ClipboardBufferPreview buffer)
    {
        _clipboard.Rotate(buffer);
        this.PreviewChange();
    }

    private void ActivateBuffer(ClipboardBufferPreview item)
    {
        _clipboard.Buffer = item;
        EditPaste();
    }

    private async Task ImportKillsAndBestiaryAsync()
    {
        if (CurrentWorld == null) return;

        var confirmResult = await App.DialogService.ShowMessageAsync(
            "This will completely replace your currently loaded world Bestiary and Kill Tally with selected file's bestiary. Continue?",
            "Load Bestiary?",
            DialogButton.YesNo,
            DialogImage.Question);

        if (confirmResult != DialogResponse.Yes)
            return;

        var ofd = new OpenFileDialog();
        ofd.Filter = "Terraria World File|*.wld";
        ofd.DefaultExt = "Terraria World File| *.wld";
        ofd.Title = "Import Bestiary and Kills from World File";
        ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
        if (!Directory.Exists(ofd.InitialDirectory))
            Directory.CreateDirectory(ofd.InitialDirectory);
        ofd.Multiselect = false;
        if ((bool)ofd.ShowDialog())
        {
            var world = CurrentWorld;

            // make a backup
            var bestiary = world.Bestiary.Copy(CurrentWorld.Version);
            var killTally = world.KilledMobs.ToArray();
            try
            {
                World.ImportKillsAndBestiary(world, ofd.FileName);
                TallyCount = KillTally.LoadTally(CurrentWorld);
            }
            catch (Exception ex)
            {
                world.Bestiary = bestiary;
                world.KilledMobs.Clear();
                world.KilledMobs.AddRange(killTally);
                await App.DialogService.ShowMessageAsync(
                    $"Error importing Bestiary data from {ofd.FileName}. Your current bestiary has been restored.\r\n{ex.Message}",
                    "Import Error",
                    DialogButton.OK,
                    DialogImage.Error);
            }
        }
    }

    private async Task ImportSchematicAsync(bool isFalseColor)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "TEdit Schematic File|*.TEditSch|Png Image (Real TileColor)|*.png|Bitmap Image (Real TileColor)|*.bmp",
            DefaultExt = "TEdit Schematic File|*.TEditSch",
            Title = "Import TEdit Schematic File",
            InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics"),
            Multiselect = true
        };

        if (!Directory.Exists(ofd.InitialDirectory))
            Directory.CreateDirectory(ofd.InitialDirectory);

        if ((bool)ofd.ShowDialog())
        {
            foreach (var file in ofd.FileNames)
            {
                try
                {
                    _clipboard.Import(file);
                }
                catch (Exception ex)
                {
                    await App.DialogService.ShowExceptionAsync(ex.Message);
                }
            }
        }
    }

    private async Task ExportSchematicAsync(ClipboardBufferPreview buffer)
    {
        var sfd = new SaveFileDialog();
        sfd.Filter = "TEdit Schematic File|*.TEditSch|Png Image (Real TileColor)|*.png";
        sfd.Title = "Export Schematic File";
        sfd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
        if (!Directory.Exists(sfd.InitialDirectory))
            Directory.CreateDirectory(sfd.InitialDirectory);

        if ((bool)sfd.ShowDialog())
        {
            try
            {
                buffer.Buffer.Save(sfd.FileName, this.CurrentWorld?.Version ?? WorldConfiguration.CompatibleVersion);
            }
            catch (Exception ex)
            {
                await App.DialogService.ShowExceptionAsync(ex.Message);
            }
        }
    }

    #endregion
}

public enum UpdateChannel
{
    Stable,
    Beta,
    Alpha
}

public enum LanguageSelection
{
    Automatic,
    English,
    Russian,
    Arabic,
    Chinese,
    ChineseTraditional,
    Polish,
    German,
    French,
    Italian,
    Japanese,
    Korean,
    Portuguese,
    Spanish
}
