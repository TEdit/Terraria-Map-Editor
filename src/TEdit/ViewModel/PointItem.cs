namespace TEdit.ViewModel;

/// <summary>A special-interest point. Name is the stable identity used for matching; Label is the localized display text.</summary>
public sealed record PointItem(string Name, string Label);