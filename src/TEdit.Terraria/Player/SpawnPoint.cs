namespace TEdit.Terraria.Player;

public partial class SpawnPoint : ReactiveObject
{
    [Reactive] private int _x;
    [Reactive] private int _y;
    [Reactive] private int _worldId;
    [Reactive] private string _worldName = string.Empty;

    public SpawnPoint() { }

    public SpawnPoint(int x, int y, int worldId, string worldName)
    {
        _x = x;
        _y = y;
        _worldId = worldId;
        _worldName = worldName;
    }
}
