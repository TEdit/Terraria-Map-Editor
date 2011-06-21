using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEditWPF.Common;
using TEditWPF.RenderWorld;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;
using TEditWPF.Tools;

namespace TEditWPF.ViewModels
{
    [Export]
    public class WorldViewModel : ObservableObject, IPartImportsSatisfiedNotification
    {
        private TaskScheduler _uiScheduler;
        private TaskFactory _uiFactory;
        
        public WorldViewModel()
        {
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiFactory = new TaskFactory(_uiScheduler);
            Tools = new OrderingCollection<ITool, IOrderMetadata>(t => t.Metadata.Order);
        }

        [Import]
        private WorldRenderer renderer;

        [ImportMany(typeof(ITool))]
        public OrderingCollection<ITool, IOrderMetadata> Tools { get; set; }

        private ITool _ActiveTool;
        public ITool ActiveTool
        {
            get { return this._ActiveTool; }
            set
            {
                if (this._ActiveTool != value)
                {
                    this._ActiveTool = value;
                    this.RaisePropertyChanged("ActiveTool");

                    foreach (var tool in Tools)
                    {
                        tool.Value.IsActive = (tool.Value == this._ActiveTool);
                    }

                }
            }
        }

        private SelectionArea _Selection;
        [Import]
        public SelectionArea Selection
        {
            get { return this._Selection; }
            set
            {
                if (this._Selection != value)
                {
                    this._Selection = value;
                    this.RaisePropertyChanged("Selection");
                }
            }
        }



        private World world = null;
        [Import("World", typeof(World))]
        public World World
        {
            get
            {
                return this.world;
            }
            set
            {
                if (this.world != value)
                {
                    this.world = null;
                    this.world = value;
                    this.RaisePropertyChanged("World");
                    this.RaisePropertyChanged("WorldZoomedHeight");
                    this.RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        private ObservableCollection<Chest> _Chests = new ObservableCollection<Chest>();
        public ObservableCollection<Chest> Chests
        {
            get { return _Chests; }
        }

        private ObservableCollection<Sign> _Signs = new ObservableCollection<Sign>();
        public ObservableCollection<Sign> Signs
        {
            get { return _Signs; }
        }

        private ObservableCollection<NPC> _Npcs = new ObservableCollection<NPC>();
        public ObservableCollection<NPC> Npcs
        {
            get { return _Npcs; }
        }


        public double WorldZoomedHeight
        {
            get
            {
                if (this._WorldImage != null)
                    return this._WorldImage.PixelHeight * this._Zoom;


                return 1;
            }
        }

        public double WorldZoomedWidth
        {
            get
            {
                if (this._WorldImage != null)
                    return this._WorldImage.PixelWidth * this._Zoom;

                return 1;
            }
        }

        private double _Zoom = 1;
        public double Zoom
        {
            get { return this._Zoom; }
            set
            {
                var limitedZoom = value;
                limitedZoom = Math.Min(Math.Max(limitedZoom, 0.05), 1000);

                if (this._Zoom != limitedZoom)
                {
                    this._Zoom = limitedZoom;
                    this.RaisePropertyChanged("Zoom");
                    this.RaisePropertyChanged("WorldZoomedHeight");
                    this.RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        private WriteableBitmap _WorldImage;
        public WriteableBitmap WorldImage
        {
            get { return this._WorldImage; }
            set
            {
                if (this._WorldImage != value)
                {
                    this._WorldImage = value;
                    this.RaisePropertyChanged("WorldImage");
                    this.RaisePropertyChanged("WorldZoomedHeight");
                    this.RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        private bool _isMouseContained;
        public bool IsMouseContained
        {
            get
            {
                return this._isMouseContained;
            }
            set
            {
                if (this._isMouseContained != value)
                {
                    this._isMouseContained = value;
                    this.RaisePropertyChanged("IsMouseContained");
                }
            }
        }

        private ICommand _setTool;
        public ICommand SetTool
        {
            get { return _setTool ?? (_setTool = new RelayCommand<ITool>(t => ActiveTool = t)); }
        }

        private ICommand _mouseMoveCommand;
        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new RelayCommand<TileMouseEventArgs>(OnMouseOverPixel)); }
        }

        private ICommand _mouseDownCommand;
        public ICommand MouseDownCommand
        {
            get { return _mouseDownCommand ?? (_mouseDownCommand = new RelayCommand<TileMouseEventArgs>(OnMouseDownPixel)); }
        }

        private ICommand _mouseUpCommand;
        public ICommand MouseUpCommand
        {
            get { return _mouseUpCommand ?? (_mouseUpCommand = new RelayCommand<TileMouseEventArgs>(OnMouseUpPixel)); }
        }

        private ICommand _mouseWheelCommand;
        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new RelayCommand<TileMouseEventArgs>(OnMouseWheel)); }
        }

        private ICommand _openWorldCommand;
        public ICommand OpenWorldCommand
        {
            get { return _openWorldCommand ?? (_openWorldCommand = new RelayCommand(LoadWorldandRender, CanLoad)); }
        }

        private bool _IsBusy;
        public bool IsBusy
        {
            get { return this._IsBusy; }
            set
            {
                if (this._IsBusy != value)
                {
                    this._IsBusy = value;
                    this.RaisePropertyChanged("IsBusy");
                }
            }
        }


        public bool CanLoad()
        {
            return !IsBusy;
        }

        private void LoadWorldandRender()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            IsBusy = true;
            if ((bool)ofd.ShowDialog())
            {
                Task.Factory.StartNew(() =>
                {
                    this.World.Load(ofd.FileName);
                    var img = renderer.RenderWorld();
                    img.Freeze();
                    _uiFactory.StartNew(() =>
                    {
                        this.WorldImage = img;
                    });
                });
            }
            IsBusy = false;
        }

        private string _WallName;
        public string WallName
        {
            get { return this._WallName; }
            set
            {
                if (this._WallName != value)
                {
                    this._WallName = value;
                    this.RaisePropertyChanged("WallName");
                }
            }
        }

        private string _TileName;
        public string TileName
        {
            get { return this._TileName; }
            set
            {
                if (this._TileName != value)
                {
                    this._TileName = value;
                    this.RaisePropertyChanged("TileName");
                }
            }
        }

        private string _FluidName;
        public string FluidName
        {
            get { return this._FluidName; }
            set
            {
                if (this._FluidName != value)
                {
                    this._FluidName = value;
                    this.RaisePropertyChanged("FluidName");
                }
            }
        }






        private void OnMouseOverPixel(TileMouseEventArgs e)
        {
            this.MouseOverTile = e.Tile;
            var overTile = world.Tiles[e.Tile.X, e.Tile.Y];

            var wallName = renderer.TileColors.WallColor[overTile.Wall].Name;
            var tileName = overTile.IsActive ? renderer.TileColors.TileColor[overTile.Type].Name : "[empty]";
            var fluidname = "[no fluid]";
            if (overTile.Liquid > 0)
            {
                fluidname = overTile.IsLava ? "Lava" : "Water";
                fluidname += " [" + overTile.Liquid.ToString() + "]";
            }

            this.FluidName = fluidname;
            this.TileName = tileName;
            this.WallName = wallName;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.ActiveTool != null)
                    this.ActiveTool.MoveTool(e.Tile);
            }            
        }

        private void OnMouseDownPixel(TileMouseEventArgs e)
        {
            this.MouseDownTile = e.Tile;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.ActiveTool != null)
                    this.ActiveTool.PressTool(e.Tile);
            }
        }

        private void OnMouseUpPixel(TileMouseEventArgs e)
        {
            this.MouseUpTile = e.Tile;

            if (e.LeftButton == MouseButtonState.Released)
            {
                if (this.ActiveTool != null)
                    this.ActiveTool.ReleaseTool(e.Tile);
            }
        }

        private void OnMouseWheel(TileMouseEventArgs e)
        {
            if (e.WheelDelta > 0)
                this.Zoom = this.Zoom * 1.1;
            if (e.WheelDelta < 0)
                this.Zoom = this.Zoom * 0.9;

        }

        private PointInt32 _mouseOverTile;
        public PointInt32 MouseOverTile
        {
            get { return this._mouseOverTile; }
            set
            {
                if (this._mouseOverTile != value)
                {
                    this._mouseOverTile = value;
                    this.RaisePropertyChanged("MouseOverTile");
                }
            }
        }

        private PointInt32 _mouseDownTile;
        public PointInt32 MouseDownTile
        {
            get { return this._mouseDownTile; }
            set
            {
                if (this._mouseDownTile != value)
                {
                    this._mouseDownTile = value;
                    this.RaisePropertyChanged("MouseDownTile");
                }
            }
        }

        private PointInt32 _mouseUpTile;
        public PointInt32 MouseUpTile
        {
            get { return this._mouseUpTile; }
            set
            {
                if (this._mouseUpTile != value)
                {
                    this._mouseUpTile = value;
                    this.RaisePropertyChanged("MouseUpTile");
                }
            }
        }

        private ProgressChangedEventArgs _progress;
        public ProgressChangedEventArgs Progress
        {
            get { return this._progress; }
            set
            {
                if (this._progress != value)
                {
                    this._progress = value;
                    this.RaisePropertyChanged("Progress");
                }
            }
        }

        public void OnImportsSatisfied()
        {
            renderer.ProgressChanged += (s, e) => { this.Progress = e; };
            world.ProgressChanged += (s, e) => { this.Progress = e; };
        }
    }
}
