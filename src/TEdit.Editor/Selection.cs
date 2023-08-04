using System;
using TEdit.Common.Reactive;
using TEdit.Geometry;

namespace TEdit.Editor;

public class Selection : ObservableObject, ISelection
{
    private RectangleInt32 _selectionArea = new RectangleInt32(0, 0, 0, 0);
    private bool _isActive;

    public bool IsActive
    {
        get { return _isActive; }
        set { Set(nameof(IsActive), ref _isActive, value); }
    }

    public bool IsValid(Vector2Int32 p)
    {
       return IsValid(p.X, p.Y);
    }
    public bool IsValid(int x, int y)
    {
        if (!IsActive)
            return true;

        return SelectionArea.Contains(x, y);
    }

    public RectangleInt32 SelectionArea
    {
        get { return _selectionArea; }
        set { Set(nameof(SelectionArea), ref _selectionArea, value); }
    }

    public void SetRectangle(Vector2Int32 p1, Vector2Int32 p2)
    {
        int x1 = p1.X < p2.X ? p1.X : p2.X;
        int y1 = p1.Y < p2.Y ? p1.Y : p2.Y;
        int width = Math.Abs(p2.X - p1.X) + 1;
        int height = Math.Abs(p2.Y - p1.Y) + 1;

        SelectionArea = new RectangleInt32(x1, y1, width, height);
        IsActive = true;
    }
}
