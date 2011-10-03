using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.Tools.History;

namespace TEdit.Tools.Tool
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 5)]
    public class FloodFill : ToolBase
    {
        [Import]
        private ToolProperties _properties;

        [Import("World", typeof(World))]
        private World _world;

        [Import]
        private WorldRenderer _renderer;
        [Import]
        private SelectionArea _selection;

        [Import]
        private TilePicker _tilePicker;

        public FloodFill()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paintcan.png"));
            _Name = "Flood Fill";
            _Type = ToolType.Pencil;
            IsActive = false;
        }

        #region Properties

        private readonly BitmapImage _Image;
        private readonly string _Name;

        private readonly ToolType _Type;
        private bool _IsActive;

        public override string Name
        {
            get { return _Name; }
        }

        public override ToolType Type
        {
            get { return _Type; }
        }

        public override BitmapImage Image
        {
            get { return _Image; }
        }

        public override bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
                if (_IsActive)
                {
                    _properties.MinHeight = 1;
                    _properties.MinWidth = 1;
                    _properties.MaxHeight = 1;
                    _properties.MaxWidth = 1;
                }
            }
        }

        #endregion

        [Import]
        private HistoryManager HistMan;

        public override bool PressTool(TileMouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Flood(e.Tile);

                _renderer.UpdateWorldImage(new Int32Rect(minX, minY, maxX - minX + 1, maxY - minY + 1));
            }
            HistMan.AddBufferToHistory();
            return true;
        }


        public override bool MoveTool(TileMouseEventArgs e)
        {
            return false;
        }



        public override bool ReleaseTool(TileMouseEventArgs e)
        {


            return false;
        }

        public override WriteableBitmap PreviewTool()
        {
            return new WriteableBitmap(
                1,
                1,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        }



        FloodFillRangeQueue ranges = new FloodFillRangeQueue();
        private bool[] tilesChecked;
        private int minX, minY, maxX, maxY;
        public void Flood(PointInt32 pt)
        {
            int bitmapWidth = _world.Header.MaxTiles.X;
            int bitmapHeight = _world.Header.MaxTiles.Y;

            int x = pt.X;
            int y = pt.Y;
            tilesChecked = new bool[bitmapWidth * bitmapHeight];

            var originTile = (Tile)_world.Tiles[x, y].Clone();
            LinearFloodFill(ref x, ref y, ref _tilePicker, ref originTile);

            while (ranges.Count > 0)
            {
                //**Get Next Range Off the Queue
                FloodFillRange range = ranges.Dequeue();

                //**Check Above and Below Each Pixel in the Floodfill Range
                int downPxIdx = (bitmapWidth * (range.Y + 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc,y+1);
                int upPxIdx = (bitmapWidth * (range.Y - 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc, y - 1);
                int upY = range.Y - 1;//so we can pass the y coord by ref
                int downY = range.Y + 1;
                for (int i = range.StartX; i <= range.EndX; i++)
                {
                    //*Start Fill Upwards
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    if (range.Y > 0 && (!tilesChecked[upPxIdx]) && CheckTileMatch(ref originTile, ref _world.Tiles[i, upY], ref _tilePicker) && _selection.IsValid(new PointInt32(i, upY)))
                        LinearFloodFill(ref i, ref upY, ref _tilePicker, ref originTile);

                    //*Start Fill Downwards
                    //if we're not below the bottom of the bitmap and the pixel below this one is within the color tolerance
                    if (range.Y < (bitmapHeight - 1) && (!tilesChecked[downPxIdx]) && CheckTileMatch(ref originTile, ref _world.Tiles[i, downY], ref _tilePicker) && _selection.IsValid(new PointInt32(i, downY)))
                        LinearFloodFill(ref i, ref downY, ref _tilePicker, ref originTile);
                    downPxIdx++;
                    upPxIdx++;
                }

                if (upY < minY)
                    minY = upY;
                if (downY > maxY)
                    maxY = downY;
            }
        }

        private static bool CheckTileMatch(ref Tile originTile, ref Tile nextTile, ref TilePicker tp)
        {
            if ((tp.Tile.IsActive) && (originTile.Type != nextTile.Type))
                return false;
            if ((tp.Tile.IsActive) && (originTile.IsActive != nextTile.IsActive))
                return false;

            if ((tp.Tile.IsActive) && (originTile.IsActive != nextTile.IsActive))
                return false;

            if ((tp.Wall.IsActive) && (originTile.Wall != nextTile.Wall))
                return false;

            if (tp.Liquid.IsActive)
            {
                if ((originTile.Liquid > 0) != (nextTile.Liquid > 0))
                    return false;

                if (originTile.IsLava != nextTile.IsLava)
                    return false;

                if (originTile.Type != nextTile.Type)
                    return false;

                if (originTile.IsActive != nextTile.IsActive)
                    return false;
            }

            return true;
        }

        private void LinearFloodFill(ref int x, ref int y, ref TilePicker tp, ref Tile originTile)
        {
            int bitmapWidth = _world.Header.MaxTiles.X;
            int bitmapHeight = _world.Header.MaxTiles.Y;

            //FIND LEFT EDGE OF COLOR AREA
            int lFillLoc = x; //the location to check/fill on the left
            int tileIndex = (bitmapWidth * y) + x;
            while (true)
            {
                if (HistMan.SaveHistory)
                    HistMan.AddTileToBuffer(lFillLoc, y, ref _world.Tiles[x, y]);
                _world.SetTileXY(ref lFillLoc, ref y, ref tp, ref _selection);
                tilesChecked[tileIndex] = true;

                lFillLoc--;
                tileIndex--;
                if (lFillLoc <= 0 || tilesChecked[tileIndex] || !CheckTileMatch(ref originTile, ref _world.Tiles[lFillLoc, y], ref tp) || !_selection.IsValid(new PointInt32(lFillLoc, y)))
                    break;			 	 //exit loop if we're at edge of bitmap or color area

            }
            lFillLoc++;
            if (lFillLoc < minX)
                minX = lFillLoc;
            //FIND RIGHT EDGE OF COLOR AREA
            int rFillLoc = x; //the location to check/fill on the left
            tileIndex = (bitmapWidth * y) + x;
            while (true)
            {
                if (HistMan.SaveHistory)
                    HistMan.AddTileToBuffer(rFillLoc, y, ref _world.Tiles[x, y]);
                _world.SetTileXY(ref rFillLoc, ref y, ref tp, ref _selection);
                tilesChecked[tileIndex] = true;

                rFillLoc++;
                tileIndex++;
                if (rFillLoc >= bitmapWidth || tilesChecked[tileIndex] || !CheckTileMatch(ref originTile, ref _world.Tiles[rFillLoc, y], ref tp) || !_selection.IsValid(new PointInt32(rFillLoc, y)))
                    break;			 	 //exit loop if we're at edge of bitmap or color area

            }
            rFillLoc--;
            if (rFillLoc > maxX)
                maxX = rFillLoc;

            FloodFillRange r = new FloodFillRange(lFillLoc, rFillLoc, y);
            ranges.Enqueue(ref r);
        }
    }
}