using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using TEdit.Common;

namespace TEdit.Tools.Clipboard
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ClipboardManager : ObservableObject
    {
        private ClipboardBuffer _Buffer;
        public ClipboardBuffer Buffer
        {
            get { return this._Buffer; }
            set
            {
                if (this._Buffer != value)
                {
                    this._Buffer = value;
                    this.RaisePropertyChanged("Buffer");
                }
            }
        }

        private readonly ObservableCollection<ClipboardBuffer> _LoadedBuffers = new ObservableCollection<ClipboardBuffer>();
        public ObservableCollection<ClipboardBuffer> LoadedBuffers
        {
            get { return _LoadedBuffers; }
        }

        public void ClearBuffers()
        {
            Buffer = null;
            LoadedBuffers.Clear();
        }
    }
}