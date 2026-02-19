using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Common.IO;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a node in the NBT Explorer TreeView.
/// Supports TagCompound, List, byte[], int[], and primitive types.
/// </summary>
[IReactiveObject]
public partial class NbtNodeViewModel
{
    public string Name { get; init; } = string.Empty;
    public string TypeLabel { get; init; } = string.Empty;
    public string ValuePreview { get; init; } = string.Empty;
    public object? RawValue { get; set; }
    public bool IsLeaf { get; init; }
    public bool IsEditable { get; init; }

    [Reactive]
    private bool _isExpanded;

    [Reactive]
    private string _editableValue = string.Empty;

    public ObservableCollection<NbtNodeViewModel> Children { get; } = [];

    /// <summary>
    /// Display text for the node: "Name: Value" for leaves, "Name (Type, Count)" for containers.
    /// </summary>
    public string DisplayText => IsLeaf
        ? $"{Name}: {ValuePreview}"
        : $"{Name} ({TypeLabel}, {Children.Count})";

    /// <summary>
    /// Recursively builds an NbtNodeViewModel tree from a TagCompound key-value pair.
    /// </summary>
    public static NbtNodeViewModel FromTag(string key, object? value)
    {
        return value switch
        {
            TagCompound compound => FromCompound(key, compound),
            byte[] bytes => FromByteArray(key, bytes),
            int[] ints => FromIntArray(key, ints),
            IList list => FromList(key, list),
            _ => FromPrimitive(key, value)
        };
    }

    private static NbtNodeViewModel FromCompound(string key, TagCompound compound)
    {
        var node = new NbtNodeViewModel
        {
            Name = key,
            TypeLabel = "Compound",
            ValuePreview = string.Empty,
            RawValue = compound,
            IsLeaf = false,
            IsEditable = false,
        };

        foreach (var kvp in compound)
        {
            node.Children.Add(FromTag(kvp.Key, kvp.Value));
        }

        return node;
    }

    private static NbtNodeViewModel FromList(string key, IList list)
    {
        // Determine element type label
        string elementType = list.Count > 0 && list[0] != null
            ? GetTypeLabel(list[0])
            : "?";

        var node = new NbtNodeViewModel
        {
            Name = key,
            TypeLabel = $"List<{elementType}>",
            ValuePreview = string.Empty,
            RawValue = list,
            IsLeaf = false,
            IsEditable = false,
        };

        for (int i = 0; i < list.Count; i++)
        {
            node.Children.Add(FromTag($"[{i}]", list[i]));
        }

        return node;
    }

    private static NbtNodeViewModel FromByteArray(string key, byte[] bytes)
    {
        string preview = bytes.Length <= 16
            ? BitConverter.ToString(bytes).Replace("-", " ")
            : BitConverter.ToString(bytes, 0, 16).Replace("-", " ") + "...";

        return new NbtNodeViewModel
        {
            Name = key,
            TypeLabel = "Byte[]",
            ValuePreview = $"[{bytes.Length}] {preview}",
            RawValue = bytes,
            IsLeaf = true,
            IsEditable = false,
        };
    }

    private static NbtNodeViewModel FromIntArray(string key, int[] ints)
    {
        string preview = ints.Length <= 8
            ? string.Join(", ", ints)
            : string.Join(", ", ints[..8]) + "...";

        return new NbtNodeViewModel
        {
            Name = key,
            TypeLabel = "Int[]",
            ValuePreview = $"[{ints.Length}] {preview}",
            RawValue = ints,
            IsLeaf = true,
            IsEditable = false,
        };
    }

    private static NbtNodeViewModel FromPrimitive(string key, object? value)
    {
        string typeLabel = GetTypeLabel(value);
        string preview = value?.ToString() ?? "(null)";

        // Truncate long strings
        if (value is string s && s.Length > 100)
            preview = s[..100] + "...";

        return new NbtNodeViewModel
        {
            Name = key,
            TypeLabel = typeLabel,
            ValuePreview = preview,
            RawValue = value,
            IsLeaf = true,
            IsEditable = value is byte or short or int or long or float or double or string,
            EditableValue = value?.ToString() ?? string.Empty,
        };
    }

    private static string GetTypeLabel(object? value) => value switch
    {
        TagCompound => "Compound",
        byte => "Byte",
        short => "Short",
        int => "Int",
        long => "Long",
        float => "Float",
        double => "Double",
        string => "String",
        byte[] => "Byte[]",
        int[] => "Int[]",
        IList => "List",
        bool => "Bool",
        null => "null",
        _ => value.GetType().Name
    };

    /// <summary>
    /// Parses EditableValue back to the correct type and updates the parent TagCompound.
    /// </summary>
    public bool ApplyEdit(TagCompound parent)
    {
        if (!IsEditable || RawValue == null) return false;

        try
        {
            object? newValue = RawValue switch
            {
                byte => byte.Parse(EditableValue, CultureInfo.InvariantCulture),
                short => short.Parse(EditableValue, CultureInfo.InvariantCulture),
                int => int.Parse(EditableValue, CultureInfo.InvariantCulture),
                long => long.Parse(EditableValue, CultureInfo.InvariantCulture),
                float => float.Parse(EditableValue, CultureInfo.InvariantCulture),
                double => double.Parse(EditableValue, CultureInfo.InvariantCulture),
                string => EditableValue,
                _ => null
            };

            if (newValue == null) return false;

            parent.Set(Name, newValue);
            RawValue = newValue;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if this node or any descendant matches the search text.
    /// </summary>
    public bool MatchesSearch(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return true;

        if (Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            return true;

        if (IsLeaf && ValuePreview.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var child in Children)
        {
            if (child.MatchesSearch(searchText))
                return true;
        }

        return false;
    }
}
