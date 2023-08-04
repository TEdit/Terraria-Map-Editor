using System;

namespace TEdit.Terraria;

public class WorldValidationStatus
{
    /// <summary>
    /// All validations passed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Version &lt; current supported version 
    /// </summary>
    public bool IsLegacy { get; set; }

    /// <summary>
    /// .twld exists
    /// </summary>
    public bool IsTModLoader { get; set; }

    /// <summary>
    /// File uses compression
    /// </summary>
    public bool IsConsole { get; set; }

    /// <summary>
    /// Magic word is xindong instead of relogic
    /// </summary>
    public bool IsChinese { get; set; }

    /// <summary>
    /// World Version
    /// </summary>
    public uint Version { get; set; }

    /// <summary>
    /// Loader Version 0,1,2
    /// </summary>
    public byte LoaderVersion { get; set; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime LastSave { get; set; }

    /// <summary>
    /// Validion error message.
    /// </summary>
    public string Message { get; set; }
}

