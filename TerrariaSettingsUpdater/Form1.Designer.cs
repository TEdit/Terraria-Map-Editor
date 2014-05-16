namespace TerrariaSettingsUpdater
{
    partial class Form1
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
            this.itemBTN = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.input = new System.Windows.Forms.RichTextBox();
            this.merge = new System.Windows.Forms.RichTextBox();
            this.output = new System.Windows.Forms.RichTextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.frameGenPixelWidth = new System.Windows.Forms.NumericUpDown();
            this.frameGenPixelHeight = new System.Windows.Forms.NumericUpDown();
            this.frameGenTileWidth = new System.Windows.Forms.NumericUpDown();
            this.frameGenTileHeight = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenPixelWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenPixelHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenTileWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenTileHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // itemBTN
            // 
            this.itemBTN.Location = new System.Drawing.Point(12, 12);
            this.itemBTN.Name = "itemBTN";
            this.itemBTN.Size = new System.Drawing.Size(75, 23);
            this.itemBTN.TabIndex = 0;
            this.itemBTN.Text = "Walls";
            this.itemBTN.UseVisualStyleBackColor = true;
            this.itemBTN.Click += new System.EventHandler(this.itemBTN_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "TileProp";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 163);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 93);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 6;
            this.button3.Text = "merge";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(13, 250);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // input
            // 
            this.input.Location = new System.Drawing.Point(93, 12);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(338, 245);
            this.input.TabIndex = 10;
            this.input.Text = "";
            // 
            // merge
            // 
            this.merge.Location = new System.Drawing.Point(93, 263);
            this.merge.Name = "merge";
            this.merge.Size = new System.Drawing.Size(649, 339);
            this.merge.TabIndex = 11;
            this.merge.Text = "";
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(437, 14);
            this.output.Name = "output";
            this.output.Size = new System.Drawing.Size(305, 245);
            this.output.TabIndex = 12;
            this.output.Text = "";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(12, 39);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 13;
            this.button5.Text = "NPC";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(12, 301);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 36);
            this.button6.TabIndex = 14;
            this.button6.Text = "Frame Generator";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // frameGenPixelWidth
            // 
            this.frameGenPixelWidth.Location = new System.Drawing.Point(45, 343);
            this.frameGenPixelWidth.Name = "frameGenPixelWidth";
            this.frameGenPixelWidth.Size = new System.Drawing.Size(43, 20);
            this.frameGenPixelWidth.TabIndex = 15;
            this.frameGenPixelWidth.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // frameGenPixelHeight
            // 
            this.frameGenPixelHeight.Location = new System.Drawing.Point(45, 363);
            this.frameGenPixelHeight.Name = "frameGenPixelHeight";
            this.frameGenPixelHeight.Size = new System.Drawing.Size(43, 20);
            this.frameGenPixelHeight.TabIndex = 16;
            this.frameGenPixelHeight.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // frameGenTileWidth
            // 
            this.frameGenTileWidth.Location = new System.Drawing.Point(45, 383);
            this.frameGenTileWidth.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.frameGenTileWidth.Name = "frameGenTileWidth";
            this.frameGenTileWidth.Size = new System.Drawing.Size(43, 20);
            this.frameGenTileWidth.TabIndex = 17;
            this.frameGenTileWidth.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // frameGenTileHeight
            // 
            this.frameGenTileHeight.Location = new System.Drawing.Point(45, 403);
            this.frameGenTileHeight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.frameGenTileHeight.Name = "frameGenTileHeight";
            this.frameGenTileHeight.Size = new System.Drawing.Size(43, 20);
            this.frameGenTileHeight.TabIndex = 18;
            this.frameGenTileHeight.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 632);
            this.Controls.Add(this.frameGenTileHeight);
            this.Controls.Add(this.frameGenTileWidth);
            this.Controls.Add(this.frameGenPixelHeight);
            this.Controls.Add(this.frameGenPixelWidth);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.output);
            this.Controls.Add(this.merge);
            this.Controls.Add(this.input);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.itemBTN);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.frameGenPixelWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenPixelHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenTileWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameGenTileHeight)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button itemBTN;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.RichTextBox input;
        private System.Windows.Forms.RichTextBox merge;
        private System.Windows.Forms.RichTextBox output;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.NumericUpDown frameGenPixelWidth;
        private System.Windows.Forms.NumericUpDown frameGenPixelHeight;
        private System.Windows.Forms.NumericUpDown frameGenTileWidth;
        private System.Windows.Forms.NumericUpDown frameGenTileHeight;
    }
}

