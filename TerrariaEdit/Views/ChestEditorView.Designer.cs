namespace TerrariaMapEditor.Views
{
    partial class ChestEditorView
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
            this.chestListBox = new System.Windows.Forms.ListBox();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.chestContentsPanel = new System.Windows.Forms.GroupBox();
            this.chestDGV = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.chestCoordsLabel = new System.Windows.Forms.Label();
            this.controlPanel.SuspendLayout();
            this.chestContentsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chestDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // chestListBox
            // 
            this.chestListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chestListBox.FormattingEnabled = true;
            this.chestListBox.Location = new System.Drawing.Point(3, 3);
            this.chestListBox.Name = "chestListBox";
            this.chestListBox.Size = new System.Drawing.Size(400, 147);
            this.chestListBox.TabIndex = 0;
            this.chestListBox.SelectedIndexChanged += new System.EventHandler(this.chestListBox_SelectedIndexChanged);
            // 
            // controlPanel
            // 
            this.controlPanel.Controls.Add(this.cancelButton);
            this.controlPanel.Controls.Add(this.saveButton);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.controlPanel.Location = new System.Drawing.Point(0, 499);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(406, 35);
            this.controlPanel.TabIndex = 2;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(12, 6);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(319, 6);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // chestContentsPanel
            // 
            this.chestContentsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chestContentsPanel.Controls.Add(this.chestDGV);
            this.chestContentsPanel.Controls.Add(this.label2);
            this.chestContentsPanel.Controls.Add(this.chestCoordsLabel);
            this.chestContentsPanel.Location = new System.Drawing.Point(12, 156);
            this.chestContentsPanel.Name = "chestContentsPanel";
            this.chestContentsPanel.Size = new System.Drawing.Size(382, 337);
            this.chestContentsPanel.TabIndex = 3;
            this.chestContentsPanel.TabStop = false;
            this.chestContentsPanel.Text = "Chest Contents";
            // 
            // chestDGV
            // 
            this.chestDGV.AllowUserToAddRows = false;
            this.chestDGV.AllowUserToDeleteRows = false;
            this.chestDGV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chestDGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.chestDGV.Location = new System.Drawing.Point(13, 37);
            this.chestDGV.Name = "chestDGV";
            this.chestDGV.RowHeadersVisible = false;
            this.chestDGV.Size = new System.Drawing.Size(363, 294);
            this.chestDGV.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Chest Coordinates:";
            // 
            // chestCoordsLabel
            // 
            this.chestCoordsLabel.AutoSize = true;
            this.chestCoordsLabel.Location = new System.Drawing.Point(112, 21);
            this.chestCoordsLabel.Name = "chestCoordsLabel";
            this.chestCoordsLabel.Size = new System.Drawing.Size(28, 13);
            this.chestCoordsLabel.TabIndex = 0;
            this.chestCoordsLabel.Text = "(0,0)";
            // 
            // ChestEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chestContentsPanel);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.chestListBox);
            this.Name = "ChestEditorView";
            this.Size = new System.Drawing.Size(406, 534);
            this.controlPanel.ResumeLayout(false);
            this.chestContentsPanel.ResumeLayout(false);
            this.chestContentsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chestDGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox chestListBox;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.GroupBox chestContentsPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label chestCoordsLabel;
        private System.Windows.Forms.DataGridView chestDGV;
    }
}
