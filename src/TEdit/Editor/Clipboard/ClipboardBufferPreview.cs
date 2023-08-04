using System.Windows.Media.Imaging;

namespace TEdit.Editor.Clipboard;

public class ClipboardBufferPreview
{
    public ClipboardBufferPreview(ClipboardBuffer buffer)
    {
        Buffer = buffer;
        Preview = ClipboardBufferRenderer.RenderBuffer(buffer, out var scale);
        PreviewScale = scale;
    }

    public ClipboardBuffer Buffer { get; }
    public WriteableBitmap Preview { get; }
    public double PreviewScale { get; }
}
