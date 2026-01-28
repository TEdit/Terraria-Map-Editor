using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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


    private readonly Dictionary<Tile, HashSet<Vector2Int32>> _undoTiles = new();
    private readonly List<Tile> _tileOrder = new(); // For order preservation
    private int _uniqueTileGroupsWritten = 0; // Track across batches for file header
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

    public UndoTile LastTile { get; set; }

    public void Add(Tile tile, int x, int y)
    {
        Add(new Vector2Int32(x, y), tile);
    }

    public bool Remove(int x, int y)
    {
        lock (UndoSaveLock)
        {
            // Find and remove the location from whichever tile contains it
            foreach (var kvp in _undoTiles)
            {
                if (kvp.Value.Remove(new Vector2Int32(x, y)))
                {
                    // If this was the last location for this tile, remove the tile entry
                    if (kvp.Value.Count == 0)
                    {
                        _undoTiles.Remove(kvp.Key);
                        _tileOrder.Remove(kvp.Key);
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public void Clear()
    {
        lock (UndoSaveLock)
        {
            _undoTiles.Clear();
            _tileOrder.Clear();
            LastTile = null;
        }
    }

    private int GetTotalTileCount()
    {
        return _undoTiles.Values.Sum(s => s.Count);
    }

    public void Add(Vector2Int32 location, Tile tile)
    {
        if (tile == null)
        {
            throw new ArgumentNullException(nameof(tile));
        }

        lock (UndoSaveLock)
        {
            // Find or create tile entry
            if (!_undoTiles.TryGetValue(tile, out var locations))
            {
                locations = new HashSet<Vector2Int32>();
                _undoTiles[tile] = locations;
                _tileOrder.Add(tile); // Track insertion order
            }

            locations.Add(location);
            LastTile = new UndoTile(tile, location); // For compatibility
        }

        if (GetTotalTileCount() > FlushSize)
        {
            SaveTileData();
        }
    }

    public bool IsEmpty => !_undoTiles.Any();

    public void SaveTileData()
    {
        var world = _world;
        var version = world?.Version ?? WorldConfiguration.CompatibleVersion;
        var tileFrameImportant = world?.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

        int maxTileId = ushort.MaxValue;
        int maxWallId = ushort.MaxValue;
        var saveVersions = WorldConfiguration.SaveConfiguration?.SaveVersions;
        if (saveVersions != null && (int)version < saveVersions.Count)
        {
            maxTileId = saveVersions[(int)version].MaxTileId;
            maxWallId = saveVersions[(int)version].MaxWallId;
        }

        lock (UndoSaveLock)
        {
            int totalCount = GetTotalTileCount();
            int uniqueThisBatch = _undoTiles.Count;

            if (totalCount == 0) return;

            Debug.WriteLine($"Flushing Undo Buffer: {totalCount} Tiles ({uniqueThisBatch} unique)");

            // Don't write count per batch - we write total at Close() in position 0
            // Write each unique tile and its locations
            foreach (var tile in _tileOrder) // Maintain order
            {
                if (!_undoTiles.TryGetValue(tile, out var locations))
                    continue;

                // Serialize tile data
                int dataIndex;
                int headerIndex;

                byte[] tileData = World.SerializeTileData(
                    tile,
                    (int)version,
                    maxTileId,
                    maxWallId,
                    tileFrameImportant,
                    out dataIndex,
                    out headerIndex);

                _writer.Write(tileData, headerIndex, dataIndex - headerIndex);

                // Write location count and locations
                _writer.Write(locations.Count);
                foreach (var loc in locations)
                {
                    _writer.Write(loc.X);
                    _writer.Write(loc.Y);
                }
            }

            // Track unique tile groups written for file header
            _uniqueTileGroupsWritten += uniqueThisBatch;

            // Clear processed data
            Clear();
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
        World.SaveTileEntities(TileEntities, _writer, version);
        _writer.BaseStream.Position = (long)0;
        _writer.Write(_uniqueTileGroupsWritten);
        _writer.Close();
        _writer.Dispose();
        _writer = null;
    }

    public bool[] TileImportance => _world?.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

    public static IEnumerable<UndoTile> ReadUndoTilesFromStream(BinaryReader br, bool[]? tileFrameImportance = null)
    {
        var tileFrameImportant = tileFrameImportance ?? WorldConfiguration.SettingsTileFrameImportant;

        int uniqueTileCount = br.ReadInt32();

        for (int i = 0; i < uniqueTileCount; i++)
        {
            // Read tile data
            int rle;
            var tile = World.DeserializeTileData(
                br,
                tileFrameImportant,
                (int)WorldConfiguration.CompatibleVersion,
                out rle);

            // Read location count
            int locationCount = br.ReadInt32();

            // Yield UndoTile for each location
            for (int j = 0; j < locationCount; j++)
            {
                int x = br.ReadInt32();
                int y = br.ReadInt32();

                // Clone tile for each location (important!)
                yield return new UndoTile((Tile)tile.Clone(), new Vector2Int32(x, y));
            }
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
