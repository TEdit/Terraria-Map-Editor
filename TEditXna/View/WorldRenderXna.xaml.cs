using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BCCL.Geometry.Primitives;
using BCCL.UI.Xaml.XnaContentHost;
using BCCL.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TEditXNA.Terraria;
using TEditXNA.Terraria.Objects;
using TEditXna.Editor;
using TEditXna.Editor.Tools;
using TEditXna.ViewModel;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace TEditXna.View
{
    /// <summary>
    /// Interaction logic for WorldRenderXna.xaml
    /// </summary>
    public partial class WorldRenderXna : UserControl
    {
        private Color _backgroundColor = Color.FromNonPremultiplied(32, 32, 32, 255);
        private readonly GameTimer _gameTimer;
        private readonly WorldViewModel _wvm;
        private bool _isMiddleMouseDown;
        private Vector2 _middleClickPoint;
        private Vector2 _mousePosition;
        private Vector2 _scrollPosition = new Vector2(0, 0);
        private SimpleProvider _serviceProvider;
        private SpriteBatch _spriteBatch;
        private Textures _textureDictionary;
        private Texture2D[] _tileMap;
        private Texture2D _preview;
        private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private Texture2D _selectionTexture;
        private float _zoom = 1;

        public WorldRenderXna()
        {
            // to stop visual studio design time crash :(
            if (!Debugging.IsInDesignMode)
            {
                InitializeComponent();
                _gameTimer = new GameTimer();
            }

            _wvm = ViewModelLocator.WorldViewModel;
            _wvm.PreviewChanged += PreviewChanged;
            _wvm.PropertyChanged += _wvm_PropertyChanged;
            _wvm.RequestZoom += _wvm_RequestZoom;
            _wvm.RequestScroll += _wvm_RequestScroll;
        }

        void _wvm_RequestScroll(object sender, BCCL.Framework.Events.EventArgs<ScrollDirection> e)
        {
            float x = _scrollPosition.X;
            float y = _scrollPosition.Y;
            float inc = 1 / _zoom * 10;
            switch (e.Value1)
            {
                case ScrollDirection.Up:
                    y += inc;
                    break;
                case ScrollDirection.Down:
                    y -= inc;
                    break;
                case ScrollDirection.Left:
                    x += inc;
                    break;
                case ScrollDirection.Right:
                    x -= inc;
                    break;
            }

            _scrollPosition = new Vector2(x, y);
            ClampScroll();
        }

        void _wvm_RequestZoom(object sender, BCCL.Framework.Events.EventArgs<bool> e)
        {
            if (e.Value1)
            {
                Zoom(1);
            }
            else
            {
                Zoom(-1);
            }
        }



        private void _wvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorld")
            {
                Zoom(0);
            }
        }

        private void PreviewChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (xnaViewport.GraphicsService.GraphicsDevice != null)
            {
                if (_wvm.ActiveTool != null)
                {
                    var preview = _wvm.ActiveTool.PreviewTool();
                    if (preview!= null)
                        _preview = preview.ToTexture2D(xnaViewport.GraphicsService.GraphicsDevice)               
                }
            }
                
        }

        private static Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        private void InitializeGraphicsComponents(GraphicsDeviceEventArgs e)
        {
            // Load services, textures and initialize spritebatch
            _serviceProvider = new SimpleProvider(xnaViewport.GraphicsService);
            _spriteBatch = new SpriteBatch(e.GraphicsDevice);
            _textureDictionary = new Textures(_serviceProvider);
        }

        public void CenterOnTile(int x, int y)
        {
            _scrollPosition = new Vector2(
                -x + (float)(xnaViewport.ActualWidth / _zoom / 2),
                -y + (float)(xnaViewport.ActualHeight / _zoom / 2));
            ClampScroll();
            ScrollBarH.Value = -_scrollPosition.X;
            ScrollBarV.Value = -_scrollPosition.Y;
        }

        #region Load Content

        private void xnaViewport_LoadContent(object sender, GraphicsDeviceEventArgs e)
        {
            // Abort rendering if in design mode or if gameTimer is already running
            if (Debugging.IsInDesignMode || _gameTimer.IsRunning)
                return;

            InitializeGraphicsComponents(e);
            if (_textureDictionary.Valid)
                LoadTerrariaTextures(e);

            _selectionTexture = new Texture2D(e.GraphicsDevice, 1, 1);
            LoadResourceTextures(e);

            _selectionTexture.SetData(new[] { Color.FromNonPremultiplied(0, 128, 255, 128) });
            // Start the Game Timer
            _gameTimer.Start();
        }

        private void LoadResourceTextures(GraphicsDeviceEventArgs e)
        {
            _textures.Add("Spawn", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.spawn_marker.png", e.GraphicsDevice));
            _textures.Add("Dungeon", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.dungeon_marker.png", e.GraphicsDevice));
            _textures.Add("Old Man", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_old_man.png", e.GraphicsDevice));
            _textures.Add("Arms Dealer", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_old_man.png", e.GraphicsDevice));
            _textures.Add("Clothier", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_clothier.png", e.GraphicsDevice));
            _textures.Add("Demolitionist", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_demolitionist.png", e.GraphicsDevice));
            _textures.Add("Dryad", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_dryad.png", e.GraphicsDevice));
            _textures.Add("Guide", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_guide.png", e.GraphicsDevice));
            _textures.Add("Merchant", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_merchant.png", e.GraphicsDevice));
            _textures.Add("Nurse", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_nurse.png", e.GraphicsDevice));
            _textures.Add("Goblin Tinkerer", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_goblin.png", e.GraphicsDevice));
            _textures.Add("Wizard", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_wizard.png", e.GraphicsDevice));
            _textures.Add("Mechanic", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_mechanic.png", e.GraphicsDevice));
            _textures.Add("Grid", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.grid.png", e.GraphicsDevice));
        }

        private void LoadTerrariaTextures(GraphicsDeviceEventArgs e)
        {
            // If the texture dictionary is valid (Found terraria and loaded content) load texture data

            foreach (var id in World.NpcIds)
            {
                _textureDictionary.GetNPC(id.Value);
            }

            //foreach (var tile in World.TileProperties.Where(t => t.IsFramed))
            //{
            //    var tileTexture = _textureDictionary.GetTile(tile.Id);
            //}

            foreach (var sprite in World.Sprites)
            {
                if (sprite.Size.X == 0 || sprite.Size.Y == 0)
                    continue;
                try
                {
                    var tile = World.TileProperties[sprite.Tile];
                    if (tile.TextureGrid.X == 0 || tile.TextureGrid.Y == 0)
                        continue;
                    var texture = new Texture2D(e.GraphicsDevice, sprite.Size.X * tile.TextureGrid.X, sprite.Size.Y * tile.TextureGrid.Y);
                    var tileTex = _textureDictionary.GetTile(sprite.Tile);
                    for (int x = 0; x < sprite.Size.X; x++)
                    {
                        for (int y = 0; y < sprite.Size.Y; y++)
                        {
                            var source = new Rectangle(x * (tile.TextureGrid.X + 2) + sprite.Origin.X, y * (tile.TextureGrid.Y + 2) + sprite.Origin.Y, tile.TextureGrid.X, tile.TextureGrid.Y);
                            if (source.Bottom > tileTex.Height)
                                source.Height -= (source.Bottom - tileTex.Height);
                            if (source.Right > tileTex.Width)
                                source.Width -= (source.Right - tileTex.Width);

                            var color = new Color[source.Height * source.Width];
                            var dest = new Rectangle(x * tile.TextureGrid.X, y * tile.TextureGrid.Y, source.Width, source.Height);
                            tileTex.GetData(0, source, color, 0, color.Length);
                            texture.SetData(0, dest, color, 0, color.Length);
                        }
                    }
                    sprite.IsPreviewTexture = true;
                    sprite.Preview = texture.Texture2DToWriteableBitmap();
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogException(ex);
                }
            }

        }

        #endregion

        #region Update

        private void xnaViewport_RenderXna(object sender, GraphicsDeviceEventArgs e)
        {
            // Clear the graphics device and texture buffer
            e.GraphicsDevice.Clear(_backgroundColor);

            // Abort rendering if in design mode or if gameTimer is not running
            if (Debugging.IsInDesignMode || !_gameTimer.IsRunning || _wvm.CurrentWorld == null)
                return;

            Update(e);
            Render(e);
        }

        private void Update(GraphicsDeviceEventArgs e)
        {
            // Update
            _gameTimer.Update();

            ScrollWorld();
        }

        private void ScrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            _scrollPosition = new Vector2(-(float)ScrollBarH.Value, -(float)ScrollBarV.Value);
            ClampScroll();
        }

        private void ScrollWorld()
        {
            if (_isMiddleMouseDown)
            {
                Vector2 stretchDistance = (_mousePosition - _middleClickPoint);
                Vector2 clampedScroll = _scrollPosition + stretchDistance / _zoom;
                _scrollPosition = clampedScroll;
                _middleClickPoint = _mousePosition;
                ClampScroll();
            }
        }

        private void ClampScroll()
        {
            if (_wvm.CurrentWorld == null || xnaViewport == null)
            {
                _scrollPosition = new Vector2(0, 0);
                ScrollBarH.Value = -_scrollPosition.X;
                ScrollBarV.Value = -_scrollPosition.Y;
                return;
            }
            int xNormalRange = -_wvm.CurrentWorld.TilesWide + (int)(xnaViewport.ActualWidth / _zoom);
            int yNormalRange = -_wvm.CurrentWorld.TilesHigh + (int)(xnaViewport.ActualHeight / _zoom);

            if (_wvm.CurrentWorld.TilesWide > (int)(xnaViewport.ActualWidth / _zoom))
                _scrollPosition.X = MathHelper.Clamp(_scrollPosition.X, xNormalRange, 0);
            else
                _scrollPosition.X = MathHelper.Clamp(_scrollPosition.X, (_wvm.CurrentWorld.TilesWide / 2 - (int)(xnaViewport.ActualWidth / _zoom) / 2), 0);

            if (_wvm.CurrentWorld.TilesHigh > (int)(xnaViewport.ActualHeight / _zoom))
                _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, yNormalRange, 0);
            else
                _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, (_wvm.CurrentWorld.TilesHigh / 2 - (int)(xnaViewport.ActualHeight / _zoom) / 2), 0);

            ScrollBarH.Value = -_scrollPosition.X;
            ScrollBarV.Value = -_scrollPosition.Y;
        }

        #endregion

        #region Render

        private void Render(GraphicsDeviceEventArgs e)
        {
            // Clear the graphics device and texture buffer
            //e.GraphicsDevice.Clear(Color.Black);
            e.GraphicsDevice.Textures[0] = null;

            GenPixelTiles(e);

            // Start SpriteBatch
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);


            // Draw Pixel Map tiles

            DrawPixelTiles();

            // Draw sprite overlays
            if (_wvm.ShowTextures && _textureDictionary.Valid)
                DrawSprites();

            if (_wvm.ShowGrid)
                DrawGrid();

            if (_wvm.ShowPoints)
            {
                _spriteBatch.End();
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                DrawPoints();
                _spriteBatch.End();
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            }

            if (_wvm.Selection.IsActive)
                DrawSelection();

            DrawToolPreview();


            // End SpriteBatch
            _spriteBatch.End();
        }

        private void GenPixelTiles(GraphicsDeviceEventArgs e)
        {
            if (_wvm != null)
            {
                if (_wvm.PixelMap != null)
                {
                    if (_tileMap == null || _tileMap.Length != _wvm.PixelMap.ColorBuffers.Length)
                    {
                        _tileMap = new Texture2D[_wvm.PixelMap.ColorBuffers.Length];
                    }

                    for (int i = 0; i < _tileMap.Length; i++)
                    {
                        if (!Check2DFrustrum(i))
                            continue;

                        // Make a new texture for nulls
                        bool init = _tileMap[i] == null;
                        if (init || _tileMap[i].Width != _wvm.PixelMap.TileWidth || _tileMap[i].Height != _wvm.PixelMap.TileHeight)
                            _tileMap[i] = new Texture2D(e.GraphicsDevice, _wvm.PixelMap.TileWidth, _wvm.PixelMap.TileHeight);

                        if (_wvm.PixelMap.BufferUpdated[i] || init)
                        {
                            _tileMap[i].SetData(_wvm.PixelMap.ColorBuffers[i]);
                            _wvm.PixelMap.BufferUpdated[i] = false;
                        }
                    }
                }
            }
        }
        private void DrawGrid()
        {
            Rectangle visibleBounds = GetViewingArea();
            var gridTex = _textures["Grid"];
            if (visibleBounds.Height * visibleBounds.Width < 25000)
            {
                for (int x = 0; x < visibleBounds.Right; x += 16)
                {
                    for (int y = 0; y < visibleBounds.Bottom; y += 16)
                    {
                        if ((x + 16 >= visibleBounds.Left || x <= visibleBounds.Right) &&
                            (y + 16 >= visibleBounds.Top || y <= visibleBounds.Bottom))
                        {

                            var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)(_zoom * 256 / 16), (int)(_zoom * 256 / 16));

                            _spriteBatch.Draw(gridTex, dest, Color.White);
                        }
                    }
                }

            }

        }
        private void DrawSprites()
        {
            Rectangle visibleBounds = GetViewingArea();
            if (visibleBounds.Height * visibleBounds.Width < 25000)
            {
                for (int x = visibleBounds.Left; x < visibleBounds.Right; x++)
                {
                    for (int y = visibleBounds.Top; y < visibleBounds.Bottom; y++)
                    {
                        var curtile = _wvm.CurrentWorld.Tiles[x, y];
                        var tileprop = World.TileProperties[curtile.Type];
                        if (tileprop.IsFramed && curtile.IsActive)
                        {
                            var source = new Rectangle(curtile.U, curtile.V, tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                            if (source.Width <= 0)
                                source.Width = 16;
                            if (source.Height <= 0)
                                source.Height = 16;

                            var tileTex = _textureDictionary.GetTile(curtile.Type);

                            if (tileTex != null)
                            {
                                if (source.Bottom > tileTex.Height)
                                    source.Height -= (source.Bottom - tileTex.Height);
                                if (source.Right > tileTex.Width)
                                    source.Width -= (source.Right - tileTex.Width);

                                if (source.Width <= 0 || source.Height <= 0)
                                    continue;

                                var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                var texsize = tileprop.TextureGrid;
                                if (texsize.X != 16 || texsize.Y != 16)
                                {
                                    dest.Width = (int)(texsize.X * (_zoom / 16));
                                    dest.Height = (int)(texsize.Y * (_zoom / 16));

                                    var frame = (tileprop.Frames.FirstOrDefault(f => f.UV == new Vector2Short(curtile.U, curtile.V)));
                                    var frameAnchor = FrameAnchor.None;
                                    if (frame != null)
                                        frameAnchor = frame.Anchor;
                                    switch (frameAnchor)
                                    {
                                        case FrameAnchor.None:
                                            dest.X += (int)(((16 - texsize.X) / 2F) * _zoom / 16);
                                            dest.Y += (int)(((16 - texsize.Y) / 2F) * _zoom / 16);
                                            break;
                                        case FrameAnchor.Left:
                                            //position.X += (16 - texsize.X) / 2;
                                            dest.Y += (int)(((16 - texsize.Y) / 2F) * _zoom / 16);
                                            break;
                                        case FrameAnchor.Right:
                                            dest.X += (int)((16 - texsize.X) * _zoom / 16);
                                            dest.Y += (int)(((16 - texsize.Y) / 2F) * _zoom / 16);
                                            break;
                                        case FrameAnchor.Top:
                                            dest.X += (int)(((16 - texsize.X) / 2F) * _zoom / 16);
                                            //position.Y += (16 - texsize.Y);
                                            break;
                                        case FrameAnchor.Bottom:
                                            dest.X += (int)(((16 - texsize.X) / 2F) * _zoom / 16);
                                            dest.Y += (int)((16 - texsize.Y) * _zoom / 16);
                                            break;
                                    }

                                }


                                _spriteBatch.Draw(tileTex,
                                                  dest,
                                                  source,
                                                  Color.White);
                            }
                        }
                    }
                }
                //_spriteBatch.Draw();
            }
        }

        private Rectangle GetViewingArea()
        {
            if (_wvm.CurrentWorld == null)
                return new Rectangle();

            var r = new Rectangle(
                (int)Math.Max(0, Math.Floor(-_scrollPosition.X)),
                (int)Math.Max(0, Math.Floor(-_scrollPosition.Y)),
                (int)Math.Ceiling(xnaViewport.ActualWidth / _zoom),
                (int)Math.Ceiling(xnaViewport.ActualHeight / _zoom));

            if (r.Right > _wvm.CurrentWorld.TilesWide)
            {
                r.Width = r.Width - (r.Right - _wvm.CurrentWorld.TilesWide);
            }

            if (r.Bottom > _wvm.CurrentWorld.TilesHigh)
            {
                r.Height = r.Height - (r.Bottom - _wvm.CurrentWorld.TilesHigh);
            }

            return r;
        }

        private void DrawPixelTiles()
        {
            //for (int i = 0; i < tileMap.Length; i++)
            //    tileMap[i].SetData<UInt32>(colors, i * tileWidth * tileHeight, tileWidth * tileHeight);
            if (_tileMap == null)
                return;

            for (int i = 0; i < _tileMap.Length; i++)
            {
                if (!Check2DFrustrum(i))
                    continue;

                _spriteBatch.Draw(
                    _tileMap[i],
                    TileOrigin(
                    (i % _wvm.PixelMap.TilesX) * _wvm.PixelMap.TileWidth,
                    (i / _wvm.PixelMap.TilesX) * _wvm.PixelMap.TileHeight),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    _zoom,
                    SpriteEffects.None,
                    0);
            }
        }

        private void DrawPoints()
        {
            foreach (var npc in _wvm.CurrentWorld.NPCs)
            {
                if (_textures.ContainsKey(npc.Name))
                {
                    _spriteBatch.Draw(_textures[npc.Name],
                                      GetMarkerLocation(npc.Home.X, npc.Home.Y),
                                      Color.White);
                }
            }

            _spriteBatch.Draw(_textures["Spawn"],
                GetMarkerLocation(_wvm.CurrentWorld.SpawnX, _wvm.CurrentWorld.SpawnY),
                Color.FromNonPremultiplied(255, 255, 255, 128));

            _spriteBatch.Draw(_textures["Dungeon"],
                GetMarkerLocation(_wvm.CurrentWorld.DungeonX, _wvm.CurrentWorld.DungeonY),
                Color.FromNonPremultiplied(255, 255, 255, 128));
        }

        private Vector2 GetMarkerLocation(int x, int y)
        {
            return new Vector2(
                (_scrollPosition.X + x) * _zoom - 10,
                (_scrollPosition.Y + y) * _zoom - 34);
        }

        private void DrawToolPreview()
        {
            if (_preview == null)
                return;
            Vector2 position;

            if (_wvm.ActiveTool.ToolType == ToolType.Brush)
            {
                position = new Vector2(1 + (_scrollPosition.X + _wvm.MouseOverTile.MouseState.Location.X - _wvm.Brush.OffsetX) * _zoom,
                                       1 + (_scrollPosition.Y + _wvm.MouseOverTile.MouseState.Location.Y - _wvm.Brush.OffsetY) * _zoom);
            }
            else
            {
                position = new Vector2(1 + (_scrollPosition.X + _wvm.MouseOverTile.MouseState.Location.X) * _zoom,
                                       1 + (_scrollPosition.Y + _wvm.MouseOverTile.MouseState.Location.Y) * _zoom);
            }
            if (_wvm.ActiveTool.Name == "Sprite" && _wvm.SelectedSprite != null)
            {
                var texsize = World.TileProperties[_wvm.SelectedSprite.Tile].TextureGrid;
                if (texsize.X != 16 || texsize.Y != 16)
                {
                    switch (_wvm.SelectedSprite.Anchor)
                    {
                        case FrameAnchor.None:
                            position.X += ((16 - texsize.X) / 2F) * _zoom / 16;
                            position.Y += ((16 - texsize.Y) / 2F) * _zoom / 16;
                            break;
                        case FrameAnchor.Left:
                            //position.X += (16 - texsize.X) / 2;
                            position.Y += ((16 - texsize.Y) / 2F) * _zoom / 16;
                            break;
                        case FrameAnchor.Right:
                            position.X += (16 - texsize.X) * _zoom / 16;
                            position.Y += ((16 - texsize.Y) / 2F) * _zoom / 16;
                            break;
                        case FrameAnchor.Top:
                            position.X += ((16 - texsize.X) / 2F) * _zoom / 16;
                            //position.Y += (16 - texsize.Y);
                            break;
                        case FrameAnchor.Bottom:
                            position.X += ((16 - texsize.X) / 2F) * _zoom / 16;
                            position.Y += (16 - texsize.Y) * _zoom / 16;
                            break;
                    }

                }
            }

            if (_wvm.ActiveTool.PreviewIsTexture)
            {
                _spriteBatch.Draw(
                    _preview,
                    position,
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    _zoom / 16,
                    SpriteEffects.None,
                    0);
            }
            else
            {
                _spriteBatch.Draw(
                    _preview,
                    position,
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    _zoom,
                    SpriteEffects.None,
                    0);
            }
        }

        private void DrawSelection()
        {
            Rectangle destinationRectangle = new Rectangle(
                (int)((_scrollPosition.X + _wvm.Selection.SelectionArea.Left) * _zoom),
                (int)((_scrollPosition.Y + _wvm.Selection.SelectionArea.Top) * _zoom),
                (int)((_wvm.Selection.SelectionArea.Width) * _zoom),
                (int)((_wvm.Selection.SelectionArea.Height) * _zoom));

            _spriteBatch.Draw(
                _selectionTexture,
                destinationRectangle,
                Color.White);
        }

        private Vector2 TileOrigin(int tileX, int tileY)
        {
            return new Vector2(
                (_scrollPosition.X + tileX) * _zoom,
                (_scrollPosition.Y + tileY) * _zoom);
        }

        private bool Check2DFrustrum(int tileIndex)
        {
            int x = tileIndex % _wvm.PixelMap.TilesX;
            // X off min side
            var xmin = (int)(-_scrollPosition.X / _wvm.PixelMap.TileWidth);
            if (x < xmin)
                return false;

            // x off max side
            if (x > 1 + xmin + (int)((xnaViewport.GraphicsService.GraphicsDevice.Viewport.Width / _zoom) / _wvm.PixelMap.TileWidth))
                return false;


            int y = tileIndex / _wvm.PixelMap.TilesX;

            var ymin = (int)(-_scrollPosition.Y / _wvm.PixelMap.TileHeight);
            if (y < ymin)
                return false;

            if (y > 1 + ymin + (int)((xnaViewport.GraphicsService.GraphicsDevice.Viewport.Height / _zoom) / _wvm.PixelMap.TileHeight))
                return false;

            return true;
        }

        private void xnaViewport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        #endregion

        #region Mouse

        private TileMouseState GetTileMouseState(HwndMouseEventArgs e)
        {
            return TileMouseState.FromHwndMouseEventArgs(e,
                                                         new Vector2Int32(
                                                             (int)MathHelper.Clamp((float)(e.Position.X / _zoom - _scrollPosition.X), 0, _wvm.CurrentWorld.TilesWide - 1),
                                                             (int)MathHelper.Clamp((float)(e.Position.Y / _zoom - _scrollPosition.Y), 0, _wvm.CurrentWorld.TilesHigh - 1)));
        }

        private void xnaViewport_HwndMouseMove(object sender, HwndMouseEventArgs e)
        {
            _mousePosition = PointToVector2(e.Position);
            if (_wvm.CurrentWorld != null)
                _wvm.MouseMoveTile(GetTileMouseState(e));
        }

        private void xnaViewport_HwndLButtonDown(object sender, HwndMouseEventArgs e)
        {
            if (_wvm.CurrentWorld != null)
                _wvm.MouseDownTile(GetTileMouseState(e));
        }

        private void xnaViewport_HwndLButtonUp(object sender, HwndMouseEventArgs e)
        {
            if (_wvm.CurrentWorld != null)
                _wvm.MouseUpTile(GetTileMouseState(e));
        }

        private void xnaViewport_HwndRButtonDown(object sender, HwndMouseEventArgs e)
        {
            if (_wvm.CurrentWorld != null)
                _wvm.MouseDownTile(GetTileMouseState(e));
        }

        private void xnaViewport_HwndRButtonUp(object sender, HwndMouseEventArgs e)
        {
            if (_wvm.CurrentWorld != null)
                _wvm.MouseUpTile(GetTileMouseState(e));
        }

        private void xnaViewport_HwndMouseWheel(object sender, HwndMouseEventArgs e)
        {
            Zoom(e.WheelDelta);
        }

        public void Zoom(int direction)
        {
            float tempZoom = _zoom;
            if (direction > 0)
                tempZoom = _zoom * 2F;
            if (direction < 0)
                tempZoom = _zoom / 2F;
            Vector2Int32 curTile = _wvm.MouseOverTile.MouseState.Location;
            _zoom = MathHelper.Clamp(tempZoom, 0.125F, 64F);
            CenterOnTile(curTile.X, curTile.Y);

            if (_wvm.CurrentWorld != null)
            {
                var r = GetViewingArea();
                ScrollBarH.ViewportSize = r.Width;
                ScrollBarV.ViewportSize = r.Height;
                ScrollBarH.Maximum = _wvm.CurrentWorld.TilesWide - ScrollBarH.ViewportSize;
                ScrollBarV.Maximum = _wvm.CurrentWorld.TilesHigh - ScrollBarV.ViewportSize;
            }
        }



        private void xnaViewport_HwndMButtonDown(object sender, HwndMouseEventArgs e)
        {
            _middleClickPoint = PointToVector2(e.Position);
            xnaViewport.SetCursor(Cursors.SizeAll);
            _isMiddleMouseDown = true;
        }

        private void xnaViewport_HwndMButtonUp(object sender, HwndMouseEventArgs e)
        {
            _isMiddleMouseDown = false;
            xnaViewport.SetCursor(Cursors.Arrow);
        }


        private void xnaViewport_HwndMouseEnter(object sender, HwndMouseEventArgs e)
        {

        }

        private void xnaViewport_HwndMouseLeave(object sender, HwndMouseEventArgs e)
        {

        }

        #endregion



    }
}