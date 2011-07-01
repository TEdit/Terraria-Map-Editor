using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TEdit.Common;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;
using TEdit.Tools;
using TEdit.Tools.Clipboard;

namespace TEdit.ViewModels
{
    [Export]
    public class WorldViewModel : ObservableObject, IPartImportsSatisfiedNotification
    {

        private readonly TaskFactory _uiFactory;
        private readonly TaskScheduler _uiScheduler;
        private ITool _activeTool;

        private ICommand _copyToClipboard;
        private string _fluidName;

        private bool _isBusy;
        private bool _isMouseContained;
        private TimeSpan _lastRender;
        private ICommand _mouseDownCommand;
        private PointInt32 _mouseDownTile;
        private ICommand _mouseMoveCommand;
        private PointInt32 _mouseOverTile;
        private ICommand _mouseUpCommand;
        private ICommand _renderCommand;
        private ICommand _importSchematic;
        private ICommand _exportSchematic;
        private PointInt32 _mouseUpTile;
        private ICommand _loadBuffer;
        private ICommand _mouseWheelCommand;
        private ICommand _openWorldCommand;
        private ICommand _pasteFromClipboard;
        private ICommand _emptyClipboard;
        private ProgressChangedEventArgs _progress;
        [Import]
        private WorldRenderer _renderer;
        private ICommand _saveWorldCommand;
        private ICommand _saveWorldAsCommand;
        [Import]
        private SelectionArea _selection;
        private ICommand _setTool;
        private string _tileName;
        [Import]
        private TilePicker _tilePicker;
        [Import]
        private ToolProperties _toolProperties;
        private string _wallName;
        [Import("World", typeof(World))]
        private World _world;
        private WorldImage _worldImage;
        private double _zoom = 1;

        public WorldViewModel()
        {
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiFactory = new TaskFactory(_uiScheduler);
            Tools = new OrderingCollection<ITool, IOrderMetadata>(t => t.Metadata.Order);
        }

        [Import]
        private ClipboardManager _clipboardMan;
        public ClipboardManager ClipboardMan
        {
            get { return this._clipboardMan; }
            set
            {
                if (this._clipboardMan != value)
                {
                    this._clipboardMan = value;
                    this.RaisePropertyChanged("ClipboardMan");
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


        [ImportMany(typeof(ITool))]
        public OrderingCollection<ITool, IOrderMetadata> Tools { get; set; }

        public ITool ActiveTool
        {
            get { return _activeTool; }
            set
            {
                // Block paste tool if no buffer
                if (value != null)
                {
                    if (value.Name == "Paste" && !CanActivatePasteTool())
                        return;
                }

                if (_activeTool != value)
                {
                    if (_activeTool != null)
                        _activeTool.IsActive = false;

                    _activeTool = value;
                    ToolProperties.Image = null;

                    if (_activeTool != null)
                    {
                        _activeTool.IsActive = true;
                        ToolProperties.Image = _activeTool.PreviewTool();
                    }
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
                    return _worldImage.Image.PixelHeight * _zoom;


                return 1;
            }
        }

        public double WorldZoomedWidth
        {
            get
            {
                if (_worldImage.Image != null)
                    return _worldImage.Image.PixelWidth * _zoom;

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
            get { return 1 / (_zoom); }
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


        public ICommand RenderCommand
        {
            get { return _renderCommand ?? (_renderCommand = new RelayCommand(RenderWorld)); }
        }

        public ICommand EmptyClipboard
        {
            get { return _emptyClipboard ?? (_emptyClipboard = new RelayCommand(ClipboardMan.ClearBuffers)); }
        }

        public ICommand ImportSchematic
        {
            get { return _importSchematic ?? (_importSchematic = new RelayCommand(ImportSchematicFile)); }
        }

        public ICommand ExportSchematic
        {
            get { return _exportSchematic ?? (_exportSchematic = new RelayCommand<ClipboardBuffer>(ExportSchematicFile)); }
        }

        public ICommand CopyToClipboard
        {
            get { return _copyToClipboard ?? (_copyToClipboard = new RelayCommand(SetClipBoard, CanSetClipboard)); }
        }

        public ICommand PasteFromClipboard
        {
            get { return _pasteFromClipboard ?? (_pasteFromClipboard = new RelayCommand(ActivatePasteTool, CanActivatePasteTool)); }
        }

        public ICommand SetTool
        {
            get { return _setTool ?? (_setTool = new RelayCommand<ITool>(t => ActiveTool = t)); }
        }

        public ICommand LoadBuffer
        {
            get
            {
                return _loadBuffer ?? (_loadBuffer = new RelayCommand<ClipboardBuffer>(b =>
                {
                    ClipboardMan.Buffer = b;
                    ActivatePasteTool();
                }));
            }
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
        public ICommand SaveWorldAsCommand
        {
            get { return _saveWorldAsCommand ?? (_saveWorldAsCommand = new RelayCommand(SaveWorldAs, CanSave)); }
        }

        public void DeleteSelection()
        {
            if (Selection.SelectionVisibility == Visibility.Visible)
            {
                for (int x = Selection.Rectangle.X; x < Selection.Rectangle.X + Selection.Rectangle.Width; x++)
                {
                    for (int y = Selection.Rectangle.Y; y < Selection.Rectangle.Y + Selection.Rectangle.Height; y++)
                    {
                        World.Tiles[x, y].IsActive = false;
                        World.Tiles[x, y].Wall = 0;
                        World.Tiles[x, y].Liquid = 0;
                    }
                }
                _renderer.UpdateWorldImage(Selection.Rectangle);
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


        private void ImportSchematicFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "TEdit Schematic File|*.TEditSch";
            ofd.Title = "Import Schematic File";
            ofd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                var buffer = ClipboardBuffer.Load(ofd.FileName);
                buffer.Preview = _renderer.RenderBuffer(buffer);
                ClipboardMan.LoadedBuffers.Insert(0, buffer);
            }
        }
        private void ExportSchematicFile(ClipboardBuffer buffer)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "TEdit Schematic File|*.TEditSch";
            sfd.Title = "Export Schematic File";
            sfd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            if ((bool)sfd.ShowDialog())
            {
                buffer.Save(sfd.FileName);
            }
        }

        private void SetClipBoard()
        {
            if (Selection.SelectionVisibility == Visibility.Visible)
            {
                var buffer = ClipboardBuffer.GetBufferedRegion(_world, Selection.Rectangle);
                buffer.Preview = _renderer.RenderBuffer(buffer);
                ClipboardMan.LoadedBuffers.Insert(0, buffer);
                ClipboardMan.Buffer = null;
                ClipboardMan.Buffer = buffer;
            }
        }

        private bool CanSetClipboard()
        {
            return (Selection.SelectionVisibility == Visibility.Visible);
        }

        private void ActivatePasteTool()
        {
            ITool pasteTool = Tools.FirstOrDefault(x => x.Value.Name == "Paste").Value;
            if (pasteTool != null)
                ActiveTool = pasteTool;
        }

        private bool CanActivatePasteTool()
        {
            return (ClipboardMan.Buffer != null);
        }

        public bool CanLoad()
        {
            return _world.CanUseFileIO;
        }

        public bool CanSave()
        {
            return !string.Equals(_world.Header.WorldName, "No World Loaded", StringComparison.InvariantCultureIgnoreCase) && _world.CanUseFileIO;
        }

        private void LoadWorldandRender()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Terrarial World File|*.wld|Terraria World Backup|*.bak|TEdit Backup File|*.TEdit";
            ofd.DefaultExt = "Terrarial World File|*.wld";
            ofd.Title = "Import Schematic File";
            ofd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = false;
            if ((bool)ofd.ShowDialog())
            {
                Task.Factory.StartNew(() => LoadWorld(ofd.FileName));
            }
        }
        private void SaveWorldAs()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Terraria World File|*.wld|TEdit Backup File|*.TEdit";
            sfd.Title = "Save World As";
            sfd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            if ((bool)sfd.ShowDialog())
            {
                Task.Factory.StartNew(() => World.SaveFile(sfd.FileName));
            }
        }

        private void LoadWorld(string filename)
        {
            try
            {
                WorldImage.Image = null;
                World.Load(filename);
                WriteableBitmap img = _renderer.RenderWorld();
                img.Freeze();
                _uiFactory.StartNew(() =>
                                        {
                                            WorldImage.Image = img.Clone();
                                            img = null;
                                            RaisePropertyChanged("WorldZoomedHeight");
                                            RaisePropertyChanged("WorldZoomedWidth");
                                        });
            }
            catch (Exception)
            {
                World.CanUseFileIO = true;
                MessageBox.Show("There was a problem loading the file. Make sure you selected a .wld, .bak or .Tedit file.", "World File Problem", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RenderWorld()
        {
            Task.Factory.StartNew(() =>
            {
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

        private void SaveWorld()
        {
            Task.Factory.StartNew(() => World.SaveFile(_world.Header.FileName));
        }

        private void OnMouseOverPixel(TileMouseEventArgs e)
        {
            MouseOverTile = e.Tile;

            if ((e.Tile.X < _world.Header.MaxTiles.X &&
                 e.Tile.Y < _world.Header.MaxTiles.Y &&
                 e.Tile.X >= 0 &&
                 e.Tile.Y >= 0) && (_world.Tiles[e.Tile.X, e.Tile.Y] != null))
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
                Frame = overTile.Frame;

                if (ActiveTool != null)
                    ActiveTool.MoveTool(e);
            }
        }

        private PointShort _Frame;
        public PointShort Frame
        {
            get { return this._Frame; }
            set
            {
                if (this._Frame != value)
                {
                    this._Frame = value;
                    this.RaisePropertyChanged("Frame");
                }
            }
        }



        private void OnMouseDownPixel(TileMouseEventArgs e)
        {
            if ((e.Tile.X < _world.Header.MaxTiles.X &&
                 e.Tile.Y < _world.Header.MaxTiles.Y &&
                 e.Tile.X >= 0 &&
                 e.Tile.Y >= 0) && (_world.Tiles[e.Tile.X, e.Tile.Y] != null))
            {
                MouseDownTile = e.Tile;

                if (ActiveTool != null)
                {
                    ActiveTool.PressTool(e);

                    if (ActiveTool.Name == "Paste")
                        ActiveTool = null;// Tools.FirstOrDefault(t => t.Value.Name == "Selection").Value;

                }
            }
        }

        private void OnMouseUpPixel(TileMouseEventArgs e)
        {
            if ((e.Tile.X < _world.Header.MaxTiles.X &&
                 e.Tile.Y < _world.Header.MaxTiles.Y &&
                 e.Tile.X >= 0 &&
                 e.Tile.Y >= 0) && (_world.Tiles[e.Tile.X, e.Tile.Y] != null))
            {
                MouseUpTile = e.Tile;

                if (ActiveTool != null)
                    ActiveTool.ReleaseTool(e);
            }
        }

        private void OnMouseWheel(TileMouseEventArgs e)
        {
            if ((e.Tile.X < _world.Header.MaxTiles.X &&
                 e.Tile.Y < _world.Header.MaxTiles.Y &&
                 e.Tile.X >= 0 &&
                 e.Tile.Y >= 0) && (_world.Tiles[e.Tile.X, e.Tile.Y] != null))
            {
                if (e.WheelDelta > 0)
                    Zoom = Zoom * 1.1;
                if (e.WheelDelta < 0)
                    Zoom = Zoom * 0.9;
            }
        }
    }
}