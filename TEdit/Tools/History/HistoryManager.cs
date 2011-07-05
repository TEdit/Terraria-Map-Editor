using System.Collections.ObjectModel;

namespace TEdit.Tools.History
{
    public class HistoryManager
    {
        private readonly ObservableCollection<HistoryTile> _HistItem = new ObservableCollection<HistoryTile>();
        public ObservableCollection<HistoryTile> HistItem
        {
            get { return _HistItem; }
        }
    }
}