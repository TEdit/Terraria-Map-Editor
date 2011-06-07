using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace TerrariaMapEditor.Controls
{
    public partial class TilePicker : UserControl, INotifyPropertyChanged
    {
        public TilePicker()
        {
            InitializeComponent();
            this.DataBindings.Add(new Binding("IsPaintTile", this.paintTileField, "Checked"));
            this.DataBindings.Add(new Binding("IsPaintWall", this.paintWallField, "Checked"));
            this.DataBindings.Add(new Binding("IsUseMask", this.isMaskField, "Checked"));
        }

        private bool _IsPaintWall;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public bool IsPaintWall
        {
            get { return _IsPaintWall; }
            set
            {
                if (_IsPaintWall != value)
                {
                    _IsPaintWall = value;
                    RaisePropertyChanged("IsPaintWall");
                }
            }
        }

        private bool _IsUseMask;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public bool IsUseMask
        {
            get { return _IsUseMask; }
            set
            {
                if (_IsUseMask != value)
                {
                    _IsUseMask = value;
                    RaisePropertyChanged("IsUseMask");
                }
            }
        }

        private bool _IsPaintTile;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public bool IsPaintTile
        {
            get { return _IsPaintTile; }
            set
            {
                if (_IsPaintTile != value)
                {
                    _IsPaintTile = value;
                    RaisePropertyChanged("IsPaintTile");
                }
            }
        }

        private Renderer.TileProperties _WallType;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public Renderer.TileProperties WallType
        {
            get { return _WallType; }
            set
            {
                if (_WallType != value)
                {
                    _WallType = value;
                    RaisePropertyChanged("WallType");
                }
            }
        }

        private Renderer.TileProperties _MaskType;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public Renderer.TileProperties MaskType
        {
            get { return _MaskType; }
            set
            {
                if (_MaskType != value)
                {
                    _MaskType = value;
                    RaisePropertyChanged("MaskType");
                }
            }
        }

        private Renderer.TileProperties _TileType;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public Renderer.TileProperties TileType
        {
            get { return _TileType; }
            set
            {
                if (_TileType != value)
                {
                    _TileType = value;
                    RaisePropertyChanged("TileType");
                }
            }
        }

        private List<Renderer.TileProperties> _Walls;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public List<Renderer.TileProperties> Walls
        {
            get { return _Walls; }
            set
            {
                if (_Walls != value)
                {
                    _Walls = value;
                    RaisePropertyChanged("Walls");
                    PopulateWallDropDown();
                }
            }
        }

        private List<Renderer.TileProperties> _Mask;
        private List<Renderer.TileProperties> _Tiles;
        /// <summary>
        /// Default Description
        /// </summary>
        [Category("Custom"),
        Browsable(true),
        Description("Default Description")]
        public List<Renderer.TileProperties> Tiles
        {
            get { return _Tiles; }
            set
            {
                if (_Tiles != value)
                {
                    _Tiles = value;
                    _Mask = _Tiles.ToList();
                    _Mask.Insert(0, new Renderer.TileProperties(0, Color.White, "Air"));
                    RaisePropertyChanged("Tiles");
                    PopulateTileDropDown();
                }
            }
        }

        private void PopulateWallDropDown()
		{
            if (this.Walls != null)
            {
                this.wallField.DataSource = this.Walls;
                this.wallField.DisplayMember = "Name";
                this.wallField.SelectedIndex = 0;
            }
		}

        private void PopulateTileDropDown()
		{
            if (this.Tiles != null)
            {
                this.tileField.DataSource = this._Tiles;
                this.tileField.DisplayMember = "Name";
                this.tileField.SelectedIndex = 0;

                this.maskField.DataSource = this._Mask;
                this.maskField.DisplayMember = "Name";
                this.maskField.SelectedIndex = 0;
            }
		}
        
        

        #region Property Change Methods and Events
        
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void RaisePropertyChanged(String propertyName)
        {
            VerifyPropertyName(propertyName);
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Warns the developer if this Object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(String propertyName)
        {
            // verify that the property name matches a real,  
            // public, instance property on this Object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail("Invalid property name: " + propertyName);
            }
        }

        #endregion

        private void wallField_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.wallField.SelectedIndex >= 0)
                this._WallType = (Renderer.TileProperties)this.wallField.SelectedItem;
        }

        private void tileField_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tileField.SelectedIndex >= 0)
                this._TileType = (Renderer.TileProperties)this.tileField.SelectedItem;

        }

        private void maskField_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.maskField.SelectedIndex >= 0)
                this._MaskType = (Renderer.TileProperties)this.maskField.SelectedItem;
        }
    }
}
