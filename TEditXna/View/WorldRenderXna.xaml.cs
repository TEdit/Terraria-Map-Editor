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
            _textureDictionary = new Textures(_serviceProvider);

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

        /* Heathtech */
        //Pretty much overwrote this whole function.  The original part is still intact, but much more hidden
        private void DrawSprites()
        {
            Rectangle visibleBounds = GetViewingArea();
            TEditXna.Terraria.Objects.BlendRules blendRules = TEditXna.Terraria.Objects.BlendRules.Instance;
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
                        neighborTile[e] = (x + 1) < _wvm.CurrentWorld.TilesWide ? _wvm.CurrentWorld.Tiles[x + 1, y] : null;
                        neighborTile[n] = (y - 1) > 0 ? _wvm.CurrentWorld.Tiles[x, y - 1] : null;
                        neighborTile[w] = (x - 1) > 0 ? _wvm.CurrentWorld.Tiles[x - 1, y] : null;
                        neighborTile[s] = (y + 1) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x, y + 1] : null;
                        neighborTile[ne] = (x + 1) < _wvm.CurrentWorld.TilesWide && (y - 1) > 0 ? _wvm.CurrentWorld.Tiles[x + 1, y - 1] : null;
                        neighborTile[nw] = (x - 1) > 0 && (y - 1) > 0 ? _wvm.CurrentWorld.Tiles[x - 1, y - 1] : null;
                        neighborTile[sw] = (x - 1) > 0 && (y + 1) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x - 1, y + 1] : null;
                        neighborTile[se] = (x + 1) < _wvm.CurrentWorld.TilesWide && (y + 1) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x + 1, y + 1] : null;

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
                                        sameStyle |= (neighborTile[e] != null && neighborTile[e].Wall == curtile.Wall) ? 0x0001 : 0x0000;
                                        sameStyle |= (neighborTile[n] != null && neighborTile[n].Wall == curtile.Wall) ? 0x0010 : 0x0000;
                                        sameStyle |= (neighborTile[w] != null && neighborTile[w].Wall == curtile.Wall) ? 0x0100 : 0x0000;
                                        sameStyle |= (neighborTile[s] != null && neighborTile[s].Wall == curtile.Wall) ? 0x1000 : 0x0000;
                                        Vector2Int32 uvBlend = blendRules.GetUVForMasks((uint)sameStyle, 0x00000000, 0);
                                        curtile.uvWallCache = (ushort)((uvBlend.Y << 8) + uvBlend.X);
                                    }

                                    var texsize = new Vector2Int32(32, 32);
                                    var source = new Rectangle((curtile.uvWallCache & 0x00FF) * (texsize.X + 4), (curtile.uvWallCache >> 8) * (texsize.Y + 4), texsize.X, texsize.Y);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x - 0.5) * _zoom), 1 + (int)((_scrollPosition.Y + y - 0.5) * _zoom), (int)_zoom * 2, (int)_zoom * 2);

                                    _spriteBatch.Draw(wallTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, 1);
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

                                    bool isTree = false, isMushroom = false;
                                    bool isLeft = false, isBase = false, isRight = false;
                                    if (curtile.Type == 5 && curtile.U >= 22 && curtile.V >= 198)
                                    {
                                        isTree = true;
                                        switch (curtile.U)
                                        {
                                            case 22: isBase = true; break;
                                            case 44: isLeft = true; break;
                                            case 66: isRight = true; break;
                                        }
                                        //Abuse uvTileCache to remember what type of tree it is, since potentially scanning a hundred of blocks PER tree tile sounds slow
                                        int treeType = (curtile.uvTileCache & 0x000F);

                                        if (treeType > 4) //Tree type not yet set
                                        {
                                            //Check tree type
                                            treeType = 0; //Default to normal in case no grass grows beneath the tree
                                            int baseX = (isLeft) ? 1 : (isRight) ? -1 : 0;
                                            for (int i = 0; i < 100; i++)
                                            {
                                                Tile checkTile = (y + i) < _wvm.CurrentWorld.TilesHigh ? _wvm.CurrentWorld.Tiles[x + baseX, y + i] : null;
                                                bool found = true;
                                                if (checkTile != null && checkTile.IsActive)
                                                {
                                                    switch (checkTile.Type)
                                                    {
                                                        case 2: treeType = 0; break; //Normal
                                                        case 23: treeType = 1; break; //Corruption
                                                        case 60: treeType = 2; break; //Jungle
                                                        case 109: treeType = 3; break; //Hallow
                                                        case 147: treeType = 4; break; //Snow
                                                        default: found = false; break;
                                                    }
                                                    if (found == true)
                                                    {
                                                        curtile.uvTileCache = (ushort)((0x00 << 8) + 0x01 * treeType);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (isBase)
                                        {
                                            tileTex = (Texture2D)_textureDictionary.GetTreeTops(treeType);
                                        }
                                        else
                                        {
                                            tileTex = (Texture2D)_textureDictionary.GetTreeBranches(treeType);
                                        }
                                    }
                                    if (curtile.Type == 72 && curtile.U >= 36)
                                    {
                                        isMushroom = true;
                                        tileTex = (Texture2D)_textureDictionary.GetShroomTop(0);
                                    }

                                    if (tileTex != null)
                                    {
                                        if (!isTree && !isMushroom)
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
                                        else if (isTree)
                                        {
                                            source = new Rectangle(0, 0, 40, 40);
                                            dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                            FrameAnchor frameAnchor = FrameAnchor.None;

                                            int treeType = (curtile.uvTileCache & 0x000F);
                                            if (isBase)
                                            {
                                                switch (treeType)
                                                {
                                                    case 0:
                                                    case 1:
                                                    case 4:
                                                        source.Width = 80;
                                                        source.Height = 80;
                                                        break;
                                                    case 2:
                                                        source.Width = 114;
                                                        source.Height = 96;
                                                        break;
                                                    case 3:
                                                        source.X = (x % 3) * (82 * 3);
                                                        source.Width = 80;
                                                        source.Height = 140;
                                                        break;
                                                }
                                                source.X += ((curtile.V - 198) / 22) * (source.Width + 2);
                                                frameAnchor = FrameAnchor.Bottom;
                                            }
                                            else if (isLeft)
                                            {
                                                source.X = 0;
                                                switch (treeType)
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
                                                switch (treeType)
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

                                        _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, 0);
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

                                        _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, 0);
                                    }
                                }
                                else if (tileprop.IsCactus)
                                {

                                    var tileTex = _textureDictionary.GetTile(curtile.Type);

                                    if ((curtile.uvTileCache & 0x00FF) >= 16)
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
                                                if (checkTile != null && checkTile.IsActive && checkTile.Type == 112) //Corruption
                                                {
                                                    uv.X += 16;
                                                    break;
                                                }
                                                else if (checkTile != null && checkTile.IsActive && checkTile.Type == 116) //Hallow
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

                                        _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, 0);
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

                                        _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, 0);
                                    }
                                }
                            }
                        }
                        if (_wvm.ShowWires)
                        {
                            if (curtile.HasWire)
                            {
                                var tileTex = (Texture2D)_textureDictionary.GetMisc("Wires");

                                if (tileTex != null)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);

                                    byte state = 0x00;
                                    state |= (byte)((neighborTile[e] != null && neighborTile[e].HasWire == true) ? 0x01 : 0x00);
                                    state |= (byte)((neighborTile[n] != null && neighborTile[n].HasWire == true) ? 0x02 : 0x00);
                                    state |= (byte)((neighborTile[w] != null && neighborTile[w].HasWire == true) ? 0x04 : 0x00);
                                    state |= (byte)((neighborTile[s] != null && neighborTile[s].HasWire == true) ? 0x08 : 0x00);
                                    Vector2Int32 uv = new Vector2Int32(0, 0);
                                    switch (state)
                                    {
                                        case 0x00: uv.X = 0; uv.Y = 3; break;
                                        case 0x01: uv.X = 4; uv.Y = 2; break;
                                        case 0x02: uv.X = 2; uv.Y = 2; break;
                                        case 0x03: uv.X = 2; uv.Y = 1; break;
                                        case 0x04: uv.X = 3; uv.Y = 2; break;
                                        case 0x05: uv.X = 1; uv.Y = 0; break;
                                        case 0x06: uv.X = 3; uv.Y = 1; break;
                                        case 0x07: uv.X = 0; uv.Y = 1; break;
                                        case 0x08: uv.X = 1; uv.Y = 2; break;
                                        case 0x09: uv.X = 0; uv.Y = 2; break;
                                        case 0x0A: uv.X = 0; uv.Y = 0; break;
                                        case 0x0B: uv.X = 2; uv.Y = 0; break;
                                        case 0x0C: uv.X = 4; uv.Y = 1; break;
                                        case 0x0D: uv.X = 4; uv.Y = 0; break;
                                        case 0x0E: uv.X = 3; uv.Y = 0; break;
                                        case 0x0F: uv.X = 1; uv.Y = 1; break;
                                    }
                                    source.X = uv.X * (source.Width + 2);
                                    source.Y = uv.Y * (source.Height + 2);

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White, 0f, default(Vector2), SpriteEffects.None, 0);
                                }
                            }
                        }
                        if (_wvm.ShowLiquid)
                        {
                            if (curtile.Liquid > 0)
                            {
                                Texture2D tileTex = null;
                                if (curtile.IsLava == false)
                                {
                                    tileTex = (Texture2D)_textureDictionary.GetLiquid(0);
                                }
                                else
                                {
                                    tileTex = (Texture2D)_textureDictionary.GetLiquid(1);
                                }

                                if (tileTex != null)
                                {
                                    var source = new Rectangle(0, 0, 16, 16);
                                    var dest = new Rectangle(1 + (int)((_scrollPosition.X + x) * _zoom), 1 + (int)((_scrollPosition.Y + y) * _zoom), (int)_zoom, (int)_zoom);
                                    float alpha = 1f;

                                    if (curtile.IsLava == false)
                                    {
                                        alpha = 0.5f;
                                    }
                                    else
                                    {
                                        alpha = 0.85f;
                                    }

                                    if (neighborTile[n] != null && neighborTile[n].Liquid > 0)
                                    {
                                        source.Y = 8;
                                        source.Height = 8;
                                    }
                                    else
                                    {
                                        source.Height = 4 + ((int)Math.Round(curtile.Liquid * 6f / 255f)) * 2;
                                        dest.Height = (int)(source.Height * _zoom / 16f);
                                        dest.Y = 1 + (int)((_scrollPosition.Y + y) * _zoom + ((16 - source.Height) * _zoom / 16f));
                                    }

                                    _spriteBatch.Draw(tileTex, dest, source, Color.White * alpha, 0f, default(Vector2), SpriteEffects.None, 0);
                                }
                            }
                        }
                    }
                }
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
                    1);
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