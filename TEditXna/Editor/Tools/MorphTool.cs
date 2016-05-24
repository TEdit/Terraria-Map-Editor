using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.Geometry.Primitives;
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
        private Vector2Int32 _endPoint;
        private int _dirtLayer;
        private int _rockLayer;
        private int _morphtype;
        private int[] _wallGrass = {63, 64, 65, 66, 67, 68, 69, 70, 81};
        private int[] _wallStone = {1, 3, 83, 28};
        private int[] _wallHardenedSand = {216, 217, 218, 219};
        private int[] _wallSandstone = {187, 220, 221, 222};
        private int[] _tileStone = {1, 25, 203, 117};
        private int[] _tileGrass = {2, 23, 199, 109};
        private int[] _tileIce = {161, 163, 200, 164};
        private int[] _tileSand = {53, 112, 234, 116};
        private int[] _tileHardenedSand = {397, 398, 399, 402};
        private int[] _tileSandstone = {396, 400, 401, 403};
        private int[] _tileMoss = {182, 180, 179, 381, 183, 181};
        private int[] _tileGrassSprite = {3, 23, 201, 110};

        public MorphTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paintbrush.png"));
            Name = "Morph";
            ToolType = ToolType.Brush;
        }

        public override void MouseDown(TileMouseState e)
        {
            if (!_isRightDown && !_isLeftDown)
            {
                _morphtype = (int)_wvm.MorphBiomeTarget;
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
                Vector2Int32 p = tile;
                Vector2Int32 p2 = tile;
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
                    if ((Keyboard.IsKeyUp(Key.LeftShift)) && (Keyboard.IsKeyUp(Key.RightShift)))
                    {
                        DrawLine(p);
                        _startPoint = p;
                        _endPoint = p;
                    }
                    else if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
                    {
                        DrawLineP2P(p2);
                        _endPoint = p2;
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

        private void DrawLineP2P(Vector2Int32 endPoint)
        {
            IEnumerable<Vector2Int32> area;
            foreach (Vector2Int32 point in Shape.DrawLineTool(_startPoint, _endPoint))
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
            
            if (_wallGrass.Contains(curtile.Wall))
            {
                if (_morphtype != 0)
                {
                    switch(_morphtype)
                    {
                        case 1:
                            curtile.Wall = 69;
                            break;
                        case 2:
                            curtile.Wall = 81;
                            break;
                        case 3:
                            curtile.Wall = 70;
                            break;
                    }
                }
                else if (curtile.Wall == 69 || curtile.Wall == 70 || curtile.Wall == 81)
                {
                    if (p.Y < _dirtLayer)
                        curtile.Wall = 63;
                    else
                        curtile.Wall = 64;
                }
            }
            if (_wallStone.Contains(curtile.Wall))
                curtile.Wall = (byte)_wallStone[_morphtype];
            if (_wallHardenedSand.Contains(curtile.Wall))
                curtile.Wall = (byte)_wallHardenedSand[_morphtype];
            if (_wallSandstone.Contains(curtile.Wall))
                curtile.Wall = (byte)_wallSandstone[_morphtype];
            if (curtile.IsActive)
            {
                if (_tileStone.Contains(curtile.Type) || _tileMoss.Contains(curtile.Type))
                {
                    if (_morphtype != 0)
                        curtile.Type = (ushort)_tileStone[_morphtype];
                    else if (curtile.Type == 25 || curtile.Type == 203 || curtile.Type == 117)
                        curtile.Type = 1;
                }
                else if (_tileGrass.Contains(curtile.Type))
                    curtile.Type = (ushort)_tileGrass[_morphtype];
                else if (_tileIce.Contains(curtile.Type))
                    curtile.Type = (ushort)_tileIce[_morphtype];
                else if (_tileSand.Contains(curtile.Type))
                    curtile.Type = (ushort)_tileSand[_morphtype];
                else if (_tileHardenedSand.Contains(curtile.Type))
                    curtile.Type = (ushort)_tileHardenedSand[_morphtype];
                else if (_tileSandstone.Contains(curtile.Type))
                    curtile.Type = (ushort)_tileSandstone[_morphtype];
                else if (curtile.Type == 32 || curtile.Type == 352)
                {
                    switch(_morphtype)
                    {
                        case 0:
                        case 3:
                            curtile.Type = 0;
                            curtile.IsActive = false;
                            break;
                        case 1:
                            curtile.Type = 32;
                            break;
                        case 2:
                            curtile.Type = 352;
                            break;
                    }
                }
                else if (curtile.Type == 52 || curtile.Type == 115 || curtile.Type == 205)
                {
                    switch(_morphtype)
                    {
                        case 0:
                            curtile.Type = 52;
                            break;
                        case 1:
                            curtile.Type = 0;
                            curtile.IsActive = false;
                            break;
                        case 2:
                            curtile.Type = 205;
                            break;
                        case 3:
                            curtile.Type = 115;
                            break;
                    }
                }
                else if (_tileGrassSprite.Contains(curtile.Type))
                    curtile.Type = (ushort)_tileGrassSprite[_morphtype];
                else if ((curtile.Type == 73 && _morphtype != 0) || ((curtile.Type == 113) && _morphtype != 3))
                {
                    curtile.Type = 0;
                    curtile.IsActive = false;
                }
                else if (curtile.Type == 165)
                {
                    if (54 <= curtile.U && curtile.U <= 90)
                        switch(_morphtype)
                        {
                            case 1:
                                curtile.U += 216;
                                break;
                            case 2:
                                curtile.U += 270;
                                break;
                            case 3:
                                curtile.U += 162;
                                break;
                        }
                    else if (216 <= curtile.U && curtile.U <= 252)
                        switch(_morphtype)
                        {
                            case 0:
                                curtile.U -= 162;
                                break;
                            case 1:
                                curtile.U += 54;
                                break;
                            case 2:
                                curtile.U += 108;
                                break;
                        }
                    else if (270 <= curtile.U && curtile.U <= 306)
                        switch(_morphtype)
                        {
                            case 0:
                                curtile.U -= 216;
                                break;
                            case 2:
                                curtile.U += 54;
                                break;
                            case 3:
                                curtile.U -= 54;
                                break;
                        }
                    else if (324 <= curtile.U && curtile.U <= 362)
                        switch(_morphtype)
                        {
                            case 0:
                                curtile.U -= 270;
                                break;
                            case 1:
                                curtile.U -= 54;
                                break;
                            case 3:
                                curtile.U -= 108;
                                break;
                        }
                }
            }
        }
    }
}
