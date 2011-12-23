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
        private readonly WorldViewModel _wmv;

        // cool XNA spritebatch in WPF
        private SpriteBatch _spriteBatch;
        // game timer for rendering
        private GameTimer _gameTimer;
        private Textures _textureDictionary;
        private SimpleProvider _serviceProvider;

        private Vector2 _scrollPosition = new Vector2(0, 0);
        private float _zoom = 1;

        private Texture2D[] tileMap;

        public WorldRenderXna()
        {
            // to stop visual studio design time crash :(
            if (!BCCL.Utility.Debugging.IsInDesignMode)
            {
                InitializeComponent();
                _gameTimer = new GameTimer();
            }

            _wmv = ViewModelLocator.WorldViewModel;
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

        private void xnaViewport_LoadContent(object sender, GraphicsDeviceEventArgs e)
        {
            // Abort rendering if in design mode or if gameTimer is already running
            if (BCCL.Utility.Debugging.IsInDesignMode || _gameTimer.IsRunning)
                return;

            InitializeGraphicsComponents(e);
            LoadTerrariaTextures();

            // Start the Game Timer
            _gameTimer.Start();
        }

        private AnimatedTexture _npc17;
        private SpriteFont _font;


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
            if (BCCL.Utility.Debugging.IsInDesignMode || !_gameTimer.IsRunning || _wmv.CurrentWorld == null)
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

        public void CenterOnTile(int x, int y)
        {
            _scrollPosition = new Vector2(
                -x + (float)(xnaViewport.ActualWidth / _zoom / 2),
                -y + (float)(xnaViewport.ActualHeight / _zoom / 2));
        }

        #region Render

        private bool Check2DFrustrum(int tileIndex)
        {
            int x = tileIndex % _wmv.PixelMap.TilesX;
            // X off min side
            int xmin = (int)(-_scrollPosition.X / _wmv.PixelMap.TileWidth);
            if (x < xmin)
                return false;

            // x off max side
            if (x > 1 + xmin + (int)((xnaViewport.GraphicsService.GraphicsDevice.Viewport.Width / _zoom) / _wmv.PixelMap.TileWidth))
                return false;


            int y = tileIndex / _wmv.PixelMap.TilesX;

            int ymin = (int)(-_scrollPosition.Y / _wmv.PixelMap.TileHeight);
            if (y < ymin)
                return false;

            if (y > 1 + ymin + (int)((xnaViewport.GraphicsService.GraphicsDevice.Viewport.Height / _zoom) / _wmv.PixelMap.TileHeight))
                return false;

            return true;
        }

        private void Render(GraphicsDeviceEventArgs e)
        {
            // Clear the graphics device and texture buffer
            e.GraphicsDevice.Clear(Color.Black);
            e.GraphicsDevice.Textures[0] = null;

            GenPixelTiles(e);

            // Start SpriteBatch
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);


            // Draw Pixel Map tiles
            DrawPixelTiles(_spriteBatch);

            // Draw sprite overlays
            DrawSprites(_spriteBatch);

            // End SpriteBatch
            _spriteBatch.End();
        }

        private void GenPixelTiles(GraphicsDeviceEventArgs e)
        {
            if (_wmv != null)
            {
                if (_wmv.PixelMap != null)
                {
                    if (tileMap == null || tileMap.Length != _wmv.PixelMap.ColorBuffers.Length)
                    {
                        tileMap = new Texture2D[_wmv.PixelMap.ColorBuffers.Length];
                    }

                    for (int i = 0; i < tileMap.Length; i++)
                    {
                        if (!Check2DFrustrum(i))
                            continue;

                        // Make a new texture for nulls
                        if (tileMap[i] == null)
                            tileMap[i] = new Texture2D(e.GraphicsDevice, _wmv.PixelMap.TileWidth, _wmv.PixelMap.TileHeight);


                        tileMap[i].SetData(_wmv.PixelMap.ColorBuffers[i]);
                    }
                }
            }
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
            //for (int i = 0; i < tileMap.Length; i++)
            //    tileMap[i].SetData<UInt32>(colors, i * tileWidth * tileHeight, tileWidth * tileHeight);
            if (tileMap == null)
                return;

            for (int i = 0; i < tileMap.Length; i++)
            {
                if (!Check2DFrustrum(i))
                    continue;

                spriteBatch.Draw(
                    tileMap[i],
                    new Vector2(
                        (_scrollPosition.X + (i % _wmv.PixelMap.TilesX) * _wmv.PixelMap.TileWidth) * _zoom,
                        (_scrollPosition.Y + (i / _wmv.PixelMap.TilesX) * _wmv.PixelMap.TileHeight) * _zoom),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    _zoom,
                    SpriteEffects.None,
                    0);
            }
        }

        #endregion

        private static Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }


        private bool isMiddleMouseDown;
        private Vector2 middleClickPoint;
        private Vector2 mousePosition;
        // speed in tile/second 

        private void ScrollWorld()
        {
            if (isMiddleMouseDown)
            {
                var stretchDistance = (mousePosition - middleClickPoint);
                var clampedScroll = _scrollPosition + stretchDistance / _zoom;
                _scrollPosition = clampedScroll;
                middleClickPoint = mousePosition;
            }

            int xNormalRange = -_wmv.CurrentWorld.TilesWide + (int)(xnaViewport.ActualWidth / _zoom);
            int yNormalRange = -_wmv.CurrentWorld.TilesHigh + (int)(xnaViewport.ActualHeight / _zoom);
            
            if (_wmv.CurrentWorld.TilesWide > (int)(xnaViewport.ActualWidth / _zoom))
                _scrollPosition.X = MathHelper.Clamp(_scrollPosition.X, xNormalRange, 0);
            else
                _scrollPosition.X = MathHelper.Clamp(_scrollPosition.X, (_wmv.CurrentWorld.TilesWide / 2 - (int)(xnaViewport.ActualWidth / _zoom) / 2), 0);

            if (_wmv.CurrentWorld.TilesHigh > (int)(xnaViewport.ActualHeight / _zoom))
                _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, yNormalRange, 0);
            else
                _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, (_wmv.CurrentWorld.TilesHigh / 2 - (int)(xnaViewport.ActualHeight / _zoom) / 2), 0);
            
        }

        private void xnaViewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        #region Mouse

        private void xnaViewport_HwndMouseMove(object sender, BCCL.UI.Xaml.XnaContentHost.HwndMouseEventArgs e)
        {
            mousePosition = PointToVector2(e.Position);
            if (_wmv.CurrentWorld != null)
                _wmv.MouseOverTile = new BCCL.Geometry.Primitives.Vector2Int32(
                    (int)MathHelper.Clamp((float)(e.Position.X / _zoom - _scrollPosition.X), 0, _wmv.CurrentWorld.TilesWide),
                    (int)MathHelper.Clamp((float)(e.Position.Y / _zoom - _scrollPosition.Y), 0, _wmv.CurrentWorld.TilesHigh));
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
            float tempZoom = _zoom;
            if (x > 0)
                tempZoom = _zoom * 2F;
            if (x < 0)
                tempZoom = _zoom / 2F;
            var curTile = _wmv.MouseOverTile;
            _zoom = MathHelper.Clamp(tempZoom, 0.125F, 64F);
            CenterOnTile(curTile.X, curTile.Y);
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
