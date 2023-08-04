namespace TEdit.Editor.Clipboard;

public class PasteOptions
{
    public bool PasteEmpty { get; set; } = true;
    public bool PasteTiles { get; set; } = true;
    public bool PasteWalls { get; set; } = true;
    public bool PasteLiquids { get; set; } = true;
    public bool PasteWires { get; set; } = true;
    public bool PasteSprites { get; set; } = true;
    public bool PasteOverTiles { get; set; } = true;

    public bool HasAction => PasteTiles || PasteWalls || PasteLiquids || PasteWires || PasteSprites || PasteEmpty;
}
