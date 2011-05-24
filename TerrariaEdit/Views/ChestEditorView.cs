using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TerrariaMapEditor.Views
{
    public partial class ChestEditorView : UserControl
    {
        public ChestEditorView()
        {
            InitializeComponent();
        }

        private TerrariaWorld.Game.Chest[] _Chests;
        public TerrariaWorld.Game.Chest[] Chests
        {
            get
            {
                return _Chests;
            }
            set
            {
                _Chests = value;
                this.chestListBox.DataSource = this._Chests;
            }
        }

        private TerrariaWorld.Game.Chest _ActiveChest;
        public TerrariaWorld.Game.Chest ActiveChest
        {
            get
            {
                return _ActiveChest;
            }
            set
            {
                _ActiveChest = value;
            }
        }

        private void chestListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._ActiveChest = this._Chests[chestListBox.SelectedIndex];
            chestDGV.DataSource = this._ActiveChest.Items;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this._ActiveChest.Items.Length; i++)
            {
                this._Chests[chestListBox.SelectedIndex].Items[i] = this._ActiveChest.Items[i];
            }
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
