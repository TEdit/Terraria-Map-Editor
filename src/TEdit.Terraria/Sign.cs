using System;
using TEdit.Common.Reactive;

namespace TEdit.Terraria;

public class Sign : ObservableObject
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

    private string _name = string.Empty;
    private int _signId = -1;

   

    public string Name
    {
        get { return _name; }
        set { Set(nameof(Name), ref _name, value); }
    }

    private string _text;
    public string Text
    {
        get { return _text; }
        set { Set(nameof(Text), ref _text, value); }
    }

    private int _x;
    private int _y;
  

    public int Y
    {
        get { return _y; }
        set { Set(nameof(Y), ref _y, value); }
    } 

    public int X
    {
        get { return _x; }
        set { Set(nameof(X), ref _x, value); }
    }


    public override string ToString()
    {
        return $"[Sign: {Text.Substring(0, Math.Max(25, Text.Length))}[{Text.Length}], ({X},{Y})]";
    }


    public Sign Copy()
    {
        return new Sign(_x, _y, _text);
    }
}
