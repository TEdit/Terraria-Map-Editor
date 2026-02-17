namespace TEdit.Editor.Plugins;

/// <summary>
/// Scaling interpolation mode for pixel art conversion.
/// </summary>
public enum ScalingMode
{
    Bilinear,
    NearestNeighbor,
    Bicubic,
    Lanczos,
    Hermite,
    Spline,
    Gaussian
}

/// <summary>
/// Rotation angle for schematic output.
/// </summary>
public enum RotationAngle
{
    None,
    Rotate90,
    Rotate180,
    Rotate270
}

/// <summary>
/// World axis orientation for schematic.
/// </summary>
public enum WorldAxis
{
    XAxis,
    YAxis
}
