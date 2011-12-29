using System.Windows;
using System.Windows.Input;
using BCCL.MvvmLight.Command;
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
    }
}