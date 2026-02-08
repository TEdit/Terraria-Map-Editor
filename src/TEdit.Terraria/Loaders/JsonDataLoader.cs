using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.Loaders;

public static class JsonDataLoader
{
    private static readonly Assembly _assembly = typeof(JsonDataLoader).Assembly;

    /// <summary>
    /// Deserialize a list of T from a JSON stream.
    /// </summary>
    public static List<T> LoadList<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<List<T>>(stream, TEditJsonSerializer.DefaultOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize list of {typeof(T).Name}.");
    }

    /// <summary>
    /// Deserialize a single T from a JSON stream.
    /// </summary>
    public static T Load<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream, TEditJsonSerializer.DefaultOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}.");
    }

    /// <summary>
    /// Load from an embedded resource, with optional filesystem override.
    /// </summary>
    public static Stream GetDataStream(string resourceName, string? dataPath = null)
    {
        // Try filesystem override first
        if (dataPath != null)
        {
            var filePath = Path.Combine(dataPath, resourceName);
            if (File.Exists(filePath))
                return File.OpenRead(filePath);
        }

        // Fall back to embedded resource
        var fullResourceName = $"TEdit.Terraria.Data.{resourceName}";
        var stream = _assembly.GetManifestResourceStream(fullResourceName);
        if (stream != null)
            return stream;

        throw new FileNotFoundException(
            $"Data file '{resourceName}' not found as embedded resource '{fullResourceName}' or in data path '{dataPath}'.");
    }

    /// <summary>
    /// Load a list from embedded resource or filesystem.
    /// </summary>
    public static List<T> LoadListFromResource<T>(string resourceName, string? dataPath = null)
    {
        using var stream = GetDataStream(resourceName, dataPath);
        return LoadList<T>(stream);
    }

    /// <summary>
    /// Load a single object from embedded resource or filesystem.
    /// </summary>
    public static T LoadFromResource<T>(string resourceName, string? dataPath = null)
    {
        using var stream = GetDataStream(resourceName, dataPath);
        return Load<T>(stream);
    }

    /// <summary>
    /// Serialize data to JSON and write to a file.
    /// </summary>
    public static void SaveToFile<T>(T data, string filePath)
    {
        using var stream = File.Create(filePath);
        JsonSerializer.Serialize(stream, data, TEditJsonSerializer.DefaultOptions);
    }
}
