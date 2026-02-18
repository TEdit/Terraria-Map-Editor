using System;

namespace TEdit.Terraria;

public partial class Sign : ReactiveObject
{
    public const int LegacyLimit = 1000;

    public Sign()
    {
        _text = string.Empty;
    }

    public Sign(int x, int y, string text)
    {
        _text = text;
        _x = x;
        _y = y;
    }

    [Reactive]
    private string _name = string.Empty;

    [Reactive]
    private string _text;

    [Reactive]
    private int _x;

    [Reactive]
    private int _y;

    public override string ToString()
    {
        var textPreview = string.IsNullOrEmpty(_text) ? string.Empty : _text.Substring(0, Math.Min(_text.Length, 25));
        return $"[Sign: {textPreview}, ({X},{Y})]";
    }

    public Sign Copy()
    {
        return new Sign(_x, _y, _text);
    }
}
