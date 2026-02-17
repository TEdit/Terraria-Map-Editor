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
