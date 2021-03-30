using System;
using System.Windows;
using TEdit.Terraria;
using TEdit.Editor.Clipboard;
using TEdit.ViewModel;
using TEdit.UI.Xaml.XnaContentHost;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;

//using Microsoft.Xna.Framework.Graphics;


namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ReplaceAllPlugin.xaml
    /// </summary>
    public partial class HouseGenPluginView : Window
    {
        //private Texture2D _previewTexture;

        private WorldViewModel _wvm;
        public WorldViewModel WorldViewModel
        {
            get { return _wvm; }
            set { _wvm = value; }
        }

        private WriteableBitmap _preview;
        public WriteableBitmap HouseGenPreview
        {
            get { return _preview; }
        }
        ClipboardBuffer _template;
        ClipboardBuffer _buffer;
        Geometry.Primitives.Vector2Int32 _bufferSize;

        public HouseGenPluginView()
        {
            InitializeComponent();
            _wvm = null;
            _bufferSize = new(43, 22);
            LoadTemplateSchematic();
        }

        private void LoadTemplateSchematic()
        {
            if (_template == null)
            {
                try
                {
                    _template = ClipboardBuffer.Load("schematics\\HouseGenTemplate.TEditSch");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Schematic File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (_template == null)
                {
                    Close();
                    return;
                }

                GenHouse();
            }
        }

        private void GenHouse()
        {
            _buffer = new(_bufferSize);
            Random rand = new();

            Geometry.Primitives.Vector2Int32 roof1_Size = new(29, 5);
            int roof1_OffsetX = 0;
            int roof1_OffsetY = 0;

            Geometry.Primitives.Vector2Int32 roof2_Size = new(16, 5);
            int roof2_OffsetX = 27;
            int roof2_OffsetY = 7;

            Geometry.Primitives.Vector2Int32 room1_Size = new(25, 7);
            int room1_OffsetX = 2;
            int room1_OffsetY = 5;

            Geometry.Primitives.Vector2Int32 room2_Size = new(24, 10);
            int room2_OffsetX = 2;
            int room2_OffsetY = 12;

            Geometry.Primitives.Vector2Int32 room3_Size = new(15, 10);
            int room3_OffsetX = 26;
            int room3_OffsetY = 12;

            int roof1_Type = rand.Next(3);
            int roof2_Type = roof1_Type;
            int room1_Type = rand.Next(3);
            int room2_Type = rand.Next(3);
            int room3_Type = rand.Next(3);

            byte roofColor = (byte)rand.Next(31);

            for (int x = 0; x < roof1_Size.X; x++)
            {
                for (int y = 0; y < roof1_Size.Y; y++)
                {
                    _buffer.Tiles[x + roof1_OffsetX, y + roof1_OffsetY] = (Tile)_template.Tiles[x + roof1_OffsetX, y + roof1_OffsetY + (_bufferSize.Y * roof1_Type)].Clone();
                    _buffer.Tiles[x + roof1_OffsetX, y + roof1_OffsetY].TileColor = roofColor;
                }
            }

            for (int x = 0; x < roof2_Size.X; x++)
            {
                for (int y = 0; y < roof2_Size.Y; y++)
                {
                    _buffer.Tiles[x + roof2_OffsetX, y + roof2_OffsetY] = (Tile)_template.Tiles[x + roof2_OffsetX, y + roof2_OffsetY + (_bufferSize.Y * roof2_Type)].Clone();
                    _buffer.Tiles[x + roof2_OffsetX, y + roof2_OffsetY].TileColor = roofColor;
                }
            }

            for (int x = 0; x < room1_Size.X; x++)
            {
                for (int y = 0; y < room1_Size.Y; y++)
                {
                    _buffer.Tiles[x + room1_OffsetX, y + room1_OffsetY] = (Tile)_template.Tiles[x + room1_OffsetX, y + room1_OffsetY + (_bufferSize.Y * room1_Type)].Clone();
                }
            }

            for (int x = 0; x < room2_Size.X; x++)
            {
                for (int y = 0; y < room2_Size.Y; y++)
                {
                    _buffer.Tiles[x + room2_OffsetX, y + room2_OffsetY] = (Tile)_template.Tiles[x + room2_OffsetX, y + room2_OffsetY + (_bufferSize.Y * room2_Type)].Clone();
                }
            }

            for (int x = 0; x < room3_Size.X; x++)
            {
                for (int y = 0; y < room3_Size.Y; y++)
                {
                    _buffer.Tiles[x + room3_OffsetX, y + room3_OffsetY] = (Tile)_template.Tiles[x + room3_OffsetX, y + room3_OffsetY + (_bufferSize.Y * room3_Type)].Clone();
                }
            }

            //Fill in any empty space.
            for (int x2 = 0; x2 < _bufferSize.X; x2++)
            {
                for (int y2 = 0; y2 < _bufferSize.Y; y2++)
                {
                    if (_buffer.Tiles[x2, y2] == null) _buffer.Tiles[x2, y2] = new Tile();
                }
            }

            _buffer.RenderBuffer();
            UpdatePreview();
            PreviewImage.Source = _preview;
        }

        private void UpdatePreview()
        {
            _preview = _buffer.Preview;
            Copy.IsEnabled = true;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Copy.IsEnabled = false;
            _buffer = new(_bufferSize);
            Hide();
        }

        private void GenButtonClick(object sender, RoutedEventArgs e)
        {
            LoadTemplateSchematic();
            GenHouse();
            UpdatePreview();
        }

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            _wvm.Clipboard.LoadedBuffers.Add(_buffer);
            Copy.IsEnabled = false;
        }

        private void HouseGenPreview_LoadContent(object sender, GraphicsDeviceEventArgs e)
        {
            /*
            // Abort rendering if in design mode or if gameTimer is already running
            if (ViewModelBase.IsInDesignModeStatic || _gameTimer.IsRunning)
            {
                return;
            }
            InitializeGraphicsComponents(e);


            if (_textureDictionary.Valid)
                LoadTerrariaTextures(e);

            _selectionTexture = new Texture2D(e.GraphicsDevice, 1, 1);
            LoadResourceTextures(e);

            _selectionTexture.SetData(new[] { Color.FromNonPremultiplied(0, 128, 255, 128) }, 0, 1);
            // Start the Game Timer
            _gameTimer.Start();
            */
        }

        private void HouseGenPreview_RenderXna(object sender, GraphicsDeviceEventArgs e)
        {
            /*
            // Abort rendering if in design mode or if gameTimer is not running
            if (!_gameTimer.IsRunning || _wvm.CurrentWorld == null || ViewModelBase.IsInDesignModeStatic)
                return;

            // Clear the graphics device and texture buffer
            e.GraphicsDevice.Clear(_backgroundColor);



            Update(e);
            Render(e);
            */
        }
    }
}
