using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using BCCL.Framework.Events;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight.Command;
using Microsoft.Win32;
using TEditXNA.Terraria;
using TEditXNA.Terraria.Objects;
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
            get { return _npcAddCommand ?? (_npcAddCommand = new RelayCommand<NpcName>(AddNpc)); }
        }

        private void AddNpc(NpcName npc)
        {
            if (CurrentWorld != null)
            {
                if (!CurrentWorld.NPCs.Any(n => n.Name == npc.Character))
                {
                    var spawn = new Vector2Int32(CurrentWorld.SpawnX, CurrentWorld.SpawnY);
                    CurrentWorld.NPCs.Add(new NPC{Home = spawn, IsHomeless = true, Name = npc.Character, Position= new Vector2(spawn.X * 16, spawn.Y * 16), SpriteId = npc.Id});
                    Points.Add(npc.Character);
                    MessageBox.Show(string.Format("{1} ({0}) added to spawn.", npc.Character, npc.Name), "NPC Added");
                }
                else
                {
                    MessageBox.Show(string.Format("{1} ({0}) is already on the map.", npc.Character, npc.Name), "NPC Exists");
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
                    SelectedSign = null;
                }
            }
            else
            {
                SelectedSign = null;
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
                    SelectedChest = null;
                }
            }
            else
            {
                SelectedChest = null;
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
            get { return _emptyClipboardCommand ?? (_emptyClipboardCommand = new RelayCommand(_clipboard.ClearBuffers, () => _clipboard.LoadedBuffers.Count > 0)); }
        }

        private void ActivateBuffer(ClipboardBuffer item)
        {
            _clipboard.Buffer = item;
            EditPaste();
        }

        private void ImportSchematic(bool isFalseColor)
        {

            var ofd = new OpenFileDialog();
            ofd.Filter = "TEdit Schematic File|*.TEditSch|Png Image (Real Color)|*.png|Bitmap Image (Real Color)|*.bmp";
            if (isFalseColor)
                ofd.Filter = "Png Image (False Color)|*.png";
            ofd.DefaultExt = "TEdit Schematic File|*.TEditSch";
            ofd.Title = "Import TEdit Schematic File";
            ofd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
            if (!Directory.Exists(ofd.InitialDirectory))
                Directory.CreateDirectory(ofd.InitialDirectory);
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                _clipboard.Import(ofd.FileName, isFalseColor);
            }
        }

        private void ExportSchematicFile(ClipboardBuffer buffer)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "TEdit Schematic File|*.TEditSch|Png Image (False Color)|*.png|Png Image (Real Color)|*.png";
            sfd.Title = "Export Schematic File";
            sfd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
            if (!Directory.Exists(sfd.InitialDirectory))
                Directory.CreateDirectory(sfd.InitialDirectory);

            if ((bool)sfd.ShowDialog())
            {
                try
                {
                    buffer.Save(sfd.FileName, (sfd.FilterIndex == 2));
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