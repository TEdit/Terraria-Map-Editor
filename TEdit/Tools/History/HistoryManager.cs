using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using TEdit.Common;
using System.Linq;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.Tools.History
{
    [Export]
    public class HistoryManager : ObservableObject
    {
        [Import("World", typeof(World))]
        private World _world;

        [Import]
        private WorldRenderer _renderer;

        private readonly ObservableCollectionEx<Queue<HistoryTile>> _UndoHistory = new ObservableCollectionEx<Queue<HistoryTile>>();
        private readonly ObservableCollectionEx<Queue<HistoryTile>> _Undo = new ObservableCollectionEx<Queue<HistoryTile>>();
        //public ObservableCollectionEx<Queue<HistoryTile>> Undo
        //{
        //    get { return _Undo; }
        //}

        private readonly ObservableCollectionEx<Queue<HistoryTile>> _RedoHistory = new ObservableCollectionEx<Queue<HistoryTile>>();
        private readonly ObservableCollectionEx<Queue<HistoryTile>> _Redo = new ObservableCollectionEx<Queue<HistoryTile>>();
        //public ObservableCollectionEx<Queue<HistoryTile>> Redo
        //{
        //    get { return _Redo; }
        //}

        private int UndoIndex = 0;

        public void AddUndo(Queue<HistoryTile> history)
        {
            if (_Undo.Count >= 100)
                _Undo.RemoveAt(0);

            _Undo.Add(history);


            ClearHistory();
        }
        private void ClearHistory()
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
                _Redo.Add(redo);
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