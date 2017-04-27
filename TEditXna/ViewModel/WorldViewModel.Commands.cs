using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TEdit.Framework.Events;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using TEditXNA.Terraria;
using TEditXna.Editor;
using TEditXna.Editor.Clipboard;
using TEditXna.Editor.Plugins;
using TEditXna.Editor.Tools;

namespace TEditXna.ViewModel
{
    public partial class WorldViewModel
    {
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

        private ICommand _requestZoomCommand;

        public event EventHandler<EventArgs<bool>> RequestZoom;

        protected virtual void OnRequestZoom(object sender, EventArgs<bool> e)
        {
            if (RequestZoom != null) RequestZoom(sender, e);
        }
        public ICommand RequestZoomCommand
        {
            get { return _requestZoomCommand ?? (_requestZoomCommand = new RelayCommand<bool>(o => OnRequestZoom(this, new EventArgs<bool>(o)))); }
        }

        public event EventHandler<EventArgs<ScrollDirection>> RequestScroll;


        protected virtual void OnRequestScroll(object sender, EventArgs<ScrollDirection> e)
        {
            if (RequestScroll != null) RequestScroll(sender, e);
        }

        private ICommand _requestScrollCommand;

        private ICommand _npcAddCommand;


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
                    CurrentWorld.NPCs.Add(new NPC{Home = spawn, IsHomeless = true, DisplayName = name, Name = name, Position= new Vector2(spawn.X * 16, spawn.Y * 16), SpriteId = npcId});
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

        public ICommand RequestScrollCommand
        {
            get { return _requestScrollCommand ?? (_requestScrollCommand = new RelayCommand<ScrollDirection>(o => OnRequestScroll(this, new EventArgs<ScrollDirection>(o)))); }
        }

        public ICommand SaveSignCommand
        {
            get { return _saveSignCommand ?? (_saveSignCommand = new RelayCommand<bool>(SaveSign)); }
        }

        public ICommand SaveMannCommand
        {
            get { return _saveMannCommand ?? (_saveMannCommand = new RelayCommand<bool>(SaveMannequin)); }
        }

        public ICommand SaveXmasCommand
        {
            get { return _saveXmasCommand ?? (_saveXmasCommand = new RelayCommand<bool>(SaveXmasTree)); }
        }

        public ICommand SaveTileEntityCommand
        {
            get { return _saveTileEntityCommand ?? (_saveTileEntityCommand = new RelayCommand<bool>(SaveTileEntity)); }
        }

        private void SaveTileEntity(bool save)
        {
            if (save)
            {
                if (SelectedItemFrame != null)
                {
                    var worldFrame = CurrentWorld.GetTileEntityAtTile(SelectedItemFrame.PosX, SelectedItemFrame.PosY);
                    if (worldFrame != null)
                    {
                        int index = CurrentWorld.TileEntities.IndexOf(worldFrame);
                        CurrentWorld.TileEntities[index] = SelectedItemFrame.Copy();
                    }
                }
            }
            else
            {
                SelectedItemFrame = null;
                SelectedSpecialTile = 0;
            }
        }

        private void SaveMannequin(bool save)
        {
            if (save)
            {
                if (SelectedMannequin != null)
                {
                    CurrentWorld.Tiles[SelectedMannequin.X, SelectedMannequin.Y].U = (short)((CurrentWorld.Tiles[SelectedMannequin.X, SelectedMannequin.Y].U % 100) + (100 * SelectedMannHead));
                    CurrentWorld.Tiles[SelectedMannequin.X, SelectedMannequin.Y + 1].U = (short)((CurrentWorld.Tiles[SelectedMannequin.X, SelectedMannequin.Y + 1].U % 100) + (100 * SelectedMannBody));
                    CurrentWorld.Tiles[SelectedMannequin.X, SelectedMannequin.Y + 2].U = (short)((CurrentWorld.Tiles[SelectedMannequin.X, SelectedMannequin.Y + 2].U % 100) + (100 * SelectedMannLegs));
                }
            }
            else
            {
                SelectedSpecialTile = 0;
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
                SelectedSpecialTile = 0;
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
                SelectedSpecialTile = 0;
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
                SelectedSpecialTile = 0;
            }
        }

        public ICommand SaveRackCommand
        {
            get { return _saveRackCommand ?? (_saveRackCommand = new RelayCommand<bool>(SaveRack)); }
        }

        private void SaveRack(bool save)
        {
            if (save)
            {
                if (SelectedRack != null)
                {
                    CurrentWorld.Tiles[SelectedRack.X, SelectedRack.Y + 1].U = (short)((((CurrentWorld.Tiles[SelectedRack.X, SelectedRack.Y].U / 18) + 1) * 5000) + 100 + SelectedRackNetId);
                    CurrentWorld.Tiles[SelectedRack.X + 1, SelectedRack.Y + 1].U = (short)((((CurrentWorld.Tiles[SelectedRack.X + 1, SelectedRack.Y].U / 18) + 1) * 5000) + (int)SelectedRackPrefix);
                }
            }
            else
            {
                SelectedSpecialTile = 0;
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
            try { System.Diagnostics.Process.Start(url); }
            catch { }
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

        #region Clipboard

        private ICommand _emptyClipboardCommand;
        private ICommand _importSchematicCommand;
        private ICommand _exportSchematicCommand;
        private ICommand _removeSchematicCommand;
        private ICommand _clipboardSetActiveCommand;
        private ICommand _clipboardFlipXCommand;
        private ICommand _clipboardFlipYCommand;


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
}
