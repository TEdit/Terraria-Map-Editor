using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/**
 * So, I know this is a lot for one option (at the moment), but we could eventually add more.
 * We could allow users to jump to chests in the editor from the world view (already accepted
 * proposal, Issue #21), and maybe even allow the user to choose how they want to jump to the
 * chest (right clicking, alt + clicking - the original suggestion -, ctrl + clicking, or
 * whatever...)
 *  -Shane / June 07, 2011
 */

namespace TerrariaMapEditor.Views
{
    public partial class ChestOpsFrm : Form
    {
        public ChestOpsFrm()
        {
            InitializeComponent();
        }

        private void ChestOpsFrm_Load(object sender, EventArgs e)
        {
            //Jump to chest option
            chkJump.Checked = Classes.ChestOptions.jumpToChest;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Chest jumper checkbox
            Classes.ChestOptions.jumpToChest = chkJump.Checked;
            Classes.ChestOptions.save();
            this.Close();
        }

        private void grpBox_Enter(object sender, EventArgs e)
        {

        }
    }
}
