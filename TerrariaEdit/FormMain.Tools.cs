using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TerrariaMapEditor
{
    partial class FormMain
    {


        private Point TileSelectionStart;
        private Point TileSelectionEnd;


        private Rectangle GetSelectionRectangle()
        {
            if (this.TileSelectionEnd != null && this.TileSelectionStart != null)
            {
                return new Rectangle((int)Math.Min(this.TileSelectionStart.X, this.TileSelectionEnd.X),
                                                            (int)Math.Min(this.TileSelectionStart.Y, this.TileSelectionEnd.Y),
                                                            (int)Math.Abs(this.TileSelectionStart.X - this.TileSelectionEnd.X),
                                                            (int)Math.Abs(this.TileSelectionStart.Y - this.TileSelectionEnd.Y));
            }
            return new Rectangle();
        }

        private void DrawSelection(Graphics g, Point topLeft)
        {
            if (this.TileSelectionEnd != null && this.TileSelectionStart != null)
            {
                Rectangle selectionArea = GetSelectionRectangle();
                selectionArea.X = selectionArea.X - topLeft.X;
                selectionArea.Y = selectionArea.Y - topLeft.Y;


                if (selectionArea.Width > 0 && selectionArea.Height > 0)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.CornflowerBlue)), selectionArea);
                }
            }
        }

        private Tool _ActiveTool;
        public Tool ActiveTool
        {
            get
            {
                return _ActiveTool;
            }
            set
            {
                _ActiveTool = value;
                this.CheckToolStripItem();
            }
        }

        private void CheckToolStripItem()
        {
            switch (this._ActiveTool)
            {
                case Tool.Arrow:
                    toolstripEditArrow.Checked = true;
                    toolstripEditBrush.Checked = false;
                    toolstripEditPencil.Checked = false;
                    toolstripEditSelect.Checked = false;
                    toolstripEditBucket.Checked = false;
                    break;
                case Tool.Selection:
                    toolstripEditArrow.Checked = false;
                    toolstripEditBrush.Checked = false;
                    toolstripEditPencil.Checked = false;
                    toolstripEditSelect.Checked = true;
                    toolstripEditBucket.Checked = false;
                    break;
                case Tool.Pencil:
                    toolstripEditArrow.Checked = false;
                    toolstripEditBrush.Checked = false;
                    toolstripEditPencil.Checked = true;
                    toolstripEditSelect.Checked = false;
                    toolstripEditBucket.Checked = false;
                    break;
                case Tool.Brush:
                    toolstripEditArrow.Checked = false;
                    toolstripEditBrush.Checked = true;
                    toolstripEditPencil.Checked = false;
                    toolstripEditSelect.Checked = false;
                    toolstripEditBucket.Checked = false;
                    break;
                case Tool.Bucket:
                    toolstripEditArrow.Checked = false;
                    toolstripEditBrush.Checked = false;
                    toolstripEditPencil.Checked = false;
                    toolstripEditSelect.Checked = false;
                    toolstripEditBucket.Checked = true;
                    break;
                default:
                    toolstripEditArrow.Checked = true;
                    toolstripEditBrush.Checked = false;
                    toolstripEditPencil.Checked = false;
                    toolstripEditSelect.Checked = false;
                    toolstripEditBucket.Checked = false;
                    break;
            }
        }

        private void toolstripEditArrow_Click(object sender, EventArgs e)
        {
            this.ActiveTool = Tool.Arrow;
        }



        private void toolstripEditSelect_Click(object sender, EventArgs e)
        {
            this.ActiveTool = Tool.Selection;
        }

        private void toolstripEditPencil_Click(object sender, EventArgs e)
        {
            this.ActiveTool = Tool.Pencil;
        }

        private void toolstripEditBrush_Click(object sender, EventArgs e)
        {
            this.ActiveTool = Tool.Brush;
        }

        private void toolstripEditBucket_Click(object sender, EventArgs e)
        {
            this.ActiveTool = Tool.Bucket;
        }

        private void worldViewportMain_MouseEnter(object sender, EventArgs e)
        {
            if (this.ActiveTool == Tool.Selection)
                this.Cursor = Cursors.Cross;
            else if (this.ActiveTool == Tool.Arrow)
                this.Cursor = Cursors.Default;
            else
                this.Cursor = Cursors.Cross;

        }

        private void worldViewportMain_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void worldViewportMain_DrawToolOverlay(object sender, PaintEventArgs e)
        {
            if (this.worldViewportMain.TileMouseOver != null)
            {
                DrawSelection(e.Graphics, e.ClipRectangle.Location);
                Point topLeftTile = e.ClipRectangle.Location;
                Point mappedLocation = new Point(this.worldViewportMain.TileMouseOver.X - topLeftTile.X,
                                                    this.worldViewportMain.TileMouseOver.Y - topLeftTile.Y);
                Rectangle brushrect = new Rectangle(new Point(mappedLocation.X - (brushsize / 2), mappedLocation.Y - (brushsize / 2)), new Size(brushsize, brushsize));
                SolidBrush brushoverlay = new SolidBrush(Color.FromArgb(128, Color.Blue));

                switch (this.ActiveTool)
                {
                    case Tool.Arrow:
                        break;
                    case Tool.Selection:
                        break;
                    case Tool.Pencil:
                        e.Graphics.FillRectangle(Brushes.Black, new Rectangle(mappedLocation, new Size(1, 1)));
                        break;
                    case Tool.Brush:
                        
                        if (this.toolstripMainBrushStyle.SelectedItem.ToString() == BrushStyle.Square.ToString())
                            e.Graphics.FillRectangle(brushoverlay, brushrect);
                        else if (this.toolstripMainBrushStyle.SelectedItem.ToString() == BrushStyle.Round.ToString())
                            e.Graphics.FillEllipse(brushoverlay, brushrect);
                        break;
                    case Tool.Bucket:
                        break;
                    default:
                        break;
                }
                if (this.ActiveTool == Tool.Pencil)
                {

                }

                brushoverlay.Dispose();
            }
        }

        private void worldViewportMain_MouseMove(object sender, MouseEventArgs e)
        {
            this.statusTileLocLabel.Text = this.worldViewportMain.TileMouseOver.ToString();
            if (this.ActiveTool != Tool.Arrow)
            {
                this.worldViewportMain.Invalidate();
            }
        }

        private void worldViewportMain_MouseMoveTile(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                switch (this.ActiveTool)
                {
                    case Tool.Arrow:
                        break;
                    case Tool.Selection:
                        this.TileSelectionEnd = e.Location;
                        break;
                    case Tool.Pencil:
                        UseToolPencil(e.Location);
                        break;
                    case Tool.Brush:
                        UseToolBrush(e.Location);
                        break;
                    case Tool.Bucket:
                        UseToolBucket(e.Location);
                        break;
                    default:
                        break;
                }
            }
        }

        private void worldViewportMain_MouseDownTile(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                switch (this.ActiveTool)
                {
                    case Tool.Arrow:
                        break;
                    case Tool.Selection:
                        this.TileSelectionStart = e.Location;
                        break;
                    case Tool.Pencil:
                        UseToolPencil(e.Location);
                        break;
                    case Tool.Brush:
                        UseToolBrush(e.Location);
                        break;
                    case Tool.Bucket:
                        UseToolBucket(e.Location);
                        break;
                    default:
                        break;
                }
            }
        }

        private void UseToolBucket(Point point)
        {
            TerrariaMapEditor.Renderer.TileProperties tile = this.tilePicker1.IsPaintTile ? this.tilePicker1.TileType : null;
            TerrariaMapEditor.Renderer.TileProperties wall = this.tilePicker1.IsPaintWall ? this.tilePicker1.WallType : null;
            TerrariaMapEditor.Renderer.TileProperties mask = this.tilePicker1.IsUseMask ? this.tilePicker1.MaskType : null;
            Rectangle brushrect = GetSelectionRectangle();
            for (int x = brushrect.X; x < (brushrect.X + brushrect.Width); x++)
            {
                for (int y = brushrect.Y; y < (brushrect.Y + brushrect.Height); y++)
                {
                    SetWorldTile(new Point(x, y), tile, wall, mask);
                }
            }
        }

        private void UseToolBrush(Point point)
        {
            TerrariaMapEditor.Renderer.TileProperties tile = this.tilePicker1.IsPaintTile ? this.tilePicker1.TileType : null;
            TerrariaMapEditor.Renderer.TileProperties wall = this.tilePicker1.IsPaintWall ? this.tilePicker1.WallType : null;
            TerrariaMapEditor.Renderer.TileProperties mask = this.tilePicker1.IsUseMask ? this.tilePicker1.MaskType : null;

            Rectangle brushrect = new Rectangle(new Point(point.X - (brushsize / 2), point.Y - (brushsize / 2)), new Size(brushsize, brushsize));
            for (int x = brushrect.X; x < (brushrect.X + brushrect.Width); x++)
            {
                for (int y = brushrect.Y; y < (brushrect.Y + brushrect.Height); y++)
                {
                    if (this.toolstripMainBrushStyle.SelectedItem.ToString() == BrushStyle.Square.ToString())
                    {
                        SetWorldTile(new Point(x, y), tile, wall, mask);
                    }
                    else if (this.toolstripMainBrushStyle.SelectedItem.ToString() == BrushStyle.Round.ToString())
                    { 
                        // Check radius
                        double distance = Math.Sqrt(Math.Pow(Math.Abs(y - point.Y),2) + Math.Pow(Math.Abs(x - point.X),2));
                        if (distance < this.brushsize / 2)
                        {
                            SetWorldTile(new Point(x, y), tile, wall, mask);
                        }
                    }
                }
            }


        }

        private void UseToolPencil(Point point)
        {
            TerrariaMapEditor.Renderer.TileProperties tile = this.tilePicker1.IsPaintTile ? this.tilePicker1.TileType : null;
            TerrariaMapEditor.Renderer.TileProperties wall = this.tilePicker1.IsPaintWall ? this.tilePicker1.WallType : null;
            TerrariaMapEditor.Renderer.TileProperties mask = this.tilePicker1.IsUseMask ? this.tilePicker1.MaskType : null;
            SetWorldTile(point, tile, wall, mask);
        }

        private void worldViewportMain_MouseUpTile(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                switch (this.ActiveTool)
                {
                    case Tool.Arrow:
                        break;
                    case Tool.Selection:
                        this.TileSelectionEnd = e.Location;
                        break;
                    case Tool.Pencil:
                        break;
                    case Tool.Brush:
                        break;
                    case Tool.Bucket:
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetWorldTile(Point location, Renderer.TileProperties tile, Renderer.TileProperties wall, Renderer.TileProperties mask)
        {
            // Check Bounds
            Rectangle worldImageRect = new Rectangle(0, 0, this._worldImage.Width, this._worldImage.Height);
            if (!worldImageRect.Contains(location))
            {
                return;
            }

            //Temporary limit to prevent overwriting framed entities
            if (this._world.Tiles[location.X, location.Y].Frame.X != -1 && this._world.Tiles[location.X, location.Y].Frame.Y != -1)
                return;

            // Validate Mask, return if fail
            if (mask != null)
            {
                if (mask.Name == "Air")
                {
                    if (this._world.Tiles[location.X, location.Y].Type != 0 ||
                        this._world.Tiles[location.X, location.Y].IsActive == true ||
                        this._world.Tiles[location.X, location.Y].Liquid > 0)
                        return;
                }

                if (this._world.Tiles[location.X, location.Y].Type != mask.ID)
                    return;
            }

            TerrariaWorld.Game.Tile updateTile = new TerrariaWorld.Game.Tile();

            if (tile != null)
            {
                if (tile.Name == "Water")
                {
                    updateTile.Liquid = 255;
                }
                else if (tile.Name == "Lava")
                {
                    updateTile.IsActive = false;
                    updateTile.Liquid = 255;
                    updateTile.IsLava = true;
                    updateTile.IsLighted = false;
                }
                else
                {
                    updateTile.IsActive = true;
                    updateTile.Type = tile.ID;
                }
            }

            if (wall != null)
            {
                updateTile.Wall = wall.ID;
            }

            this._world.Tiles[location.X, location.Y] = updateTile;
            this._worldRenderer.UpdateSingleTile(this._world, this._worldImage, location);
            return;
        }
    }

    public enum Tool
    {
        Arrow,
        Selection,
        Pencil,
        Brush,
        Bucket
    }

    public enum BrushStyle
    {
        Round,
        Square
    }
}
