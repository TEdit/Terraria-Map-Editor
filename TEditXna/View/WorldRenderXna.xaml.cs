using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BCCL.UI.Xaml.XnaContentHost;
using BCCL.UI.Xaml.XnaContentHost.Primitives2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TEditXNA.Terraria;
using TEditXna.ViewModel;
using Point = System.Windows.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace TEditXna.View
{
    /// <summary>
    /// Interaction logic for WorldRenderXna.xaml
    /// </summary>
    public partial class WorldRenderXna : UserControl
    {
        private readonly WorldViewModel _localWvmReference;

        // cool XNA spritebatch in WPF
        private SpriteBatch _spriteBatch;
        // game timer for rendering
        private GameTimer _gameTimer;
        private Textures _textureDictionary;
        private SimpleProvider _serviceProvider;

        private Texture2D[] tileMap;

        public WorldRenderXna()
        {
            // to stop visual studio design time crash :(
            if (!BCCL.Utility.Debugging.IsInDesignMode)
            {
                InitializeComponent();
                _gameTimer = new GameTimer();
            }

            _localWvmReference = ViewModelLocator.WorldViewModel;

        }

        private void InitializeGraphicsComponents(GraphicsDeviceEventArgs e)
        {
            // Load services, textures and initialize spritebatch
            _serviceProvider = new SimpleProvider(xnaViewport.GraphicsService);
            _spriteBatch = new SpriteBatch(e.GraphicsDevice);
            _textureDictionary = new Textures(_serviceProvider);
        }

        private UInt32[] colors;
        private int tileWidth = 256;
        private int tileHeight = 256;
        private Rectangle worldBounds = new Rectangle(0, 0, 2048, 2048);

        private void xnaViewport_LoadContent(object sender, GraphicsDeviceEventArgs e)
        {
            // Abort rendering if in design mode or if gameTimer is already running
            if (BCCL.Utility.Debugging.IsInDesignMode || _gameTimer.IsRunning)
                return;

            InitializeGraphicsComponents(e);
            GenerateDummyContent(e);
            LoadTerrariaTextures();

            // Start the Game Timer
            _gameTimer.Start();
        }

        private AnimatedTexture _npc17;
        private SpriteFont _font;

        private void GenerateDummyContent(GraphicsDeviceEventArgs e)
        {
            // Generate some dummy content to render
            tileMap = new Texture2D[10];
            colors = new UInt32[2048 * 2048];
            int a = 255;
            int r = 0;
            int g = 0;
            int b = 0;

            for (int i = 0; i < 2048 * 2048; i++)
            {
                colors[i] = (UInt32)(b++ % 256 | (g++ % 256 << 8) | (r++ % 256 << 16) | (a << 24));
                b = b + 2;
                g = g + 3;
            }
            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new Texture2D(e.GraphicsDevice, tileWidth, tileHeight);
            }
        }



        private void LoadTerrariaTextures()
        {
            // If the texture dictionary is valid (Found terraria and loaded content) load texture data
            if (_textureDictionary.Valid)
            {
                _textureDictionary.GetNPC(17);
                //_font = _textureDictionary.ContentManager.Load<SpriteFont>("Fonts\\Mouse_Text");
                _npc17 = new AnimatedTexture(_textureDictionary.Npcs[17], new Vector2(0, 0), 0F, 0F, 0F, 16, 20F, 2, int.MaxValue);
                //_npc17.SetAnimation(5F, 0, 1);
            }
        }



        private void xnaViewport_RenderXna(object sender, GraphicsDeviceEventArgs e)
        {
            // Abort rendering if in design mode or if gameTimer is not running
            if (BCCL.Utility.Debugging.IsInDesignMode || !_gameTimer.IsRunning)
                return;

            Update(e);
            Render(e);
        }

        private void Update(GraphicsDeviceEventArgs e)
        {
            // Update
            _gameTimer.Update();

            if (_npc17 != null)
                _npc17.UpdateFrame((float)_gameTimer.ElapsedGameTime.TotalSeconds);

            
            ScrollWorld();
        }

        private void GetWorldMap()
        {
            if (_localWvmReference != null)
            {
                if (_localWvmReference.PixelMap != null)
                {
                    tileMap[0].SetData<Color>(_localWvmReference.PixelMap, (_localWvmReference.CurrentWorld.TilesHigh / 2) * _localWvmReference.CurrentWorld.TilesWide, tileHeight * tileWidth);
                }
            }
        }

        #region Render

        private void Render(GraphicsDeviceEventArgs e)
        {
            // Clear the graphics device and texture buffer
            e.GraphicsDevice.Clear(Color.Gray);
            e.GraphicsDevice.Textures[0] = null;

            // Start SpriteBatch
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            
            // Draw Pixel Map tiles
            DrawPixelTiles(_spriteBatch);
            
            // Draw sprite overlays
            DrawSprites(_spriteBatch);

            // End SpriteBatch
            _spriteBatch.End();
        }

        private void DrawSprites(SpriteBatch spriteBatch)
        {
            if (!_textureDictionary.Valid)
                return;

            if (_npc17 != null)
                _npc17.DrawFrame(spriteBatch, new Vector2(100, 10));
        }

        private void DrawPixelTiles(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < tileMap.Length; i++)
                tileMap[i].SetData<UInt32>(colors, i * tileWidth * tileHeight, tileWidth * tileHeight);
            GetWorldMap();
            for (int i = 0; i < tileMap.Length; i++)
            {
                spriteBatch.Draw(
                    tileMap[i],
                    new Vector2(
                        (scrollPosition.X + (i % 3) * tileWidth) * zoom,
                        (scrollPosition.Y + (i / 3) * tileHeight) * zoom),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    zoom,
                    SpriteEffects.None,
                    0);
            }
        }

        #endregion

        private static Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        private Vector2 scrollPosition = new Vector2(0, 0);
        private bool isMiddleMouseDown;
        private Vector2 middleClickPoint;
        private Vector2 mousePosition;
        private float zoom = 1;

        // speed in tile/second 

        private void ScrollWorld()
        {
            if (isMiddleMouseDown)
            {
                var stretchDistance = (mousePosition - middleClickPoint);
                var clampedScroll = scrollPosition + stretchDistance / zoom;
                clampedScroll.X = MathHelper.Clamp(clampedScroll.X, -worldBounds.Width, 0);
                clampedScroll.Y = MathHelper.Clamp(clampedScroll.Y, -worldBounds.Height, 0);
                scrollPosition = clampedScroll;
                middleClickPoint = mousePosition;
            }
        }

        private void xnaViewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        #region Mouse

        private void xnaViewport_HwndMouseMove(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {
            mousePosition = PointToVector2(e.Position);
        }

        private void xnaViewport_HwndLButtonDown(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {

        }

        private void xnaViewport_HwndLButtonUp(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {

        }

        private void xnaViewport_HwndRButtonDown(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {

        }

        private void xnaViewport_HwndRButtonUp(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {

        }

        private void xnaViewport_HwndMouseWheel(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {
            int x = e.WheelDelta;
            float tempZoom = zoom;
            if (x > 0)
                tempZoom = zoom * 2F;
            if (x < 0)
                tempZoom = zoom / 2F;

            zoom = MathHelper.Clamp(tempZoom, 0.03125F, 64F);
        }

        private void xnaViewport_HwndMButtonDown(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {
            middleClickPoint = PointToVector2(e.Position);
            isMiddleMouseDown = true;
        }

        private void xnaViewport_HwndMButtonUp(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {
            isMiddleMouseDown = false;
        }

        private void xnaViewport_HwndMouseEnter(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {

        }

        private void xnaViewport_HwndMouseLeave(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {

        }

        #endregion
    }
}
