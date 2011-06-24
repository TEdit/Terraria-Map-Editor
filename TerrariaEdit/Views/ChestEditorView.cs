using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TerrariaMapEditor.Views
{
    public partial class ChestEditorView : UserControl
    {
        private Controls.WorldViewport wvp;
        public ChestEditorView(Controls.WorldViewport wvp)
        {
            this.wvp = wvp;
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
            if (Classes.ChestOptions.jumpToChest)
            {
                this.wvp.ScrollToTile(new Point(this._ActiveChest.Location.X, this._ActiveChest.Location.Y));
            }
        }

        public void changeListBoxSelectedIndex(int newIndex)
        {
            chestListBox.SelectedIndex = newIndex;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this._ActiveChest.Items.Length; i++)
            {
                this._Chests[chestListBox.SelectedIndex].Items[i] = this._ActiveChest.Items[i];
            }
            this.OnSave(this, e);
        }

        public int findChestBasedOnLocation(Point p)
        {
            int size = this._Chests.Length;
            for(int i = 0; i < size; i++){
                try
                {
                    TerrariaWorld.Game.Chest c_temp = this._Chests[i];
                    if(c_temp != null)
                    {
                        TerrariaWorld.Common.Point t = c_temp.Location;
                        if ((p.X == t.X || p.X == (t.X + 1)) && (p.Y == t.Y || p.Y == t.Y+1))
                        {
                            return i;
                        }
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    //??
                }
            }
            return -1;
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

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void chstViewOpsCtxt_Click(object sender, EventArgs e)
        {
            ChestOpsFrm optionsForm = new ChestOpsFrm();
            optionsForm.ShowDialog(this);
        }
    }
}
