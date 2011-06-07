namespace TerrariaMapEditor.Views
{
    partial class ChestOpsFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChestOpsFrm));
            this.grpBox = new System.Windows.Forms.GroupBox();
            this.lblJumpDetails = new System.Windows.Forms.Label();
            this.chkJump = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBox
            // 
            this.grpBox.Controls.Add(this.lblJumpDetails);
            this.grpBox.Controls.Add(this.chkJump);
            this.grpBox.Location = new System.Drawing.Point(7, 6);
            this.grpBox.Name = "grpBox";
            this.grpBox.Size = new System.Drawing.Size(427, 101);
            this.grpBox.TabIndex = 0;
            this.grpBox.TabStop = false;
            this.grpBox.Text = "Chest Editor";
            // 
            // lblJumpDetails
            // 
            this.lblJumpDetails.AutoSize = true;
            this.lblJumpDetails.Location = new System.Drawing.Point(35, 39);
            this.lblJumpDetails.Name = "lblJumpDetails";
            this.lblJumpDetails.Size = new System.Drawing.Size(377, 39);
            this.lblJumpDetails.TabIndex = 1;
            this.lblJumpDetails.Text = resources.GetString("lblJumpDetails.Text");
            // 
            // chkJump
            // 
            this.chkJump.AutoSize = true;
            this.chkJump.Location = new System.Drawing.Point(15, 19);
            this.chkJump.Name = "chkJump";
            this.chkJump.Size = new System.Drawing.Size(135, 17);
            this.chkJump.TabIndex = 0;
            this.chkJump.Text = "Jump to selected chest";
            this.chkJump.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(260, 112);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(149, 21);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(45, 113);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(154, 19);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ChestOpsFrm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(441, 144);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ChestOpsFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chest Editor Options";
            this.Load += new System.EventHandler(this.ChestOpsFrm_Load);
            this.grpBox.ResumeLayout(false);
            this.grpBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpBox;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkJump;
        private System.Windows.Forms.Label lblJumpDetails;
    }
}