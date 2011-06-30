using System;
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
    }
}