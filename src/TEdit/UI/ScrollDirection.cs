namespace TEdit.Editor;

public enum ScrollDirection
{
    Up,
    Down,
    Left,
    Right
}

public class ScrollEventArgs: System.EventArgs
{
    public ScrollEventArgs(ScrollDirection direction, int amount)
    {
        Amount = amount;
        Direction = direction;
    }

    public int Amount { get; }
    public ScrollDirection Direction { get;}
}