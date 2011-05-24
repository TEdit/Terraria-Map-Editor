namespace TerrariaMapEditor
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editEntitiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.signToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nPCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.worldSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideSideBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wikiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.statusTileLocLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolstripMainUndo = new System.Windows.Forms.ToolStripButton();
            this.toolstripMainRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolstripMainZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolstripmainZoomField = new System.Windows.Forms.ToolStripComboBox();
            this.toolstripmainZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolstripMainRenderSelection = new System.Windows.Forms.ToolStripButton();
            this.toolstripMainRender = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolstripMainBrushSize = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolstripMainBrushStyle = new System.Windows.Forms.ToolStripComboBox();
            this.editToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolstripEditArrow = new System.Windows.Forms.ToolStripButton();
            this.toolstripEditPencil = new System.Windows.Forms.ToolStripButton();
            this.toolstripEditSelect = new System.Windows.Forms.ToolStripButton();
            this.toolstripEditBrush = new System.Windows.Forms.ToolStripButton();
            this.toolstripEditBucket = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.worldViewportMain = new TerrariaMapEditor.Controls.WorldViewport();
            this.editorTabs = new System.Windows.Forms.TabControl();
            this.tabWorld = new System.Windows.Forms.TabPage();
            this.worldEditorView1 = new TerrariaMapEditor.Views.WorldEditorView();
            this.tabChests = new System.Windows.Forms.TabPage();
            this.chestEditorView1 = new TerrariaMapEditor.Views.ChestEditorView();
            this.tabSigns = new System.Windows.Forms.TabPage();
            this.tabNPCs = new System.Windows.Forms.TabPage();
            this.tilePicker1 = new TerrariaMapEditor.Controls.TilePicker();
            this.mainMenu.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            this.editToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.editorTabs.SuspendLayout();
            this.tabWorld.SuspendLayout();
            this.tabChests.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editEntitiesToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(1109, 24);
            this.mainMenu.TabIndex = 12;
            this.mainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(121, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editEntitiesToolStripMenuItem
            // 
            this.editEntitiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chestToolStripMenuItem,
            this.signToolStripMenuItem,
            this.nPCToolStripMenuItem,
            this.toolStripSeparator1,
            this.worldSettingsToolStripMenuItem,
            this.hideSideBarToolStripMenuItem});
            this.editEntitiesToolStripMenuItem.Name = "editEntitiesToolStripMenuItem";
            this.editEntitiesToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.editEntitiesToolStripMenuItem.Text = "&Edit";
            // 
            // chestToolStripMenuItem
            // 
            this.chestToolStripMenuItem.Name = "chestToolStripMenuItem";
            this.chestToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.chestToolStripMenuItem.Text = "&Chest";
            // 
            // signToolStripMenuItem
            // 
            this.signToolStripMenuItem.Name = "signToolStripMenuItem";
            this.signToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.signToolStripMenuItem.Text = "&Sign";
            // 
            // nPCToolStripMenuItem
            // 
            this.nPCToolStripMenuItem.Name = "nPCToolStripMenuItem";
            this.nPCToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.nPCToolStripMenuItem.Text = "&NPC";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // worldSettingsToolStripMenuItem
            // 
            this.worldSettingsToolStripMenuItem.Name = "worldSettingsToolStripMenuItem";
            this.worldSettingsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.worldSettingsToolStripMenuItem.Text = "&World Settings";
            this.worldSettingsToolStripMenuItem.Click += new System.EventHandler(this.worldSettingsToolStripMenuItem_Click);
            // 
            // hideSideBarToolStripMenuItem
            // 
            this.hideSideBarToolStripMenuItem.Name = "hideSideBarToolStripMenuItem";
            this.hideSideBarToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.hideSideBarToolStripMenuItem.Text = "Hide Side Bar";
            this.hideSideBarToolStripMenuItem.Click += new System.EventHandler(this.hideSideBarToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdatesToolStripMenuItem,
            this.wikiToolStripMenuItem,
            this.toolStripSeparator3,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Enabled = false;
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for &Updates";
            // 
            // wikiToolStripMenuItem
            // 
            this.wikiToolStripMenuItem.Enabled = false;
            this.wikiToolStripMenuItem.Name = "wikiToolStripMenuItem";
            this.wikiToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.wikiToolStripMenuItem.Text = "&Wiki";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(171, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusTileLocLabel,
            this.toolStripStatusLabel2,
            this.statusLabel,
            this.statusProgressBar});
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 645);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Size = new System.Drawing.Size(1109, 22);
            this.mainStatusStrip.TabIndex = 0;
            this.mainStatusStrip.Text = "statusStrip1";
            // 
            // statusTileLocLabel
            // 
            this.statusTileLocLabel.Name = "statusTileLocLabel";
            this.statusTileLocLabel.Size = new System.Drawing.Size(31, 17);
            this.statusTileLocLabel.Text = "(x,y)";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(961, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // statusProgressBar
            // 
            this.statusProgressBar.Name = "statusProgressBar";
            this.statusProgressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.CanOverflow = false;
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator6,
            this.toolstripMainUndo,
            this.toolstripMainRedo,
            this.toolStripSeparator5,
            this.toolstripMainZoomOut,
            this.toolstripmainZoomField,
            this.toolstripmainZoomIn,
            this.toolStripSeparator4,
            this.toolstripMainRenderSelection,
            this.toolstripMainRender,
            this.toolStripSeparator7,
            this.toolStripLabel1,
            this.toolstripMainBrushSize,
            this.toolStripLabel2,
            this.toolstripMainBrushStyle});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 24);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(1109, 25);
            this.mainToolStrip.TabIndex = 23;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // toolstripMainUndo
            // 
            this.toolstripMainUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripMainUndo.Enabled = false;
            this.toolstripMainUndo.Image = global::TerrariaMapEditor.Properties.Resources.arrow_rotate_anticlockwise;
            this.toolstripMainUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripMainUndo.Name = "toolstripMainUndo";
            this.toolstripMainUndo.Size = new System.Drawing.Size(23, 22);
            this.toolstripMainUndo.Text = "Undo";
            // 
            // toolstripMainRedo
            // 
            this.toolstripMainRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripMainRedo.Enabled = false;
            this.toolstripMainRedo.Image = global::TerrariaMapEditor.Properties.Resources.arrow_rotate_clockwise;
            this.toolstripMainRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripMainRedo.Name = "toolstripMainRedo";
            this.toolstripMainRedo.Size = new System.Drawing.Size(23, 22);
            this.toolstripMainRedo.Text = "Redo";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // toolstripMainZoomOut
            // 
            this.toolstripMainZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripMainZoomOut.Image = global::TerrariaMapEditor.Properties.Resources.zoom_out;
            this.toolstripMainZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripMainZoomOut.Name = "toolstripMainZoomOut";
            this.toolstripMainZoomOut.Size = new System.Drawing.Size(23, 22);
            this.toolstripMainZoomOut.Text = "Zoom Out";
            this.toolstripMainZoomOut.Click += new System.EventHandler(this.toolstripMainZoomOutButton_Click);
            // 
            // toolstripmainZoomField
            // 
            this.toolstripmainZoomField.DropDownWidth = 100;
            this.toolstripmainZoomField.Items.AddRange(new object[] {
            "4000%",
            "3200%",
            "2400%",
            "2000%",
            "1600%",
            "1200%",
            "1000%",
            "800%",
            "400%",
            "200%",
            "100%",
            "66%",
            "50%",
            "33%",
            "25%",
            "10%",
            "5%",
            "1%",
            "Auto"});
            this.toolstripmainZoomField.Name = "toolstripmainZoomField";
            this.toolstripmainZoomField.Size = new System.Drawing.Size(80, 25);
            this.toolstripmainZoomField.Text = "Auto";
            this.toolstripmainZoomField.SelectedIndexChanged += new System.EventHandler(this.toolstripZoomField_SelectedIndexChanged);
            this.toolstripmainZoomField.Leave += new System.EventHandler(this.toolstripZoomField_Leave);
            this.toolstripmainZoomField.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolstripZoomField_KeyPress);
            // 
            // toolstripmainZoomIn
            // 
            this.toolstripmainZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripmainZoomIn.Image = global::TerrariaMapEditor.Properties.Resources.zoom_in;
            this.toolstripmainZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripmainZoomIn.Name = "toolstripmainZoomIn";
            this.toolstripmainZoomIn.Size = new System.Drawing.Size(23, 22);
            this.toolstripmainZoomIn.Text = "Zoom In";
            this.toolstripmainZoomIn.Click += new System.EventHandler(this.toolstripmainZoomInButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolstripMainRenderSelection
            // 
            this.toolstripMainRenderSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripMainRenderSelection.Image = global::TerrariaMapEditor.Properties.Resources.world_selection;
            this.toolstripMainRenderSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripMainRenderSelection.Name = "toolstripMainRenderSelection";
            this.toolstripMainRenderSelection.Size = new System.Drawing.Size(23, 22);
            this.toolstripMainRenderSelection.Text = "Render Selection";
            // 
            // toolstripMainRender
            // 
            this.toolstripMainRender.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripMainRender.Image = global::TerrariaMapEditor.Properties.Resources.world;
            this.toolstripMainRender.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripMainRender.Name = "toolstripMainRender";
            this.toolstripMainRender.Size = new System.Drawing.Size(23, 22);
            this.toolstripMainRender.Text = "Render Whole World";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(56, 22);
            this.toolStripLabel1.Text = "Brush Size";
            // 
            // toolstripMainBrushSize
            // 
            this.toolstripMainBrushSize.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "12",
            "15",
            "20",
            "25",
            "30",
            "40",
            "50",
            "60",
            "70",
            "100"});
            this.toolstripMainBrushSize.Name = "toolstripMainBrushSize";
            this.toolstripMainBrushSize.Size = new System.Drawing.Size(75, 25);
            this.toolstripMainBrushSize.Text = "50";
            this.toolstripMainBrushSize.SelectedIndexChanged += new System.EventHandler(this.toolStripMainBrushSize_SelectedIndexChanged);
            this.toolstripMainBrushSize.Leave += new System.EventHandler(this.toolStripMainBrushSize_Leave);
            this.toolstripMainBrushSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripMainBrushSize_KeyPress);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(61, 22);
            this.toolStripLabel2.Text = "Brush Style";
            // 
            // toolstripMainBrushStyle
            // 
            this.toolstripMainBrushStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolstripMainBrushStyle.Name = "toolstripMainBrushStyle";
            this.toolstripMainBrushStyle.Size = new System.Drawing.Size(121, 25);
            // 
            // editToolStrip
            // 
            this.editToolStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.editToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolstripEditArrow,
            this.toolstripEditPencil,
            this.toolstripEditSelect,
            this.toolstripEditBrush,
            this.toolstripEditBucket});
            this.editToolStrip.Location = new System.Drawing.Point(0, 100);
            this.editToolStrip.Name = "editToolStrip";
            this.editToolStrip.Size = new System.Drawing.Size(24, 545);
            this.editToolStrip.TabIndex = 24;
            this.editToolStrip.Text = "toolStrip2";
            // 
            // toolstripEditArrow
            // 
            this.toolstripEditArrow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripEditArrow.Image = global::TerrariaMapEditor.Properties.Resources.cursor;
            this.toolstripEditArrow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripEditArrow.Name = "toolstripEditArrow";
            this.toolstripEditArrow.Size = new System.Drawing.Size(21, 20);
            this.toolstripEditArrow.Text = "Arrow";
            this.toolstripEditArrow.Click += new System.EventHandler(this.toolstripEditArrow_Click);
            // 
            // toolstripEditPencil
            // 
            this.toolstripEditPencil.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripEditPencil.Image = global::TerrariaMapEditor.Properties.Resources.pencil;
            this.toolstripEditPencil.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripEditPencil.Name = "toolstripEditPencil";
            this.toolstripEditPencil.Size = new System.Drawing.Size(21, 20);
            this.toolstripEditPencil.Text = "Pencil";
            this.toolstripEditPencil.Click += new System.EventHandler(this.toolstripEditPencil_Click);
            // 
            // toolstripEditSelect
            // 
            this.toolstripEditSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripEditSelect.Image = global::TerrariaMapEditor.Properties.Resources.shape_square;
            this.toolstripEditSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripEditSelect.Name = "toolstripEditSelect";
            this.toolstripEditSelect.Size = new System.Drawing.Size(21, 20);
            this.toolstripEditSelect.Text = "Select";
            this.toolstripEditSelect.ToolTipText = "Rectangular Selection";
            this.toolstripEditSelect.Click += new System.EventHandler(this.toolstripEditSelect_Click);
            // 
            // toolstripEditBrush
            // 
            this.toolstripEditBrush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripEditBrush.Image = global::TerrariaMapEditor.Properties.Resources.paintbrush;
            this.toolstripEditBrush.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripEditBrush.Name = "toolstripEditBrush";
            this.toolstripEditBrush.Size = new System.Drawing.Size(21, 20);
            this.toolstripEditBrush.Text = "Brush";
            this.toolstripEditBrush.Click += new System.EventHandler(this.toolstripEditBrush_Click);
            // 
            // toolstripEditBucket
            // 
            this.toolstripEditBucket.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolstripEditBucket.Image = global::TerrariaMapEditor.Properties.Resources.paintcan;
            this.toolstripEditBucket.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripEditBucket.Name = "toolstripEditBucket";
            this.toolstripEditBucket.Size = new System.Drawing.Size(21, 20);
            this.toolstripEditBucket.Text = "Fill";
            this.toolstripEditBucket.Click += new System.EventHandler(this.toolstripEditBucket_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(24, 100);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.worldViewportMain);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel2.Controls.Add(this.editorTabs);
            this.splitContainer1.Size = new System.Drawing.Size(1085, 545);
            this.splitContainer1.SplitterDistance = 811;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 2;
            // 
            // worldViewportMain
            // 
            this.worldViewportMain.AutoScroll = true;
            this.worldViewportMain.AutoScrollMinSize = new System.Drawing.Size(150, 150);
            this.worldViewportMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.worldViewportMain.Image = null;
            this.worldViewportMain.IsAutoZoom = true;
            this.worldViewportMain.IsMouseDown = false;
            this.worldViewportMain.Location = new System.Drawing.Point(0, 0);
            this.worldViewportMain.MaxZoom = 10000F;
            this.worldViewportMain.Name = "worldViewportMain";
            this.worldViewportMain.Size = new System.Drawing.Size(811, 545);
            this.worldViewportMain.TabIndex = 1;
            this.worldViewportMain.TileLastClicked = new System.Drawing.Point(0, 0);
            this.worldViewportMain.TileLastReleased = new System.Drawing.Point(0, 0);
            this.worldViewportMain.TileMouseOver = new System.Drawing.Point(0, 0);
            this.worldViewportMain.Zoom = 1F;
            this.worldViewportMain.DrawToolOverlay += new System.Windows.Forms.PaintEventHandler(this.worldViewportMain_DrawToolOverlay);
            this.worldViewportMain.MouseDownTile += new System.Windows.Forms.MouseEventHandler(this.worldViewportMain_MouseDownTile);
            this.worldViewportMain.MouseMoveTile += new System.Windows.Forms.MouseEventHandler(this.worldViewportMain_MouseMoveTile);
            this.worldViewportMain.MouseUpTile += new System.Windows.Forms.MouseEventHandler(this.worldViewportMain_MouseUpTile);
            this.worldViewportMain.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.worldViewportMain_PropertyChanged);
            this.worldViewportMain.MouseEnter += new System.EventHandler(this.worldViewportMain_MouseEnter);
            this.worldViewportMain.MouseLeave += new System.EventHandler(this.worldViewportMain_MouseLeave);
            this.worldViewportMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.worldViewportMain_MouseMove);
            // 
            // editorTabs
            // 
            this.editorTabs.Controls.Add(this.tabWorld);
            this.editorTabs.Controls.Add(this.tabChests);
            this.editorTabs.Controls.Add(this.tabSigns);
            this.editorTabs.Controls.Add(this.tabNPCs);
            this.editorTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorTabs.Location = new System.Drawing.Point(0, 0);
            this.editorTabs.Name = "editorTabs";
            this.editorTabs.SelectedIndex = 0;
            this.editorTabs.Size = new System.Drawing.Size(271, 545);
            this.editorTabs.TabIndex = 0;
            // 
            // tabWorld
            // 
            this.tabWorld.Controls.Add(this.worldEditorView1);
            this.tabWorld.Location = new System.Drawing.Point(4, 22);
            this.tabWorld.Name = "tabWorld";
            this.tabWorld.Padding = new System.Windows.Forms.Padding(3);
            this.tabWorld.Size = new System.Drawing.Size(263, 519);
            this.tabWorld.TabIndex = 0;
            this.tabWorld.Text = "World";
            this.tabWorld.UseVisualStyleBackColor = true;
            // 
            // worldEditorView1
            // 
            this.worldEditorView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.worldEditorView1.Location = new System.Drawing.Point(3, 3);
            this.worldEditorView1.Name = "worldEditorView1";
            this.worldEditorView1.Size = new System.Drawing.Size(257, 513);
            this.worldEditorView1.TabIndex = 0;
            this.worldEditorView1.WorldHeader = null;
            this.worldEditorView1.Save += new System.EventHandler(this.wev_Save);
            this.worldEditorView1.Cancel += new System.EventHandler(this.wev_Cancel);
            // 
            // tabChests
            // 
            this.tabChests.Controls.Add(this.chestEditorView1);
            this.tabChests.Location = new System.Drawing.Point(4, 22);
            this.tabChests.Name = "tabChests";
            this.tabChests.Padding = new System.Windows.Forms.Padding(3);
            this.tabChests.Size = new System.Drawing.Size(263, 519);
            this.tabChests.TabIndex = 1;
            this.tabChests.Text = "Chests";
            this.tabChests.UseVisualStyleBackColor = true;
            // 
            // chestEditorView1
            // 
            this.chestEditorView1.ActiveChest = null;
            this.chestEditorView1.Chests = null;
            this.chestEditorView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chestEditorView1.Location = new System.Drawing.Point(3, 3);
            this.chestEditorView1.Name = "chestEditorView1";
            this.chestEditorView1.Size = new System.Drawing.Size(257, 513);
            this.chestEditorView1.TabIndex = 0;
            this.chestEditorView1.Save += new System.EventHandler(this.chestEditorView1_Save);
            this.chestEditorView1.Cancel += new System.EventHandler(this.chestEditorView1_Cancel);
            // 
            // tabSigns
            // 
            this.tabSigns.Location = new System.Drawing.Point(4, 22);
            this.tabSigns.Name = "tabSigns";
            this.tabSigns.Padding = new System.Windows.Forms.Padding(3);
            this.tabSigns.Size = new System.Drawing.Size(263, 519);
            this.tabSigns.TabIndex = 2;
            this.tabSigns.Text = "Signs";
            this.tabSigns.UseVisualStyleBackColor = true;
            // 
            // tabNPCs
            // 
            this.tabNPCs.Location = new System.Drawing.Point(4, 22);
            this.tabNPCs.Name = "tabNPCs";
            this.tabNPCs.Padding = new System.Windows.Forms.Padding(3);
            this.tabNPCs.Size = new System.Drawing.Size(263, 519);
            this.tabNPCs.TabIndex = 3;
            this.tabNPCs.Text = "NPCs";
            this.tabNPCs.UseVisualStyleBackColor = true;
            // 
            // tilePicker1
            // 
            this.tilePicker1.BackColor = System.Drawing.SystemColors.Control;
            this.tilePicker1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tilePicker1.IsPaintTile = false;
            this.tilePicker1.IsPaintWall = false;
            this.tilePicker1.IsUseMask = false;
            this.tilePicker1.Location = new System.Drawing.Point(0, 49);
            this.tilePicker1.MaskType = null;
            this.tilePicker1.Name = "tilePicker1";
            this.tilePicker1.Size = new System.Drawing.Size(1109, 51);
            this.tilePicker1.TabIndex = 1;
            this.tilePicker1.Tiles = null;
            this.tilePicker1.TileType = null;
            this.tilePicker1.Walls = null;
            this.tilePicker1.WallType = null;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1109, 667);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.editToolStrip);
            this.Controls.Add(this.tilePicker1);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.mainMenu);
            this.Controls.Add(this.mainStatusStrip);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.Name = "FormMain";
            this.Text = "Terraria Map Editor (TEdit)";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.editToolStrip.ResumeLayout(false);
            this.editToolStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.editorTabs.ResumeLayout(false);
            this.tabWorld.ResumeLayout(false);
            this.tabChests.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.WorldViewport worldViewportMain;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editEntitiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nPCToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem worldSettingsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar statusProgressBar;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripComboBox toolstripmainZoomField;
        private System.Windows.Forms.ToolStripButton toolstripmainZoomIn;
        private System.Windows.Forms.ToolStripButton toolstripMainZoomOut;
        private System.Windows.Forms.ToolStrip editToolStrip;
        private System.Windows.Forms.ToolStripButton toolstripEditSelect;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wikiToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripButton toolstripMainRender;
        private System.Windows.Forms.ToolStripButton toolstripMainRenderSelection;
        private System.Windows.Forms.ToolStripButton toolstripEditPencil;
        private System.Windows.Forms.ToolStripButton toolstripEditBrush;
        private System.Windows.Forms.ToolStripButton toolstripEditBucket;
        private System.Windows.Forms.ToolStripButton toolstripMainUndo;
        private System.Windows.Forms.ToolStripButton toolstripMainRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolstripEditArrow;
        private System.Windows.Forms.TabControl editorTabs;
        private System.Windows.Forms.TabPage tabWorld;
        private Views.WorldEditorView worldEditorView1;
        private System.Windows.Forms.TabPage tabChests;
        private System.Windows.Forms.TabPage tabSigns;
        private System.Windows.Forms.TabPage tabNPCs;
        private System.Windows.Forms.ToolStripStatusLabel statusTileLocLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private Controls.TilePicker tilePicker1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolstripMainBrushSize;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox toolstripMainBrushStyle;
        private System.Windows.Forms.ToolStripMenuItem hideSideBarToolStripMenuItem;
        private Views.ChestEditorView chestEditorView1;
    }
}

