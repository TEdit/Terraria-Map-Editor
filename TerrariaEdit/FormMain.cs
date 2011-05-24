using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;

namespace TerrariaMapEditor
{
    public partial class FormMain : Form
    {
        private Bitmap _worldImage;
        private TerrariaWorld.Game.World _world;
        private Renderer.WorldRenderer _worldRenderer;

        private TaskFactory uiFactory;
        private TaskScheduler uiScheduler;

        public FormMain()
        {
            InitializeComponent();

            TerrariaWorld.Game.TileProperties.InitializeTileProperties();
            
            this._world = new TerrariaWorld.Game.World();
            this._worldRenderer = new Renderer.WorldRenderer();

            TerrariaWorld.Game.World.ProgressChanged += new ProgressChangedEventHandler(World_ProgressChanged);
            this._worldRenderer.ProgressChanged += new ProgressChangedEventHandler(_worldRenderer_ProgressChanged);
            this.worldViewportMain.Scroll += new ScrollEventHandler(worldViewportMain_Scroll);

            
            this.tilePicker1.Walls = this._worldRenderer.TileColors.WallColor.Values.ToList();

            List<Renderer.TileProperties> tiles = new List<Renderer.TileProperties>();
            foreach (var item in this._worldRenderer.TileColors.TileColor)
            {
                if (!TerrariaWorld.Game.TileProperties.IsFrameImportant[item.Key])
                {
                    tiles.Add(item.Value);
                }
            }
            tiles.AddRange(this._worldRenderer.TileColors.LiquidColor.Values);
            this.tilePicker1.Tiles = tiles;

            foreach (var type in Enum.GetNames(typeof(BrushStyle)))
            {
                this.toolstripMainBrushStyle.Items.Add(type);
            }
            this.toolstripMainBrushStyle.SelectedIndex = 0;
            this.toolstripMainBrushSize.Text = "50";
            this.brushsize = 50;
            
        }

        private void worldViewportMain_Scroll(object sender, ScrollEventArgs e)
        {
            var scrollSize = worldViewportMain.AutoScrollMinSize;
            var windowSize = worldViewportMain.Size;
            var scrollLoc = worldViewportMain.AutoScrollPosition;
        }

        private void World_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SetProgress(e);
        }

        private void _worldRenderer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SetProgress(e);
        }

        private void SetProgress(ProgressChangedEventArgs e)
        {
            if (mainStatusStrip.InvokeRequired)
            {
                mainStatusStrip.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.statusProgressBar.Value = e.ProgressPercentage;
                    this.statusLabel.Text = e.UserState.ToString();
                }));
            }
            else
            {
                this.statusProgressBar.Value = e.ProgressPercentage;
                this.statusLabel.Text = e.UserState.ToString();
            }
        }

        private void OpenWorld()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Terraria World File";
            ofd.Multiselect = false;
            ofd.DefaultExt = "Terraria World File|*.wld";
            ofd.Filter = "Terraria World File|*.wld";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Task.Factory.StartNew(() =>
                {
                    // Load world from file
                    this._world = TerrariaWorld.Game.World.Load(ofd.FileName);

                    // Perform initial render of world (full render)
                    var worldimage = _worldRenderer.RenderRegion(this._world, new Rectangle(0, 0, _world.Header.MaxTiles.X, _world.Header.MaxTiles.Y));
                    this.uiFactory.StartNew(() =>
                    {
                        this._worldImage = worldimage;
                        this.worldViewportMain.Image = _worldImage;
                        this.worldEditorView1.WorldHeader = this._world.Header;
                        this.chestEditorView1.Chests = this._world.Chests;
                    });
                });
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            uiFactory = new TaskFactory(uiScheduler);
            this.ActiveTool = Tool.Arrow;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWorld();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void worldSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.worldEditorView1.WorldHeader = this._world.Header.Clone();
            this.splitContainer1.Panel2Collapsed = false;
            this.editorTabs.SelectedTab = this.tabWorld;
        }

        void wev_Cancel(object sender, EventArgs e)
        {
            Views.WorldEditorView wev = sender as Views.WorldEditorView;
            wev.WorldHeader = this._world.Header.Clone();
        }

        void wev_Save(object sender, EventArgs e)
        {
            Views.WorldEditorView wev = sender as Views.WorldEditorView;
            this._world.Header = wev.WorldHeader;
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
                {
                    this._world.SaveFile(this._world.Header.FileName);
                });
        }

        private void SetZoomLevel(string zoom)
        {
            // Check for autozoom and return
            this.worldViewportMain.IsAutoZoom = (toolstripmainZoomField.Text == "Auto");
            if (this.worldViewportMain.IsAutoZoom)
                return;

            if (zoom.Contains("%"))
            {
                zoom = zoom.Trim('%', ' ');

                float z = 0;
                float.TryParse(zoom, out z);
                z /= 100;

                if (z != 0)
                {
                    this.worldViewportMain.Zoom = z;
                }
            }
        }

        private void toolstripZoomField_Leave(object sender, EventArgs e)
        {
            SetZoomFromDropDown();
        }

        private void SetZoomFromDropDown()
        {
            if (Common.Utility.IsNumeric(toolstripmainZoomField.Text, System.Globalization.NumberStyles.Any))
            {
                if (!toolstripmainZoomField.Text.Contains("%"))
                    toolstripmainZoomField.Text += "%";
            }
            SetZoomLevel(toolstripmainZoomField.Text);
        }

        private void toolstripZoomField_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return || e.KeyChar == (char)Keys.Escape)
            {
                SetZoomFromDropDown();
                e.Handled = true;
            }
        }

        private void toolstripZoomField_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetZoomFromDropDown();
        }

        private void toolstripMainZoomOutButton_Click(object sender, EventArgs e)
        {
            this.worldViewportMain.IsAutoZoom = false;
            float newZoom = (float)Math.Round(100 * this.worldViewportMain.Zoom * 0.8F, 0);
            this.worldViewportMain.Zoom = newZoom / 100;
        }

        private void toolstripmainZoomInButton_Click(object sender, EventArgs e)
        {
            this.worldViewportMain.IsAutoZoom = false;
            float newZoom = (float)Math.Round(100 * this.worldViewportMain.Zoom * 1.2F, 0);
            this.worldViewportMain.Zoom = newZoom / 100;
        }

        private void worldViewportMain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Zoom")
            {
                if (worldViewportMain.IsAutoZoom)
                    this.toolstripmainZoomField.Text = "Auto";
                else
                    this.toolstripmainZoomField.Text = (this.worldViewportMain.Zoom * 100).ToString("0") + "%";
            }
        }

        private void hideSidePanel_Click(object sender, EventArgs e)
        {
            
        }

        private int brushsize = 1;
        private void toolStripMainBrushSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetBrushSize();
        }

        private void SetBrushSize()
        {
            if (Common.Utility.IsNumeric(this.toolstripMainBrushSize.Text, System.Globalization.NumberStyles.Any))
            {
                int.TryParse(this.toolstripMainBrushSize.Text, out this.brushsize);
            }
        }

        private void toolStripMainBrushSize_Leave(object sender, EventArgs e)
        {
            SetBrushSize();
        }

        private void toolStripMainBrushSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return || e.KeyChar == (char)Keys.Escape)
            {
                SetBrushSize();
            }
        }

        private void hideSideBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Views.AboutBox about = new Views.AboutBox();
            about.ShowDialog();
        }

        private void chestEditorView1_Cancel(object sender, EventArgs e)
        {
            this.chestEditorView1.Chests = this._world.Chests;
        }

        private void chestEditorView1_Save(object sender, EventArgs e)
        {
            for (int i = 0; i < this._world.Chests.Length; i++)
            {
                this._world.Chests[i] = this.chestEditorView1.Chests[i];
            }
        }
    }
}
