using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TEdit.Framework.Events;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using TEdit.Terraria;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Plugins;
using TEdit.Editor.Tools;
using TEdit.Helper;
using TEdit.Properties;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;

namespace TEdit.ViewModel
{
    public partial class WorldViewModel
    {
        private ICommand _saveAsVersionCommand;
        private ICommand _saveAsCommand;
        private ICommand _saveCommand;
        private ICommand _setTool;
        private ICommand _closeApplication;
        private ICommand _commandOpenWorld;
        private ICommand _deleteCommand;
        private ICommand _pasteCommand;
        private ICommand _copyCommand;
        private ICommand _undoCommand;
        private ICommand _redoCommand;
        private ICommand _newWorldCommand;
        private ICommand _runPluginCommand;
        private ICommand _saveChestCommand;
        private ICommand _saveSignCommand;
        private ICommand _saveMannCommand;
        private ICommand _saveXmasCommand;
        private ICommand _saveTileEntityCommand;
        private ICommand _saveRackCommand;
        private ICommand _npcRemoveCommand;
        private ICommand _importBestiaryCommand;
        private ICommand _completeBestiaryCommand;

        private ICommand _npcAddCommand;
        private ICommand _requestZoomCommand;
        private ICommand _requestScrollCommand;
        private ICommand _requestPanCommand;



        public event EventHandler<EventArgs<bool>> RequestZoom;
        public event EventHandler<EventArgs<bool>> RequestPan;
        public event EventHandler<ScrollEventArgs> RequestScroll;

        protected virtual void OnRequestZoom(object sender, EventArgs<bool> e)
        {
            if (RequestZoom != null) RequestZoom(sender, e);
        }

        protected virtual void OnRequestPan(object sender, EventArgs<bool> e)
        {
            if (RequestPan != null) RequestPan(sender, e);
        }

        protected virtual void OnRequestScroll(object sender, ScrollEventArgs e)
        {
            if (RequestScroll != null) RequestScroll(sender, e);
        }

        public ICommand RequestPanCommand
        {
            get { return _requestPanCommand ?? (_requestPanCommand = new RelayCommand<bool>(o => OnRequestPan(this, new EventArgs<bool>(o)))); }
        }

        public ICommand RequestZoomCommand
        {
            get { return _requestZoomCommand ?? (_requestZoomCommand = new RelayCommand<bool>(o => OnRequestZoom(this, new EventArgs<bool>(o)))); }
        }

        public ICommand RequestScrollCommand
        {
            get { return _requestScrollCommand ?? (_requestScrollCommand = new RelayCommand<ScrollEventArgs>(o => OnRequestScroll(this, new ScrollEventArgs(o.Direction, o.Amount)))); }
        }

        public ICommand NpcAddCommand
        {
            get { return _npcAddCommand ?? (_npcAddCommand = new RelayCommand<int>(AddNpc)); }
        }

        private void AddNpc(int npcId)
        {
            if (CurrentWorld != null && World.NpcNames.ContainsKey(npcId))
            {
                string name = World.NpcNames[npcId];
                if (CurrentWorld.NPCs.All(n => n.SpriteId != npcId))
                {
                    var spawn = new Vector2Int32(CurrentWorld.SpawnX, CurrentWorld.SpawnY);
                    CurrentWorld.NPCs.Add(new NPC { Home = spawn, IsHomeless = true, DisplayName = name, Name = name, Position = new Vector2(spawn.X * 16, spawn.Y * 16), SpriteId = npcId });
                    Points.Add(name);
                    MessageBox.Show($"{name} added to spawn.", "NPC Added");
                }
                else
                {
                    MessageBox.Show($"{name} is already on the map.", "NPC Exists");
                }
            }
            else
            {
                MessageBox.Show($"Choose an NPC. NPC {npcId} not found.", "NPC Error");
            }
        }

        public ICommand NpcRemoveCommand
        {
            get { return _npcRemoveCommand ?? (_npcRemoveCommand = new RelayCommand<NPC>(RemoveNpc)); }
        }

        private void RemoveNpc(NPC npc)
        {
            if (CurrentWorld != null)
            {
                try
                {
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(RemoveNpc), properties: new Dictionary<string, string> { ["ID"] = npc.SpriteId.ToString(), ["Name"] = npc.Name });
                    CurrentWorld.NPCs.Remove(npc);
                    Points.Remove(npc.Name);
                    MessageBox.Show(string.Format("{1} ({0}) removed.", npc.Name, npc.DisplayName), "NPC Removed");
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show(string.Format("{1} ({0}) was not on the map.", npc.Name, npc.DisplayName), "NPC Doesn't Exist");
                }
            }
        }

        public ICommand SaveSignCommand
        {
            get { return _saveSignCommand ?? (_saveSignCommand = new RelayCommand<bool>(SaveSign)); }
        }

        public ICommand SaveXmasCommand
        {
            get { return _saveXmasCommand ?? (_saveXmasCommand = new RelayCommand<bool>(SaveXmasTree)); }
        }

        public ICommand SaveTileEntityCommand
        {
            get { return _saveTileEntityCommand ?? (_saveTileEntityCommand = new RelayCommand<bool>(SaveTileEntity)); }
        }

        public ICommand ImportBestiaryCommand
        {
            get { return _importBestiaryCommand ?? (_importBestiaryCommand = new RelayCommand(ImportKillsAndBestiary)); }
        }

        public ICommand CompleteBestiaryCommand
        {
            get { return _completeBestiaryCommand ?? (_completeBestiaryCommand = new RelayCommand(CompleteBestiary)); }
        }

        private void SaveTileEntity(bool save)
        {
            if (save)
            {
                if (SelectedTileEntity != null)
                {
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(SaveTileEntity), properties: new Dictionary<string, string> { ["ID"] = SelectedTileEntity.NetId.ToString(), ["StackSize"] = SelectedTileEntity.StackSize.ToString() });
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
                    TileEntity.PlaceEntity(SelectedTileEntity, this);
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
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(SaveXmasTree));

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

        private void SaveSign(bool save)
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

        private ICommand _updateCommand;
        public ICommand UpdateCommand
        {
            get { return _updateCommand ?? (_updateCommand = new RelayCommand(Update)); }
        }

        public void Update()
        {
            string url = "http://www.binaryconstruct.com/downloads/";
            try
            {
                ErrorLogging.TelemetryClient?.TrackEvent(nameof(Update));

                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
            }
        }

        public ICommand SaveChestCommand
        {
            get { return _saveChestCommand ?? (_saveChestCommand = new RelayCommand<bool>(SaveChest)); }
        }

        public ICommand RunPluginCommand
        {
            get { return _runPluginCommand ?? (_runPluginCommand = new RelayCommand<IPlugin>(RunPlugin)); }
        }

        private void RunPlugin(IPlugin plugin)
        {
            plugin.Execute();
        }

        public ICommand NewWorldCommand
        {
            get { return _newWorldCommand ?? (_newWorldCommand = new RelayCommand(NewWorld)); }
        }
        public ICommand RedoCommand
        {
            get { return _redoCommand ?? (_redoCommand = new RelayCommand(UndoManager.Redo)); }
        }

        public ICommand UndoCommand
        {
            get { return _undoCommand ?? (_undoCommand = new RelayCommand(UndoManager.Undo)); }
        }

        public ICommand CopyCommand
        {
            get { return _copyCommand ?? (_copyCommand = new RelayCommand(EditCopy)); }
        }
        private bool CanCopy()
        {
            return _selection.IsActive;
        }

        public ICommand PasteCommand
        {
            get { return _pasteCommand ?? (_pasteCommand = new RelayCommand(EditPaste)); }
        }
        private bool CanPaste()
        {
            return _clipboard.Buffer != null;
        }
        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(EditDelete)); }
        }

        public ICommand CloseApplicationCommand
        {
            get { return _closeApplication ?? (_closeApplication = new RelayCommand(Application.Current.Shutdown)); }
        }

        public ICommand OpenCommand
        {
            get { return _commandOpenWorld ?? (_commandOpenWorld = new RelayCommand(OpenWorld)); }
        }

        public ICommand SaveAsVersionCommand
        {
            get { return _saveAsVersionCommand ?? (_saveAsVersionCommand = new RelayCommand(SaveWorldAsVersion)); }
        }

        public ICommand SaveAsCommand
        {
            get { return _saveAsCommand ?? (_saveAsCommand = new RelayCommand(SaveWorldAs)); }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(SaveWorld)); }
        }

        public ICommand SetTool
        {
            get { return _setTool ?? (_setTool = new RelayCommand<ITool>(SetActiveTool)); }
        }

        public ICommand SetLanguageCommand
        {
            get { return _setLanguage ?? (_setLanguage = new RelayCommand<LanguageSelection>(SetLanguage)); }
        }

        private LanguageSelection _currentLanguage;

        public LanguageSelection CurrentLanguage
        {

            get { return _currentLanguage; }
            set
            {
                Set(nameof(CurrentLanguage), ref _currentLanguage, value);
                UpdateRenderWorld();
            }

        }

        private void SetLanguage(LanguageSelection language)
        {
            CurrentLanguage = language;
            ErrorLogging.TelemetryClient?.TrackEvent(nameof(SetLanguage), properties: new Dictionary<string, string> { ["language"] = language.ToString() });
            Settings.Default.Language = language;
            try { Settings.Default.Save(); } catch (Exception ex) { ErrorLogging.LogException(ex); }

            if (MessageBox.Show($"Language changed to {language}. Do you wish to restart now?", "Restart to change language", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            }
        }

        // Chest Commands
        private ICommand _copyChestItemCommand;
        private ICommand _pasteChestItemCommand;

        public ICommand CopyChestItemCommand
        {
            get { return _copyChestItemCommand ?? (_copyChestItemCommand = new RelayCommand<Item>(CopyChestItem)); }
        }
        public ICommand PasteChestItemCommand
        {
            get { return _pasteChestItemCommand ?? (_pasteChestItemCommand = new RelayCommand<Item>(PasteChestItem)); }
        }

        private Item _chestItemClipboard;

        private void CopyChestItem(Item item)
        {
            _chestItemClipboard = item?.Copy();
        }

        private void PasteChestItem(Item item)
        {
            if (_chestItemClipboard != null)
            {
                item.NetId = _chestItemClipboard.NetId;
                item.Prefix = _chestItemClipboard.Prefix;
                item.StackSize = _chestItemClipboard.StackSize;
            } else
            {
                item.NetId = 0;
            }
        }

        #region Clipboard

        private ICommand _emptyClipboardCommand;
        private ICommand _importSchematicCommand;
        private ICommand _exportSchematicCommand;
        private ICommand _removeSchematicCommand;
        private ICommand _clipboardSetActiveCommand;
        private ICommand _clipboardFlipXCommand;
        private ICommand _clipboardFlipYCommand;
        private ICommand _setLanguage;

        public ICommand ClipboardSetActiveCommand
        {
            get { return _clipboardSetActiveCommand ?? (_clipboardSetActiveCommand = new RelayCommand<ClipboardBuffer>(ActivateBuffer)); }
        }

        public ICommand RemoveSchematicCommand
        {
            get { return _removeSchematicCommand ?? (_removeSchematicCommand = new RelayCommand<ClipboardBuffer>(_clipboard.Remove)); }
        }

        public ICommand ExportSchematicCommand
        {
            get { return _exportSchematicCommand ?? (_exportSchematicCommand = new RelayCommand<ClipboardBuffer>(ExportSchematicFile)); }
        }

        public ICommand ImportSchematicCommand
        {
            get { return _importSchematicCommand ?? (_importSchematicCommand = new RelayCommand<bool>(ImportSchematic)); }
        }

        public ICommand EmptyClipboardCommand
        {
            get { return _emptyClipboardCommand ?? (_emptyClipboardCommand = new RelayCommand(_clipboard.ClearBuffers)); }
        }

        public ICommand ClipboardFlipXCommand
        {
            get { return _clipboardFlipXCommand ?? (_clipboardFlipXCommand = new RelayCommand<ClipboardBuffer>(_clipboard.FlipX)); }
        }
        public ICommand ClipboardFlipYCommand
        {
            get { return _clipboardFlipYCommand ?? (_clipboardFlipYCommand = new RelayCommand<ClipboardBuffer>(_clipboard.FlipY)); }
        }

        private void ActivateBuffer(ClipboardBuffer item)
        {
            _clipboard.Buffer = item;
            EditPaste();
        }

        public void CompleteBestiary()
        {
            if (MessageBox.Show(
                "This will completely replace your currently loaded world Bestiary and Kill Tally with a completed bestiary. Continue?",
                "Complete Bestiary?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            var world = CurrentWorld;

            // make a backup
            var bestiary = world.Bestiary.Copy(CurrentWorld.Version);
            var killTally = world.KilledMobs.ToArray();
            try
            {
                ErrorLogging.TelemetryClient?.TrackEvent(nameof(CompleteBestiary));

                World.CompleteBestiary(world);
                TallyCount = KillTally.LoadTally(CurrentWorld);
            }
            catch (Exception ex)
            {
                world.Bestiary = bestiary;
                world.KilledMobs.Clear();
                world.KilledMobs.AddRange(killTally);
                MessageBox.Show($"Error completing Bestiary data. Your current bestiary has been restored.\r\n{ex.Message}");

            }
        }

        private void ImportKillsAndBestiary()
        {
            if (MessageBox.Show(
                "This will completely replace your currently loaded world Bestiary and Kill Tally with selected file's bestiary. Continue?",
                "Load Bestiary?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
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
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(ImportKillsAndBestiary));

                    World.ImportKillsAndBestiary(world, ofd.FileName);
                    TallyCount = KillTally.LoadTally(CurrentWorld);
                }
                catch (Exception ex)
                {
                    world.Bestiary = bestiary;
                    world.KilledMobs.Clear();
                    world.KilledMobs.AddRange(killTally);
                    MessageBox.Show($"Error importing Bestiary data from {ofd.FileName}. Your current bestiary has been restored.\r\n{ex.Message}");
                }
            }
        }

        private void ImportSchematic(bool isFalseColor)
        {

            var ofd = new OpenFileDialog();
            ofd.Filter = "TEdit Schematic File|*.TEditSch|Png Image (Real TileColor)|*.png|Bitmap Image (Real TileColor)|*.bmp";
            ofd.DefaultExt = "TEdit Schematic File|*.TEditSch";
            ofd.Title = "Import TEdit Schematic File";
            ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
            if (!Directory.Exists(ofd.InitialDirectory))
                Directory.CreateDirectory(ofd.InitialDirectory);
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                ErrorLogging.TelemetryClient?.TrackEvent(nameof(ImportSchematic));
                _clipboard.Import(ofd.FileName);
            }
        }

        private void ExportSchematicFile(ClipboardBuffer buffer)
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
                    ErrorLogging.TelemetryClient?.TrackEvent(nameof(ExportSchematicFile));
                    buffer.Save(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving Schematic");
                }

            }
        }

        #endregion
    }

    public enum LanguageSelection
    {
        Automatic,
        English,
        Russian,
        Arabic,
        Chinese,
        Polish,
        German,
        Portuguese,
        Spanish
    }
}
