using System;
using System.IO;
using System.Linq;
using System.Threading;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Terraria.Objects;
using System.Diagnostics;

namespace TEdit.Editor.Undo
{
    public class UndoManager : ObservableObject, IDisposable
    {
        private static Random r = new Random();
        private static int uniqueVal;
        private static Timer undoAliveTimer;
        private static string UndoAliveFile;

        static UndoManager()
        {
            r = new Random();
            uniqueVal = r.Next(999999999);
            Dir = Path.Combine(WorldViewModel.TempPath, "undo_" + uniqueVal);
            UndoFile = Path.Combine(Dir, "undo_temp_{0}");
            RedoFile = Path.Combine(Dir, "redo_temp_{0}");
            UndoAliveFile = Path.Combine(Dir, "alive.txt");

            if (!Directory.Exists(Dir))
            {
                ErrorLogging.Log($"Creating Undo cache: {Dir}");

                Directory.CreateDirectory(Dir);
                File.Create(UndoAliveFile).Close();
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
            try
            {
                foreach (var file in Directory.GetFiles(WorldViewModel.TempPath).ToList())
                {
                    ErrorLogging.Log($"Removing old undo file: {file}");
                    File.Delete(file);
                }

                foreach (var dir in Directory.GetDirectories(WorldViewModel.TempPath).ToList())
                {
                    if (!Equals(dir, Dir) && !IsUndoDirAlive(dir))
                    {
                        ErrorLogging.Log($"Removing old undo cache: {dir}");
                        Directory.Delete(dir, true);
                    }
                }

                if (forceCleanup)
                {
                    ErrorLogging.Log($"Removing undo cache: {Dir}");
                    Directory.Delete(Dir, true);
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

        private readonly WorldViewModel _wvm;
        private UndoBuffer _buffer;
        private int _currentIndex = 0;
        private int _maxIndex = 0;

        public event EventHandler Undid;
        public event EventHandler Redid;
        public event EventHandler UndoSaved;

        public string GetUndoFileName()
        {
            return string.Format(UndoFile, _currentIndex);
        }
        protected virtual void OnUndoSaved(object sender, EventArgs e)
        {
            if (UndoSaved != null) UndoSaved(sender, e);
        }

        protected virtual void OnRedid(object sender, EventArgs e)
        {
            if (Redid != null) Redid(sender, e);
        }

        protected virtual void OnUndid(object sender, EventArgs e)
        {
            if (Undid != null) Undid(sender, e);
        }

        public UndoManager(WorldViewModel viewModel)
        {
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            _wvm = viewModel;
            _buffer = new UndoBuffer(GetUndoFileName());
        }

        public UndoBuffer Buffer
        {
            get { return _buffer; }
            set { Set(nameof(Buffer), ref _buffer, value); }
        }

        public void SaveUndo(bool updateMax = true)
        {
            // no tiles to undo, skip save
            if (_buffer == null || (_buffer.Count == 0 && _buffer.UndoTiles.Count == 0))
            {
                return;
            }

            //ValidateAndRemoveChests();
            if (updateMax) { _maxIndex = _currentIndex; }
            _currentIndex++;
            _buffer.Close();
            CreateBuffer();
            OnUndoSaved(this, EventArgs.Empty);

        }

        private void CreateBuffer()
        {
            _buffer?.Dispose();
            _buffer = null;
            Buffer = new UndoBuffer(GetUndoFileName());
        }

        public void SaveTile(Vector2Int32 p)
        {
            SaveTile(p.X, p.Y);
        }
        public void SaveTile(int x, int y)
        {
            if (_buffer == null) {  CreateBuffer(); }

            ValidateAndRemoveChests();
            SaveTileToBuffer(Buffer, x, y, false);
        }

        private void SaveTileToBuffer(UndoBuffer buffer, int x, int y, bool removeEntities = false)
        {
            var curTile = (Tile)_wvm.CurrentWorld.Tiles[x, y].Clone();

            if (Tile.IsChest(curTile.Type))
            {
                var curchest = _wvm.CurrentWorld.GetChestAtTile(x, y);
                if (curchest != null)
                {
                    if (removeEntities) { _wvm.CurrentWorld.Chests.Remove(curchest); }
                    var chest = curchest.Copy();
                    buffer.Chests.Add(chest);
                }
            }
            if (Tile.IsSign(curTile.Type))
            {
                var cursign = _wvm.CurrentWorld.GetSignAtTile(x, y);
                if (cursign != null)
                {
                    if (removeEntities) { _wvm.CurrentWorld.Signs.Remove(cursign); }
                    var sign = cursign.Copy();
                    buffer.Signs.Add(sign);
                }
            }
            if (Tile.IsTileEntity(curTile.Type))
            {
                var curTe = _wvm.CurrentWorld.GetTileEntityAtTile(x, y);
                if (curTe != null)
                {
                    if (removeEntities) { _wvm.CurrentWorld.TileEntities.Remove(curTe); }
                    var te = curTe.Copy();
                    buffer.TileEntities.Add(te);
                }
            }
            buffer.Add(new Vector2Int32(x, y), curTile);
        }

        private void ValidateAndRemoveChests()
        {
            if (Buffer == null || Buffer.LastTile == null)
                return;


            var lastTile = Buffer.LastTile;
            var existingLastTile = _wvm.CurrentWorld.Tiles[lastTile.Location.X, lastTile.Location.Y];

            // remove deleted chests or signs if required
            if (Tile.IsChest(lastTile.Tile.Type))
            {
                if (!Tile.IsChest(existingLastTile.Type) || !existingLastTile.IsActive)
                {
                    var curchest = _wvm.CurrentWorld.GetChestAtTile(lastTile.Location.X, lastTile.Location.Y);
                    if (curchest != null)
                    {
                        _wvm.CurrentWorld.Chests.Remove(curchest);
                    }
                }
            }
            else if (Tile.IsSign(lastTile.Tile.Type))
            {
                if (!Tile.IsSign(existingLastTile.Type) || !existingLastTile.IsActive)
                {
                    var cursign = _wvm.CurrentWorld.GetSignAtTile(lastTile.Location.X, lastTile.Location.Y);
                    if (cursign != null)
                    {
                        _wvm.CurrentWorld.Signs.Remove(cursign);
                    }
                }
            }
            else if (Tile.IsTileEntity(lastTile.Tile.Type))
            {
                if (!Tile.IsTileEntity(existingLastTile.Type) || !existingLastTile.IsActive)
                {
                    var curTe = _wvm.CurrentWorld.GetTileEntityAtTile(lastTile.Location.X, lastTile.Location.Y);
                    if (curTe != null)
                    {
                        _wvm.CurrentWorld.TileEntities.Remove(curTe);
                    }
                }
            }

            // Add new chests and signs if required
            if (Tile.IsChest(existingLastTile.Type))
            {
                var curchest = _wvm.CurrentWorld.GetChestAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curchest == null)
                {
                    _wvm.CurrentWorld.Chests.Add(new Chest(lastTile.Location.X, lastTile.Location.Y));
                }

            }
            else if (Tile.IsSign(existingLastTile.Type))
            {
                var cursign = _wvm.CurrentWorld.GetSignAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (cursign == null)
                {
                    _wvm.CurrentWorld.Signs.Add(new Sign(lastTile.Location.X, lastTile.Location.Y, string.Empty));
                }
            }
            else if (Tile.IsTileEntity(existingLastTile.Type))
            {
                var curTe = _wvm.CurrentWorld.GetTileEntityAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curTe == null)
                {
                    _wvm.CurrentWorld.TileEntities.Add(TileEntity.CreateForTile(existingLastTile, lastTile.Location.X, lastTile.Location.Y, _wvm.CurrentWorld.TileEntities.Count));
                }
            }
        }

        public void Undo()
        {
            if (_currentIndex <= 0)
                return;

            ErrorLogging.TelemetryClient.TrackEvent(nameof(Undo));


            string undoFileName = string.Format(UndoFile, _currentIndex - 1); // load previous undo file
            string redoFileName = string.Format(RedoFile, _currentIndex);     // create redo file at current index
            UndoBuffer redo = new UndoBuffer(redoFileName);

            Debug.WriteLine($"Opening undo file for undo: {Path.GetFileNameWithoutExtension(undoFileName)}");
            using (var stream = new FileStream(undoFileName, FileMode.Open))
            using (BinaryReader br = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
            {
                foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br))
                {

                    Vector2Int32 pixel = undoTile.Location;
                    SaveTileToBuffer(redo, pixel.X, pixel.Y, true);
                    _wvm.CurrentWorld.Tiles[pixel.X, pixel.Y] = (Tile)undoTile.Tile;
                    _wvm.UpdateRenderPixel(pixel);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                }

                redo.Close();
                redo.Dispose();
                redo = null;

                foreach (var chest in World.LoadChestData(br))
                {
                    _wvm.CurrentWorld.Chests.Add(chest);
                }
                foreach (var sign in World.LoadSignData(br))
                {
                    _wvm.CurrentWorld.Signs.Add(sign);
                }
                foreach (var te in World.LoadTileEntityData(br, World.CompatibleVersion))
                {
                    _wvm.CurrentWorld.TileEntities.Add(te);
                }
            }

            _currentIndex--; // move index back one, create a new buffer
            CreateBuffer();
            _wvm.CurrentWorld.UpgradeLegacyTileEntities();
            OnUndid(this, EventArgs.Empty);
        }



        public void Redo()
        {
            if (_currentIndex > _maxIndex || _currentIndex < 0)
                return;

            ErrorLogging.TelemetryClient.TrackEvent(nameof(Redo));


            // close current undo buffer and get a new one with a new name after redo
            string redoFileName = string.Format(RedoFile, _currentIndex + 1); // load redo file at +1

            Debug.WriteLine($"Opening redo file for redo: {Path.GetFileNameWithoutExtension(redoFileName)}");
            using (var stream = new FileStream(redoFileName, FileMode.Open))
            using (BinaryReader br = new BinaryReader(stream))
            {
                foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br))
                {
                    var curTile = (Tile)_wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y];
                    SaveTile(undoTile.Location);

                    if (Tile.IsChest(curTile.Type))
                    {
                        var curchest = _wvm.CurrentWorld.GetChestAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (curchest != null)
                            _wvm.CurrentWorld.Chests.Remove(curchest);
                    }
                    if (Tile.IsSign(curTile.Type))
                    {
                        var cursign = _wvm.CurrentWorld.GetSignAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (cursign != null)
                            _wvm.CurrentWorld.Signs.Remove(cursign);
                    }
                    if (Tile.IsTileEntity(curTile.Type))
                    {
                        var curTe = _wvm.CurrentWorld.GetTileEntityAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (curTe != null)
                            _wvm.CurrentWorld.TileEntities.Remove(curTe);
                    }

                    _wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y] = (Tile)undoTile.Tile;
                    _wvm.UpdateRenderPixel(undoTile.Location);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, undoTile.Location.X, undoTile.Location.Y, 1, 1);
                }
                foreach (var chest in World.LoadChestData(br))
                {
                    _wvm.CurrentWorld.Chests.Add(chest);
                }
                foreach (var sign in World.LoadSignData(br))
                {
                    _wvm.CurrentWorld.Signs.Add(sign);
                }
                foreach (var te in World.LoadTileEntityData(br, World.CompatibleVersion))
                {
                    _wvm.CurrentWorld.TileEntities.Add(te);
                }
            }

            SaveUndo(updateMax: false);

            _wvm.CurrentWorld.UpgradeLegacyTileEntities();
            OnRedid(this, EventArgs.Empty);
        }

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




                CleanupOldUndoFiles(true);
                disposed = true;
            }
        }

        ~UndoManager()
        {
            Dispose(false);
        }

        #endregion
    }
}