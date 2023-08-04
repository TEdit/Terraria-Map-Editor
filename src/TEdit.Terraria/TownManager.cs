using System;
using TEdit.Common.Reactive;
using TEdit.Geometry;

namespace TEdit.Terraria;

public class TownManager : ObservableObject
{
    private Vector2Int32 _home;
    private int _npcId;

    public int NpcId
    {
        get { return _npcId; }
        set { Set(nameof(NpcId), ref _npcId, value); }
    }

    public Vector2Int32 Home
    {
        get { return _home; }
        set { Set(nameof(Home), ref _home, value); }
    }
}
