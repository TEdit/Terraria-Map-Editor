using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry;
using BCCL.Geometry.Primitives;
using TEditXna.ViewModel;
using System.Linq;

namespace TEditXna.Editor.Tools
{
    public sealed class MorphTool : BaseTool
    {

        private bool _isLeftDown;
        private bool _isRightDown;
        private Vector2Int32 _startPoint;
        private int _dirtLayer;
        private int _rockLayer;
        private List<BiomeData> Biomes = new List<BiomeData>();
        private BiomeData _currentBiome;

        public MorphTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            CreateTileLookup();
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paintbrush.png"));
            Name = "Morph";
            ToolType = ToolType.Brush;
        }

        private void CreateTileLookup()
        {
            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Grass,
                DirtToStone = false,
                Stone = 1,
                Grass = 2,
                Plant1 = 3,
                Plant2 = 73,
                Sand = 53,
                Wall = 0,
                Vines = 52
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Corruption,
                DirtToStone = false,
                Stone = 25,
                Grass = 23,
                Plant1 = 24,
                Plant2 = 73,
                Sand = 112,
                Wall = 3,
                Vines = 32
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Jungle,
                DirtToStone = true,
                Stone = 59,
                Grass = 60,
                Plant1 = 61,
                Plant2 = 74,
                Sand = 53,
                Wall = 15,
                Vines = 62
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Hallowed,
                DirtToStone = false,
                Stone = 117,
                Grass = 109,
                Plant1 = 110,
                Plant2 = 113,
                Sand = 116,
                Wall = 0,
                Vines = 52
            });
        }

        public override void MouseDown(TileMouseState e)
        {
            if (!_isRightDown && !_isLeftDown)
            {
                _startPoint = e.Location;
                _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
                _dirtLayer = (int)_wvm.CurrentWorld.GroundLevel;
                _rockLayer = (int)_wvm.CurrentWorld.RockLevel;
                _currentBiome = Biomes.FirstOrDefault(b => b.Biome == _wvm.MorphBiomeTarget);
            }

            CheckDirectionandDraw(e.Location);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
        }

        public override void MouseMove(TileMouseState e)
        {
            CheckDirectionandDraw(e.Location);
        }

        public override void MouseUp(TileMouseState e)
        {
            CheckDirectionandDraw(e.Location);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            _wvm.UndoManager.SaveUndo();
        }

        public override WriteableBitmap PreviewTool()
        {
            var bmp = new WriteableBitmap(_wvm.Brush.Width + 1, _wvm.Brush.Height + 1, 96, 96, PixelFormats.Bgra32, null);

            bmp.Clear();
            if (_wvm.Brush.Shape == BrushShape.Square)
                bmp.FillRectangle(0, 0, _wvm.Brush.Width, _wvm.Brush.Height, Color.FromArgb(127, 0, 90, 255));
            else
                bmp.FillEllipse(0, 0, _wvm.Brush.Width, _wvm.Brush.Height, Color.FromArgb(127, 0, 90, 255));

            _preview = bmp;
            return _preview;
        }

        private void CheckDirectionandDraw(Vector2Int32 tile)
        {
            if (_currentBiome != null)
            {
                Vector2Int32 p = tile;
                if (_isRightDown)
                {
                    if (_isLeftDown)
                        p.X = _startPoint.X;
                    else
                        p.Y = _startPoint.Y;

                    DrawLine(p);
                    _startPoint = p;
                }
                else if (_isLeftDown)
                {
                    DrawLine(p);
                    _startPoint = p;
                }
            }
        }

        private void DrawLine(Vector2Int32 to)
        {
            IEnumerable<Vector2Int32> area;
            foreach (Vector2Int32 point in Shape.DrawLineTool(_startPoint, to))
            {
                if (_wvm.Brush.Shape == BrushShape.Round)
                {
                    area = Fill.FillEllipseCentered(point, new Vector2Int32(_wvm.Brush.Width / 2, _wvm.Brush.Height / 2));
                    FillSolid(area);
                }
                else if (_wvm.Brush.Shape == BrushShape.Square)
                {
                    area = Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height));
                    FillSolid(area);
                }
            }
        }

        private void FillSolid(IEnumerable<Vector2Int32> area)
        {
            foreach (Vector2Int32 pixel in area)
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;
                if (!_wvm.CheckTiles[index])
                {
                    _wvm.CheckTiles[index] = true;
                    if (_wvm.Selection.IsValid(pixel))
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        MorphTile(pixel);
                        _wvm.UpdateRenderPixel(pixel);
                    }
                }
            }
        }

        private void MorphTile(Vector2Int32 p)
        {
            var curtile = _wvm.CurrentWorld.Tiles[p.X, p.Y];

            if (curtile.IsActive)
            {
                if (curtile.Type == 0 && _currentBiome.DirtToStone)
                    curtile.Type = _currentBiome.Stone;
                else
                {
                    foreach (var biome in Biomes)
                    {
                        if (curtile.Type == biome.Stone)
                        {
                            if (!_currentBiome.DirtToStone && (p.Y < _dirtLayer))
                                curtile.Type = 0;
                            else
                                curtile.Type = _currentBiome.Stone;
                        }
                        else if (curtile.Type == biome.Grass)
                            curtile.Type = _currentBiome.Grass;
                        else if (curtile.Type == biome.Sand)
                            curtile.Type = _currentBiome.Sand;
                        else if (curtile.Type == biome.Stone)
                            curtile.Type = _currentBiome.Stone;
                        else if (curtile.Type == biome.Vines)
                            curtile.Type = _currentBiome.Vines;
                        else if (curtile.Type == biome.Plant1 || curtile.Type == biome.Plant2)
                            curtile.IsActive = false;
                    }
                }

                if (curtile.Type == 0 || (curtile.Type == _currentBiome.Stone && _currentBiome.DirtToStone))
                {
                    if (BordersAir(p))
                    {
                        curtile.Type = _currentBiome.Grass;
                    }
                }
            }

            if (p.Y > _dirtLayer && p.Y < _wvm.CurrentWorld.TilesHigh - 182)
            {
                if (Biomes.Any(x => x.Wall == curtile.Wall) || curtile.Wall == 2)
                {
                    //if (p.Y < _dirtLayer && _currentBiome.Wall == 0)
                    //    curtile.Wall = 2;
                    //else
                    curtile.Wall = _currentBiome.Wall;
                }
            }
            //else if (p.Y < _dirtLayer && _currentBiome.Wall == 0) 
        }

        private bool BordersAir(Vector2Int32 p)
        {
            int x1 = p.X - 1;
            if (x1 < 0) x1 = 0;
            int x2 = p.X + 1;
            if (x2 >= _wvm.CurrentWorld.TilesWide) x2 = p.X;
            int y1 = p.Y - 1;
            if (y1 < 0) y1 = 0;
            int y2 = p.Y + 2;
            if (y2 >= _wvm.CurrentWorld.TilesHigh) y2 = p.X;

            if (!_wvm.CurrentWorld.Tiles[p.X, y1].IsActive || !_wvm.CurrentWorld.Tiles[p.X, y2].IsActive ||
                !_wvm.CurrentWorld.Tiles[x1, p.Y].IsActive || !_wvm.CurrentWorld.Tiles[x2, p.Y].IsActive)
                return true;
            return false;
        }

        private class BiomeData
        {
            public MorphBiome Biome;
            public byte Plant1;
            public byte Plant2;
            public byte Stone;
            public byte Grass;
            public byte Sand;
            public byte Wall;
            public byte Vines;
            public bool DirtToStone;
        }
    }
}