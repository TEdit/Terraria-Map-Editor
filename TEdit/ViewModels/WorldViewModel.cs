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
        private readonly TaskFactory _uiFactory;
        private readonly TaskScheduler _uiScheduler;

        private ICommand _mouseDownCommand;
        private ICommand _mouseMoveCommand;
        private ICommand _mouseUpCommand;
        private ICommand _mouseWheelCommand;
        private ICommand _openWorldCommand;
        private ICommand _saveWorldCommand;
        private ICommand _setTool;
        private ITool _activeTool;
        private PointInt32 _mouseDownTile;
        private PointInt32 _mouseOverTile;
        private PointInt32 _mouseUpTile;
        private ProgressChangedEventArgs _progress;
        private TimeSpan _lastRender;
        private WorldImage _worldImage;
        private bool _isBusy;
        private bool _isMouseContained;
        private double _zoom = 1;
        private int _frameRate;
        private int _frameTimesIndex;
        private readonly int[] _frameTimes = new int[100];
        private string _fluidName;
        private string _tileName;
        private string _wallName;

        [Import] private SelectionArea _selection;
        [Import] private TilePicker _tilePicker;
        [Import] private ToolProperties _toolProperties;
        [Import] private WorldRenderer _renderer;
        [Import("World", typeof (World))] private World _world;

        public WorldViewModel()
        {
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiFactory = new TaskFactory(_uiScheduler);
            Tools = new OrderingCollection<ITool, IOrderMetadata>(t => t.Metadata.Order);
            CompositionTarget.Rendering += CompTargetRender;
        }

        public int FrameRate
        {
            get { return _frameRate; }
            set
            {
                if (_frameRate != value)
                {
                    _frameRate = value;
                    RaisePropertyChanged("FrameRate");
                }
            }
        }

        public ToolProperties ToolProperties
        {
            get { return _toolProperties; }
            set
            {
                if (_toolProperties != value)
                {
                    _toolProperties = null;
                    _toolProperties = value;
                    RaisePropertyChanged("ToolProperties");
                }
            }
        }


        public TilePicker TilePicker
        {
            get { return _tilePicker; }
            set
            {
                if (_tilePicker != value)
                {
                    _tilePicker = value;
                    RaisePropertyChanged("TilePicker");
                }
            }
        }


        [ImportMany(typeof (ITool))]
        public OrderingCollection<ITool, IOrderMetadata> Tools { get; set; }

        public ITool ActiveTool
        {
            get { return _activeTool; }
            set
            {
                if (_activeTool != value)
                {
                    if (_activeTool != null)
                        _activeTool.IsActive = false;

                    _activeTool = value;
                    _activeTool.IsActive = true;
                    //foreach (var tool in Tools)
                    //{
                    //    tool.Value.IsActive = (tool.Value == _ActiveTool);
                    //}

                    ToolProperties.Image = null;
                    ToolProperties.Image = _activeTool.PreviewTool();
                    RaisePropertyChanged("ActiveTool");
                }
            }
        }


        public SelectionArea Selection
        {
            get { return _selection; }
            set
            {
                if (_selection != value)
                {
                    _selection = value;
                    RaisePropertyChanged("Selection");
                }
            }
        }


        public World World
        {
            get { return _world; }
            set
            {
                if (_world != value)
                {
                    _world = null;
                    _world = value;
                    RaisePropertyChanged("World");
                    RaisePropertyChanged("WorldZoomedHeight");
                    RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        public double WorldZoomedHeight
        {
            get
            {
                if (_worldImage.Image != null)
                    return _worldImage.Image.PixelHeight*_zoom;


                return 1;
            }
        }

        public double WorldZoomedWidth
        {
            get
            {
                if (_worldImage.Image != null)
                    return _worldImage.Image.PixelWidth*_zoom;

                return 1;
            }
        }

        public double Zoom
        {
            get { return _zoom; }
            set
            {
                double limitedZoom = value;
                limitedZoom = Math.Min(Math.Max(limitedZoom, 0.05), 1000);

                if (_zoom != limitedZoom)
                {
                    _zoom = limitedZoom;
                    RaisePropertyChanged("Zoom");
                    RaisePropertyChanged("ZoomInverted");
                    RaisePropertyChanged("WorldZoomedHeight");
                    RaisePropertyChanged("WorldZoomedWidth");
                }
            }
        }

        public double ZoomInverted
        {
            get { return 1/(_zoom); }
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
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged("IsBusy");
                }
            }
        }


        public string WallName
        {
            get { return _wallName; }
            set
            {
                if (_wallName != value)
                {
                    _wallName = value;
                    RaisePropertyChanged("WallName");
                }
            }
        }

        public string TileName
        {
            get { return _tileName; }
            set
            {
                if (_tileName != value)
                {
                    _tileName = value;
                    RaisePropertyChanged("TileName");
                }
            }
        }

        public string FluidName
        {
            get { return _fluidName; }
            set
            {
                if (_fluidName != value)
                {
                    _fluidName = value;
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
            _renderer.ProgressChanged += (s, e) => { Progress = e; };
            _world.ProgressChanged += (s, e) => { Progress = e; };
            _toolProperties.ToolPreviewRequest += (s, e) =>
                                                      {
                                                          if (_activeTool != null)
                                                          {
                                                              ToolProperties.Image = _activeTool.PreviewTool();
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
            TimeSpan dt = (renderArgs.RenderingTime - _lastRender);
            var framrate = (int) (1000/dt.TotalMilliseconds);

            if (framrate > 0)
            {
                _frameTimesIndex = (_frameTimesIndex + 1)%_frameTimes.Length;
                _frameTimes[_frameTimesIndex] = framrate;
                FrameRate = (int) _frameTimes.Average();
            }
            // About to render...
            _lastRender = renderArgs.RenderingTime;
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
                                              WriteableBitmap img = _renderer.RenderWorld();
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
                                          World.SaveFile(_world.Header.FileName);
                                          _uiFactory.StartNew(() => IsBusy = false);
                                      });
        }

        private void OnMouseOverPixel(TileMouseEventArgs e)
        {
            MouseOverTile = e.Tile;

            if (e.Tile.X < _world.Header.MaxTiles.X &&
                e.Tile.Y < _world.Header.MaxTiles.X &&
                e.Tile.X >= 0 &&
                e.Tile.Y >= 0)
            {
                Tile overTile = _world.Tiles[e.Tile.X, e.Tile.Y];


                string wallName = Settings.Walls[overTile.Wall].Name;
                string tileName = overTile.IsActive ? Settings.Tiles[overTile.Type].Name : "[empty]";
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