using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using TEdit.Common;
using System.Linq;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;

namespace TEdit.Tools.History
{
    [Export]
    public class HistoryManager : ObservableObject
    {
        [Import("World", typeof(World))]
        private World _world;

        [Import]
        private WorldRenderer _renderer;

        private readonly ObservableCollectionEx<HistoryTile[]> _UndoHistory = new ObservableCollectionEx<HistoryTile[]>();
        private readonly ObservableCollectionEx<HistoryTile[]> _Undo = new ObservableCollectionEx<HistoryTile[]>();
        //public ObservableCollectionEx<Queue<HistoryTile>> Undo
        //{
        //    get { return _Undo; }
        //}

        private readonly ObservableCollectionEx<HistoryTile[]> _RedoHistory = new ObservableCollectionEx<HistoryTile[]>();
        private readonly ObservableCollectionEx<HistoryTile[]> _Redo = new ObservableCollectionEx<HistoryTile[]>();
        //public ObservableCollectionEx<Queue<HistoryTile>> Redo
        //{
        //    get { return _Redo; }
        //}

        private int UndoIndex = 0;

        private Queue<HistoryTile> buffer = new Queue<HistoryTile>();

        private bool _SaveHistory = true;
        public bool SaveHistory
        {
            get { return this._SaveHistory; }
            set
            {
                if (this._SaveHistory != value)
                {
                    this._SaveHistory = value;

                    if (this._SaveHistory == false)
                        PurgeBuffer();

                    this.RaisePropertyChanged("SaveHistory");
                }
            }
        }



        public void PurgeBuffer()
        {
            buffer = new Queue<HistoryTile>();
            ClearRedoHistory();
        }
        public void AddTileToBuffer(int x, int y, ref Tile tile)
        {
            // pass by ref to avoid as much garbage
            if (_SaveHistory)
                buffer.Enqueue(new HistoryTile(new PointInt32(x, y), (Tile)tile.Clone()));
        }

        public void AddBufferToHistory()
        {
            if (!_SaveHistory)
                return;

            if (_Undo.Count >= 100)
                _Undo.RemoveAt(0);

            _Undo.Add(buffer.ToArray());
            buffer.Clear();

            ClearRedoHistory();
        }

        private void ClearRedoHistory()
        {
            _UndoHistory.Clear();
            _Redo.Clear();
            _RedoHistory.Clear();
        }

        public void ProcessUndo()
        {
            if (_Undo.Count > 0)
            {
                var item = _Undo.Last();
                _Undo.Remove(item);

                var redo = new Queue<HistoryTile>();
                foreach (var historyTile in item.Reverse())
                {
                    redo.Enqueue(new HistoryTile(historyTile.Location, (Tile)_world.Tiles[historyTile.Location.X, historyTile.Location.Y].Clone()));
                    _world.Tiles[historyTile.Location.X, historyTile.Location.Y] = historyTile.Tile;
                    _renderer.UpdateWorldImage(new PointInt32(historyTile.Location.X, historyTile.Location.Y));
                }

                _UndoHistory.Add(item);
                _Redo.Add(redo.ToArray());
            }
        }

        public void ProcessRedo()
        {
            if (_Redo.Count > 0)
            {
                var item = _Redo.Last();
                _Redo.Remove(item);

                var undoItem = _UndoHistory.Last();
                _Undo.Add(undoItem);
                _UndoHistory.Remove(undoItem);

                foreach (var historyTile in item.Reverse())
                {
                    _world.Tiles[historyTile.Location.X, historyTile.Location.Y] = historyTile.Tile;
                    _renderer.UpdateWorldImage(new PointInt32(historyTile.Location.X, historyTile.Location.Y));
                }
            }
        }
    }
}