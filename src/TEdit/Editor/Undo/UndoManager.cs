using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.ViewModel;

namespace TEdit.Editor.Undo;


public partial class UndoManager : ReactiveObject, IUndoManager
{
    private static string uniqueVal;
    private static Timer undoAliveTimer;
    private static string UndoAliveFile;

    static UndoManager()
    {
        uniqueVal = DateTime.Now.ToString("yyyyMMddHHmmss");
        Dir = Path.Combine(WorldViewModel.TempPath, "undo", "undo_" + uniqueVal);
        UndoFile = Path.Combine(Dir, "undo_temp_{0}");
        RedoFile = Path.Combine(Dir, "redo_temp_{0}");
        UndoAliveFile = Path.Combine(Dir, "alive.txt");

        try
        {
            if (!Directory.Exists(Dir))
            {
                ErrorLogging.LogInfo($"Creating Undo cache: {Dir}");

                Directory.CreateDirectory(Dir);
                File.Create(UndoAliveFile).Close();
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn("Unable to create undo temp folder.");
            ErrorLogging.LogException(ex);
            System.Windows.Forms.MessageBox.Show($"Unable to create undo temp folder. Application will exit.\r\n{Dir}\r\n{ex.Message}",
                "Unable to create undo folder.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            App.Current.Shutdown();
        }

        // cleanup old undo folders
        foreach (var dir in Directory.GetDirectories(WorldViewModel.TempPath, "undo*", SearchOption.AllDirectories))
        {
            try
            {
                var fi = new FileInfo(Path.Combine(dir, "alive.txt"));
                if (fi.Exists && fi.LastWriteTime < DateTime.Now.AddDays(-7))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
            }
        }

        undoAliveTimer = new Timer(UndoAlive, null, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(5));
    }

    private static void UndoAlive(object state)
    {
        if (!File.Exists(UndoAliveFile))
            File.Create(UndoAliveFile).Close();

        File.SetLastWriteTimeUtc(UndoAliveFile, DateTime.UtcNow);
    }

    private static bool IsUndoDirAlive(string directory)
    {
        string aliveFile = Path.Combine(directory, "alive.txt");

        // if the keep alive file is missing, this undo dir is dead, remove it
        if (!File.Exists(aliveFile))
            return false;

        // if the keep alive file is older than 20 minutes, this undo dir is dead, remove it
        if (File.GetLastAccessTimeUtc(aliveFile) < DateTime.UtcNow.AddMinutes(-20))
            return false;

        return true;
    }

    private static void CleanupOldUndoFiles(bool forceCleanup = false)
    {
        var undoPath = Path.Combine(WorldViewModel.TempPath, "Undo");
        try
        {
            foreach (var file in Directory.GetFiles(undoPath).ToList())
            {
                ErrorLogging.LogDebug($"Removing old undo file: {file}");
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    // Undo temp files may be locked by concurrent operations, swallow the exception
                }
            }

            foreach (var dir in Directory.GetDirectories(undoPath).ToList())
            {
                if (!Equals(dir, undoPath) && !IsUndoDirAlive(dir))
                {
                    ErrorLogging.LogDebug($"Removing old undo cache: {dir}");
                    Directory.Delete(dir, true);
                }
            }

            if (forceCleanup)
            {
                ErrorLogging.LogDebug($"Removing undo cache: {undoPath}");
                Directory.Delete(undoPath, true);
            }
        }
        catch (Exception err)
        {
            ErrorLogging.LogException(err);
        }
    }

    private static readonly string Dir;
    private static readonly string UndoFile;
    private static readonly string RedoFile;

    private readonly World _world;
    private readonly NotifyTileChanged _notifyTileChanged;
    private readonly Action _undoApplied;

    [Reactive]
    private UndoBuffer _buffer;

    private int _currentIndex = 0;
    private int _maxIndex = 0;
    private Task _pendingClose;

    public int CurrentIndex => _currentIndex;



    public string GetUndoFileName()
    {
        return string.Format(UndoFile, _currentIndex);
    }

    public UndoManager(
        World world,
        NotifyTileChanged? notifyTileChanged,
        Action undoApplied)
    {
        if (!Directory.Exists(Dir))
        {
            Directory.CreateDirectory(Dir);
        }

        _world = world;
        _buffer = new UndoBuffer(GetUndoFileName(), _world);
        _notifyTileChanged = notifyTileChanged;
        _undoApplied = undoApplied;
    }

    public void SaveUndo(bool updateMax = true)
    {
        // no tiles to undo, skip save
        if (_buffer == null || _buffer.IsEmpty)
        {
            return;
        }

        //ValidateAndRemoveChests();
        if (updateMax) { _maxIndex = _currentIndex; }
        _currentIndex++;
        _pendingClose = _buffer.CloseAsync();
        CreateBuffer();
        _undoApplied?.Invoke();

    }

    /// <summary>
    /// Blocks until any pending undo file serialization is complete.
    /// Must be called before reading undo/redo files.
    /// </summary>
    private void WaitForPendingClose()
    {
        if (_pendingClose != null)
        {
            _pendingClose.Wait();
            _pendingClose = null;
        }
    }

    private void CreateBuffer()
    {
        _buffer?.Dispose();
        _buffer = null;
        Buffer = new UndoBuffer(GetUndoFileName(), _world);
    }

    public void SaveTile(Vector2Int32 p)
    {
        SaveTile(p.X, p.Y);
    }
    public void SaveTile(int x, int y, bool removeEntities = false)
    {
        if (_buffer == null) { CreateBuffer(); }

        ValidateAndRemoveChests();
        SaveTileToBuffer(Buffer, x, y, removeEntities);
    }

    private void SaveTileToBuffer(UndoBuffer buffer, int x, int y, bool removeEntities = false)
    {
        var curTile = _world.Tiles[x, y];

        if (curTile.IsActive && curTile.Type < buffer.TileImportance.Length && buffer.TileImportance[curTile.Type])
        {
            if (_world.IsAnchor(x, y))
            {
                if (curTile.IsChest())
                {
                    var curchest = _world.GetChestAtTile(x, y);
                    if (curchest != null)
                    {
                        if (removeEntities) { _world.Chests.Remove(curchest); }
                        var chest = curchest.Copy();
                        buffer.Chests.Add(chest);
                    }
                }
                if (curTile.IsSign())
                {
                    var cursign = _world.GetSignAtTile(x, y);
                    if (cursign != null)
                    {
                        if (removeEntities) { _world.Signs.Remove(cursign); }
                        var sign = cursign.Copy();
                        buffer.Signs.Add(sign);
                    }
                }
                if (curTile.IsTileEntity())
                {
                    var curTe = _world.GetTileEntityAtTile(x, y);
                    if (curTe != null)
                    {
                        if (removeEntities) { _world.TileEntities.Remove(curTe); }
                        var te = curTe.Copy();
                        buffer.TileEntities.Add(te);
                    }
                }
            }
        }
        buffer.Add(new Vector2Int32(x, y), curTile);
    }

    private void ValidateAndRemoveChests()
    {
        if (Buffer == null || Buffer.LastTile == null)
            return;

        var lastTile = Buffer.LastTile;

        // Fast path: skip all entity validation if neither the saved tile
        // nor the current world tile at that location is a chest, sign, or entity.
        // This avoids the expensive IsAnchor/GetAnchor/FirstOrDefault calls
        // for the vast majority of tiles during brush painting.
        bool savedIsEntity = lastTile.Tile.IsActive &&
            (lastTile.Tile.IsChest() || lastTile.Tile.IsSign() || lastTile.Tile.IsTileEntity());

        var existingLastTile = _world.Tiles[lastTile.Location.X, lastTile.Location.Y];
        bool currentIsEntity = existingLastTile.IsActive &&
            (existingLastTile.IsChest() || existingLastTile.IsSign() || existingLastTile.IsTileEntity());

        if (!savedIsEntity && !currentIsEntity) return;

        // remove deleted chests or signs if required
        if (lastTile.Tile.IsChest())
        {
            if (!existingLastTile.IsChest() || !existingLastTile.IsActive)
            {
                var curchest = _world.GetChestAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curchest != null)
                {
                    _world.Chests.Remove(curchest);
                }
            }
        }
        else if (lastTile.Tile.IsSign())
        {
            if (!existingLastTile.IsSign() || !existingLastTile.IsActive)
            {
                var cursign = _world.GetSignAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (cursign != null)
                {
                    _world.Signs.Remove(cursign);
                }
            }
        }
        else if (lastTile.Tile.IsTileEntity())
        {
            if (!existingLastTile.IsTileEntity() || !existingLastTile.IsActive)
            {
                var curTe = _world.GetTileEntityAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curTe != null)
                {
                    _world.TileEntities.Remove(curTe);
                }
            }
        }

        // Add new chests and signs if required
        if (_world.IsAnchor(lastTile.Location.X, lastTile.Location.Y))
        {
            if (existingLastTile.IsChest())
            {
                var curchest = _world.GetChestAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curchest == null)
                {
                    _world.Chests.Add(new Chest(lastTile.Location.X, lastTile.Location.Y));
                }
            }
            else if (existingLastTile.IsSign())
            {
                var cursign = _world.GetSignAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (cursign == null)
                {
                    _world.Signs.Add(new Sign(lastTile.Location.X, lastTile.Location.Y, string.Empty));
                }
            }
            else if (existingLastTile.IsTileEntity())
            {
                var curTe = _world.GetTileEntityAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curTe == null)
                {
                    _world.TileEntities.Add(TileEntity.CreateForTile(existingLastTile, lastTile.Location.X, lastTile.Location.Y, _world.TileEntities.Count));
                }
            }
        }
    }

    public IReadOnlyList<Vector2Int32> Undo()
    {
        if (_currentIndex <= 0)
            return Array.Empty<Vector2Int32>();

        WaitForPendingClose();

        var version = _world?.Version ?? WorldConfiguration.CompatibleVersion;

        string undoFileName = string.Format(UndoFile, _currentIndex - 1); // load previous undo file
        string redoFileName = string.Format(RedoFile, _currentIndex);     // create redo file at current index
        UndoBuffer redo = new UndoBuffer(redoFileName, _world);

        Debug.WriteLine($"Opening undo file for undo: {Path.GetFileNameWithoutExtension(undoFileName)}");
        var changedTiles = new List<Vector2Int32>();

        using (var stream = new FileStream(undoFileName, FileMode.Open))
        using (BinaryReader br = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
        {
            foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br, _world.TileFrameImportant))
            {
                Vector2Int32 pixel = undoTile.Location;
                SaveTileToBuffer(redo, pixel.X, pixel.Y, true);
                _world.Tiles[pixel.X, pixel.Y] = (Tile)undoTile.Tile;
                changedTiles.Add(pixel);
            }

            redo.Close();
            redo.Dispose();
            redo = null;

            var loadedChests = World.LoadChestData(br, version).ToList();
            foreach (var chest in loadedChests) { _world.Chests.Add(chest); }
            foreach (var sign in World.LoadSignData(br))
            {
                _world.Signs.Add(sign);
            }
            var loadedEntities = World.LoadTileEntityData(br, version).ToList();
            foreach (var te in loadedEntities) { _world.TileEntities.Add(te); }
            ModDataSerializer.LoadModPayload(br, loadedChests, loadedEntities);
        }

        _currentIndex--; // move index back one, create a new buffer
        CreateBuffer();
        _undoApplied?.Invoke();

        return changedTiles;
    }



    public IReadOnlyList<Vector2Int32> Redo()
    {
        if (_currentIndex > _maxIndex)
            return Array.Empty<Vector2Int32>();

        WaitForPendingClose();

        var version = _world?.Version ?? WorldConfiguration.CompatibleVersion;

        // close current undo buffer and get a new one with a new name after redo
        string redoFileName = string.Format(RedoFile, _currentIndex + 1); // load redo file at +1

        if (!File.Exists(redoFileName))
        {
            return Array.Empty<Vector2Int32>();
        }

        Debug.WriteLine($"Opening redo file for redo: {Path.GetFileNameWithoutExtension(redoFileName)}");
        var changedTiles = new List<Vector2Int32>();

        using (var stream = new FileStream(redoFileName, FileMode.Open))
        using (BinaryReader br = new BinaryReader(stream))
        {
            foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br, _world.TileFrameImportant))
            {
                var curTile = (Tile)_world.Tiles[undoTile.Location.X, undoTile.Location.Y];
                SaveTile(undoTile.Location);

                if (curTile.IsChest())
                {
                    var curchest = _world.GetChestAtTile(undoTile.Location.X, undoTile.Location.Y, true);
                    if (curchest != null)
                        _world.Chests.Remove(curchest);
                }
                if (curTile.IsSign())
                {
                    var cursign = _world.GetSignAtTile(undoTile.Location.X, undoTile.Location.Y, true);
                    if (cursign != null)
                        _world.Signs.Remove(cursign);
                }
                if (curTile.IsTileEntity())
                {
                    var curTe = _world.GetTileEntityAtTile(undoTile.Location.X, undoTile.Location.Y, true);
                    if (curTe != null)
                        _world.TileEntities.Remove(curTe);
                }

                _world.Tiles[undoTile.Location.X, undoTile.Location.Y] = (Tile)undoTile.Tile;
                changedTiles.Add(undoTile.Location);
            }
            var loadedChests = World.LoadChestData(br, version).ToList();
            foreach (var chest in loadedChests) { _world.Chests.Add(chest); }
            foreach (var sign in World.LoadSignData(br))
            {
                _world.Signs.Add(sign);
            }
            var loadedEntities = World.LoadTileEntityData(br, version).ToList();
            foreach (var te in loadedEntities) { _world.TileEntities.Add(te); }
            ModDataSerializer.LoadModPayload(br, loadedChests, loadedEntities);
        }

        SaveUndo(updateMax: false);

        return changedTiles;
    }

    #region IUndoManager

    public void SaveTile(World world, Vector2Int32 location, bool removeEntities = false) =>
        SaveTile(location.X, location.Y, removeEntities);

    public void SaveTile(World world, int x, int y, bool removeEntities = false) =>
        SaveTile(x, y, removeEntities);

    public Task SaveUndoAsync()
    {
        SaveUndo();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Vector2Int32>> UndoAsync(World world)
    {
        return Task.FromResult(Undo());
    }

    public Task<IReadOnlyList<Vector2Int32>> RedoAsync(World world)
    {
        return Task.FromResult(Redo());
    }

    public Task StartUndoAsync() => Task.CompletedTask;

    #endregion

    #region Destructor to cleanup files
    private bool disposed = false;
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
                if (_buffer != null)
                {
                    _buffer.Dispose();
                }
                _buffer = null;
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.




            CleanupOldUndoFiles(false);
            disposed = true;
        }
    }

    ~UndoManager()
    {
        Dispose(false);
    }

    #endregion
}
