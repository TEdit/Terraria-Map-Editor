using System.Collections.Generic;
using ReactiveUI;
using TEdit.Terraria;

namespace TEdit.ViewModel;

public class NpcListItem : ReactiveObject
{
    public int SpriteId { get; }
    public string DefaultName { get; }
    public List<string> AvailableVariants { get; }
    public bool CanShimmer { get; }
    public bool HasVariants => AvailableVariants?.Count > 1;

    private World _world;
    public World World
    {
        get => _world;
        set
        {
            this.RaiseAndSetIfChanged(ref _world, value);
            this.RaisePropertyChanged(nameof(IsShimmered));
            this.RaisePropertyChanged(nameof(IsPartying));
        }
    }

    private NPC _worldNpc;
    public NPC WorldNpc
    {
        get => _worldNpc;
        set
        {
            this.RaiseAndSetIfChanged(ref _worldNpc, value);
            this.RaisePropertyChanged(nameof(IsOnMap));
            this.RaisePropertyChanged(nameof(VariantIndex));
            this.RaisePropertyChanged(nameof(IsShimmered));
            this.RaisePropertyChanged(nameof(IsPartying));
        }
    }

    public bool IsOnMap => WorldNpc != null;

    public int VariantIndex
    {
        get => WorldNpc?.TownNpcVariationIndex ?? 0;
        set
        {
            if (WorldNpc != null)
            {
                WorldNpc.TownNpcVariationIndex = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public bool IsShimmered
    {
        get => _world != null
            && SpriteId >= 0
            && SpriteId < _world.ShimmeredTownNPCs.Count
            && _world.ShimmeredTownNPCs[SpriteId] != 0;
        set
        {
            if (_world != null && SpriteId >= 0)
            {
                // Pad collection if NPC ID is beyond current size
                while (_world.ShimmeredTownNPCs.Count <= SpriteId)
                {
                    _world.ShimmeredTownNPCs.Add(0);
                }

                int newVal = value ? 1 : 0;
                _world.ShimmeredTownNPCs[SpriteId] = newVal;
                this.RaisePropertyChanged(nameof(IsShimmered));
            }
        }
    }

    public bool IsPartying
    {
        get => _world?.PartyingNPCs.Contains(SpriteId) ?? false;
        set
        {
            if (_world != null)
            {
                bool current = _world.PartyingNPCs.Contains(SpriteId);
                if (value && !current)
                    _world.PartyingNPCs.Add(SpriteId);
                else if (!value && current)
                    _world.PartyingNPCs.Remove(SpriteId);
                this.RaisePropertyChanged(nameof(IsPartying));
            }
        }
    }

    public NpcListItem(int spriteId, string defaultName, List<string> variants = null, bool canShimmer = false)
    {
        SpriteId = spriteId;
        DefaultName = defaultName;
        AvailableVariants = variants;
        CanShimmer = canShimmer;
    }
}
