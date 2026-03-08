using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TEdit.Common.IO;

/// <summary>
/// Dictionary-based NBT compound tag, compatible with tModLoader's TagCompound.
/// Stores key→value pairs where values can be: byte, short, int, long, float, double,
/// byte[], string, List&lt;T&gt;, TagCompound, int[].
/// </summary>
public class TagCompound : IEnumerable<KeyValuePair<string, object>>
{
    private readonly Dictionary<string, object> _tags = new(StringComparer.Ordinal);

    public int Count => _tags.Count;

    public object this[string key]
    {
        get => _tags.TryGetValue(key, out var val) ? val : null;
        set
        {
            if (value == null)
                _tags.Remove(key);
            else
                _tags[key] = value;
        }
    }

    public bool ContainsKey(string key) => _tags.ContainsKey(key);

    public bool Remove(string key) => _tags.Remove(key);

    public void Set(string key, object value)
    {
        if (value == null)
            _tags.Remove(key);
        else
            _tags[key] = value;
    }

    public T Get<T>(string key)
    {
        if (!_tags.TryGetValue(key, out var val))
            return default;

        if (val is T typed)
            return typed;

        // Handle numeric conversions (tModLoader stores as exact types but callers may request wider types)
        try
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public byte GetByte(string key) => Get<byte>(key);
    public short GetShort(string key) => Get<short>(key);
    public int GetInt(string key) => Get<int>(key);
    public long GetLong(string key) => Get<long>(key);
    public float GetFloat(string key) => Get<float>(key);
    public double GetDouble(string key) => Get<double>(key);
    public string GetString(string key) => Get<string>(key) ?? string.Empty;
    public byte[] GetByteArray(string key) => Get<byte[]>(key);
    public int[] GetIntArray(string key) => Get<int[]>(key);

    public bool GetBool(string key)
    {
        if (!_tags.TryGetValue(key, out var val))
            return false;

        if (val is byte b)
            return b != 0;

        if (val is bool bv)
            return bv;

        return false;
    }

    public TagCompound GetCompound(string key)
    {
        return Get<TagCompound>(key) ?? new TagCompound();
    }

    public List<T> GetList<T>(string key)
    {
        if (!_tags.TryGetValue(key, out var val))
            return new List<T>();

        if (val is List<T> typed)
            return typed;

        // Handle IList with element conversion
        if (val is IList list)
        {
            var result = new List<T>(list.Count);
            foreach (var item in list)
            {
                if (item is T t)
                    result.Add(t);
                else
                {
                    try { result.Add((T)Convert.ChangeType(item, typeof(T))); }
                    catch { result.Add(default); }
                }
            }
            return result;
        }

        return new List<T>();
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _tags.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        return $"TagCompound({Count} entries)";
    }
}
