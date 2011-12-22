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
using Point = System.Windows.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using XNA = Microsoft.Xna.Framework;

namespace TEditXna.View
{
    /// <summary>
    /// Interaction logic for WorldRenderXna.xaml
    /// </summary>
    public partial class WorldRenderXna : UserControl
    {
        // cool XNA spritebatch in WPF
        private XNA.Graphics.SpriteBatch _spriteBatch;
        // game timer for rendering
        private GameTimer _gameTimer;
        private TEditXNA.Terraria.Textures _textureDictionary;
        private SimpleProvider _serviceProvider;

        private XNA.Graphics.Texture2D[] tileMap;

        public WorldRenderXna()
        {
            // to stop visual studio design time crash :(
            if (!BCCL.Utility.Debugging.IsInDesignMode)
            {
                InitializeComponent();
                Initialize();
            }
        }

        void Initialize()
        {
            _gameTimer = new GameTimer();
        }

        private UInt32[] colors;
        private bool contentLoaded = false;
        private int tileWidth = 256;
        private int tileHeight = 256;
        private Rectangle tileRectangle = new Rectangle(0, 0, 256, 256);
        private Rectangle worldBounds = new Rectangle(0, 0, 2048, 2048);

        private void xnaViewport_LoadContent(object sender, BCCL.UI.Xaml.XnaContentHost.GraphicsDeviceEventArgs e)
        {
            if (BCCL.Utility.Debugging.IsInDesignMode)
                return;

            if (!_gameTimer.IsRunning)
            {
                tileMap = new XNA.Graphics.Texture2D[10];
                _serviceProvider = new SimpleProvider(xnaViewport.GraphicsService);
                _spriteBatch = new XNA.Graphics.SpriteBatch(e.GraphicsDevice);
                _textureDictionary = new Textures(_serviceProvider);
                _textureDictionary.GetNPC(17);
                _font = _textureDictionary.ContentManager.Load<SpriteFont>("Fonts\\Mouse_Text");
                colors = new UInt32[2048 * 2048];
                int a = 255;
                int r = 0;
                int g = 0;
                int b = 0;

                for (int i = 0; i < 2048 * 2048; i++)
                {
                    colors[i] = (UInt32)(b++ % 256 | (g++ % 256 << 8) | (r++ % 256 << 16) | (a << 24));
                    b = b + 1;
                    g = g + 3;

                }

                for (int i = 0; i < tileMap.Length; i++)
                {
                    tileMap[i] = new XNA.Graphics.Texture2D(e.GraphicsDevice, tileWidth, tileHeight);
                }
                xnaViewport.GraphicsService.GraphicsDevice.Textures[0] = null;
                _npc17 = new AnimatedTexture(_textureDictionary.Npcs[17], new Vector2(0, 0), 0F, 0F, 0F, 16, 20F, 2, int.MaxValue);
                //_npc17.SetAnimation(5F, 0, 1);
                _gameTimer.Start();
            }
        }

        private AnimatedTexture _npc17;

        private SpriteFont _font;

        private void xnaViewport_RenderXna(object sender, BCCL.UI.Xaml.XnaContentHost.GraphicsDeviceEventArgs e)
        {
            if (BCCL.Utility.Debugging.IsInDesignMode)
                return;

            if (!_gameTimer.IsRunning)
                return;

            // Update
            _gameTimer.Update();
            _npc17.UpdateFrame((float)_gameTimer.ElapsedGameTime.TotalSeconds);
            ScrollWorld();




            // Draw
            e.GraphicsDevice.Clear(Color.Gray);
            e.GraphicsDevice.Textures[0] = null;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            
            DrawTiles();

            _npc17.DrawFrame(_spriteBatch, new Vector2(100, 10));

            _spriteBatch.End();
        }

        private void DrawTiles()
        {
            for (int i = 0; i < tileMap.Length; i++)
                tileMap[i].SetData<UInt32>(colors, i*tileWidth*tileHeight, tileWidth*tileHeight);

            

            for (int i = 0; i < tileMap.Length; i++)
            {
                _spriteBatch.Draw(
                    tileMap[i],
                    new XNA.Vector2(
                        (scrollPosition.X + (i%3)*tileWidth)*zoom,
                        (scrollPosition.Y + (i/3)*tileHeight)*zoom),
                    null,
                    XNA.Color.White,
                    0,
                    Vector2.Zero,
                    zoom,
                    XNA.Graphics.SpriteEffects.None,
                    0);
            }
        }

        private XNA.Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
        private XNA.Vector2 scrollPosition = new XNA.Vector2(0, 0);
        private bool isMiddleMouseDown;
        private XNA.Vector2 middleClickPoint;
        private XNA.Vector2 mousePosition;
        private float zoom = 1;

        // speed in tile/second 

        private void ScrollWorld()
        {
            if (isMiddleMouseDown)
            {
                var stretchDistance = (mousePosition - middleClickPoint);
                var clampedScroll = scrollPosition + stretchDistance / zoom;
                clampedScroll.X = XNA.MathHelper.Clamp(clampedScroll.X, -worldBounds.Width, 0);
                clampedScroll.Y = XNA.MathHelper.Clamp(clampedScroll.Y, -worldBounds.Height, 0);
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
