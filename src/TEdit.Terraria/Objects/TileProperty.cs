#nullable enable
using System.Linq;
using TEdit.Geometry;
using TEdit.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TEdit.Terraria.Objects;

/// <summary>
/// Axis for texture UV wrapping when tile variants exceed texture dimensions.
/// </summary>
public enum TextureWrapAxis
{
    None,   // No wrapping (default)
    U,      // Wrap on U-axis (horizontal) - most common
    V       // Wrap on V-axis (vertical) - e.g., lamps
}

/// <summary>
/// Configuration for texture UV wrapping when tile variants exceed texture dimensions.
/// The WrapThreshold is computed at runtime from actual texture dimensions.
/// </summary>
public class TextureWrap
{
    /// <summary>
    /// Which axis to wrap on (U = horizontal, V = vertical).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TextureWrapAxis Axis { get; set; } = TextureWrapAxis.None;

    /// <summary>
    /// How much to offset the perpendicular axis per wrap (e.g., 36 pixels).
    /// </summary>
    public short OffsetIncrement { get; set; }

    /// <summary>
    /// Optional: only apply wrapping when V equals this value (e.g., 18 for type 185).
    /// </summary>
    public short? ConditionalV { get; set; }

    /// <summary>
    /// Runtime-computed wrap threshold from texture.Width (U-axis) or texture.Height (V-axis).
    /// Cached after texture loading for performance.
    /// </summary>
    [JsonIgnore]
    public int WrapThreshold { get; set; }
}

public class TileProperty : ITile
{
    public override string ToString() => Name;

    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    [JsonPropertyOrder(2)]
    public TEditColor Color { get; set; }

    public Vector2Short TextureGrid { get; set; } = new Vector2Short(16, 16);
    public Vector2Short FrameGap { get; set; } = new Vector2Short(2, 2);

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FramePlacement Placement { get; set; }

    public bool IsAnimated { get; set; }
    public bool IsLight { get; set; }
    public bool IsSolidTop { get; set; }
    public bool IsSolid { get; set; }
    public bool SaveSlope { get; set; }

    [JsonIgnore]
    public bool HasSlopes => IsSolid || SaveSlope;

    public List<FrameProperty>? Frames { get; set; }
    public Vector2Short[]? FrameSize { get; set; } = [new Vector2Short(1, 1)];

    public bool IsFramed { get; set; }
    public bool IsGrass { get; set; }
    public bool IsPlatform { get; set; }
    public bool IsCactus { get; set; }
    public bool IsStone { get; set; }
    public bool CanBlend { get; set; }
    public int? MergeWith { get; set; }
    public string? FrameNameSuffix { get; set; }
    public TextureWrap? TextureWrap { get; set; }

    public bool Merges(int other)
    {
        if (other == this.Id) return true;

        if (!MergeWith.HasValue) return false;

        return MergeWith.Value == other;
    }

    public bool Merges(TileProperty other)
    {
        if (other.MergeWith.HasValue && other.MergeWith.Value == Id) return true;
        if (MergeWith.HasValue && MergeWith.Value == other.Id) return true;
        if (MergeWith.HasValue && other.MergeWith.HasValue && MergeWith.Value == other.MergeWith.Value) return true;

        return false;
    }

    public int GetFrameSizeIndex(short v)
    {
        if (FrameSize == null || FrameSize.Length <= 1)
            return 0;

        int row = v / TextureGrid.Y;
        int rowTest = 0;

        for (int pos = 0; pos < FrameSize.Length; pos++)
        {
            if (row == rowTest)
            {
                return pos;
            }

            rowTest += FrameSize[pos].Y;
        }
        return FrameSize.Length - 1;
    }

    public Vector2Short GetFrameSize(short v) => FrameSize?[GetFrameSizeIndex(v)] ?? new Vector2Short(1, 1);

    public bool IsOrigin(Vector2Short uv)
    {
        if (uv == Vector2Short.Zero) return true;
        if (FrameSize == null) return false;

        var renderUV = GetRenderUV((ushort)Id, uv.X, uv.Y);
        var frameSizeIx = GetFrameSizeIndex((short)renderUV.Y);
        var frameSize = FrameSize[frameSizeIx];

        if (frameSizeIx == 0)
        {
            return (renderUV.X % ((TextureGrid.X + FrameGap.X) * frameSize.X) == 0 &&
                    renderUV.Y % ((TextureGrid.Y + FrameGap.Y) * frameSize.Y) == 0);
        }
        else
        {
            int y = 0;
            for (int i = 0; i < frameSizeIx; i++)
            {
                y += FrameSize[i].Y * (TextureGrid.Y + FrameGap.Y);
            }
            return (renderUV.X % ((TextureGrid.X + FrameGap.X) * frameSize.X) == 0 && renderUV.Y == y);
        }
    }


    public bool IsOrigin(Vector2Short uv, out FrameProperty? frame)
    {
        frame = Frames?.FirstOrDefault(f => f.UV == uv);

        return frame != null;
    }

    public static Vector2Short GetWorldUV(ushort type, ushort U, ushort V)
    {
        int worldU = U;
        int worldV = V;

        if (type < WorldConfiguration.TileProperties.Count)
        {
            var tile = WorldConfiguration.TileProperties[type];
            var wrap = tile.TextureWrap;

            if (wrap != null && wrap.Axis != TextureWrapAxis.None && wrap.WrapThreshold > 0)
            {
                if (wrap.Axis == TextureWrapAxis.U)
                {
                    // For conditional wraps, check against wrapped V value
                    if (wrap.ConditionalV.HasValue)
                    {
                        int wrapCount = (V - wrap.ConditionalV.Value) / wrap.OffsetIncrement;
                        if (wrapCount > 0)
                        {
                            worldU += wrap.WrapThreshold * wrapCount;
                            worldV -= wrap.OffsetIncrement * wrapCount;
                        }
                    }
                    else
                    {
                        int wrapCount = V / wrap.OffsetIncrement;
                        worldU += wrap.WrapThreshold * wrapCount;
                        worldV -= wrap.OffsetIncrement * wrapCount;
                    }
                }
                else // TextureWrapAxis.V
                {
                    int wrapCount = U / wrap.OffsetIncrement;
                    worldU -= wrap.OffsetIncrement * wrapCount;
                    worldV += wrap.WrapThreshold * wrapCount;
                }
            }
        }

        return new Vector2Short((short)worldU, (short)worldV);
    }


    public static Vector2Short GetRenderUV(ushort type, short U, short V)
    {
        int renderU = U;
        int renderV = V;

        if (type < WorldConfiguration.TileProperties.Count)
        {
            var tile = WorldConfiguration.TileProperties[type];
            var wrap = tile.TextureWrap;

            if (wrap != null && wrap.Axis != TextureWrapAxis.None && wrap.WrapThreshold > 0)
            {
                // Check conditional constraint (e.g., type 185 only when V == 18)
                if (wrap.ConditionalV.HasValue && V != wrap.ConditionalV.Value)
                    return new Vector2Short((short)renderU, (short)renderV);

                if (wrap.Axis == TextureWrapAxis.U)
                {
                    int wrapCount = U / wrap.WrapThreshold;
                    renderU -= wrap.WrapThreshold * wrapCount;
                    renderV += wrap.OffsetIncrement * wrapCount;
                }
                else // TextureWrapAxis.V
                {
                    int wrapCount = V / wrap.WrapThreshold;
                    renderU += wrap.OffsetIncrement * wrapCount;
                    renderV -= wrap.WrapThreshold * wrapCount;
                }
            }
        }

        return new Vector2Short((short)renderU, (short)renderV);
    }
}
