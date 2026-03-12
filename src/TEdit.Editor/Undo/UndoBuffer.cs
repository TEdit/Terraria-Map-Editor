using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TEdit.Terraria;
using TEdit.Geometry;

namespace TEdit.Editor.Undo;

public class UndoBuffer : IDisposable
{
    private const int FlushSize = 10000;
    private string file;
    private static readonly object UndoSaveLock = new object();

    // Background flush infrastructure
    private readonly record struct FlushBatch(
        List<Tile> TileOrder,
        Dictionary<Tile, HashSet<Vector2Int32>> UndoTiles,
        uint Version,
        int MaxTileId,
        int MaxWallId,
        bool[] TileFrameImportant);
    private readonly BlockingCollection<FlushBatch> _flushQueue = new();
    private readonly Task _writerTask;

    public UndoBuffer(string fileName, World world)
    {
        file = Path.GetFileNameWithoutExtension(fileName);
        Debug.WriteLine($"Creating UNDO file {fileName}");
        _writer = new BinaryWriter(new FileStream(fileName, FileMode.Create), System.Text.Encoding.UTF8, false);
        _writer.Write((Int32)0);
        _world = world;

        // Start background writer
        _writerTask = Task.Run(BackgroundWriter);
    }

    private void BackgroundWriter()
    {
        foreach (var batch in _flushQueue.GetConsumingEnumerable())
        {
            foreach (var tile in batch.TileOrder)
            {
                if (!batch.UndoTiles.TryGetValue(tile, out var locations))
                    continue;

                // Serialize tile data
                byte[] tileData = World.SerializeTileData(
                    tile,
                    (int)batch.Version,
                    batch.MaxTileId,
                    batch.MaxWallId,
                    batch.TileFrameImportant,
                    out int dataIndex,
                    out int headerIndex);

                _writer.Write(tileData, headerIndex, dataIndex - headerIndex);

                // Write location count and locations
                _writer.Write(locations.Count);
                foreach (var loc in locations)
                {
                    _writer.Write(loc.X);
                    _writer.Write(loc.Y);
                }
            }
            _uniqueTileGroupsWritten += batch.UndoTiles.Count;
        }
    }

    private readonly Dictionary<Tile, HashSet<Vector2Int32>> _undoTiles = new();
    private readonly List<Tile> _tileOrder = new(); // For order preservation
    private int _totalTileCount; // Maintained incrementally to avoid LINQ enumeration
    private int _uniqueTileGroupsWritten = 0; // Track across batches for file header
    private readonly List<Sign> _signs = new List<Sign>();
    private readonly List<Chest> _chests = new List<Chest>();
    private readonly List<TileEntity> _tileEntities = new List<TileEntity>();
    private readonly List<ModTileOverlayEntry> _modTileOverlays = new();
    private readonly List<ModWallOverlayEntry> _modWallOverlays = new();
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
                    _totalTileCount--;
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
            _totalTileCount = 0;
            LastTile = null;
        }
    }

    private int GetTotalTileCount() => _totalTileCount;

    public void Add(Vector2Int32 location, Tile tile)
    {
        bool shouldFlush = false;

        lock (UndoSaveLock)
        {
            // Find or create tile entry
            if (!_undoTiles.TryGetValue(tile, out var locations))
            {
                locations = new HashSet<Vector2Int32>();
                _undoTiles[tile] = locations;
                _tileOrder.Add(tile); // Track insertion order
            }

            if (locations.Add(location))
                _totalTileCount++;
            LastTile = new UndoTile(tile, location); // For compatibility
            shouldFlush = _totalTileCount > FlushSize;

            // Track mod tile/wall overlays for NBT serialization
            if (tile.IsActive && tile.Type >= WorldConfiguration.TileCount)
            {
                _modTileOverlays.Add(new ModTileOverlayEntry
                {
                    X = location.X, Y = location.Y,
                    Type = tile.Type,
                    Color = tile.TileColor,
                    U = tile.U, V = tile.V,
                });
            }
            if (tile.Wall >= WorldConfiguration.WallCount)
            {
                _modWallOverlays.Add(new ModWallOverlayEntry
                {
                    X = location.X, Y = location.Y,
                    Wall = tile.Wall,
                    WallColor = tile.WallColor,
                });
            }
        }

        if (shouldFlush)
        {
            SaveTileData();
        }
    }

    public bool IsEmpty => !_undoTiles.Any() && _uniqueTileGroupsWritten == 0 && _flushQueue.Count == 0;

    public void SaveTileData()
    {
        var world = _world;
        var version = world?.Version ?? WorldConfiguration.CompatibleVersion;
        var tileFrameImportant = world?.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

        int maxTileId = WorldConfiguration.SaveConfiguration.GetData(version).MaxTileId;
        int maxWallId = WorldConfiguration.SaveConfiguration.GetData(version).MaxWallId;

        FlushBatch batch;

        lock (UndoSaveLock)
        {
            if (!_undoTiles.Any()) return;

            int totalCount = GetTotalTileCount();
            Debug.WriteLine($"Flushing Undo Buffer: {totalCount} Tiles ({_undoTiles.Count} unique)");

            // Snapshot the collections — Tile is a struct so dictionary keys are value copies
            var snapshotTiles = new Dictionary<Tile, HashSet<Vector2Int32>>(_undoTiles);
            var snapshotOrder = new List<Tile>(_tileOrder);

            batch = new FlushBatch(snapshotOrder, snapshotTiles, version, maxTileId, maxWallId, tileFrameImportant);

            // Clear immediately so Add() can continue with fresh collections
            _undoTiles.Clear();
            _tileOrder.Clear();
            _totalTileCount = 0;
            LastTile = null;
        }

        // Queue for background serialization + writing — returns instantly
        _flushQueue.Add(batch);
    }

    /// <summary>
    /// Initiates an async close. Returns a Task that completes when the file is fully written.
    /// The caller does NOT need to await unless it needs to read the file immediately.
    /// </summary>
    public Task CloseAsync()
    {
        Debug.WriteLine($"Saving {file}");
        SaveTileData();

        // Signal no more batches
        _flushQueue.CompleteAdding();

        // Capture state needed for finalization
        var world = _world;
        var version = world?.Version ?? WorldConfiguration.CompatibleVersion;
        var chests = _chests.ToList();
        var signs = _signs.ToList();
        var tileEntities = _tileEntities.ToList();
        var modTiles = _modTileOverlays.ToList();
        var modWalls = _modWallOverlays.ToList();

        // Chain finalization after background writer completes
        _closeTask = _writerTask.ContinueWith(_ =>
        {
            World.SaveChests(chests, _writer, (int)version);
            World.SaveSigns(signs, _writer, (int)version);
            World.SaveTileEntities(tileEntities, _writer, version);
            ModDataSerializer.SaveModPayload(_writer, chests, tileEntities, modTiles, modWalls, world?.TwldData);
            _writer.BaseStream.Position = (long)0;
            _writer.Write(_uniqueTileGroupsWritten);
            _writer.Close();
            _writer.Dispose();
            _writer = null;
        }, TaskContinuationOptions.ExecuteSynchronously);

        return _closeTask;
    }

    /// <summary>
    /// Synchronous close — blocks until fully written. Use for redo buffers
    /// or anywhere the file must be complete before continuing.
    /// </summary>
    public void Close()
    {
        CloseAsync().Wait();
    }

    private Task _closeTask;

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

                yield return new UndoTile(tile, new Vector2Int32(x, y));
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
                // Wait for async close if in progress, otherwise signal and wait
                if (_closeTask != null)
                {
                    _closeTask.Wait();
                }
                else
                {
                    if (!_flushQueue.IsAddingCompleted)
                        _flushQueue.CompleteAdding();
                    _writerTask?.Wait();
                }
                _flushQueue.Dispose();
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
