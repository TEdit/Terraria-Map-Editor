using System;
using System.Collections.Generic;
using System.IO;

namespace TEdit.Common;

/// <summary>
/// Pure utility methods for file path manipulation. No WPF dependencies.
/// </summary>
public static class FilePathUtility
{
    private static readonly HashSet<string> BackupExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".tedit", ".bak", ".autosave", ".tmp"
    };

    /// <summary>
    /// Strips backup extensions (.TEdit, .bak, .autosave, .tmp) iteratively until a .wld file remains.
    /// </summary>
    public static string NormalizeWorldFilePath(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return filename;

        string result = filename;
        while (true)
        {
            string ext = Path.GetExtension(result);
            if (string.IsNullOrEmpty(ext) || !BackupExtensions.Contains(ext))
                break;
            result = Path.ChangeExtension(result, null);
        }

        return result;
    }

    /// <summary>
    /// Returns true if the file has a backup extension (.TEdit, .bak, .autosave, .tmp).
    /// </summary>
    public static bool IsBackupFile(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return false;

        string ext = Path.GetExtension(filename);
        return !string.IsNullOrEmpty(ext) && BackupExtensions.Contains(ext);
    }

    /// <summary>
    /// Name for a world's backups and autosaves: the file stem plus a short hash of the full path,
    /// e.g. "World_1.1a2b3c4d". The hash keeps worlds with the same name in different folders
    /// (local and Steam Cloud) from overwriting or pruning each other's backups.
    /// </summary>
    public static string GetBackupKey(string worldPath)
    {
        string fullPath = Path.GetFullPath(NormalizeWorldFilePath(worldPath));
        // Windows paths are case-insensitive: the same world must always get the same key.
        // netstandard2.0-compatible: no SHA256.HashData / Convert.ToHexString.
        using var sha = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(fullPath.ToUpperInvariant()));
        string hex = BitConverter.ToString(hash, 0, 4).Replace("-", "").ToLowerInvariant();
        return $"{Path.GetFileNameWithoutExtension(fullPath)}.{hex}";
    }

    /// <summary>
    /// Formats a byte count as a human-readable string.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
