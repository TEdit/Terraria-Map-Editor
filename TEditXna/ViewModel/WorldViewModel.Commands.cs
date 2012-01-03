using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using BCCL.MvvmLight.Command;
using Microsoft.Win32;
using TEditXna.Editor.Clipboard;
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
            get { return _importSchematicCommand ?? (_importSchematicCommand = new RelayCommand(ImportSchematic)); }
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

        private void ImportSchematic()
        {

            var ofd = new OpenFileDialog();
            ofd.Filter = "TEdit Schematic File|*.TEditSch";
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
            sfd.Filter = "TEdit Schematic File|*.TEditSch";
            sfd.Title = "Export Schematic File";
            sfd.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics");
            if (!Directory.Exists(sfd.InitialDirectory))
                Directory.CreateDirectory(sfd.InitialDirectory);

            if ((bool)sfd.ShowDialog())
            {
                buffer.Save(sfd.FileName);
            }
        }

        #endregion
    }
}