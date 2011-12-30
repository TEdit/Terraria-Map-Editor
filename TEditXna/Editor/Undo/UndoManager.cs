using System;
using System.IO;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Undo
{
    public class UndoManager : ObservableObject
    {
        private readonly WorldViewModel _wvm;
        public UndoManager(WorldViewModel viewModel)
        {
            if (!Directory.Exists("undo"))
            {
                Directory.CreateDirectory("undo");
            }
            _wvm = viewModel;
        }
        private const string UndoFile = "undo\\undo_temp_{0}";
        private const string RedoFile = "undo\\redo_temp_{0}";

        private UndoBuffer _buffer = new UndoBuffer();
        private int _currentIndex = 0;
        private int _maxIndex = 0;

        public UndoBuffer Buffer
        {
            get { return _buffer; }
            set { Set("Buffer", ref _buffer, value); }
        }

        public void SaveUndo()
        {
            _maxIndex = _currentIndex;
            _buffer.Write(string.Format(UndoFile, _currentIndex));
            _currentIndex++;
            Buffer = new UndoBuffer();
        }

        public void SaveTile(Vector2Int32 p)
        {
            SaveTile(p.X, p.Y);
        }
        public void SaveTile(int x, int y)
        {
            var curTile = (Tile)_wvm.CurrentWorld.Tiles[x, y].Clone();
            if (curTile.Type == 21)
            {
                var curchest = _wvm.CurrentWorld.GetChestAtTile(x, y);
                if (curchest != null)
                {
                    _wvm.CurrentWorld.Chests.Remove(curchest);
                    var chest = curchest.Copy();
                    Buffer.Chests.Add(chest);
                }
            }
            if (curTile.Type == 55 || curTile.Type == 85)
            {
                var cursign = _wvm.CurrentWorld.GetSignAtTile(x, y);
                if (cursign != null)
                {
                    _wvm.CurrentWorld.Signs.Remove(cursign);
                    var sign = cursign.Copy();
                    Buffer.Signs.Add(sign);
                }
            }
            Buffer.Tiles.Add(new UndoTile(new Vector2Int32(x, y), curTile));
        }

        public void Undo()
        {
            if (_currentIndex <= 0)
                return;

            _currentIndex--;
            var buffer = UndoBuffer.Read(string.Format(UndoFile, _currentIndex));
            var redo = new UndoBuffer();

            foreach (var undoTile in buffer.Tiles)
            {
                var curTile = (Tile)_wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y].Clone();
                if (curTile.Type == 21)
                {
                    var curchest = _wvm.CurrentWorld.GetChestAtTile(undoTile.Location.X, undoTile.Location.Y);
                    if (curchest != null)
                    {
                        _wvm.CurrentWorld.Chests.Remove(curchest);
                        var chest = curchest.Copy();
                        redo.Chests.Add(chest);
                    }
                }
                if (curTile.Type == 55 || curTile.Type == 85)
                {
                    var cursign = _wvm.CurrentWorld.GetSignAtTile(undoTile.Location.X, undoTile.Location.Y);
                    if (cursign != null)
                    {
                        _wvm.CurrentWorld.Signs.Remove(cursign);
                        var sign = cursign.Copy();
                        redo.Signs.Add(sign);
                    }
                }
                redo.Tiles.Add(new UndoTile(undoTile.Location, curTile));

                _wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y] = (Tile)undoTile.Tile.Clone();
                _wvm.UpdateRenderPixel(undoTile.Location);
            }
            foreach (var chest in buffer.Chests)
            {
                _wvm.CurrentWorld.Chests.Add(chest.Copy());
            }
            foreach (var sign in buffer.Signs)
            {
                _wvm.CurrentWorld.Signs.Add(sign.Copy());
            }
            redo.Write(string.Format(RedoFile, _currentIndex));
        }

        public void Redo()
        {
            if (_currentIndex > _maxIndex || _currentIndex < 0)
                return;

            var buffer = UndoBuffer.Read(string.Format(RedoFile, _currentIndex));

            foreach (var undoTile in buffer.Tiles)
            {
                var curTile = (Tile)_wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y].Clone();
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

                _wvm.CurrentWorld.Tiles[undoTile.Location.X, undoTile.Location.Y] = (Tile)undoTile.Tile.Clone();
                _wvm.UpdateRenderPixel(undoTile.Location);
            }
            foreach (var chest in buffer.Chests)
            {
                _wvm.CurrentWorld.Chests.Add(chest.Copy());
            }
            foreach (var sign in buffer.Signs)
            {
                _wvm.CurrentWorld.Signs.Add(sign.Copy());
            }
            _currentIndex++;
        }

        #region Destructor to cleanup files

        ~UndoManager()
        {
            foreach (var file in Directory.GetFiles("undo"))
            {
                File.Delete(file);
            }
        }

        #endregion
    }
}