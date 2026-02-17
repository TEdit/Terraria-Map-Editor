using TEdit.Editor;

namespace TEdit.Scripting.Api;

public class SelectionApi
{
    private readonly ISelection _selection;

    public SelectionApi(ISelection selection)
    {
        _selection = selection;
    }

    public bool IsActive => _selection.IsActive;
    public int X => _selection.SelectionArea.X;
    public int Y => _selection.SelectionArea.Y;
    public int Width => _selection.SelectionArea.Width;
    public int Height => _selection.SelectionArea.Height;
    public int Left => _selection.SelectionArea.Left;
    public int Top => _selection.SelectionArea.Top;
    public int Right => _selection.SelectionArea.Right;
    public int Bottom => _selection.SelectionArea.Bottom;

    public void Set(int x, int y, int width, int height)
    {
        _selection.SetRectangle(
            new Geometry.Vector2Int32(x, y),
            new Geometry.Vector2Int32(x + width, y + height));
        _selection.IsActive = true;
    }

    public void Clear()
    {
        _selection.IsActive = false;
    }

    public bool Contains(int x, int y) => _selection.IsValid(x, y);
}
