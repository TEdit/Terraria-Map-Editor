using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TEdit.ViewModel;

/// <summary>
/// Groups backup/autosave entries by world name in the World Explorer (Tab 2 tree root).
/// </summary>
[IReactiveObject]
public partial class BackupWorldGroupViewModel
{
    public BackupWorldGroupViewModel(string worldName)
    {
        WorldName = worldName;
        IsExpanded = true;
    }

    public string WorldName { get; }
    public ObservableCollection<BackupEntryViewModel> Entries { get; } = [];

    [Reactive]
    private bool _isExpanded;

    public string HeaderText
    {
        get
        {
            int backups = 0, autosaves = 0;
            foreach (var e in Entries)
            {
                if (e.IsAutosave) autosaves++;
                else backups++;
            }

            var parts = new System.Collections.Generic.List<string>();
            if (backups > 0) parts.Add($"{backups} backup{(backups != 1 ? "s" : "")}");
            if (autosaves > 0) parts.Add($"{autosaves} autosave{(autosaves != 1 ? "s" : "")}");
            return $"{WorldName} ({string.Join(", ", parts)})";
        }
    }
}
