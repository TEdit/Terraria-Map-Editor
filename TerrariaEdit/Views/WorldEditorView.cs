using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TerrariaMapEditor.Views
{
    public partial class WorldEditorView : UserControl
    {
        private TerrariaWorld.Game.WorldHeader _WorldHeader;
        public TerrariaWorld.Game.WorldHeader WorldHeader
        {
            get
            {
                return this._WorldHeader;
            }
            set
            {
                this._WorldHeader = value;
                this.propertyGrid1.SelectedObject = this.WorldHeader;
            }
        }

        public WorldEditorView()
        {
            InitializeComponent();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            this.OnSave(this, e);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.OnCancel(this, e);
        }

        public event EventHandler Save;
        protected virtual void OnSave(object sender, EventArgs e)
        {
            if (Save != null)
                Save(sender, e);
        }

        public event EventHandler Cancel;
        protected virtual void OnCancel(object sender, EventArgs e)
        {
            if (Cancel != null)
                Cancel(sender, e);

        }
    }
}
