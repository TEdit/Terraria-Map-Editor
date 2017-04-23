using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using TEditXNA.Terraria;
using TEditXna.ViewModel;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Tools
{
    public sealed class FillTool : BaseTool
    {
        private readonly FloodFillRangeQueue _ranges = new FloodFillRangeQueue();
        private int _minX = 0;
        private int _minY = 0;
        private int _maxX = 0;
        private int _maxY = 0;

        public FillTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paintcan.png"));
            Name = "Fill";
            ToolType = ToolType.Pixel;
        }

        public override void MouseDown(TileMouseState e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Flood(e.Location);
                //_wvm.UpdateRenderRegion(new Rectangle(_minX, _minY, _maxX - _minX + 1, _maxY - _minY + 1));
                _wvm.UndoManager.SaveUndo();
            }
        }

        public void Flood(Vector2Int32 pt)
        {
            int bitmapWidth = _wvm.CurrentWorld.TilesWide;
            int bitmapHeight = _wvm.CurrentWorld.TilesHigh;

            int x = pt.X;
            int y = pt.Y;
            _wvm.CheckTiles = new bool[bitmapWidth * bitmapHeight];

            var originTile = (Tile)_wvm.CurrentWorld.Tiles[x, y].Clone();
            LinearFloodFill(ref x, ref y, ref originTile);

            while (_ranges.Count > 0)
            {
                //**Get Next Range Off the Queue
                FloodFillRange range = _ranges.Dequeue();

                //**Check Above and Below Each Pixel in the Floodfill Range
                int downPxIdx = (bitmapWidth * (range.Y + 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc,y+1);
                int upPxIdx = (bitmapWidth * (range.Y - 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc, y - 1);
                int upY = range.Y - 1;//so we can pass the y coord by ref
                int downY = range.Y + 1;
                for (int i = range.StartX; i <= range.EndX; i++)
                {
                    //*Start Fill Upwards
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    if (range.Y > 0 && (!_wvm.CheckTiles[upPxIdx]) && CheckTileMatch(ref originTile, ref _wvm.CurrentWorld.Tiles[i, upY]) && _wvm.Selection.IsValid(i, upY))
                        LinearFloodFill(ref i, ref upY, ref originTile);

                    //*Start Fill Downwards
                    //if we're not below the bottom of the bitmap and the pixel below this one is within the color tolerance
                    if (range.Y < (bitmapHeight - 1) && (!_wvm.CheckTiles[downPxIdx]) && CheckTileMatch(ref originTile, ref _wvm.CurrentWorld.Tiles[i, downY]) && _wvm.Selection.IsValid(i, downY))
                        LinearFloodFill(ref i, ref downY, ref originTile);
                    downPxIdx++;
                    upPxIdx++;
                }

                if (upY < _minY)
                    _minY = upY;
                if (downY > _maxY)
                    _maxY = downY;
            }
        }

        private bool CheckTileMatch(ref Tile originTile, ref Tile nextTile)
        {
            switch (_wvm.TilePicker.PaintMode)
            {
                case PaintMode.TileAndWall:
                    if ((originTile.Type != nextTile.Type || originTile.IsActive != nextTile.IsActive) && _wvm.TilePicker.TileStyleActive)
                        return false;
                    if (originTile.Wall != nextTile.Wall && _wvm.TilePicker.WallStyleActive)
                        return false;
                    if (originTile.BrickStyle != nextTile.BrickStyle && _wvm.TilePicker.BrickStyleActive)
                        return false;
                    if (_wvm.TilePicker.TilePaintActive && (originTile.Type != nextTile.Type || originTile.IsActive != nextTile.IsActive))
                        return false;
                    if (_wvm.TilePicker.WallPaintActive && (originTile.Wall != nextTile.Wall || (originTile.IsActive && World.TileProperties[originTile.Type].IsSolid) ||
                        (nextTile.IsActive && World.TileProperties[nextTile.Type].IsSolid)))
                        return false;
                    if (_wvm.TilePicker.ExtrasActive)
                        return false;
                    break;
                case PaintMode.Wire:
                    return false;
                case PaintMode.Liquid:
                    if ((originTile.LiquidAmount > 0 != nextTile.LiquidAmount > 0) ||
                        originTile.LiquidType != nextTile.LiquidType ||
                        (originTile.IsActive && World.TileProperties[originTile.Type].IsSolid) ||
                        (nextTile.IsActive && World.TileProperties[nextTile.Type].IsSolid))
                        return false;
                    break;
            }

            return true;
        }

        private void LinearFloodFill(ref int x, ref int y, ref Tile originTile)
        {
            int bitmapWidth = _wvm.CurrentWorld.TilesWide;
            int bitmapHeight = _wvm.CurrentWorld.TilesHigh;

            //FIND LEFT EDGE OF COLOR AREA
            int lFillLoc = x; //the location to check/fill on the left
            int tileIndex = (bitmapWidth * y) + x;
            while (true)
            {
                if (!_wvm.CheckTiles[tileIndex])
                {
                    _wvm.UndoManager.SaveTile(lFillLoc, y);
                    _wvm.SetPixel(lFillLoc, y);
                    _wvm.UpdateRenderPixel(lFillLoc, y);
                    _wvm.CheckTiles[tileIndex] = true;
                }

                lFillLoc--;
                tileIndex--;
                if (lFillLoc <= 0 || _wvm.CheckTiles[tileIndex] || !CheckTileMatch(ref originTile, ref _wvm.CurrentWorld.Tiles[lFillLoc, y]) || !_wvm.Selection.IsValid(lFillLoc, y))
                    break; //exit loop if we're at edge of bitmap or color area

            }
            /* Heathtech */
            BlendRules.ResetUVCache(_wvm, lFillLoc + 1, y, x - lFillLoc, 1);

            lFillLoc++;
            if (lFillLoc < _minX)
                _minX = lFillLoc;
            //FIND RIGHT EDGE OF COLOR AREA
            int rFillLoc = x; //the location to check/fill on the left
            tileIndex = (bitmapWidth * y) + x;
            while (true)
            {
                if (!_wvm.CheckTiles[tileIndex])
                {
                    _wvm.UndoManager.SaveTile(rFillLoc, y);
                    _wvm.SetPixel(rFillLoc, y);
                    _wvm.UpdateRenderPixel(rFillLoc, y);
                    _wvm.CheckTiles[tileIndex] = true;
                    BlendRules.ResetUVCache(_wvm, rFillLoc, y, 1, 1);
                }

                rFillLoc++;
                tileIndex++;
                if (rFillLoc >= bitmapWidth || _wvm.CheckTiles[tileIndex] || !CheckTileMatch(ref originTile, ref _wvm.CurrentWorld.Tiles[rFillLoc, y]) || !_wvm.Selection.IsValid(rFillLoc, y))
                    break; //exit loop if we're at edge of bitmap or color area
            }
            /* Heathtech */
            BlendRules.ResetUVCache(_wvm, x, y, rFillLoc - x, 1);

            rFillLoc--;
            if (rFillLoc > _maxX)
                _maxX = rFillLoc;

            var r = new FloodFillRange(lFillLoc, rFillLoc, y);
            _ranges.Enqueue(ref r);
        }
    }
}