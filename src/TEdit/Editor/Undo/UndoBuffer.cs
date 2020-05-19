using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TEdit.Geometry.Primitives;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Undo
{
    public class UndoBuffer
    {
        private const int FlushSize = 10000;

        private static readonly object UndoSaveLock = new object();

        public UndoBuffer(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Create);
            _writer = new BinaryWriter(stream);
            _writer.Write((Int32)0);
        }

        private readonly List<UndoTile> _undoTiles = new List<UndoTile>();
        private readonly List<Sign> _signs = new List<Sign>();
        private readonly List<Chest> _chests = new List<Chest>();

        public IList<Chest> Chests
        {
            get { return _chests; }
        }

        public IList<Sign> Signs
        {
            get { return _signs; }
        }

        public List<UndoTile> UndoTiles
        {
            get { return _undoTiles; }
        }

        public UndoTile LastTile { get; set; }

        public void Add(Vector2Int32 location, Tile tile)
        {
            var undoTile = new UndoTile(location, tile);

            if (undoTile == null)
            {
                throw new Exception("Null undo?");
            }

            lock (UndoSaveLock)
            {
                UndoTiles.Add(undoTile);
                LastTile = undoTile;
            }
            if (UndoTiles.Count > FlushSize)
            {
                Flush();
            }
        }

        public int Count { get; private set; }

        public void Flush()
        {
            lock (UndoSaveLock)
            {
                int count = _undoTiles.Count;
                var tiles = UndoTiles.ToArray();
                _undoTiles.RemoveRange(0, count);

                Debug.WriteLine("Flushing Undo Buffer: {0} Tiles", count);
                for (int i = 0; i < count; i++)
                {
                    var tile = tiles[i];

                    if (tile == null || tile.Tile == null)
                        continue;

                    _writer.Write(tile.Location.X);
                    _writer.Write(tile.Location.Y);

                    int dataIndex;
                    int headerIndex;

                    byte[] tileData = World.SerializeTileData(tile.Tile, out dataIndex, out headerIndex);

                    _writer.Write(tileData, headerIndex, dataIndex - headerIndex);
                }

                Count += count;
            }
        }

        public void Close()
        {
            Flush();

            World.SaveChests(Chests, _writer);
            World.SaveSigns(Signs, _writer);

            _writer.BaseStream.Position = (long)0;
            _writer.Write(Count);
            _writer.Close();
            _writer.Dispose();
        }

        public static IEnumerable<UndoTile> ReadUndoTilesFromStream(BinaryReader br)
        {
            var tilecount = br.ReadInt32();
            for (int i = 0; i < tilecount; i++)
            {
                int rle;
                int x = br.ReadInt32();
                int y = br.ReadInt32();
                var curTile = World.DeserializeTileData(br, out rle);

                yield return new UndoTile(new Vector2Int32(x, y), curTile);
            }
        }

        #region Destructor to cleanup files
        private bool disposed = false;
        private readonly BinaryWriter _writer;
        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // free managed
                    if (_writer != null)
                    {
                        _writer.Dispose();
                    }
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.

                disposed = true;
            }
        }

        ~UndoBuffer()
        {
            Dispose(false);
        }

        #endregion
    }
}