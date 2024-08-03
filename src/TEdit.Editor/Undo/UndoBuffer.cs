using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Editor.Undo;

public class UndoBuffer : IDisposable
{
    private const int FlushSize = 10000;
    private string file;
    private static readonly object UndoSaveLock = new object();

    public UndoBuffer(string fileName, World world)
    {
        file = Path.GetFileNameWithoutExtension(fileName);
        Debug.WriteLine($"Creating UNDO file {fileName}");
        _writer = new BinaryWriter(new FileStream(fileName, FileMode.Create), System.Text.Encoding.UTF8, false);
        _writer.Write((Int32)0);
        _world = world;
    }


    private readonly List<UndoTile> _undoTiles = new List<UndoTile>();
    private readonly List<Sign> _signs = new List<Sign>();
    private readonly List<Chest> _chests = new List<Chest>();
    private readonly List<TileEntity> _tileEntities = new List<TileEntity>();
    private readonly World _world;

    public IList<Chest> Chests
    {
        get { return _chests; }
    }

    public IList<Sign> Signs
    {
        get { return _signs; }
    }

    public IList<TileEntity> TileEntities
    {
        get { return _tileEntities; }
    }

    public List<UndoTile> UndoTiles
    {
        get { return _undoTiles; }
    }

    public UndoTile LastTile { get; set; }

    public void Add(Vector2Int32 location, Tile tile)
    {
        var undoTile = new UndoTile(tile, location);

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
            SaveTileData();
        }
    }

    public int Count { get; private set; }

    public void SaveTileData()
    {
        var world = _world;
        var version = world?.Version ?? WorldConfiguration.CompatibleVersion;
        var tileFrameImportant = world?.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

        int maxTileId = WorldConfiguration.SaveConfiguration.SaveVersions[(int)version].MaxTileId;
        int maxWallId = WorldConfiguration.SaveConfiguration.SaveVersions[(int)version].MaxWallId;
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

                byte[] tileData = World.SerializeTileData(tile.Tile, (int)version, maxTileId, maxWallId, tileFrameImportant, out dataIndex, out headerIndex);

                _writer.Write(tileData, headerIndex, dataIndex - headerIndex);
            }

            Count += count;
        }
    }

    public void Close()
    {
        var world = _world;
        var version = world?.Version ?? WorldConfiguration.CompatibleVersion;

        Debug.WriteLine($"Saving {file}");
        SaveTileData();
        World.SaveChests(Chests, _writer, (int)version);
        World.SaveSigns(Signs, _writer, (int)version);
        World.SaveTileEntities(TileEntities, _writer);
        _writer.BaseStream.Position = (long)0;
        _writer.Write(Count);
        _writer.Close();
        _writer.Dispose();
        _writer = null;
    }

    public bool[] TileImportance => _world?.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

    public static IEnumerable<UndoTile> ReadUndoTilesFromStream(BinaryReader br, bool[]? tileFrameImportance = null)
    {
        var tileFrameImportant = tileFrameImportance ?? WorldConfiguration.SettingsTileFrameImportant;

        var tilecount = br.ReadInt32();
        for (int i = 0; i < tilecount; i++)
        {
            int rle;
            int x = br.ReadInt32();
            int y = br.ReadInt32();
            var curTile = World.DeserializeTileData(br, tileFrameImportant, (int)WorldConfiguration.CompatibleVersion, out rle);

            yield return new UndoTile(curTile, new Vector2Int32(x, y));
        }
    }

    #region Destructor to cleanup files
    private bool disposed = false;
    private BinaryWriter _writer;
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
                Debug.WriteLine($"Disposing {file}");
                // free managed
                _writer?.Dispose();
            }

            disposed = true;
        }
    }

    ~UndoBuffer()
    {
        Dispose(false);
    }

    #endregion
}
