namespace TEdit.Terraria.Player;

public partial class PlayerBuff : ReactiveObject
{
    [Reactive] private int _type;
    [Reactive] private int _time;

    public PlayerBuff() { }

    public PlayerBuff(int type, int time)
    {
        _type = type;
        _time = time;
    }
}
