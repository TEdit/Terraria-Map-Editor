using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.UI.Xaml.XnaContentHost;
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
        private const float LayerTilePixels = 1 - 0;

        private const float LayerTileBackgroundTextures = 1 - 0.01f;
        private const float LayerTileWallTextures = 1 - 0.02f;
        private const float LayerTileTrackBack = 1 - 0.03f;
        private const float LayerTileTextures = 1 - 0.04f;
        private const float LayerTileTrack = 1 - 0.05f;
        private const float LayerTileActuator = 1 - 0.06f;
        private const float LayerLiquid = 1 - 0.07f;
        private const float LayerRedWires = 1 - 0.08f;
        private const float LayerBlueWires = 1 - 0.09f;
        private const float LayerGreenWires = 1 - 0.10f;
        private const float LayerYellowWires = 1 - 0.11f;

        private const float LayerGrid = 1 - 0.15f;
        private const float LayerLocations = 1 - 0.20f;
        private const float LayerSelection = 1 - 0.25f;
        private const float LayerTools = 1 - 0.30f;

        private Color _backgroundColor = Color.FromNonPremultiplied(32, 32, 32, 255);
        private readonly GameTimer _gameTimer;
        private readonly WorldViewModel _wvm;
        private bool _isMiddleMouseDown;
        private Vector2 _middleClickPoint;
        private Vector2 _mousePosition;
        private Vector2 _dpiScale;
        private Vector2 _scrollPosition = new Vector2(0, 0);
        private SimpleProvider _serviceProvider;
        private SpriteBatch _spriteBatch;
        private Textures _textureDictionary;
        private Texture2D[] _tileMap;
        private Texture2D _preview;
        private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private Texture2D _selectionTexture;
        private float _zoom = 1;
        private float _minNpcScale = 0.75f;


        public WorldRenderXna()
        {
            _wvm = ViewModelLocator.WorldViewModel;

            if (ViewModelBase.IsInDesignModeStatic)
                return;

            InitializeComponent();
            _gameTimer = new GameTimer();
            _wvm.PreviewChanged += PreviewChanged;
            _wvm.PropertyChanged += _wvm_PropertyChanged;
            _wvm.RequestZoom += _wvm_RequestZoom;
            _wvm.RequestScroll += _wvm_RequestScroll;

        }

        void _wvm_RequestScroll(object sender, TEdit.Framework.Events.EventArgs<ScrollDirection> e)
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

        void _wvm_RequestZoom(object sender, TEdit.Framework.Events.EventArgs<bool> e)
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
                    if (preview != null)
                        _preview = preview.ToTexture2D(xnaViewport.GraphicsService.GraphicsDevice);
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
            _textureDictionary = new Textures(_serviceProvider, e.GraphicsDevice);

            System.Windows.Media.Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
            _dpiScale = new Vector2((float)m.M11, (float)m.M22);
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
        }

        private void LoadResourceTextures(GraphicsDeviceEventArgs e)
        {
            _textures.Add("Spawn"          , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.spawn_marker.png", e.GraphicsDevice));
            _textures.Add("Dungeon"        , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.dungeon_marker.png", e.GraphicsDevice));
            _textures.Add("Old Man"        , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_old_man.png", e.GraphicsDevice));
            _textures.Add("Arms Dealer"    , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_arms_dealer.png", e.GraphicsDevice));
            _textures.Add("Clothier"       , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_clothier.png", e.GraphicsDevice));
            _textures.Add("Demolitionist"  , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_demolitionist.png", e.GraphicsDevice));
            _textures.Add("Dryad"          , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_dryad.png", e.GraphicsDevice));
            _textures.Add("Guide"          , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_guide.png", e.GraphicsDevice));
            _textures.Add("Merchant"       , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_merchant.png", e.GraphicsDevice));
            _textures.Add("Nurse"          , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_nurse.png", e.GraphicsDevice));
            _textures.Add("Goblin Tinkerer", WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_goblin.png", e.GraphicsDevice));
            _textures.Add("Wizard"         , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_wizard.png", e.GraphicsDevice));
            _textures.Add("Mechanic"       , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_mechanic.png", e.GraphicsDevice));
            _textures.Add("Santa Claus"    , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_santa_claus.png", e.GraphicsDevice));
            _textures.Add("Truffle"        , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_truffle.png", e.GraphicsDevice));
            _textures.Add("Steampunker"    , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_steampunker.png", e.GraphicsDevice));
            _textures.Add("Dye Trader"     , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_dyetrader.png", e.GraphicsDevice));
            _textures.Add("Party Girl"     , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_partygirl.png", e.GraphicsDevice));
            _textures.Add("Cyborg"         , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_cyborg.png", e.GraphicsDevice));
            _textures.Add("Painter"        , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_painter.png", e.GraphicsDevice));
            _textures.Add("Witch Doctor"   , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_witch_doctor.png", e.GraphicsDevice));
            _textures.Add("Pirate"         , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_pirate.png", e.GraphicsDevice));
            _textures.Add("Stylist"        , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_stylist.png", e.GraphicsDevice));
            _textures.Add("Angler"         , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_angler.png", e.GraphicsDevice));
            _textures.Add("Tax Collector"  , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_tax_collector.png", e.GraphicsDevice));
            _textures.Add("Tavernkeep"     , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.npc_tavernkeep.png", e.GraphicsDevice));
            _textures.Add("Grid"           , WriteableBitmapEx.ResourceToTexture2D("TEditXna.Images.Overlays.grid.png", e.GraphicsDevice));
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
                            if (sprite.Tile == 171)
                                source = new Rectangle(x * (tile.TextureGrid.X) + sprite.Origin.X, y * (tile.TextureGrid.Y) + sprite.Origin.Y, tile.TextureGrid.X, tile.TextureGrid.Y);
                            if (source.Bottom > tileTex.Height)
                                source.Height -= (source.Bottom - tileTex.Height);
                            if (source.Right > tileTex.Width)
                                source.Width -= (source.Right - tileTex.Width);

                            if (source.Height <= 0 || source.Width <= 0) continue;

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
                    ErrorLogging.Log(e.GraphicsDevice.GraphicsDeviceStatus.ToString());
                }
            }

        }

        #endregion

        #region Update

        private void xnaViewport_RenderXna(object sender, GraphicsDeviceEventArgs e)
        {
            // Abort rendering if in design mode or if gameTimer is not running
            if (!_gameTimer.IsRunning || _wvm.CurrentWorld == null || ViewModelBase.IsInDesignModeStatic)
                return;

            // Clear the graphics device and texture buffer
            e.GraphicsDevice.Clear(_backgroundColor);



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
            //e.GraphicsDevice.Clear(TileColor.Black);
            e.GraphicsDevice.Textures[0] = null;

            GenPixelTiles(e);

            // Start SpriteBatch
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            DrawPixelTiles();
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);


            // Draw sprite overlays
            if (_wvm.ShowTextures && _textureDictionary.Valid)
                DrawSprites();

            if (_wvm.ShowGrid)
                DrawGrid();

            if (_wvm.ShowPoints)
                DrawPoints();

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
            Rectangle src = new Rectangle(0, 0, gridTex.Width, gridTex.Height);
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

                            _spriteBatch.Draw(gridTex, dest, src, Color.White, 0, Vector2.Zero, SpriteEffects.None, LayerGrid);
                        }
                    }
                }

            }

        }

        /* Heathtech */
        //Pretty much overwrote this whole function.  The original part is still intact, but much more hidden
        private void DrawSprites()
        {
            Rectangle visibleBounds = GetViewingArea();
            Terraria.Objects.BlendRules blendRules = Terraria.Objects.BlendRules.Instance;
            var width  = _wvm.CurrentWorld.TilesWide;
            var height = _wvm.CurrentWorld.TilesHigh;


            if (visibleBounds.Height * visibleBounds.Width < 25000)
            {
                //Extended the viewing space to give tiles time to cache their UV's
                for (int y = visibleBounds.Top - 1; y < visibleBounds.Bottom + 2; y++)
                {
                    for (int x = visibleBounds.Left - 1; x < visibleBounds.Right + 2; x++)
                    {
                        if (x < 0 || y < 0 || x >= _wvm.CurrentWorld.TilesWide || y >= _wvm.CurrentWorld.TilesHigh)
                        {
                            continue;
                        }

                        var curtile = _wvm.CurrentWorld.Tiles[x, y];
                        var tileprop = World.TileProperties[curtile.Type];

                        //Neighbor tiles are often used when dynamically determining which UV position to render
                        int e = 0, n = 1, w = 2, s = 3, ne = 4, nw = 5, sw = 6, se = 7;
                        Tile[] neighborTile = new Tile[8];
                        neighborTile[ e] = (x + 1) < width                     ? _wvm.CurrentWorld.Tiles[x + 1, y    ] : null;
                        neighborTile[ n] = (y - 1) > 0                         ? _wvm.CurrentWorld.Tiles[x    , y - 1] : null;
                        neighborTile[ w] = (x - 1) > 0                         ? _wvm.CurrentWorld.Tiles[x - 1, y    ] : null;
                        neighborTile[ s] = (y + 1) < height                    ? _wvm.CurrentWorld.Tiles[x    , y + 1] : null;
                        neighborTile[ne] = (x + 1) < width && (y - 1) > 0      ? _wvm.CurrentWorld.Tiles[x + 1, y - 1] : null;
                        neighborTile[nw] = (x - 1) > 0     && (y - 1) > 0      ? _wvm.CurrentWorld.Tiles[x - 1, y - 1] : null;
                        neighborTile[sw] = (x - 1) > 0     && (y + 1) < height ? _wvm.CurrentWorld.Tiles[x - 1, y + 1] : null;
                        neighborTile[se] = (x + 1) < width && (y + 1) < height ? _wvm.CurrentWorld.Tiles[x + 1, y + 1] : null;

                        //draw background textures
                        if (y >= 80)
                        {
                            int[,] backstyle = {
                                {66, 67, 68, 69, 128, 125, 185},
                                {70, 71, 68, 72, 128, 125, 185},
                                {73, 74, 75, 76, 134, 125, 185},
                                {77, 78, 79, 82, 134, 125, 185},
                                {83, 84, 85, 86, 137, 125, 185},
                                {83, 87, 88, 89, 137, 125, 185},
                                {121, 122, 123, 124, 140, 125, 185},
                                {153, 147, 148, 149, 150, 125, 185},
                                {146, 154, 155, 156, 157, 125, 185}
                            };
                            int hellback = _wvm.CurrentWorld.HellBackStyle;
                            int backX = 0;
                            if (x <= _wvm.CurrentWorld.CaveBackX0)
                                backX = _wvm.CurrentWorld.CaveBackStyle0;
                            else if (x > _wvm.CurrentWorld.CaveBackX0 && x <= _wvm.CurrentWorld.CaveBackX1)
                                backX = _wvm.CurrentWorld.CaveBackStyle1;
                            else if (x > _wvm.CurrentWorld.CaveBackX1 && x <= _wvm.CurrentWorld.CaveBackX2)
                                backX = _wvm.CurrentWorld.CaveBackStyle2;
                            else if (x > _wvm.CurrentWorld.CaveBackX2)
                                backX = _wvm.CurrentWorld.CaveBackStyle3;
                            var source = new Rectangle(0, 0, 16, 16);
                            var backTex = _textureDictionary.GetBackground(0);
                            if (y < _wvm.CurrentWorld.GroundLevel)
                            {
                                backTex = _textureDictionary.GetBackground(0);
                                source.Y += (y - 80) * 16;
                            }
                            else if (y == _wvm.CurrentWorld.GroundLevel)
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 0]);
                                source.X += (x % 8) * 16;
                            }
                            else if (y > _wvm.CurrentWorld.GroundLevel && y < _wvm.CurrentWorld.RockLevel)
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 1]);
                                source.X += (x % 8) * 16;
                                source.Y += ((y - 1 - (int)_wvm.CurrentWorld.GroundLevel) % 6) * 16;
                            }
                            else if (y == _wvm.CurrentWorld.RockLevel)
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 2]);
                                source.X += (x % 8) * 16;
                            }
                            else if (y > _wvm.CurrentWorld.RockLevel && y < (_wvm.CurrentWorld.TilesHigh - 327))
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 3]);
                                source.X += (x % 8) * 16;
                                source.Y += ((y - 1 - (int)_wvm.CurrentWorld.RockLevel) % 6) * 16;
                            }
                            else if (y == (_wvm.CurrentWorld.TilesHigh - 327))
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 4] + hellback);
                                source.X += (x % 8) * 16;
                            }
                            else if (y > (_wvm.CurrentWorld.TilesHigh - 327) && y < (_wvm.CurrentWorld.TilesHigh - 200))
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 5] + hellback);
                                source.X += (x % 8) * 16;
                                source.Y += ((y - 1 - (int)_wvm.CurrentWorld.TilesHigh + 327) % 18) * 16;
                            }
                            else if (y == (_wvm.CurrentWorld.TilesHigh - 200))
                            {
                                backTex = _textureDictionary.GetBackground(backstyle[backX, 6] + hellback);
                                source.X += (x % 8) * 16;
                            }
                            else
                            {
                                backTex = _textureDictionary.GetUnderworld(4);
                                source.Y += (y - (int)_wvm.CurrentWorld.TilesHigh + 200) * 16;
                            }

                            var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                            _spriteBatch.Draw(backTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileBackgroundTextures);
                        }

                        if (_wvm.ShowWalls)
                        {
                            if (curtile.Wall > 0)
                            {
                                var wallTex = _textureDictionary.GetWall(curtile.Wall);

                                if (wallTex != null)
                                {
                                    if (curtile.uvWallCache == 0xFFFF)
                                    {
                                        int sameStyle = 0x00000000;
                                        sameStyle |= (neighborTile[e] != null && neighborTile[e].Wall > 0) ? 0x0001 : 0x0000;
                                        sameStyle |= (neighborTile[n] != null && neighborTile[n].Wall > 0) ? 0x0010 : 0x0000;
                                        sameStyle |= (neighborTile[w] != null && neighborTile[w].Wall > 0) ? 0x0100 : 0x0000;
                                        sameStyle |= (neighborTile[s] != null && neighborTile[s].Wall > 0) ? 0x1000 : 0x0000;

                                        Vector2Int32 uvBlend = blendRules.GetUVForMasks((uint)sameStyle, 0x00000000, 0);
                                        curtile.uvWallCache = (ushort)((uvBlend.Y << 8) + uvBlend.X);
                                    }

                                    var texsize = new Vector2Int32(32, 32);
                                    var source = new Rectangle((curtile.uvWallCache & 0x00FF) * (texsize.X + 4), (curtile.uvWallCache >> 8) * (texsize.Y + 4), texsize.X, texsize.Y);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x - 0.5) * _zoom), 1 + (int)((_scrollPosition.Y + y - 0.5) * _zoom), (int)_zoom * 2, (int)_zoom * 2);

                                    _spriteBatch.Draw(wallTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileWallTextures);
                                }
                            }
                        }
                        if (_wvm.ShowTiles)
                        {
                            if (curtile.IsActive)
                            {
                                if (tileprop.IsFramed)
                                {
                                    Rectangle source = new Rectangle(), dest = new Rectangle();
                                    var tileTex = _textureDictionary.GetTile(curtile.Type);

                                    bool isTreeSpecial = false, isMushroom = false;
                                    bool isLeft = false, isBase = false, isRight = false;
                                    if (curtile.Type == (int)TileType.Tree)
                                    {
                                        int baseX = 0;
                                        if (curtile.U == 66 && curtile.V <= 45)
                                            ++baseX;
                                        if (curtile.U == 88 && curtile.V >= 66 && curtile.V <= 110)
                                            --baseX;
                                        if (curtile.U == 22 && curtile.V >= 132 && curtile.V < 198)
                                            --baseX;
                                        if (curtile.U == 44 && curtile.V >= 132 && curtile.V < 198)
                                            ++baseX;
                                        if (curtile.U >= 22 && curtile.V >= 198)
                                        {
                                            isTreeSpecial = true;
                                            switch (curtile.U)
                                            {
                                                case 22: isBase = true; break;
                                                case 44: isLeft = true; ++baseX; break;
                                                case 66: isRight = true; --baseX; break;
                                            }
                                        }

                                        //Check tree type
                                        int treeType = -1; //Default to normal in case no grass grows beneath the tree
                                        for (int i = 0; i < 100; i++)
                                        {
                                            Tile checkTile = (y + i) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x + baseX, y + i] : null;
                                            if (checkTile != null && checkTile.IsActive)
                                            {
                                                bool found = true;
                                                switch (checkTile.Type)
                                                {
                                                    case 2: treeType = -1; break; //Normal
                                                    case 23: treeType = 0; break; //Corruption
                                                    case 60:
                                                        if (y <= _wvm.CurrentWorld.GroundLevel)
                                                        {
                                                            treeType = 1; break; // Jungle
                                                        }
                                                        treeType = 5; break; // Underground Jungle
                                                    case 70: treeType = 6; break; // Surface Mushroom
                                                    case 109: treeType = 2; break; // Hallow
                                                    case 147: treeType = 3; break; // Snow
                                                    case 199: treeType = 4; break; // Crimson
                                                    default: found = false; break;
                                                }
                                                if (found)
                                                    break;
                                            }
                                        }
                                        if (isTreeSpecial)
                                        {
                                            int treeStyle = 0; // default branches and tops
                                            switch (treeType)
                                            {
                                                case -1:
                                                    if (x <= _wvm.CurrentWorld.TreeX0)
                                                        treeStyle = _wvm.CurrentWorld.TreeStyle0;
                                                    else if (x <= _wvm.CurrentWorld.TreeX1)
                                                        treeStyle = _wvm.CurrentWorld.TreeStyle1;
                                                    else if (x <= _wvm.CurrentWorld.TreeX2)
                                                        treeStyle = _wvm.CurrentWorld.TreeStyle2;
                                                    else
                                                        treeStyle = _wvm.CurrentWorld.TreeStyle3;
                                                    if (treeStyle == 0)
                                                    {
                                                        break;
                                                    }
                                                    if (treeStyle == 5)
                                                    {
                                                        treeStyle = 10; break;
                                                    }
                                                    treeStyle = 5 + treeStyle; break;
                                                case 0:
                                                    treeStyle = 1; break;
                                                case 1:
                                                    treeStyle = 2;
                                                    if (_wvm.CurrentWorld.BgJungle == 1)
                                                        treeStyle = 11;
                                                    break;
                                                case 2:
                                                    treeStyle = 3; break;
                                                case 3:
                                                    treeStyle = 4;
                                                    if (_wvm.CurrentWorld.BgSnow == 0)
                                                    {
                                                        treeStyle = 12;
                                                        if (x % 10 == 0)
                                                            treeStyle = 18;
                                                    }
                                                    if (_wvm.CurrentWorld.BgSnow != 2 && _wvm.CurrentWorld.BgSnow != 3 && _wvm.CurrentWorld.BgSnow != 32 && _wvm.CurrentWorld.BgSnow != 4 && _wvm.CurrentWorld.BgSnow != 42)
                                                    {
                                                        break;
                                                    }
                                                    if (_wvm.CurrentWorld.BgSnow % 2 == 0)
                                                    {
                                                        if (x < _wvm.CurrentWorld.TilesWide / 2)
                                                        {
                                                            treeStyle = 16; break;
                                                        }
                                                        treeStyle = 17; break;
                                                    }
                                                    else
                                                    {
                                                        if (x > _wvm.CurrentWorld.TilesWide / 2)
                                                        {
                                                            treeStyle = 16; break;
                                                        }
                                                        treeStyle = 17; break;
                                                    }
                                                case 4:
                                                    treeStyle = 5; break;
                                                case 5:
                                                    treeStyle = 13; break;
                                                case 6:
                                                    treeStyle = 14; break;
                                            }
                                            //Abuse uvTileCache to remember what type of tree it is, since potentially scanning a hundred of blocks PER tree tile sounds slow
                                            curtile.uvTileCache = (ushort)((0x00 << 8) + 0x01 * treeStyle);
                                            if (isBase)
                                            {
                                                tileTex = (Texture2D)_textureDictionary.GetTreeTops(treeStyle);
                                            }
                                            else
                                            {
                                                tileTex = (Texture2D)_textureDictionary.GetTreeBranches(treeStyle);
                                            }
                                        }
                                        else
                                        {
                                            tileTex = _textureDictionary.GetTree(treeType);
                                        }
                                    }
                                    if (curtile.Type == (int)TileType.MushroomTree && curtile.U >= 36)
                                    {
                                        isMushroom = true;
                                        tileTex = (Texture2D)_textureDictionary.GetShroomTop(0);
                                    }
                                    if (curtile.Type == 323)
                                    {
                                        if (curtile.U >= 88 && curtile.U <= 132)
                                        {
                                            isTreeSpecial = true;
                                            isBase = true;
                                            tileTex = (Texture2D)_textureDictionary.GetTreeTops(15);
                                        }
                                        int treeType = 0;
                                        for (int i = 0; i < 100; i++)
                                        {
                                            Tile checkTile = (y + i) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x, y + i] : null;
                                            if (checkTile != null && checkTile.IsActive)
                                            {
                                                bool found = true;
                                                switch (checkTile.Type)
                                                {
                                                    case 53: treeType = 0; break; //Palm
                                                    case 112: treeType = 3; break; //Ebonsand Palm
                                                    case 116: treeType = 2; break; //Pearlsand Palm
                                                    case 234: treeType = 1; break; //Crimsand Palm
                                                    default: found = false; break;
                                                }
                                                if (found)
                                                    break;
                                            }
                                        }
                                        curtile.uvTileCache = (ushort)((0x00 << 8) + 0x01 * treeType);
                                    }

                                    if (tileTex != null)
                                    {
                                        if ((curtile.Type == 128 || curtile.Type == 269) && curtile.U >= 100)
                                        {
                                            int armor = curtile.U / 100;
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                            switch (curtile.V / 18)
                                            {
                                                case 0:
                                                    tileTex = (Texture2D)_textureDictionary.GetArmorHead(armor);
                                                    source = new Rectangle (2, 0, 36, 36);
                                                    dest.Width = (int)(_zoom * source.Width / 16f);
                                                    dest.Height = (int)(_zoom * source.Height / 16f);
                                                    dest.Y += (int)(((16 - source.Height - 4) / 2F) * _zoom / 16);
                                                    dest.X -= (int)((2 * _zoom / 16));
                                                    break;
                                                case 1:
                                                    if (curtile.Type == 128)
                                                        tileTex = (Texture2D)_textureDictionary.GetArmorBody(armor);
                                                    else
                                                        tileTex = (Texture2D)_textureDictionary.GetArmorFemale(armor);
                                                    source = new Rectangle (2, 0, 36, 54);
                                                    dest.Width = (int)(_zoom * source.Width / 16f);
                                                    dest.Height = (int)(_zoom * source.Height / 16f);
                                                    dest.Y += (int)(((16 - source.Height - 18) / 2F) * _zoom / 16);
                                                    dest.X -= (int)((2 * _zoom / 16));
                                                    break;
                                                case 2:
                                                    tileTex = (Texture2D)_textureDictionary.GetArmorLegs(armor);
                                                    source = new Rectangle (2, 42, 36, 12);
                                                    dest.Width = (int)(_zoom * source.Width / 16f);
                                                    dest.Height = (int)(_zoom * source.Height / 16f);
                                                    dest.Y -= (int)((2 * _zoom / 16));
                                                    dest.X -= (int)((2 * _zoom / 16));
                                                    break;
                                            }
                                            if (curtile.U % 100 < 36)
                                                _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.FlipHorizontally, LayerTileTrack);
                                            else
                                                _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                            tileTex = _textureDictionary.GetTile(curtile.Type);
                                            source = new Rectangle((curtile.U % 100), curtile.V, tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                        }
                                        else if (curtile.Type == 334 && curtile.U >= 5000)
                                        {
                                            if (_wvm.CurrentWorld.Tiles[x + 1, y].U >= 5000)
                                            {
                                                int weapon = (curtile.U % 5000) - 100;
                                                tileTex = (Texture2D)_textureDictionary.GetItem(weapon);
                                                int flip = curtile.U / 5000;
                                                float scale = 1f;
                                                if (tileTex.Width > 40 || tileTex.Height > 40)
                                                {
                                                    if (tileTex.Width > tileTex.Height)
                                                        scale = 40f / (float)tileTex.Width;
                                                    else
                                                        scale = 40f / (float)tileTex.Height;
                                                }
                                                scale *= World.ItemProperties[weapon].Scale;
                                                source = new Rectangle(0, 0, tileTex.Width, tileTex.Height);
                                                SpriteEffects effect = SpriteEffects.None;
                                                if (flip >= 3)
                                                {
                                                    effect = SpriteEffects.FlipHorizontally;
                                                }
                                                _spriteBatch.Draw(tileTex, new Vector2(1 + (int)((_scrollPosition.X + x + 1.5) * _zoom) , 1 + (int)((_scrollPosition.Y + y + .5) * _zoom)), source, Color.White, 0f, new Vector2((float)(tileTex.Width / 2), (float)(tileTex.Height / 2)), scale * _zoom / 16f, effect, LayerTileTrack);
                                            }
                                            source = new Rectangle(((curtile.U / 5000) - 1) * 18, curtile.V, tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                            tileTex = _textureDictionary.GetTile(curtile.Type);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                        }
                                        else if (curtile.Type == 395 && curtile.V == 0 && curtile.U % 36 == 0)
                                        {
                                            TileEntity entity = _wvm.CurrentWorld.GetTileEntityAtTile(x, y);
                                            if (entity != null)
                                            {
                                                int item = entity.NetId;
                                                if (item > 0)
                                                {
                                                    tileTex = (Texture2D)_textureDictionary.GetItem(item);
                                                    float scale = 1f;
                                                    if (tileTex.Width > 20 || tileTex.Height > 20)
                                                    {
                                                        if (tileTex.Width > tileTex.Height)
                                                            scale = 20f / (float)tileTex.Width;
                                                        else
                                                            scale = 20f / (float)tileTex.Height;
                                                    }
                                                    scale *= World.ItemProperties[item].Scale;
                                                    source = new Rectangle(0, 0, tileTex.Width, tileTex.Height);
                                                    _spriteBatch.Draw(tileTex, new Vector2(1 + (int)((_scrollPosition.X + x + 1) * _zoom) , 1 + (int)((_scrollPosition.Y + y + 1) * _zoom)), source, Color.White, 0f, new Vector2((float)(tileTex.Width / 2), (float)(tileTex.Height / 2)), scale * _zoom / 16f, SpriteEffects.FlipHorizontally, LayerTileTrack);
                                                }
                                            }
                                            source = new Rectangle(curtile.U, curtile.V, tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                            tileTex = _textureDictionary.GetTile(curtile.Type);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                        }
                                        else if (curtile.Type == 171) // Christmas Tree
                                        {
                                            if (curtile.U >= 10)
                                            {
                                                int star = curtile.V & 7;
                                                int garland = (curtile.V >> 3) & 7;
                                                int bulb = (curtile.V >> 6) & 0xf;
                                                int light = (curtile.V >> 10) & 0xf;
                                                source = new Rectangle(0, 0, 64, 128);
                                                dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom * 4, (int)_zoom * 8);
                                                if (star > 0)
                                                {
                                                    tileTex = (Texture2D)_textureDictionary.GetMisc("Xmas_3");
                                                    source.X = 66 * (star - 1);
                                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                                }
                                                if (garland > 0)
                                                {
                                                    tileTex = (Texture2D)_textureDictionary.GetMisc("Xmas_1");
                                                    source.X = 66 * (garland - 1);
                                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                                }
                                                if (bulb > 0)
                                                {
                                                    tileTex = (Texture2D)_textureDictionary.GetMisc("Xmas_2");
                                                    source.X = 66 * (bulb - 1);
                                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                                }
                                                if (light > 0)
                                                {
                                                    tileTex = (Texture2D)_textureDictionary.GetMisc("Xmas_4");
                                                    source.X = 66 * (light - 1);
                                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                                }
                                                source.X = 0;
                                                tileTex = (Texture2D)_textureDictionary.GetMisc("Xmas_0");
                                            }
                                        }
                                        else if (curtile.Type == 314)
                                        {
                                            source = new Rectangle(0, 0, 16, 16);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                            if (curtile.V >= 0) // Switch Track, Y is back tile if not -1
                                            {
                                                Vector2Int32 uvback = TrackUV(curtile.V);
                                                source.X = uvback.X * (source.Width + 2);
                                                source.Y = uvback.Y * (source.Height + 2);
                                                _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrackBack);
                                            }
                                            if ((curtile.U >= 2 && curtile.U <= 3) || (curtile.U >= 10 && curtile.U <= 13))
                                            { // Adding regular endcap
                                                dest.Y = 1 + (int)((_scrollPosition.Y + y - 1) * _zoom);
                                                source.X = 0;
                                                source.Y = 126;
                                                _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                            }
                                            if (curtile.U >= 24 && curtile.U <= 29)
                                            { // Adding bumper endcap
                                                dest.Y = 1 + (int)((_scrollPosition.Y + y - 1) * _zoom);
                                                source.X = 18;
                                                source.Y = 126;
                                                _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTrack);
                                            }
                                            if (curtile.U == 4 || curtile.U == 9 || curtile.U == 10 || curtile.U == 16 || curtile.U == 26 || curtile.U == 33 || curtile.U == 35 || curtile.V == 4)
                                            { // Adding angle track bottom right
                                                dest.Y = 1 + (int)((_scrollPosition.Y + y + 1) * _zoom);
                                                source.X = 0;
                                                source.Y = 108;
                                                for (int slice = 0; slice < 6; slice++)
                                                {
                                                    Rectangle? sourceSlice = new Rectangle(source.X + slice * 2, source.Y, 2, 12 - slice * 2);
                                                    Vector2 destSlice = new Vector2((int)(dest.X + slice * _zoom / 8.0f), dest.Y);

                                                    _spriteBatch.Draw(tileTex, destSlice, sourceSlice, Color.White, 0f, default(Vector2), _zoom / 16, SpriteEffects.None, LayerTileTrack);
                                                }
                                            }
                                            if (curtile.U == 5 || curtile.U == 8 || curtile.U == 11 || curtile.U == 17 || curtile.U == 27 || curtile.U == 32 || curtile.U == 34 || curtile.V == 5)
                                            { // Adding angle track bottom left
                                                dest.Y =  1 + (int)((_scrollPosition.Y + y + 1) * _zoom);
                                                source.X = 18;
                                                source.Y = 108;
                                                for (int slice = 2; slice < 8; slice++)
                                                {
                                                    Rectangle? sourceSlice = new Rectangle(source.X + slice * 2, source.Y, 2, slice * 2 - 2);
                                                    Vector2 destSlice = new Vector2((int)(dest.X + slice * _zoom / 8.0f), dest.Y);

                                                    _spriteBatch.Draw(tileTex, destSlice, sourceSlice, Color.White, 0f, default(Vector2), _zoom / 16, SpriteEffects.None, LayerTileTrack);
                                                }
                                            }
                                            dest.Y =  1 + (int)((_scrollPosition.Y + y) * _zoom);
                                            Vector2Int32 uv = TrackUV(curtile.U);
                                            source.X = uv.X * (source.Width + 2);
                                            source.Y = uv.Y * (source.Height + 2);

                                        }
                                        else if (isTreeSpecial)
                                        {
                                            source = new Rectangle(0, 0, 40, 40);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                            FrameAnchor frameAnchor = FrameAnchor.None;

                                            int treeStyle = (curtile.uvTileCache & 0x000F);
                                            if (isBase)
                                            {
                                                source.Width = 80;
                                                source.Height = 80;
                                                if (curtile.Type == 323)
                                                {
                                                    source.Y = treeStyle * (source.Height + 2);
                                                    source.X = ((curtile.U - 88) / 22) * (source.Width + 2);
                                                    dest.X += (int)(curtile.V * _zoom / 16);
                                                }
                                                else
                                                {
                                                    switch (treeStyle)
                                                    {
                                                        case 2:
                                                        case 11:
                                                        case 13:
                                                            source.Width = 114;
                                                            source.Height = 96;
                                                            break;
                                                        case 3:
                                                            source.X = (x % 3) * (82 * 3);
                                                            source.Height = 140;
                                                            break;
                                                    }
                                                    source.X += ((curtile.V - 198) / 22) * (source.Width + 2);
                                                }
                                                frameAnchor = FrameAnchor.Bottom;
                                            }
                                            else if (isLeft)
                                            {
                                                source.X = 0;
                                                switch (treeStyle)
                                                {
                                                    case 3:
                                                        source.Y = (x % 3) * (42 * 3);
                                                        break;
                                                }
                                                frameAnchor = FrameAnchor.Right;
                                                source.Y += ((curtile.V - 198) / 22) * (source.Height + 2);
                                            }
                                            else if (isRight)
                                            {
                                                source.X = 42;
                                                switch (treeStyle)
                                                {
                                                    case 3:
                                                        source.Y = (x % 3) * (42 * 3);
                                                        break;
                                                }
                                                frameAnchor = FrameAnchor.Left;
                                                source.Y += ((curtile.V - 198) / 22) * (source.Height + 2);
                                            }
                                            dest.Width = (int)(_zoom * source.Width / 16f);
                                            dest.Height = (int)(_zoom * source.Height / 16f);
                                            switch (frameAnchor)
                                            {
                                                case FrameAnchor.None:
                                                    dest.X += (int)(((16 - source.Width) / 2F) * _zoom / 16);
                                                    dest.Y += (int)(((16 - source.Height) / 2F) * _zoom / 16);
                                                    break;
                                                case FrameAnchor.Left:
                                                    dest.Y += (int)(((16 - source.Height) / 2F) * _zoom / 16);
                                                    break;
                                                case FrameAnchor.Right:
                                                    dest.X += (int)((16 - source.Width) * _zoom / 16);
                                                    dest.Y += (int)(((16 - source.Height) / 2F) * _zoom / 16);
                                                    break;
                                                case FrameAnchor.Top:
                                                    dest.X += (int)(((16 - source.Width) / 2F) * _zoom / 16);
                                                    break;
                                                case FrameAnchor.Bottom:
                                                    dest.X += (int)(((16 - source.Width) / 2F) * _zoom / 16);
                                                    dest.Y += (int)((16 - source.Height) * _zoom / 16);
                                                    break;
                                            }
                                        }
                                        else if (isMushroom)
                                        {
                                            source = new Rectangle(0, 0, 60, 42);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                            source.X = (curtile.V / 18) * 62;

                                            dest.Width = (int)(_zoom * source.Width / 16f);
                                            dest.Height = (int)(_zoom * source.Height / 16f);
                                            dest.X += (int)(((16 - source.Width) / 2F) * _zoom / 16);
                                            dest.Y += (int)((16 - source.Height) * _zoom / 16);
                                        }
                                        else if ((curtile.Type >= 373 && curtile.Type <= 375) || curtile.Type == 461)
                                        {
                                          //skip rendering drips
                                        }
                                        else
                                        {
                                            source = new Rectangle(curtile.U, curtile.V, tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                            if (source.Width <= 0)
                                                source.Width = 16;
                                            if (source.Height <= 0)
                                                source.Height = 16;

                                            if (source.Bottom > tileTex.Height)
                                                source.Height -= (source.Bottom - tileTex.Height);
                                            if (source.Right > tileTex.Width)
                                                source.Width -= (source.Right - tileTex.Width);

                                            if (source.Width <= 0 || source.Height <= 0)
                                                continue;

                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                            if (curtile.Type == 323)
                                            {
                                                dest.X += (int)(curtile.V * _zoom / 16);
                                                int treeType = (curtile.uvTileCache & 0x000F);
                                                source.Y = 22 * treeType;
                                            }
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
                                        }

                                        _spriteBatch.Draw(tileTex, dest, source, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTextures);
                                        // Actuator Overlay
                                        if (curtile.Actuator && _wvm.ShowActuators)
                                            _spriteBatch.Draw(_textureDictionary.Actuator, dest, _textureDictionary.ZeroSixteenRectangle, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileActuator);

                                    }
                                }
                                else if (tileprop.IsPlatform)
                                {
                                    var tileTex = _textureDictionary.GetTile(curtile.Type);

                                    if (tileTex != null)
                                    {
                                        Vector2Int32 uv;
                                        if (curtile.uvTileCache == 0xFFFF)
                                        {
                                            uv = new Vector2Int32(0, 0);
                                            byte state = 0x00;
                                            state |= (byte)((neighborTile[w] != null && neighborTile[w].IsActive && neighborTile[w].Type == curtile.Type) ? 0x01 : 0x00);
                                            state |= (byte)((neighborTile[w] != null && neighborTile[w].IsActive && World.TileProperties[neighborTile[w].Type].IsSolid && neighborTile[w].Type != curtile.Type) ? 0x02 : 0x00);
                                            state |= (byte)((neighborTile[e] != null && neighborTile[e].IsActive && neighborTile[e].Type == curtile.Type) ? 0x04 : 0x00);
                                            state |= (byte)((neighborTile[e] != null && neighborTile[e].IsActive && World.TileProperties[neighborTile[e].Type].IsSolid && neighborTile[e].Type != curtile.Type) ? 0x08 : 0x00);
                                            switch (state)
                                            {
                                                case 0x00:
                                                case 0x0A:
                                                    uv.X = 5;
                                                    break;
                                                case 0x01:
                                                    uv.X = 1;
                                                    break;
                                                case 0x02:
                                                    uv.X = 6;
                                                    break;
                                                case 0x04:
                                                    uv.X = 2;
                                                    break;
                                                case 0x05:
                                                    uv.X = 0;
                                                    break;
                                                case 0x06:
                                                    uv.X = 3;
                                                    break;
                                                case 0x08:
                                                    uv.X = 7;
                                                    break;
                                                case 0x09:
                                                    uv.X = 4;
                                                    break;
                                            }
                                            uv.Y = blendRules.randomVariation.Next(3);
                                            curtile.uvTileCache = (ushort)((uv.Y << 8) + uv.X);
                                        }

                                        var texsize = new Vector2Int32(tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                        if (texsize.X == 0 || texsize.Y == 0)
                                        {
                                            texsize = new Vector2Int32(16, 16);
                                        }
                                        var source = new Rectangle((curtile.uvTileCache & 0x00FF) * (texsize.X + 2), (curtile.uvTileCache >> 8) * (texsize.Y + 2), texsize.X, texsize.Y);
                                        var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                        _spriteBatch.Draw(tileTex, dest, source, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTextures);
                                        // Actuator Overlay
                                        if (curtile.Actuator && _wvm.ShowActuators)
                                            _spriteBatch.Draw(_textureDictionary.Actuator, dest, _textureDictionary.ZeroSixteenRectangle, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileActuator);

                                    }
                                }
                                else if (tileprop.IsCactus)
                                {

                                    var tileTex = _textureDictionary.GetTile(curtile.Type);

                                    if ((curtile.uvTileCache & 0x00FF) >= 24)
                                    {
                                        tileTex = (Texture2D)_textureDictionary.GetMisc("Crimson_Cactus");
                                    }
                                    else if ((curtile.uvTileCache & 0x00FF) >= 16)
                                    {
                                        tileTex = (Texture2D)_textureDictionary.GetMisc("Evil_Cactus");
                                    }
                                    else if ((curtile.uvTileCache & 0x00FF) >= 8)
                                    {
                                        tileTex = (Texture2D)_textureDictionary.GetMisc("Good_Cactus");
                                    }

                                    if (tileTex != null)
                                    {
                                        Vector2Int32 uv;
                                        if (curtile.uvTileCache == 0xFFFF || curtile.hasLazyChecked == false)
                                        {
                                            bool isLeft = false, isRight = false, isBase = false;

                                            //Has this cactus been base-evaluated yet?
                                            int neighborX = (neighborTile[w].uvTileCache & 0x00FF) % 8; //Why % 8? If X >= 8, use hallow, If X >= 16, use corruption
                                            if (neighborX == 0 || neighborX == 1 || neighborX == 4 || neighborX == 5)
                                            {
                                                isRight = true;
                                            }
                                            neighborX = neighborTile[e].uvTileCache & 0x00FF;
                                            if (neighborX == 0 || neighborX == 1 || neighborX == 4 || neighborX == 5)
                                            {
                                                isLeft = true;
                                            }
                                            neighborX = curtile.uvTileCache & 0x00FF;
                                            if (neighborX == 0 || neighborX == 1 || neighborX == 4 || neighborX == 5)
                                            {
                                                isBase = true;
                                            }

                                            //Evaluate Base
                                            if (isLeft == false && isRight == false && isBase == false)
                                            {
                                                int length1 = 0;
                                                int length2 = 0;
                                                while (true)
                                                {
                                                    Tile checkTile = (y + length1) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x, y + length1] : null;
                                                    if (checkTile == null || checkTile.IsActive == false || checkTile.Type != curtile.Type)
                                                    {
                                                        break;
                                                    }
                                                    length1++;
                                                }
                                                if (x + 1 < _wvm.CurrentWorld.TilesWide)
                                                {
                                                    while (true)
                                                    {
                                                        Tile checkTile = (y + length2) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x + 1, y + length2] : null;
                                                        if (checkTile == null || checkTile.IsActive == false || checkTile.Type != curtile.Type)
                                                        {
                                                            break;
                                                        }
                                                        length2++;
                                                    }
                                                }
                                                int baseX = 0;
                                                int baseY = length1;
                                                isBase = true;
                                                if (length2 >= length1)
                                                {
                                                    baseX = 1;
                                                    baseY = length2;
                                                    isBase = false;
                                                    isLeft = true;
                                                }
                                                for (int cy = y; cy < y + baseY; cy++)
                                                {
                                                    if (_wvm.CurrentWorld.Tiles[x + baseX, cy].uvTileCache == 0xFFFF)
                                                    {
                                                        if (cy == y)
                                                        {
                                                            _wvm.CurrentWorld.Tiles[x + baseX, cy].uvTileCache = 0x00 << 8 + 0x00;
                                                        }
                                                        else
                                                        {
                                                            _wvm.CurrentWorld.Tiles[x + baseX, cy].uvTileCache = 0x01 << 8 + 0x00;
                                                        }
                                                    }
                                                }
                                            }

                                            uv = new Vector2Int32(0, 0);
                                            byte state = 0x00;
                                            state |= (byte)((neighborTile[e] != null && neighborTile[e].IsActive && neighborTile[e].Type == curtile.Type) ? 0x01 : 0x00);
                                            state |= (byte)((neighborTile[n] != null && neighborTile[n].IsActive && neighborTile[n].Type == curtile.Type) ? 0x02 : 0x00);
                                            state |= (byte)((neighborTile[w] != null && neighborTile[w].IsActive && neighborTile[w].Type == curtile.Type) ? 0x04 : 0x00);
                                            state |= (byte)((neighborTile[s] != null && neighborTile[s].IsActive && neighborTile[s].Type == curtile.Type) ? 0x08 : 0x00);
                                            //state |= (byte)((neighborTile[ne] != null && neighborTile[ne].IsActive && neighborTile[ne].Type == curtile.Type) ? 0x10 : 0x00);
                                            //state |= (byte)((neighborTile[nw] != null && neighborTile[nw].IsActive && neighborTile[nw].Type == curtile.Type) ? 0x20 : 0x00);
                                            state |= (byte)((neighborTile[sw] != null && neighborTile[sw].IsActive && neighborTile[sw].Type == curtile.Type) ? 0x40 : 0x00);
                                            state |= (byte)((neighborTile[se] != null && neighborTile[se].IsActive && neighborTile[se].Type == curtile.Type) ? 0x80 : 0x00);

                                            if (isLeft)
                                            {
                                                uv.X = 3;
                                                if ((state & 0x08) != 0x00) //s
                                                {
                                                    if ((state & 0x02) != 0x00) //n
                                                    {
                                                        uv.Y = 1;
                                                    }
                                                    else //!n
                                                    {
                                                        uv.Y = 0;
                                                    }
                                                }
                                                else //!s
                                                {
                                                    if ((state & 0x02) != 0x00) //n
                                                    {
                                                        uv.Y = 2;
                                                    }
                                                    else //!n
                                                    {
                                                        uv.X = 6;
                                                        uv.Y = 2;
                                                    }
                                                }
                                            }
                                            if (isRight)
                                            {
                                                uv.X = 2;
                                                if ((state & 0x08) != 0x00) //s
                                                {
                                                    if ((state & 0x02) != 0x00) //n
                                                    {
                                                        uv.Y = 1;
                                                    }
                                                    else //!n
                                                    {
                                                        uv.Y = 0;
                                                    }
                                                }
                                                else //!s
                                                {
                                                    if ((state & 0x02) != 0x00) //n
                                                    {
                                                        uv.Y = 2;
                                                    }
                                                    else //!n
                                                    {
                                                        uv.X = 6;
                                                        uv.Y = 1;
                                                    }
                                                }
                                            }
                                            if (isBase)
                                            {
                                                if ((state & 0x02) != 0x00) //n
                                                {
                                                    uv.Y = 2;
                                                    if ((state & 0x04) != 0x00 && (state & 0x40) == 0x00 && ((state & 0x01) == 0x00 || (state & 0x80) != 0x00)) //w !sw (!e or se)
                                                    {
                                                        uv.X = 4;
                                                    }
                                                    else if ((state & 0x01) != 0x00 && (state & 0x80) == 0x00 && ((state & 0x04) == 0x00 || (state & 0x40) != 0x00)) //e !se (!w or sw)
                                                    {
                                                        uv.X = 1;
                                                    }
                                                    else if ((state & 0x04) != 0x00 && (state & 0x40) == 0x00 && (state & 0x01) != 0x00 && (state & 0x80) == 0x00) //w !sw e !se
                                                    {
                                                        uv.X = 5;
                                                    }
                                                    else
                                                    {
                                                        uv.X = 0;
                                                        uv.Y = 1;
                                                    }
                                                }
                                                else //!n
                                                {
                                                    uv.Y = 0;
                                                    if ((state & 0x04) != 0x00 && (state & 0x40) == 0x00 && ((state & 0x01) == 0x00 || (state & 0x80) != 0x00)) //w !sw (!e or se)
                                                    {
                                                        uv.X = 4;
                                                    }
                                                    else if ((state & 0x01) != 0x00 && (state & 0x80) == 0x00 && ((state & 0x04) == 0x00 || (state & 0x40) != 0x00)) //e !se (!w or sw)
                                                    {
                                                        uv.X = 1;
                                                    }
                                                    else if ((state & 0x04) != 0x00 && (state & 0x40) == 0x00 && (state & 0x01) != 0x00 && (state & 0x80) == 0x00) //w !sw e !se
                                                    {
                                                        uv.X = 5;
                                                    }
                                                    else
                                                    {
                                                        uv.X = 0;
                                                        uv.Y = 0;
                                                    }
                                                }
                                            }

                                            //Check if cactus is good or evil
                                            for (int i = 0; i < 100; i++)
                                            {
                                                int baseX = (isLeft) ? 1 : (isRight) ? -1 : 0;
                                                Tile checkTile = (y + i) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x + baseX, y + i] : null;
                                                if (checkTile != null && checkTile.IsActive && checkTile.Type == (int)TileType.CrimsandBlock) //Crimson
                                                {
                                                    uv.X += 24;
                                                    break;
                                                }
                                                if (checkTile != null && checkTile.IsActive && checkTile.Type == (int)TileType.EbonsandBlock) //Corruption
                                                {
                                                    uv.X += 16;
                                                    break;
                                                }
                                                else if (checkTile != null && checkTile.IsActive && checkTile.Type == (int)TileType.PearlsandBlock) //Hallow
                                                {
                                                    uv.X += 8;
                                                    break;
                                                }
                                            }
                                            curtile.hasLazyChecked = true;

                                            curtile.uvTileCache = (ushort)((uv.Y << 8) + uv.X);
                                        }

                                        var texsize = new Vector2Int32(tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                        if (texsize.X == 0 || texsize.Y == 0)
                                        {
                                            texsize = new Vector2Int32(16, 16);
                                        }
                                        var source = new Rectangle(((curtile.uvTileCache & 0x00FF) % 8) * (texsize.X + 2), (curtile.uvTileCache >> 8) * (texsize.Y + 2), texsize.X, texsize.Y);
                                        var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                        _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTextures);
                                    }
                                }
                                else if (tileprop.CanBlend)
                                {
                                    var tileTex = _textureDictionary.GetTile(curtile.Type);

                                    if (tileTex != null)
                                    {
                                        if (curtile.uvTileCache == 0xFFFF || curtile.hasLazyChecked == false)
                                        {
                                            int sameStyle = 0x00000000;
                                            int mergeMask = 0x00000000;
                                            int strictness = 0;
                                            if (tileprop.MergeWith.HasValue && tileprop.MergeWith.Value == -1) //Basically for cobweb
                                            {
                                                sameStyle |= (neighborTile[e] != null && neighborTile[e].IsActive) ? 0x0001 : 0x0000;
                                                sameStyle |= (neighborTile[n] != null && neighborTile[n].IsActive) ? 0x0010 : 0x0000;
                                                sameStyle |= (neighborTile[w] != null && neighborTile[w].IsActive) ? 0x0100 : 0x0000;
                                                sameStyle |= (neighborTile[s] != null && neighborTile[s].IsActive) ? 0x1000 : 0x0000;
                                            }
                                            else if (tileprop.IsStone) //Stone & Gems
                                            {
                                                sameStyle |= (neighborTile[e] != null && neighborTile[e].IsActive && World.TileProperties[neighborTile[e].Type].IsStone) ? 0x0001 : 0x0000;
                                                sameStyle |= (neighborTile[n] != null && neighborTile[n].IsActive && World.TileProperties[neighborTile[n].Type].IsStone) ? 0x0010 : 0x0000;
                                                sameStyle |= (neighborTile[w] != null && neighborTile[w].IsActive && World.TileProperties[neighborTile[w].Type].IsStone) ? 0x0100 : 0x0000;
                                                sameStyle |= (neighborTile[s] != null && neighborTile[s].IsActive && World.TileProperties[neighborTile[s].Type].IsStone) ? 0x1000 : 0x0000;
                                                sameStyle |= (neighborTile[ne] != null && neighborTile[ne].IsActive && World.TileProperties[neighborTile[ne].Type].IsStone) ? 0x00010000 : 0x00000000;
                                                sameStyle |= (neighborTile[nw] != null && neighborTile[nw].IsActive && World.TileProperties[neighborTile[nw].Type].IsStone) ? 0x00100000 : 0x00000000;
                                                sameStyle |= (neighborTile[sw] != null && neighborTile[sw].IsActive && World.TileProperties[neighborTile[sw].Type].IsStone) ? 0x01000000 : 0x00000000;
                                                sameStyle |= (neighborTile[se] != null && neighborTile[se].IsActive && World.TileProperties[neighborTile[se].Type].IsStone) ? 0x10000000 : 0x00000000;
                                            }
                                            else //Everything else
                                            {
                                                //Join to nearby tiles if their merge type is this tile's type
                                                sameStyle |= (neighborTile[e] != null && neighborTile[e].IsActive && World.TileProperties[neighborTile[e].Type].MergeWith.HasValue && World.TileProperties[neighborTile[e].Type].MergeWith.Value == curtile.Type) ? 0x0001 : 0x0000;
                                                sameStyle |= (neighborTile[n] != null && neighborTile[n].IsActive && World.TileProperties[neighborTile[n].Type].MergeWith.HasValue && World.TileProperties[neighborTile[n].Type].MergeWith.Value == curtile.Type) ? 0x0010 : 0x0000;
                                                sameStyle |= (neighborTile[w] != null && neighborTile[w].IsActive && World.TileProperties[neighborTile[w].Type].MergeWith.HasValue && World.TileProperties[neighborTile[w].Type].MergeWith.Value == curtile.Type) ? 0x0100 : 0x0000;
                                                sameStyle |= (neighborTile[s] != null && neighborTile[s].IsActive && World.TileProperties[neighborTile[s].Type].MergeWith.HasValue && World.TileProperties[neighborTile[s].Type].MergeWith.Value == curtile.Type) ? 0x1000 : 0x0000;
                                                sameStyle |= (neighborTile[ne] != null && neighborTile[ne].IsActive && World.TileProperties[neighborTile[ne].Type].MergeWith.HasValue && World.TileProperties[neighborTile[ne].Type].MergeWith.Value == curtile.Type) ? 0x00010000 : 0x00000000;
                                                sameStyle |= (neighborTile[nw] != null && neighborTile[nw].IsActive && World.TileProperties[neighborTile[nw].Type].MergeWith.HasValue && World.TileProperties[neighborTile[nw].Type].MergeWith.Value == curtile.Type) ? 0x00100000 : 0x00000000;
                                                sameStyle |= (neighborTile[sw] != null && neighborTile[sw].IsActive && World.TileProperties[neighborTile[sw].Type].MergeWith.HasValue && World.TileProperties[neighborTile[sw].Type].MergeWith.Value == curtile.Type) ? 0x01000000 : 0x00000000;
                                                sameStyle |= (neighborTile[se] != null && neighborTile[se].IsActive && World.TileProperties[neighborTile[se].Type].MergeWith.HasValue && World.TileProperties[neighborTile[se].Type].MergeWith.Value == curtile.Type) ? 0x10000000 : 0x00000000;
                                                //Join if nearby tiles have the same type as this tile's type
                                                sameStyle |= (neighborTile[e] != null && neighborTile[e].IsActive && curtile.Type == neighborTile[e].Type) ? 0x0001 : 0x0000;
                                                sameStyle |= (neighborTile[n] != null && neighborTile[n].IsActive && curtile.Type == neighborTile[n].Type) ? 0x0010 : 0x0000;
                                                sameStyle |= (neighborTile[w] != null && neighborTile[w].IsActive && curtile.Type == neighborTile[w].Type) ? 0x0100 : 0x0000;
                                                sameStyle |= (neighborTile[s] != null && neighborTile[s].IsActive && curtile.Type == neighborTile[s].Type) ? 0x1000 : 0x0000;
                                                sameStyle |= (neighborTile[ne] != null && neighborTile[ne].IsActive && curtile.Type == neighborTile[ne].Type) ? 0x00010000 : 0x00000000;
                                                sameStyle |= (neighborTile[nw] != null && neighborTile[nw].IsActive && curtile.Type == neighborTile[nw].Type) ? 0x00100000 : 0x00000000;
                                                sameStyle |= (neighborTile[sw] != null && neighborTile[sw].IsActive && curtile.Type == neighborTile[sw].Type) ? 0x01000000 : 0x00000000;
                                                sameStyle |= (neighborTile[se] != null && neighborTile[se].IsActive && curtile.Type == neighborTile[se].Type) ? 0x10000000 : 0x00000000;
                                            }
                                            if (curtile.hasLazyChecked == false)
                                            {
                                                bool lazyCheckReady = true;
                                                lazyCheckReady &= (neighborTile[e] == null || neighborTile[e].IsActive == false || World.TileProperties[neighborTile[e].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[e].Type].MergeWith.Value != curtile.Type) ? true : (neighborTile[e].lazyMergeId != 0xFF);
                                                lazyCheckReady &= (neighborTile[n] == null || neighborTile[n].IsActive == false || World.TileProperties[neighborTile[n].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[n].Type].MergeWith.Value != curtile.Type) ? true : (neighborTile[n].lazyMergeId != 0xFF);
                                                lazyCheckReady &= (neighborTile[w] == null || neighborTile[w].IsActive == false || World.TileProperties[neighborTile[w].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[w].Type].MergeWith.Value != curtile.Type) ? true : (neighborTile[w].lazyMergeId != 0xFF);
                                                lazyCheckReady &= (neighborTile[s] == null || neighborTile[s].IsActive == false || World.TileProperties[neighborTile[s].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[s].Type].MergeWith.Value != curtile.Type) ? true : (neighborTile[s].lazyMergeId != 0xFF);
                                                if (lazyCheckReady)
                                                {
                                                    sameStyle &= 0x11111110 | ((neighborTile[e] == null || neighborTile[e].IsActive == false || World.TileProperties[neighborTile[e].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[e].Type].MergeWith.Value != curtile.Type) ? 0x00000001 : ((neighborTile[e].lazyMergeId & 0x04) >> 2));
                                                    sameStyle &= 0x11111101 | ((neighborTile[n] == null || neighborTile[n].IsActive == false || World.TileProperties[neighborTile[n].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[n].Type].MergeWith.Value != curtile.Type) ? 0x00000010 : ((neighborTile[n].lazyMergeId & 0x08) << 1));
                                                    sameStyle &= 0x11111011 | ((neighborTile[w] == null || neighborTile[w].IsActive == false || World.TileProperties[neighborTile[w].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[w].Type].MergeWith.Value != curtile.Type) ? 0x00000100 : ((neighborTile[w].lazyMergeId & 0x01) << 8));
                                                    sameStyle &= 0x11110111 | ((neighborTile[s] == null || neighborTile[s].IsActive == false || World.TileProperties[neighborTile[s].Type].MergeWith.HasValue == false || World.TileProperties[neighborTile[s].Type].MergeWith.Value != curtile.Type) ? 0x00001000 : ((neighborTile[s].lazyMergeId & 0x02) << 11));
                                                    curtile.hasLazyChecked = true;
                                                }
                                            }
                                            if (tileprop.MergeWith.HasValue && tileprop.MergeWith.Value > -1) //Merges with a specific type
                                            {
                                                mergeMask |= (neighborTile[e] != null && neighborTile[e].IsActive && neighborTile[e].Type == tileprop.MergeWith.Value) ? 0x0001 : 0x0000;
                                                mergeMask |= (neighborTile[n] != null && neighborTile[n].IsActive && neighborTile[n].Type == tileprop.MergeWith.Value) ? 0x0010 : 0x0000;
                                                mergeMask |= (neighborTile[w] != null && neighborTile[w].IsActive && neighborTile[w].Type == tileprop.MergeWith.Value) ? 0x0100 : 0x0000;
                                                mergeMask |= (neighborTile[s] != null && neighborTile[s].IsActive && neighborTile[s].Type == tileprop.MergeWith.Value) ? 0x1000 : 0x0000;
                                                mergeMask |= (neighborTile[ne] != null && neighborTile[ne].IsActive && neighborTile[ne].Type == tileprop.MergeWith.Value) ? 0x00010000 : 0x00000000;
                                                mergeMask |= (neighborTile[nw] != null && neighborTile[nw].IsActive && neighborTile[nw].Type == tileprop.MergeWith.Value) ? 0x00100000 : 0x00000000;
                                                mergeMask |= (neighborTile[sw] != null && neighborTile[sw].IsActive && neighborTile[sw].Type == tileprop.MergeWith.Value) ? 0x01000000 : 0x00000000;
                                                mergeMask |= (neighborTile[se] != null && neighborTile[se].IsActive && neighborTile[se].Type == tileprop.MergeWith.Value) ? 0x10000000 : 0x00000000;
                                                strictness = 1;
                                            }
                                            if (tileprop.IsGrass)
                                            {
                                                strictness = 2;
                                            }

                                            Vector2Int32 uvBlend = blendRules.GetUVForMasks((uint)sameStyle, (uint)mergeMask, strictness);
                                            curtile.uvTileCache = (ushort)((uvBlend.Y << 8) + uvBlend.X);
                                            curtile.lazyMergeId = blendRules.lazyMergeValidation[uvBlend.Y, uvBlend.X];
                                        }

                                        var texsize = new Vector2Int32(tileprop.TextureGrid.X, tileprop.TextureGrid.Y);
                                        if (texsize.X == 0 || texsize.Y == 0)
                                        {
                                            texsize = new Vector2Int32(16, 16);
                                        }
                                        var source = new Rectangle((curtile.uvTileCache & 0x00FF) * (texsize.X + 2), (curtile.uvTileCache >> 8) * (texsize.Y + 2), texsize.X, texsize.Y);
                                        var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);


                                        // hack for some slopes
                                        switch (curtile.BrickStyle)
                                        {

                                            case BrickStyle.HalfBrick:
                                                source.Height /= 2;
                                                dest.Y += (int)(_zoom * 0.5);
                                                dest.Height = (int)(_zoom / 2.0f);
                                                _spriteBatch.Draw(tileTex, dest, source, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTextures);
                                                break;
                                            case BrickStyle.SlopeTopRight:

                                                for (int slice = 0; slice < 8; slice++)
                                                {
                                                    Rectangle? sourceSlice = new Rectangle(source.X + slice * 2, source.Y, 2, 16 - slice * 2);
                                                    Vector2 destSlice = new Vector2((int)(dest.X + slice * _zoom / 8.0f), (int)(dest.Y + slice * _zoom / 8.0f));

                                                    _spriteBatch.Draw(tileTex, destSlice, sourceSlice, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), _zoom / 16, SpriteEffects.None, LayerTileTextures);
                                                }

                                                break;
                                            case BrickStyle.SlopeTopLeft:
                                                for (int slice = 0; slice < 8; slice++)
                                                {
                                                    Rectangle? sourceSlice = new Rectangle(source.X + slice * 2, source.Y, 2, slice * 2 + 2);
                                                    Vector2 destSlice = new Vector2((int)(dest.X + slice * _zoom / 8.0f), (int)(dest.Y + (7 - slice) * _zoom / 8.0f));

                                                    _spriteBatch.Draw(tileTex, destSlice, sourceSlice, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), _zoom / 16, SpriteEffects.None, LayerTileTextures);
                                                }

                                                break;
                                            case BrickStyle.SlopeBottomRight:
                                                for (int slice = 0; slice < 8; slice++)
                                                {
                                                    Rectangle? sourceSlice = new Rectangle(source.X + slice * 2, source.Y + slice * 2, 2, 16 - slice * 2);
                                                    Vector2 destSlice = new Vector2((int)(dest.X + slice * _zoom / 8.0f), dest.Y);

                                                    _spriteBatch.Draw(tileTex, destSlice, sourceSlice, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), _zoom / 16, SpriteEffects.None, LayerTileTextures);
                                                }

                                                break;
                                            case BrickStyle.SlopeBottomLeft:
                                                for (int slice = 0; slice < 8; slice++)
                                                {
                                                    Rectangle? sourceSlice = new Rectangle(source.X + slice * 2, source.Y, 2, slice * 2 + 2);
                                                    Vector2 destSlice = new Vector2((int)(dest.X + slice * _zoom / 8.0f), dest.Y);

                                                    _spriteBatch.Draw(tileTex, destSlice, sourceSlice, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), _zoom / 16, SpriteEffects.None, LayerTileTextures);
                                                }

                                                break;
                                            case BrickStyle.Full:
                                            default:
                                                _spriteBatch.Draw(tileTex, dest, source, curtile.InActive ? Color.Gray : Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileTextures);
                                                break;
                                        }


                                        // Actuator Overlay
                                        if (curtile.Actuator && _wvm.ShowActuators)
                                            _spriteBatch.Draw(_textureDictionary.Actuator, dest, _textureDictionary.ZeroSixteenRectangle, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerTileActuator);

                                    }
                                }
                            }
                        }
                        if (_wvm.ShowRedWires || _wvm.ShowBlueWires || _wvm.ShowGreenWires || _wvm.ShowYellowWires)
                        {
                            var tileTex = (Texture2D)_textureDictionary.GetMisc("WiresNew");
                            if (tileTex != null)
                            {
                                int voffset = 0;
                                if (curtile.Type == 424)
                                    voffset = (curtile.U / 18 + 1) * 72;
                                if (curtile.Type == 445)
                                    voffset = 72;
                                if (curtile.WireRed && _wvm.ShowRedWires)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                    byte state = 0x00;
                                    state |= (byte)((neighborTile[n] != null && neighborTile[n].WireRed == true) ? 0x01 : 0x00);
                                    state |= (byte)((neighborTile[e] != null && neighborTile[e].WireRed == true) ? 0x02 : 0x00);
                                    state |= (byte)((neighborTile[s] != null && neighborTile[s].WireRed == true) ? 0x04 : 0x00);
                                    state |= (byte)((neighborTile[w] != null && neighborTile[w].WireRed == true) ? 0x08 : 0x00);
                                    source.X = state * 18;
                                    source.Y = voffset;

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerRedWires);
                                }
                                if (curtile.WireBlue && _wvm.ShowBlueWires)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                    byte state = 0x00;
                                    state |= (byte)((neighborTile[n] != null && neighborTile[n].WireBlue == true) ? 0x01 : 0x00);
                                    state |= (byte)((neighborTile[e] != null && neighborTile[e].WireBlue == true) ? 0x02 : 0x00);
                                    state |= (byte)((neighborTile[s] != null && neighborTile[s].WireBlue == true) ? 0x04 : 0x00);
                                    state |= (byte)((neighborTile[w] != null && neighborTile[w].WireBlue == true) ? 0x08 : 0x00);
                                    source.X = state * 18;
                                    source.Y = 18 + voffset;

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerBlueWires);
                                }
                                if (curtile.WireGreen && _wvm.ShowGreenWires)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                    byte state = 0x00;
                                    state |= (byte)((neighborTile[n] != null && neighborTile[n].WireGreen == true) ? 0x01 : 0x00);
                                    state |= (byte)((neighborTile[e] != null && neighborTile[e].WireGreen == true) ? 0x02 : 0x00);
                                    state |= (byte)((neighborTile[s] != null && neighborTile[s].WireGreen == true) ? 0x04 : 0x00);
                                    state |= (byte)((neighborTile[w] != null && neighborTile[w].WireGreen == true) ? 0x08 : 0x00);
                                    source.X = state * 18;
                                    source.Y = 36 + voffset;

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerGreenWires);
                                }
                                if (curtile.WireYellow && _wvm.ShowYellowWires)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                    byte state = 0x00;
                                    state |= (byte)((neighborTile[n] != null && neighborTile[n].WireYellow == true) ? 0x01 : 0x00);
                                    state |= (byte)((neighborTile[e] != null && neighborTile[e].WireYellow == true) ? 0x02 : 0x00);
                                    state |= (byte)((neighborTile[s] != null && neighborTile[s].WireYellow == true) ? 0x04 : 0x00);
                                    state |= (byte)((neighborTile[w] != null && neighborTile[w].WireYellow == true) ? 0x08 : 0x00);
                                    source.X = state * 18;
                                    source.Y = 54 + voffset;

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, LayerYellowWires);
                                }
                            }
                        }
                        if (_wvm.ShowLiquid)
                        {
                            if (curtile.LiquidAmount > 0)
                            {
                                Texture2D tileTex = null;
                                if (curtile.LiquidType == LiquidType.Lava)
                                {
                                    tileTex = (Texture2D)_textureDictionary.GetLiquid(1);
                                }
                                else if (curtile.LiquidType == LiquidType.Honey)
                                {
                                    tileTex = (Texture2D)_textureDictionary.GetLiquid(11); // Not sure if yellow Desert water, or Honey, but looks fine.
                                }
                                else
                                {
                                    tileTex = (Texture2D)_textureDictionary.GetLiquid(0);
                                }

                                if (tileTex != null)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                    float alpha = 1f;

                                    if (curtile.LiquidType != LiquidType.Lava)
                                    {
                                        alpha = 0.5f;
                                    }
                                    else
                                    {
                                        alpha = 0.85f;
                                    }

                                    if (neighborTile[n] != null && neighborTile[n].LiquidAmount > 0)
                                    {
                                        source.Y = 8;
                                        source.Height = 8;
                                    }
                                    else
                                    {
                                        source.Height = 4 + ((int)Math.Round(curtile.LiquidAmount * 6f / 255f)) * 2;
                                        dest.Height = (int)(source.Height * _zoom / 16f);
                                        dest.Y = 1 + (int)((_scrollPosition.Y + y) * _zoom + ((16 - source.Height) * _zoom / 16f));
                                    }

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White * alpha, 0f, default(Vector2), SpriteEffects.None, LayerLiquid);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vector2Int32 TrackUV(int num)
        {
            var uv = new Vector2Int32(0, 0);
            switch (num)
            {
                case 0: uv.X = 0; uv.Y = 0; break;
                case 1: uv.X = 1; uv.Y = 0; break;
                case 2: uv.X = 2; uv.Y = 1; break;
                case 3: uv.X = 3; uv.Y = 1; break;
                case 4: uv.X = 0; uv.Y = 2; break;
                case 5: uv.X = 1; uv.Y = 2; break;
                case 6: uv.X = 0; uv.Y = 1; break;
                case 7: uv.X = 1; uv.Y = 1; break;
                case 8: uv.X = 0; uv.Y = 3; break;
                case 9: uv.X = 1; uv.Y = 3; break;
                case 10: uv.X = 4; uv.Y = 1; break;
                case 11: uv.X = 5; uv.Y = 1; break;
                case 12: uv.X = 6; uv.Y = 1; break;
                case 13: uv.X = 7; uv.Y = 1; break;
                case 14: uv.X = 2; uv.Y = 0; break;
                case 15: uv.X = 3; uv.Y = 0; break;
                case 16: uv.X = 4; uv.Y = 0; break;
                case 17: uv.X = 5; uv.Y = 0; break;
                case 18: uv.X = 6; uv.Y = 0; break;
                case 19: uv.X = 7; uv.Y = 0; break;
                case 20: uv.X = 0; uv.Y = 4; break;
                case 21: uv.X = 1; uv.Y = 4; break;
                case 22: uv.X = 0; uv.Y = 5; break;
                case 23: uv.X = 1; uv.Y = 5; break;
                case 24: uv.X = 2; uv.Y = 2; break;
                case 25: uv.X = 3; uv.Y = 2; break;
                case 26: uv.X = 4; uv.Y = 2; break;
                case 27: uv.X = 5; uv.Y = 2; break;
                case 28: uv.X = 6; uv.Y = 2; break;
                case 29: uv.X = 7; uv.Y = 2; break;
                case 30: uv.X = 2; uv.Y = 3; break;
                case 31: uv.X = 3; uv.Y = 3; break;
                case 32: uv.X = 4; uv.Y = 3; break;
                case 33: uv.X = 5; uv.Y = 3; break;
                case 34: uv.X = 6; uv.Y = 3; break;
                case 35: uv.X = 7; uv.Y = 3; break;
                case 36: uv.X = 0; uv.Y = 6; break;
                case 37: uv.X = 1; uv.Y = 6; break;
                case 38: uv.X = 0; uv.Y = 7; break;
                case 39: uv.X = 1; uv.Y = 7; break;
            }
            return uv;
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
                    LayerTilePixels);
            }
        }

        private void DrawPoints()
        {
            Boolean useTextures = _wvm.ShowTextures && _textureDictionary.Valid;

            foreach (var npc in _wvm.CurrentWorld.NPCs)
            {
                if (useTextures)
                    DrawNpcTexture(npc);
                else
                    DrawNpcOverlay(npc);
            }

            _spriteBatch.Draw(
                _textures["Spawn"],
                GetOverlayLocation(_wvm.CurrentWorld.SpawnX, _wvm.CurrentWorld.SpawnY),
                _textures["Spawn"].Bounds,
                Color.FromNonPremultiplied(255, 255, 255, 128),
                0f,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                LayerLocations);

            _spriteBatch.Draw(
                _textures["Dungeon"],
                GetOverlayLocation(_wvm.CurrentWorld.DungeonX, _wvm.CurrentWorld.DungeonY),
                _textures["Dungeon"].Bounds,
                Color.FromNonPremultiplied(255, 255, 255, 128),
                0f,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                LayerLocations);
        }

        private void DrawNpcTexture(NPC npc)
        {
            int npcId = npc.SpriteId;

            if (_textureDictionary.Npcs.ContainsKey(npcId))
            {
                Texture2D npcTexture = (Texture2D)_textureDictionary.GetNPC(npcId);
                int frames = World.NpcFrames[npcId];
                int width = npcTexture.Width;
                int height = npcTexture.Height / frames;
                float scale = 1.0f * _zoom / 16;
                if (scale < _minNpcScale)
                    scale = _minNpcScale;
                Vector2 home = GetNpcLocation(npc.Home.X, npc.Home.Y, width, (int)(height * scale));
                _spriteBatch.Draw(npcTexture, home, new Rectangle(0, 0, width, height), Color.White, 0.0f, new Vector2(0, 0), scale, SpriteEffects.None, LayerLocations);
            }
            else
            {
                DrawNpcOverlay(npc);
            }
        }

        private void DrawNpcOverlay(NPC npc)
        {
            string npcName = npc.Name;

            if (_textures.ContainsKey(npcName))
            {
                _spriteBatch.Draw(
                    _textures[npcName],
                    GetOverlayLocation(npc.Home.X, npc.Home.Y),
                    _textures[npcName].Bounds,
                    Color.FromNonPremultiplied(255, 255, 255, 128),
                    0f,
                    Vector2.Zero,
                    Vector2.One,
                    SpriteEffects.None,
                    LayerLocations);
            }
        }

        private Vector2 GetOverlayLocation(int x, int y)
        {
            return new Vector2(
                (_scrollPosition.X + x) * _zoom - 10,
                (_scrollPosition.Y + y) * _zoom - 34);
        }

        private Vector2 GetNpcLocation(int x, int y, int width, int height)
        {
            return new Vector2(
                (_scrollPosition.X + x) * _zoom - 6,
                (_scrollPosition.Y + y) * _zoom - height + 4);
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
                    (_zoom / 16) * (float)_wvm.ActiveTool.PreviewScale,
                    SpriteEffects.None,
                    LayerTools);
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
                    _zoom * (float)_wvm.ActiveTool.PreviewScale,
                    SpriteEffects.None,
                    LayerTools);
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
                destinationRectangle, null,
                 Color.White, 0, Vector2.Zero, SpriteEffects.None,
                 LayerSelection);
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
            if (xnaViewport.GraphicsService == null)
                return;

            var present = xnaViewport.GraphicsService.GraphicsDevice.PresentationParameters;
            present.BackBufferWidth  = (int) xnaViewport.RenderSize.Width;
            present.BackBufferHeight = (int) xnaViewport.RenderSize.Height;
            xnaViewport.GraphicsService.GraphicsDevice.Reset(present);
        }

        #endregion

        #region Mouse

        private TileMouseState GetTileMouseState(HwndMouseEventArgs e)
        {
            return TileMouseState.FromHwndMouseEventArgs(e,
                                                         new Vector2Int32(
                                                             (int)MathHelper.Clamp((float)(e.Position.X / _dpiScale.X / _zoom - _scrollPosition.X), 0, _wvm.CurrentWorld.TilesWide - 1),
                                                             (int)MathHelper.Clamp((float)(e.Position.Y / _dpiScale.Y / _zoom - _scrollPosition.Y), 0, _wvm.CurrentWorld.TilesHigh - 1)));
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

        public void ZoomFocus(int x, int y)
        {
            _zoom = 8;
            CenterOnTile(x, y);

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
