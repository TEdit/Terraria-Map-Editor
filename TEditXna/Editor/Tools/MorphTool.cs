using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry;
using BCCL.Geometry.Primitives;
using TEditXna.ViewModel;
using System.Linq;
using TEditXna.Terraria.Objects;

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
                Dirt = 0,
                Stone = 1,
                Grass = 2,
                Plant = 73,
                Tree = 5,
                Sand = 53,
                Silt = 40,
                Wall = 2,
                Vines = 52,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Underground,
                Dirt = 0,
                Stone = 1,
                Grass = 2,
                Plant = 73,
                Tree = 5,
                Sand = 53,
                Silt = 123,
                Wall = 0,
                Vines = 52,
                DirtToSand = false,
                DirtToStone = true,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Jungle,
                Dirt = 59,
                Stone = 1,
                Grass = 60,
                Plant = 61,
                Tree = 5,
                Sand = 53,
                Silt = 123,
                Wall = 15,
                Vines = 62,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Underground_Jungle,
                Dirt = 59,
                Stone = 1,
                Grass = 60,
                Plant = 61,
                Tree = 5,
                Sand = 53,
                Silt = 123,
                Wall = 15,
                Vines = 62,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = true,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Corruption,
                Dirt = 0,
                Stone = 25,
                Grass = 23,
                Plant = 24,
                Tree = 5,
                Sand = 112,
                Silt = 123,
                Wall = 3,
                Vines = 32,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Hallowed,
                Dirt = 0,
                Stone = 117,
                Grass = 109,
                Plant = 110,
                Tree = 5,
                Sand = 116,
                Silt = 123,
                Wall = 28,
                Vines = 52,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Crimson,
                Dirt = 0,
                Stone = 203,
                Grass = 199,
                Plant = 201,
                Tree = 5,
                Sand = 112,
                Silt = 123,
                Wall = 83,
                Vines = 32,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Snow,
                Dirt = 147,
                Stone = 161,
                Grass = 147,
                Plant = 3,
                Tree = 5,
                Sand = 147,
                Silt = 224,
                Wall = 40,
                Vines = 52,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Ice,
                Dirt = 147,
                Stone = 161,
                Grass = 147,
                Plant = 185,
                Tree = 5,
                Sand = 147,
                Silt = 224,
                Wall = 71,
                Vines = 165,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Desert,
                Dirt = 0,
                Stone = 1,
                Grass = 53,
                Plant = 83,
                Tree = 80,
                Sand = 53,
                Wall = 0,
                Vines = 52,
                DirtToSand = true,
                DirtToStone = false,
                SandToDirt = false,
                StoneToDirt = false,
            });

            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Corrupt_Desert,
                Dirt = 112,
                Stone = 25,
                Grass = 112,
                Plant = 3,
                Tree = 80,
                Sand = 112,
                Wall = 3,
                Vines = 32,
                DirtToSand = true,
                DirtToStone = false,
                SandToDirt = false,
                StoneToDirt = false,
            });
            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Hallowed_Desert,
                Dirt = 0,
                Stone = 117,
                Grass = 109,
                Plant = 3,
                Tree = 80,
                Sand = 116,
                Wall = 28,
                Vines = 52,
                DirtToSand = true,
                DirtToStone = false,
                SandToDirt = false,
                StoneToDirt = false,
            });
            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Crimson_Desert,
                Dirt = 0,
                Stone = 203,
                Grass = 109,
                Plant = 201,
                Tree = 80,
                Sand = 234,
                Wall = 83,
                Vines = 52,
                DirtToSand = true,
                DirtToStone = false,
                SandToDirt = false,
                StoneToDirt = false,
            });
            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Hell,
                Dirt = 0,
                Stone = 57,
                Grass = 0,
                Plant = 82,
                Tree = 0,
                Sand = 57,
                Wall = 0,
                Vines = 0,
                DirtToSand = false,
                DirtToStone = true,
                SandToDirt = false,
                StoneToDirt = false,
            });
            Biomes.Add(new BiomeData
            {
                Biome = MorphBiome.Mushroom,
                Dirt = 59,
                Stone = 1,
                Grass = 70,
                Plant = 71,
                Tree = 72,
                Sand = 57,
                Wall = 0,
                Vines = 51,
                DirtToSand = false,
                DirtToStone = false,
                SandToDirt = true,
                StoneToDirt = true,
            });
        }

        public override void MouseDown(TileMouseState e)
        {
            if (!_isRightDown && !_isLeftDown)
            {
                _currentBiome = Biomes.FirstOrDefault(b=>b.Biome== _wvm.MorphBiomeTarget);
                _startPoint = e.Location;
                _dirtLayer = (int)_wvm.CurrentWorld.GroundLevel;
                _rockLayer = (int)_wvm.CurrentWorld.RockLevel;
                _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
            }

            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            CheckDirectionandDraw(e.Location);
        }

        public override void MouseMove(TileMouseState e)
        {
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
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

                        /* Heathtech */
                        BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                    }
                }
            }
        }

        private void MorphTile(Vector2Int32 p)
        {
            var curtile = _wvm.CurrentWorld.Tiles[p.X, p.Y];

            if (curtile.IsActive)
            {
                    foreach (var biome in Biomes)
                    {
                        if ((curtile.Type == biome.Stone || ((curtile.Type == biome.Dirt || curtile.Type == biome.Grass) && _currentBiome.DirtToStone)) && !_currentBiome.StoneToDirt)
                             curtile.Type = _currentBiome.Stone;
                        else if ((curtile.Type == biome.Dirt || (curtile.Type == biome.Sand && _currentBiome.SandToDirt) || (curtile.Type == biome.Stone && _currentBiome.StoneToDirt)) && !_currentBiome.DirtToSand)
                            if (BordersAir(p))
                                curtile.Type = _currentBiome.Grass;
                            else
                                curtile.Type = _currentBiome.Dirt;
                        else if (curtile.Type == biome.Grass)
                            curtile.Type = _currentBiome.Grass;
                        else if (curtile.Type == biome.Sand || ((curtile.Type == biome.Dirt || curtile.Type == biome.Grass) && _currentBiome.DirtToSand))
                            curtile.Type = _currentBiome.Sand;
                        else if (curtile.Type == biome.Vines)
                            curtile.Type = _currentBiome.Vines;
                        else if (curtile.Type == biome.Plant)
                        {
                            curtile.Type = _currentBiome.Plant;
                        }
                        else if (curtile.Type == biome.Tree)
                        {
                            curtile.Type = _currentBiome.Tree;
                        }
                    }

            }
            if (curtile.Wall == 1 || curtile.Wall == 2 || curtile.Wall == 3 || curtile.Wall == 15 || curtile.Wall == 16 || curtile.Wall == 28)
            {
                curtile.Wall = _currentBiome.Wall;
            }
        }

        private bool BordersAir(Vector2Int32 p)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (!_wvm.CurrentWorld.Tiles[p.X + x, p.Y + y].IsActive)
                        return true;
                }
            }
            return false;
        }

        private class BiomeData
        {
            public MorphBiome Biome;
            public ushort Tree;
            public ushort Plant;
            public ushort Stone;
            public ushort Grass;
            public ushort Sand;
            public ushort Silt;
            public byte Wall;
            public ushort Vines;
            public ushort Dirt;
            public bool DirtToSand;
            public bool DirtToStone;
            public bool SandToDirt;
            public bool StoneToDirt;
        }
    }
}