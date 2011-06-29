using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TEdit.Common;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;
using TEdit.Tools;

namespace TEdit.ViewModels
{
    [Export]
    public class WorldViewModel : ObservableObject, IPartImportsSatisfiedNotification
    {
        private readonly ObservableCollection<Chest> _Chests = new ObservableCollection<Chest>();
        private readonly ObservableCollection<NPC> _Npcs = new ObservableCollection<NPC>();
        private readonly ObservableCollection<Sign> _Signs = new ObservableCollection<Sign>();
        private readonly TaskFactory _uiFactory;
        private readonly TaskScheduler _uiScheduler;

        private readonly int[] frameTimes = new int[100];
        private ITool _ActiveTool;
        private string _FluidName;

        private int _FrameRate;
        private bool _IsBusy;
        [Import] private SelectionArea _Selection;

        private string _TileName;
        [Import] private TilePicker _TilePicker;

        [Import] private ToolProperties _ToolProperties;


        private string _WallName;
        private double _Zoom = 1;
        private bool _isMouseContained;
        private ICommand _mouseDownCommand;
        private PointInt32 _mouseDownTile;
        private ICommand _mouseMoveCommand;
        private PointInt32 _mouseOverTile;
        private ICommand _mouseUpCommand;
        private PointInt32 _mouseUpTile;
        private ICommand _mouseWheelCommand;
        private ICommand _openWorldCommand;
        private ProgressChangedEventArgs _progress;
        private ICommand _saveWorldCommand;
        private ICommand _setTool;
        private WorldImage _worldImage;
        private int frameTimesIndex;
        private TimeSpan lastRender;

        [Import] private WorldRenderer renderer;

        [Import("World", typeof (World))] private World world;

        public WorldViewModel()
        {
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiFactory = new TaskFactory(_uiScheduler);
            Tools = new OrderingCollection<ITool, IOrderMetadata>(t => t.Metadata.Order);
            CompositionTarget.Rendering += CompTargetRender;
        }

        public int FrameRate
        {
            get { return _FrameRate; }
            set
            {
                if (_FrameRate != value)
                {
                    _FrameRate = value;
                    RaisePropertyChanged("FrameRate");
                }
            }
        }

        public ToolProperties ToolProperties
        {
            get { return _ToolProperties; }
            set
            {
                if (_ToolProperties != value)
                {
                    _ToolProperties = null;
                    _ToolProperties = value;
                    RaisePropertyChanged("ToolProperties");
                }
            }
        }


        public TilePicker TilePicker
        {
            get { return _TilePicker; }
            set
            {
                if (_TilePicker != value)
                {
                    _TilePicker = value;
                    RaisePropertyChanged("TilePicker");
                }
            }
        }


        [ImportMany(typeof (ITool))]
        public OrderingCollection<ITool, IOrderMetadata> Tools { get; set; }

        public ITool ActiveTool
        {
            get { return _ActiveTool; }
            set
            {
                if (_ActiveTool != value)
                {
                    if (_ActiveTool != null)
                        _ActiveTool.IsActive = false;

                    _ActiveTool = value;
                    _ActiveTool.IsActive = true;
                    //foreach (var tool in Tools)
                    //{
                    //    tool.Value.IsActive = (tool.Value == _ActiveTool);
                    //}

                    ToolProperties.Image = null;
                    ToolProperties.Image = _ActiveTool.PreviewTool();
                    RaisePropertyChanged("ActiveTool");
                }
            }
        }


        public SelectionArea Selection
        {
            get { return _Selection; }
            set
            {
                if (_Selection != value)
                {
                    _Selection = value;
                    RaisePropertyChanged("Selection");
                }
            }
        }


        public World World
        {
            get { return world; }
            set
            {
                if (world != value)
                {
                    world = null;
                    world = value;
                    RaisePropertyChanged("World");
                    RaisePropertyChanged("WorldZoomedHeight");
                    RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        public ObservableCollection<Chest> Chests
        {
            get { return _Chests; }
        }

        public ObservableCollection<Sign> Signs
        {
            get { return _Signs; }
        }

        public ObservableCollection<NPC> Npcs
        {
            get { return _Npcs; }
        }


        public double WorldZoomedHeight
        {
            get
            {
                if (_worldImage.Image != null)
                    return _worldImage.Image.PixelHeight*_Zoom;


                return 1;
            }
        }

        public double WorldZoomedWidth
        {
            get
            {
                if (_worldImage.Image != null)
                    return _worldImage.Image.PixelWidth*_Zoom;

                return 1;
            }
        }

        public double Zoom
        {
            get { return _Zoom; }
            set
            {
                double limitedZoom = value;
                limitedZoom = Math.Min(Math.Max(limitedZoom, 0.05), 1000);

                if (_Zoom != limitedZoom)
                {
                    _Zoom = limitedZoom;
                    RaisePropertyChanged("Zoom");
                    RaisePropertyChanged("ZoomInverted");
                    RaisePropertyChanged("WorldZoomedHeight");
                    RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        public double ZoomInverted
        {
            get { return 1/(_Zoom); }
        }

        [Import]
        public WorldImage WorldImage
        {
            get { return _worldImage; }
            set
            {
                if (_worldImage != value)
                {
                    _worldImage = value;
                    RaisePropertyChanged("WorldImage");
                }
            }
        }

        public bool IsMouseContained
        {
            get { return _isMouseContained; }
            set
            {
                if (_isMouseContained != value)
                {
                    _isMouseContained = value;
                    RaisePropertyChanged("IsMouseContained");
                }
            }
        }

        public ICommand SetTool
        {
            get { return _setTool ?? (_setTool = new RelayCommand<ITool>(t => ActiveTool = t)); }
        }

        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new RelayCommand<TileMouseEventArgs>(OnMouseOverPixel)); }
        }

        public ICommand MouseDownCommand
        {
            get { return _mouseDownCommand ?? (_mouseDownCommand = new RelayCommand<TileMouseEventArgs>(OnMouseDownPixel)); }
        }

        public ICommand MouseUpCommand
        {
            get { return _mouseUpCommand ?? (_mouseUpCommand = new RelayCommand<TileMouseEventArgs>(OnMouseUpPixel)); }
        }

        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new RelayCommand<TileMouseEventArgs>(OnMouseWheel)); }
        }

        public ICommand OpenWorldCommand
        {
            get { return _openWorldCommand ?? (_openWorldCommand = new RelayCommand(LoadWorldandRender, CanLoad)); }
        }

        public ICommand SaveWorldCommand
        {
            get { return _saveWorldCommand ?? (_saveWorldCommand = new RelayCommand(SaveWorld, CanSave)); }
        }

        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    RaisePropertyChanged("IsBusy");
                }
            }
        }


        public string WallName
        {
            get { return _WallName; }
            set
            {
                if (_WallName != value)
                {
                    _WallName = value;
                    RaisePropertyChanged("WallName");
                }
            }
        }

        public string TileName
        {
            get { return _TileName; }
            set
            {
                if (_TileName != value)
                {
                    _TileName = value;
                    RaisePropertyChanged("TileName");
                }
            }
        }

        public string FluidName
        {
            get { return _FluidName; }
            set
            {
                if (_FluidName != value)
                {
                    _FluidName = value;
                    RaisePropertyChanged("FluidName");
                }
            }
        }


        public PointInt32 MouseOverTile
        {
            get { return _mouseOverTile; }
            set
            {
                if (_mouseOverTile != value)
                {
                    _mouseOverTile = value;
                    RaisePropertyChanged("MouseOverTile");
                    RaisePropertyChanged("ToolLocation");
                }
            }
        }

        public PointInt32 ToolLocation
        {
            get { return _mouseOverTile - ToolProperties.Offset; }
        }

        public PointInt32 MouseDownTile
        {
            get { return _mouseDownTile; }
            set
            {
                if (_mouseDownTile != value)
                {
                    _mouseDownTile = value;
                    RaisePropertyChanged("MouseDownTile");
                }
            }
        }

        public PointInt32 MouseUpTile
        {
            get { return _mouseUpTile; }
            set
            {
                if (_mouseUpTile != value)
                {
                    _mouseUpTile = value;
                    RaisePropertyChanged("MouseUpTile");
                }
            }
        }

        public ProgressChangedEventArgs Progress
        {
            get { return _progress; }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    RaisePropertyChanged("Progress");
                }
            }
        }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            renderer.ProgressChanged += (s, e) => { Progress = e; };
            world.ProgressChanged += (s, e) => { Progress = e; };
            _ToolProperties.ToolPreviewRequest += (s, e) =>
                                                      {
                                                          if (_ActiveTool != null)
                                                          {
                                                              ToolProperties.Image = _ActiveTool.PreviewTool();
                                                          }
                                                      };
        }

        #endregion

        private void CompTargetRender(object sender, EventArgs e)
        {
            CalcFrameRate((RenderingEventArgs) e);
        }

        private void CalcFrameRate(RenderingEventArgs renderArgs)
        {
            TimeSpan dt = (renderArgs.RenderingTime - lastRender);
            var framrate = (int) (1000/dt.TotalMilliseconds);

            if (framrate > 0)
            {
                frameTimesIndex = (frameTimesIndex + 1)%frameTimes.Length;
                frameTimes[frameTimesIndex] = framrate;
                FrameRate = (int) frameTimes.Average();
            }
            // About to render...
            lastRender = renderArgs.RenderingTime;
        }

        public bool CanLoad()
        {
            return !IsBusy;
        }

        public bool CanSave()
        {
            return !IsBusy;
        }

        private void LoadWorldandRender()
        {
            var ofd = new OpenFileDialog();
            IsBusy = true;
            if ((bool) ofd.ShowDialog())
            {
                Task.Factory.StartNew(() =>
                                          {
                                              World.Load(ofd.FileName);
                                              WriteableBitmap img = renderer.RenderWorld();
                                              img.Freeze();
                                              _uiFactory.StartNew(() =>
                                                                      {
                                                                          WorldImage.Image = img.Clone();
                                                                          img = null;
                                                                          RaisePropertyChanged("WorldZoomedHeight");
                                                                          RaisePropertyChanged("WorldZoomedWidth");
                                                                      });
                                          });
            }
            IsBusy = false;
        }

        private void SaveWorld()
        {
            Task.Factory.StartNew(() =>
                                      {
                                          IsBusy = true;
                                          World.SaveFile(world.Header.FileName);
                                          _uiFactory.StartNew(() => IsBusy = false);
                                      });
        }

        private void OnMouseOverPixel(TileMouseEventArgs e)
        {
            MouseOverTile = e.Tile;

            if (e.Tile.X < world.Header.MaxTiles.X &&
                e.Tile.Y < world.Header.MaxTiles.X &&
                e.Tile.X >= 0 &&
                e.Tile.Y >= 0)
            {
                Tile overTile = world.Tiles[e.Tile.X, e.Tile.Y];


                string wallName = TileColors.Walls[overTile.Wall].Name;
                string tileName = overTile.IsActive ? TileColors.Tiles[overTile.Type].Name : "[empty]";
                string fluidname = "[no fluid]";
                if (overTile.Liquid > 0)
                {
                    fluidname = overTile.IsLava ? "Lava" : "Water";
                    fluidname += " [" + overTile.Liquid.ToString() + "]";
                }

                FluidName = fluidname;
                TileName = tileName;
                WallName = wallName;

                if (ActiveTool != null)
                    ActiveTool.MoveTool(e);
            }
        }

        private void OnMouseDownPixel(TileMouseEventArgs e)
        {
            MouseDownTile = e.Tile;

            if (ActiveTool != null)
                ActiveTool.PressTool(e);
        }

        private void OnMouseUpPixel(TileMouseEventArgs e)
        {
            MouseUpTile = e.Tile;

            if (ActiveTool != null)
                ActiveTool.ReleaseTool(e);
        }

        private void OnMouseWheel(TileMouseEventArgs e)
        {
            if (e.WheelDelta > 0)
                Zoom = Zoom*1.1;
            if (e.WheelDelta < 0)
                Zoom = Zoom*0.9;
        }
    }
}