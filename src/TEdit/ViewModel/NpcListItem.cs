using ReactiveUI;
using TEdit.Terraria;

namespace TEdit.ViewModel;

public class NpcListItem : ReactiveObject
{
    public int SpriteId { get; }
    public string DefaultName { get; }

    private NPC _worldNpc;
    public NPC WorldNpc
    {
        get => _worldNpc;
        set
        {
            this.RaiseAndSetIfChanged(ref _worldNpc, value);
            this.RaisePropertyChanged(nameof(IsOnMap));
        }
    }

    public bool IsOnMap => WorldNpc != null;

    public NpcListItem(int spriteId, string defaultName)
    {
        SpriteId = spriteId;
        DefaultName = defaultName;
    }
}
