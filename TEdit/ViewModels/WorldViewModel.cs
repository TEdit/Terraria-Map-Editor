using System;
using System.Collections.Generic;
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
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.Tools;
using TEdit.Tools.Clipboard;
using TEdit.Tools.History;

namespace TEdit.ViewModels
{
    [Export]
    public class WorldViewModel : ObservableObject, IPartImportsSatisfiedNotification
    {
        #region Fields

        private readonly TaskFactory _uiFactory;
        private readonly TaskScheduler _uiScheduler;
        private ITool _activeTool;

        private ICommand _copyToClipboard;
        private ICommand _validateWorldCommand;
        private string _fluidName;

        private bool _isBusy;
        private bool _isMouseContained;
        private TimeSpan _lastRender;
        private ICommand _mouseDownCommand;
        private PointInt32 _mouseDownTile;
        private PointShort _frame;
        private ICommand _mouseMoveCommand;
        private PointInt32 _mouseOverTile;
        private ICommand _mouseUpCommand;
        private ICommand _renderCommand;
        private ICommand _importSchematic;
        private ICommand _exportSchematic;
        private ICommand _undo;
        private ICommand _redo;
        private ICommand _removeSchematic;
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

        [Import] private SpritePicker _spritePicker;
        [Import]
        private ToolProperties _toolProperties;
        private string _wallName;
        [Import("World", typeof(World))]
        private World _world;
        private WorldImage _worldImage;
        private double _zoom = 1;

        #endregion

        #region CTOR

        public WorldViewModel()
        {
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _uiFactory = new TaskFactory(_uiScheduler);
            Tools = new OrderingCollection<ITool, IOrderMetadata>(t => t.Metadata.Order);
        }

        public void OnImportsSatisfied()
        {
            _renderer.ProgressChanged += (s, e) => { Progress = e; };
            _world.ProgressChanged += (s, e) => { Progress = e; };
            
            _spritePicker.PropertyChanged += (s, e) =>
            {
                if (ActiveTool != null)
                {
                    if (e.PropertyName == "SelectedSprite" && ActiveTool.Name == "Sprite Placer Tool")
                    {
                        ToolProperties.Image = _activeTool.PreviewTool();
                    }
                }
            };

            _toolProperties.ToolPreviewRequest += (s, e) =>
            {
                if (_activeTool != null)
                {
                    ToolProperties.Image = _activeTool.PreviewTool();
                }
            };

            GenNewWorld();
        }



        #endregion

        #region Propertie


        public PointShort Frame
        {
            get { return this._frame; }
            set { SetProperty(ref _frame, ref value, "Frame"); }
        }

        [Import]
        private HistoryManager _histMan;
        public HistoryManager HistMan
        {
            get { return _histMan; }
            set { SetProperty(ref _histMan, ref value, "HistMan"); }
        }

        [Import]
        private ClipboardManager _clipboardMan;
        public ClipboardManager ClipboardMan
        {
            get { return this._clipboardMan; }
            set { SetProperty(ref _clipboardMan, ref value, "ClipboardMan"); }
        }

        public ToolProperties ToolProperties
        {
            get { return _toolProperties; }
            set { SetProperty(ref _toolProperties, ref value, "ToolProperties"); } // null
        }

        public SpritePicker SpritePicker
        {
            get { return _spritePicker; }
            set { SetProperty(ref _spritePicker, ref value, "SpritePicker"); }
        }

        public TilePicker TilePicker
        {
            get { return _tilePicker; }
            set { SetProperty(ref _tilePicker, ref value, "TilePicker"); }
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
            set { SetProperty(ref _selection, ref value, "Selection"); }
        }

        public World World
        {
            get { return _world; }
            set { SetProperty(ref _world, ref value, "World", "WorldZoomedHeight", "WorldZoomedWidth"); }
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
                SetProperty(ref _zoom, ref limitedZoom, "Zoom", "ZoomInverted", "WorldZoomedHeight", "WorldZoomedWidth");
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
            set { SetProperty(ref _worldImage, ref value, "WorldImage"); }
        }

        public bool IsMouseContained
        {
            get { return _isMouseContained; }
            set { SetProperty(ref _isMouseContained, ref value, "IsMouseContained"); }
        }

        public string WallName
        {
            get { return _wallName; }
            set { SetProperty(ref _wallName, ref value, "WallName"); }
        }

        public string TileName
        {
            get { return _tileName; }
            set { SetProperty(ref _tileName, ref value, "TileName"); }
        }

        public string FluidName
        {
            get { return _fluidName; }
            set { SetProperty(ref _fluidName, ref value, "FluidName"); }
        }

        public PointInt32 MouseOverTile
        {
            get { return _mouseOverTile; }
            set { SetProperty(ref _mouseOverTile, ref value, "MouseOverTile", "ToolLocation"); }
        }

        public PointInt32 ToolLocation
        {
            get { return _mouseOverTile - ToolProperties.Offset; }
        }

        public PointInt32 MouseDownTile
        {
            get { return _mouseDownTile; }
            set { SetProperty(ref _mouseDownTile, ref value, "MouseDownTile"); }
        }

        public PointInt32 MouseUpTile
        {
            get { return _mouseUpTile; }
            set { SetProperty(ref _mouseUpTile, ref value, "MouseUpTile"); }
        }

        public ProgressChangedEventArgs Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, ref value, "Progress"); }
        }
        #endregion

        #region Commands

        public ICommand Undo
        {
            get { return _undo ?? (_undo = new RelayCommand(() => HistMan.ProcessUndo())); }
        }

        public ICommand Redo
        {
            get { return _redo ?? (_redo = new RelayCommand(() => HistMan.ProcessRedo())); }
        }

        public ICommand RenderCommand
        {
            get { return _renderCommand ?? (_renderCommand = new RelayCommand(RenderWorld, CanRender)); }
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

        public ICommand RemoveSchematic
        {
            get { return _removeSchematic ?? (_removeSchematic = new RelayCommand<ClipboardBuffer>(RemoveSchematicFile)); }
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

        public ICommand ValidateWorldCommand
        {
            get { return _validateWorldCommand ?? (_validateWorldCommand = new RelayCommand(ValidateWorld, CanSave)); }
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
                var hist = new Queue<HistoryTile>();
                for (int x = Selection.Rectangle.X; x < Selection.Rectangle.X + Selection.Rectangle.Width; x++)
                {
                    for (int y = Selection.Rectangle.Y; y < Selection.Rectangle.Y + Selection.Rectangle.Height; y++)
                    {
                        hist.Enqueue(new HistoryTile(new PointInt32(x, y), (Tile)World.Tiles[x, y].Clone()));
                        World.Tiles[x, y].IsActive = false;
                        World.Tiles[x, y].Type = 0;
                        World.Tiles[x, y].Wall = 0;
                        World.Tiles[x, y].Liquid = 0;
                    }
                }
                _renderer.UpdateWorldImage(Selection.Rectangle);
                HistMan.AddBufferToHistory();
            }

        }

        public void GenNewWorld()
        {
            World.NewWorld(2000, 300);
            RenderWorld();
        }

        #endregion

        #region Clipboard and Schematics

        private bool CanActivatePasteTool()
        {
            return (ClipboardMan.Buffer != null);
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

        private void SetClipBoard()
        {
            if (Selection.SelectionVisibility == Visibility.Visible)
            {
                var buffer = _clipboardMan.GetBufferedRegion(_world, Selection.Rectangle);
                buffer.Preview = _renderer.RenderBuffer(buffer);
                ClipboardMan.LoadedBuffers.Insert(0, buffer);
                ClipboardMan.Buffer = null;
                ClipboardMan.Buffer = buffer;
            }
        }

        private void ImportSchematicFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "TEdit Schematic File|*.TEditSch";
            ofd.Title = "Import Schematic File";
            ofd.InitialDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
            ofd.Multiselect = true;
            if ((bool)ofd.ShowDialog())
            {
                foreach (string file in ofd.FileNames)
                {
                    var buffer = ClipboardBuffer.Load(file);
                    buffer.Preview = _renderer.RenderBuffer(buffer);
                    ClipboardMan.LoadedBuffers.Insert(0, buffer);
                }

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

        private void RemoveSchematicFile(ClipboardBuffer buffer)
        {
            ClipboardMan.LoadedBuffers.Remove(buffer);
        }

        #endregion

        #region World File IO and Rendering

        public bool CanLoad()
        {
            return !_world.IsUsingIo && !_renderer.IsRenderingFullMap;
        }

        public bool CanSave()
        {
            return _world.IsValid && !_world.IsUsingIo && !_renderer.IsRenderingFullMap;
        }

        public bool CanRender()
        {
            return _world.IsValid && !_world.IsUsingIo && !_renderer.IsRenderingFullMap;
        }

        private void ValidateWorld()
        {
            Task.Factory.StartNew(() => _world.Validate())
            .ContinueWith(t => RenderWorld(), _uiScheduler);
        }

        private void SaveWorld()
        {
            //if (!CanSave())
            //    return;

            if (World.IsSaved && !string.IsNullOrWhiteSpace(_world.Header.FileName))
            {
                // Only perform save if file exists, otherwise create a new file using SaveAs dialog
                Task.Factory.StartNew(() => World.SaveFile(_world.Header.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), _uiScheduler);
            }
            else
            {
                SaveWorldAs();
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
                Task.Factory.StartNew(() => World.SaveFile(sfd.FileName))
                .ContinueWith(t => CommandManager.InvalidateRequerySuggested(), _uiScheduler);
            }
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
                Task.Factory.StartNew(() => LoadWorld(ofd.FileName))
                    .ContinueWith(t => RenderWorld());
            }
        }

        private void LoadWorld(string filename)
        {
            try
            {
                WorldImage.Image = null;
                World.Load(filename);
            }
            catch (Exception)
            {
                World.IsUsingIo = true;
                MessageBox.Show("There was a problem loading the file. Make sure you selected a .wld, .bak or .Tedit file.", "World File Problem", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RenderWorld()
        {
            Task<WriteableBitmap>.Factory.StartNew(
                () =>
                {
                    WriteableBitmap img = _renderer.RenderWorld();
                    if (img != null)
                    {
                        img.Freeze();
                    }
                    return img;
                })
            .ContinueWith(
                t =>
                {
                    if (t.Result != null)
                    {
                        WorldImage.Image = t.Result.Clone();
                        RaisePropertyChanged("WorldZoomedHeight");
                        RaisePropertyChanged("WorldZoomedWidth");
                        CommandManager.InvalidateRequerySuggested();
                    }
                }, _uiScheduler);
        }

        #endregion

        #region Mouse Handlers

        private void OnMouseOverPixel(TileMouseEventArgs e)
        {
            MouseOverTile = e.Tile;

            if ((e.Tile.X < _world.Header.MaxTiles.X &&
                 e.Tile.Y < _world.Header.MaxTiles.Y &&
                 e.Tile.X >= 0 &&
                 e.Tile.Y >= 0) && (_world.Tiles[e.Tile.X, e.Tile.Y] != null))
            {
                Tile overTile = _world.Tiles[e.Tile.X, e.Tile.Y];


                string wallName = WorldSettings.Walls[overTile.Wall].Name + "[" + overTile.Wall + "]";
                string tileName = overTile.IsActive ? WorldSettings.Tiles[overTile.Type].Name + "[" + overTile.Type + "]" : "[empty]";
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

        #endregion
    }
}