using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using TEditXNA.Terraria;
using TEditXna.ViewModel;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Undo
{
    public class UndoManager : ObservableObject, IDisposable
    {
        private static readonly string Dir = Path.Combine(WorldViewModel.TempPath, "undo");
        private static readonly string UndoFile = Path.Combine(Dir, "undo_temp_{0}");
        private static readonly string RedoFile = Path.Combine(Dir, "redo_temp_{0}");

        private readonly WorldViewModel _wvm;
        private UndoBuffer _buffer = new UndoBuffer();
        private int _currentIndex = 0;
        private int _maxIndex = 0;

        public event EventHandler Undid;
        public event EventHandler Redid;
        public event EventHandler UndoSaved;


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
        }

        public UndoBuffer Buffer
        {
            get { return _buffer; }
            set { Set("Buffer", ref _buffer, value); }
        }

        public void SaveUndo()
        {
            //ValidateAndRemoveChests();
            _maxIndex = _currentIndex;
            _buffer.Write(string.Format(UndoFile, _currentIndex));
            _currentIndex++;
            Buffer = new UndoBuffer();
            OnUndoSaved(this, EventArgs.Empty);
        }

        public void SaveTile(Vector2Int32 p)
        {
            SaveTile(p.X, p.Y);
        }
        public void SaveTile(int x, int y)
        {
            ValidateAndRemoveChests();
            var curTile = (Tile)_wvm.CurrentWorld.Tiles[x, y].Clone();
            if (curTile.Type == 21 && !Buffer.Chests.Any(c => c.X == x && c.Y == y))
            {

                var curchest = _wvm.CurrentWorld.GetChestAtTile(x, y);
                if (curchest != null)
                {
                    var chest = curchest.Copy();
                    Buffer.Chests.Add(chest);
                }
            }
            else if ((curTile.Type == 55 || curTile.Type == 85) && !Buffer.Signs.Any(c => c.X == x && c.Y == y))
            {
                var cursign = _wvm.CurrentWorld.GetSignAtTile(x, y);
                if (cursign != null)
                {
                    var sign = cursign.Copy();
                    Buffer.Signs.Add(sign);
                }
            }
            Buffer.Tiles.Add(new UndoTile(new Vector2Int32(x, y), curTile));
        }

        private void ValidateAndRemoveChests()
        {
            if (Buffer == null || Buffer.Tiles.Count <= 0)
                return;

            var lastTile = Buffer.Tiles.Last();
            var existingLastTile = _wvm.CurrentWorld.Tiles[lastTile.Location.X, lastTile.Location.Y];

            // remove deleted chests or signs if required
            if (lastTile.Tile.Type == 21)
            {
                if (existingLastTile.Type != 21 || !existingLastTile.IsActive)
                {
                    var curchest = _wvm.CurrentWorld.GetChestAtTile(lastTile.Location.X, lastTile.Location.Y);
                    if (curchest != null)
                    {
                        _wvm.CurrentWorld.Chests.Remove(curchest);
                    }
                }
            }
            else if (lastTile.Tile.Type == 55 || lastTile.Tile.Type == 85)
            {
                if ((existingLastTile.Type != 55 && existingLastTile.Type != 85) || !existingLastTile.IsActive)
                {
                    var cursign = _wvm.CurrentWorld.GetSignAtTile(lastTile.Location.X, lastTile.Location.Y);
                    if (cursign != null)
                    {
                        _wvm.CurrentWorld.Signs.Remove(cursign);
                    }
                }
            }

            // Add new chests and signs if required
            if (existingLastTile.Type == 21)
            {
                var curchest = _wvm.CurrentWorld.GetChestAtTile(lastTile.Location.X, lastTile.Location.Y);
                if (curchest == null)
                {
                    _wvm.CurrentWorld.Chests.Add(new Chest(lastTile.Location.X, lastTile.Location.Y));
                }

            }
            else if (existingLastTile.Type == 55 || existingLastTile.Type == 85)
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
            List<Chest> Chests = new List<Chest>();
            List<Sign> Signs = new List<Sign>();

            using (var ws = new FileStream(string.Format(RedoFile, _currentIndex), FileMode.Create))
            using (var bw = new BinaryWriter(ws))
            using (var stream = new FileStream(string.Format(UndoFile, _currentIndex), FileMode.Open))
            using (BinaryReader br = new BinaryReader(stream))
            {
                int count = br.ReadInt32();
                br.BaseStream.Position -= 4;
                bw.Write(count);

                foreach (var undoTile in UndoBuffer.ReadUndoTilesFromStream(br))
                {
                    var curTile = (Tile)_wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y];
                    if (curTile.Type == 21)
                    {
                        var curchest = _wvm.CurrentWorld.GetChestAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (curchest != null)
                        {
                            _wvm.CurrentWorld.Chests.Remove(curchest);
                            var chest = curchest.Copy();
                            Chests.Add(chest);
                        }
                    }
                    if (curTile.Type == 55 || curTile.Type == 85)
                    {
                        var cursign = _wvm.CurrentWorld.GetSignAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (cursign != null)
                        {
                            _wvm.CurrentWorld.Signs.Remove(cursign);
                            var sign = cursign.Copy();
                            Signs.Add(sign);
                        }
                    }

                    bw.Write(undoTile.Location.X);
                    bw.Write(undoTile.Location.Y);
                    World.WriteTileDataToStreamV1(curTile, bw);

                    _wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y] = (Tile)undoTile.Tile;
                    _wvm.UpdateRenderPixel(undoTile.Location);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, undoTile.Location.X, undoTile.Location.Y, 1, 1);
                }
                World.WriteChestDataToStreamV1(Chests, bw);
                World.WriteSignDataToStreamV1(Signs, bw);
                foreach (var chest in World.ReadChestDataFromStreamV1(br, World.CompatibleVersion))
                {
                    _wvm.CurrentWorld.Chests.Add(chest);
                }
                foreach (var sign in World.ReadSignDataFromStreamV1(br))
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
                    if (curTile.Type == 21)
                    {
                        var curchest = _wvm.CurrentWorld.GetChestAtTile(undoTile.Location.X, undoTile.Location.Y);
                        if (curchest != null)
                            _wvm.CurrentWorld.Chests.Remove(curchest);
                    }
                    if (curTile.Type == 55 || curTile.Type == 85)
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
                foreach (var chest in World.ReadChestDataFromStreamV1(br, World.CompatibleVersion))
                {
                    _wvm.CurrentWorld.Chests.Add(chest);
                }
                foreach (var sign in World.ReadSignDataFromStreamV1(br))
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
                    // free managed
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _buffer = null;
                foreach (var file in Directory.GetFiles(Dir))
                {
                    File.Delete(file);
                }
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