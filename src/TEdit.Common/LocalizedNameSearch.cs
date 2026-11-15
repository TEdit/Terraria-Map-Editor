using System;

namespace TEdit.Common;

public static class LocalizedNameSearch
{
    public static bool Matches(string? searchText, params string?[] names)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        foreach (var name in names)
        {
            if (name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                return true;
        }

        return false;
    }
}
