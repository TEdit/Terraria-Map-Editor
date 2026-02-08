namespace TEdit.Configuration;

public class AppSettings
{
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 768;
    public int ClipboardRenderSize { get; set; } = 512;
    public string TerrariaContentPath { get; set; }
    public int? SteamUserId { get; set; }
}
