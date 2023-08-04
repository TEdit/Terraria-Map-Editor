using System;
using System.Collections.Generic;

namespace TEdit.Terraria;

public class TownNpcKey : IEquatable<TownNpcKey>
{
    public TownNpcKey()
    {

    }

    public TownNpcKey(string name, int variant, bool isPartying, bool isShimmered)
    {
        Name = name;
        IsPartying = isPartying;
        IsShimmered = isShimmered;
        Variant = variant;
    }

    public int Variant { get; set;}
    public string Name { get; set; }
    public bool IsPartying { get; set; }
    public bool IsShimmered { get; set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as TownNpcKey);
    }

    public bool Equals(TownNpcKey other)
    {
        return other is not null &&
               Variant == other.Variant &&
               Name == other.Name &&
               IsPartying == other.IsPartying &&
               IsShimmered == other.IsShimmered;
    }

    public override int GetHashCode()
    {
        int hashCode = -1661845228;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + Variant.GetHashCode();
        hashCode = hashCode * -1521134295 + IsPartying.GetHashCode();
        hashCode = hashCode * -1521134295 + IsShimmered.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(TownNpcKey left, TownNpcKey right)
    {
        return EqualityComparer<TownNpcKey>.Default.Equals(left, right);
    }

    public static bool operator !=(TownNpcKey left, TownNpcKey right)
    {
        return !(left == right);
    }
}
