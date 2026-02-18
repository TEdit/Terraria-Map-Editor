namespace TEdit.Terraria;

public partial class NPC : ReactiveObject
{
    [Reactive]
    private Vector2Int32Observable _home;

    [Reactive]
    private bool _isHomeless;

    [Reactive]
    private bool _homelessDespawn;

    [Reactive]
    private string _name;

    [Reactive]
    private Vector2FloatObservable _position;

    [Reactive]
    private int _spriteId;

    [Reactive]
    private string _displayName;

    [Reactive]
    private int _townNpcVariationIndex;

    public override string ToString()
    {
        return Name;
    }
}
