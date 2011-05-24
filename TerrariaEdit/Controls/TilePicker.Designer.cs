namespace TerrariaMapEditor.Controls
{
    partial class TilePicker
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.wallField = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tileField = new System.Windows.Forms.ComboBox();
            this.maskField = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.isMaskField = new System.Windows.Forms.CheckBox();
            this.paintWallField = new System.Windows.Forms.CheckBox();
            this.paintTileField = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.wallField);
            this.groupBox1.Location = new System.Drawing.Point(3, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(120, 48);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // wallField
            // 
            this.wallField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wallField.FormattingEnabled = true;
            this.wallField.Location = new System.Drawing.Point(6, 20);
            this.wallField.Name = "wallField";
            this.wallField.Size = new System.Drawing.Size(108, 21);
            this.wallField.TabIndex = 6;
            this.wallField.SelectedIndexChanged += new System.EventHandler(this.wallField_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tileField);
            this.groupBox2.Location = new System.Drawing.Point(129, 1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(120, 48);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            // 
            // tileField
            // 
            this.tileField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tileField.FormattingEnabled = true;
            this.tileField.Location = new System.Drawing.Point(6, 20);
            this.tileField.Name = "tileField";
            this.tileField.Size = new System.Drawing.Size(108, 21);
            this.tileField.TabIndex = 5;
            this.tileField.SelectedIndexChanged += new System.EventHandler(this.tileField_SelectedIndexChanged);
            // 
            // maskField
            // 
            this.maskField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maskField.FormattingEnabled = true;
            this.maskField.Location = new System.Drawing.Point(6, 20);
            this.maskField.Name = "maskField";
            this.maskField.Size = new System.Drawing.Size(108, 21);
            this.maskField.TabIndex = 4;
            this.maskField.SelectedIndexChanged += new System.EventHandler(this.maskField_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.isMaskField);
            this.groupBox3.Controls.Add(this.maskField);
            this.groupBox3.Location = new System.Drawing.Point(255, 1);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(120, 48);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            // 
            // isMaskField
            // 
            this.isMaskField.AutoSize = true;
            this.isMaskField.Location = new System.Drawing.Point(6, 0);
            this.isMaskField.Name = "isMaskField";
            this.isMaskField.Size = new System.Drawing.Size(74, 17);
            this.isMaskField.TabIndex = 6;
            this.isMaskField.Text = "Use Mask";
            this.isMaskField.UseVisualStyleBackColor = true;
            // 
            // paintWallField
            // 
            this.paintWallField.AutoSize = true;
            this.paintWallField.Location = new System.Drawing.Point(10, 0);
            this.paintWallField.Name = "paintWallField";
            this.paintWallField.Size = new System.Drawing.Size(74, 17);
            this.paintWallField.TabIndex = 6;
            this.paintWallField.Text = "Paint Wall";
            this.paintWallField.UseVisualStyleBackColor = true;
            // 
            // paintTileField
            // 
            this.paintTileField.AutoSize = true;
            this.paintTileField.Checked = true;
            this.paintTileField.CheckState = System.Windows.Forms.CheckState.Checked;
            this.paintTileField.Location = new System.Drawing.Point(136, 0);
            this.paintTileField.Name = "paintTileField";
            this.paintTileField.Size = new System.Drawing.Size(70, 17);
            this.paintTileField.TabIndex = 7;
            this.paintTileField.Text = "Paint Tile";
            this.paintTileField.UseVisualStyleBackColor = true;
            // 
            // TilePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.paintTileField);
            this.Controls.Add(this.paintWallField);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "TilePicker";
            this.Size = new System.Drawing.Size(380, 51);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox wallField;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox tileField;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox maskField;
        private System.Windows.Forms.CheckBox isMaskField;
        private System.Windows.Forms.CheckBox paintWallField;
        private System.Windows.Forms.CheckBox paintTileField;
    }
}
