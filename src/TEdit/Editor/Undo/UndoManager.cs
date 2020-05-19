using System;
using System.IO;
using System.Linq;
using System.Threading;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEditXNA.Terraria;
using TEditXna.ViewModel;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Undo
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
            set { Set("Buffer", ref _buffer, value); }
        }

        public void SaveUndo()
        {
            // no tiles to undo, skip save
            if (_buffer == null)
            {
                return;
            }

            //ValidateAndRemoveChests();
            _maxIndex = _currentIndex;
            _buffer.Close();
            _currentIndex++;
            _buffer = null;
            Buffer = new UndoBuffer(GetUndoFileName());
            OnUndoSaved(this, EventArgs.Empty);

        }

        public void SaveTile(Vector2Int32 p)
        {
            SaveTile(p.X, p.Y);
        }
        public void SaveTile(int x, int y)
        {
            if (_buffer == null)
                Buffer = new UndoBuffer(GetUndoFileName());

            ValidateAndRemoveChests();
            var curTile = (Tile)_wvm.CurrentWorld.Tiles[x, y].Clone();
            if (Tile.IsChest(curTile.Type) && !Buffer.Chests.Any(c => c.X == x && c.Y == y))
            {

                var curchest = _wvm.CurrentWorld.GetChestAtTile(x, y);
                if (curchest != null)
                {
                    var chest = curchest.Copy();
                    Buffer.Chests.Add(chest);
                }
            }
            else if (Tile.IsSign(curTile.Type) && !Buffer.Signs.Any(c => c.X == x && c.Y == y))
            {
                var cursign = _wvm.CurrentWorld.GetSignAtTile(x, y);
                if (cursign != null)
                {
                    var sign = cursign.Copy();
                    Buffer.Signs.Add(sign);
                }
            }
            Buffer.Add(new Vector2Int32(x, y), curTile);
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
        }

        public void Undo()
        {
            if (_currentIndex <= 0)
                return;

            _currentIndex--;

            UndoBuffer redo = new UndoBuffer(string.Format(RedoFile, _currentIndex));

            using (var stream = new FileStream(string.Format(UndoFile, _currentIndex), FileMode.Open))
            using (BinaryReader br = new BinaryReader(stream))
            {
                foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br))
                {

                    var curTile = (Tile)_wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y];
                    redo.Add(undoTile.Location, curTile);

                    if (Tile.IsChest(curTile.Type))
                    {
                        var curchest = _wvm.CurrentWorld.GetChestAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (curchest != null)
                        {
                            _wvm.CurrentWorld.Chests.Remove(curchest);
                            var chest = curchest.Copy();
                            redo.Chests.Add(chest);
                        }
                    }
                    if (Tile.IsSign(curTile.Type))
                    {
                        var cursign = _wvm.CurrentWorld.GetSignAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (cursign != null)
                        {
                            _wvm.CurrentWorld.Signs.Remove(cursign);
                            var sign = cursign.Copy();
                            redo.Signs.Add(sign);
                        }
                    }
                    _wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y] = (Tile)undoTile.Tile;
                    _wvm.UpdateRenderPixel(undoTile.Location);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, undoTile.Location.X, undoTile.Location.Y, 1, 1);
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
            }

            OnUndid(this, EventArgs.Empty);
        }

        public void Redo()
        {
            if (_currentIndex > _maxIndex || _currentIndex < 0)
                return;

            using (var stream = new FileStream(string.Format(RedoFile, _currentIndex), FileMode.Open))
            using (BinaryReader br = new BinaryReader(stream))
            {
                foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br))
                {
                    var curTile = (Tile)_wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y];
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
            }
            _currentIndex++;
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