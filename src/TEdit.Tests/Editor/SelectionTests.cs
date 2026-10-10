using System.Collections.Generic;
using System.Linq;
using TEdit.Editor;
using TEdit.Geometry;
using Xunit;

namespace TEdit.Tests.Editor;

public class SelectionTests
{
    private static Vector2Int32 P(int x, int y) => new(x, y);

    private static HashSet<Vector2Int32> SelectedTiles(Selection selection)
    {
        var result = new HashSet<Vector2Int32>();
        var area = selection.SelectionArea;
        for (int x = area.Left; x < area.Right; x++)
            for (int y = area.Top; y < area.Bottom; y++)
                if (selection.IsValid(x, y))
                    result.Add(P(x, y));
        return result;
    }

    private static Selection Rectangle(int x1, int y1, int x2, int y2)
    {
        var selection = new Selection();
        selection.SetRectangle(P(x1, y1), P(x2, y2));
        return selection;
    }

    // ── Rectangle behavior (unchanged) ──────────────────────────────

    [Fact]
    public void Defaults_AreInactiveRectangleMode()
    {
        var selection = new Selection();

        Assert.False(selection.IsActive);
        Assert.False(selection.HasMask);
        Assert.Equal(SelectionShape.Rectangle, selection.Mode);
    }

    [Fact]
    public void IsValid_WhenInactive_AllowsEverything()
    {
        var selection = new Selection();

        Assert.True(selection.IsValid(-5, -5));
        Assert.True(selection.IsValid(1000, 1000));
    }

    [Fact]
    public void SetRectangle_SelectsInclusiveRectangle()
    {
        var selection = Rectangle(5, 6, 2, 3);

        Assert.True(selection.IsActive);
        Assert.False(selection.HasMask);
        Assert.Equal(new RectangleInt32(2, 3, 4, 4), selection.SelectionArea);
        Assert.True(selection.IsValid(2, 3));
        Assert.True(selection.IsValid(5, 6));
        Assert.False(selection.IsValid(6, 6));
        Assert.False(selection.IsValid(1, 3));
    }

    // ── Adding tiles (brush) ────────────────────────────────────────

    [Fact]
    public void SetTiles_OnInactiveSelection_SelectsOnlyThoseTiles()
    {
        var selection = new Selection();

        selection.SetTiles(new[] { P(10, 10), P(12, 11) }, selected: true);

        Assert.True(selection.IsActive);
        Assert.True(selection.HasMask);
        Assert.Equal(new RectangleInt32(10, 10, 3, 2), selection.SelectionArea);
        Assert.Equal(new HashSet<Vector2Int32> { P(10, 10), P(12, 11) }, SelectedTiles(selection));
        Assert.False(selection.IsValid(11, 10));
        Assert.False(selection.IsValid(9, 10));
    }

    [Fact]
    public void SetTiles_DisjointRegions_KeepsGapUnselected()
    {
        var selection = new Selection();

        selection.SetTiles(new[] { P(0, 0), P(1, 0) }, selected: true);
        selection.SetTiles(new[] { P(20, 5) }, selected: true);

        Assert.Equal(new RectangleInt32(0, 0, 21, 6), selection.SelectionArea);
        Assert.Equal(new HashSet<Vector2Int32> { P(0, 0), P(1, 0), P(20, 5) }, SelectedTiles(selection));
    }

    [Fact]
    public void SetTiles_AddingToRectangle_KeepsRectangleAndAddsTiles()
    {
        var selection = Rectangle(0, 0, 1, 1);

        selection.SetTiles(new[] { P(5, 5) }, selected: true);

        Assert.True(selection.HasMask);
        Assert.Equal(new HashSet<Vector2Int32> { P(0, 0), P(1, 0), P(0, 1), P(1, 1), P(5, 5) }, SelectedTiles(selection));
    }

    [Fact]
    public void SetTiles_GrowingInEveryDirection_PreservesExistingMask()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(10, 10) }, selected: true);

        selection.SetTiles(new[] { P(8, 9) }, selected: true);   // grows up-left
        selection.SetTiles(new[] { P(13, 14) }, selected: true); // grows down-right

        Assert.Equal(new RectangleInt32(8, 9, 6, 6), selection.SelectionArea);
        Assert.Equal(new HashSet<Vector2Int32> { P(10, 10), P(8, 9), P(13, 14) }, SelectedTiles(selection));
    }

    [Fact]
    public void SetTiles_InsideExistingBounds_DoesNotChangeArea()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0), P(4, 4) }, selected: true);
        var area = selection.SelectionArea;

        selection.SetTiles(new[] { P(2, 2) }, selected: true);

        Assert.Equal(area, selection.SelectionArea);
        Assert.Equal(new HashSet<Vector2Int32> { P(0, 0), P(4, 4), P(2, 2) }, SelectedTiles(selection));
    }

    [Fact]
    public void SetTiles_DuplicatePoints_AreHandled()
    {
        var selection = new Selection();

        selection.SetTiles(new[] { P(3, 3), P(3, 3), P(3, 3) }, selected: true);

        Assert.Equal(new RectangleInt32(3, 3, 1, 1), selection.SelectionArea);
        Assert.Single(SelectedTiles(selection));
    }

    [Fact]
    public void SetTiles_EmptyPoints_IsNoOp()
    {
        var selection = Rectangle(0, 0, 2, 2);

        selection.SetTiles(new Vector2Int32[0], selected: true);

        Assert.True(selection.IsActive);
        Assert.False(selection.HasMask);
        Assert.Equal(new RectangleInt32(0, 0, 3, 3), selection.SelectionArea);
    }

    // ── Removing tiles ──────────────────────────────────────────────

    [Fact]
    public void SetTiles_RemovingFromRectangle_CutsHole()
    {
        var selection = Rectangle(0, 0, 2, 2);

        selection.SetTiles(new[] { P(1, 1) }, selected: false);

        Assert.True(selection.IsActive);
        Assert.True(selection.HasMask);
        Assert.Equal(new RectangleInt32(0, 0, 3, 3), selection.SelectionArea);
        Assert.False(selection.IsValid(1, 1));
        Assert.Equal(8, SelectedTiles(selection).Count);
    }

    [Fact]
    public void SetTiles_RemovingOutsideSelection_ChangesNothing()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0) }, selected: true);

        selection.SetTiles(new[] { P(50, 50) }, selected: false);

        Assert.Equal(new RectangleInt32(0, 0, 1, 1), selection.SelectionArea);
        Assert.True(selection.IsValid(0, 0));
    }

    [Fact]
    public void SetTiles_RemovingEverything_DeactivatesSelection()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0), P(1, 0) }, selected: true);

        selection.SetTiles(new[] { P(0, 0), P(1, 0) }, selected: false);

        Assert.False(selection.IsActive);
        Assert.False(selection.HasMask);
    }

    [Fact]
    public void SetTiles_RemovingFromInactiveSelection_IsNoOp()
    {
        var selection = new Selection();

        selection.SetTiles(new[] { P(1, 1) }, selected: false);

        Assert.False(selection.IsActive);
        Assert.False(selection.HasMask);
    }

    // ── Clipping ────────────────────────────────────────────────────

    [Fact]
    public void SetTiles_Clip_DropsPointsOutsideWorld()
    {
        var selection = new Selection();
        var world = new RectangleInt32(0, 0, 10, 10);

        selection.SetTiles(new[] { P(-1, 0), P(0, 0), P(9, 9), P(10, 9) }, selected: true, clip: world);

        Assert.Equal(new RectangleInt32(0, 0, 10, 10), selection.SelectionArea);
        Assert.Equal(new HashSet<Vector2Int32> { P(0, 0), P(9, 9) }, SelectedTiles(selection));
    }

    [Fact]
    public void SetTiles_AllPointsClipped_IsNoOp()
    {
        var selection = new Selection();

        selection.SetTiles(new[] { P(-3, -3) }, selected: true, clip: new RectangleInt32(0, 0, 10, 10));

        Assert.False(selection.IsActive);
    }

    // ── Interaction with rectangle edits ────────────────────────────

    [Fact]
    public void MovingSelectionArea_KeepsMaskShape()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0), P(2, 1) }, selected: true);

        var area = selection.SelectionArea;
        area.Offset(5, 5);
        selection.SelectionArea = area;

        Assert.True(selection.HasMask);
        Assert.Equal(new HashSet<Vector2Int32> { P(5, 5), P(7, 6) }, SelectedTiles(selection));
    }

    [Fact]
    public void ResizingSelectionArea_DropsMask()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0), P(2, 1) }, selected: true);

        selection.SelectionArea = new RectangleInt32(0, 0, 4, 4);

        Assert.False(selection.HasMask);
        Assert.True(selection.IsValid(1, 1));
    }

    [Fact]
    public void SetRectangle_ReplacesFreeFormSelection()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0), P(2, 2) }, selected: true);

        // Same size as the masked area: the mask must still be dropped.
        selection.SetRectangle(P(0, 0), P(2, 2));

        Assert.False(selection.HasMask);
        Assert.Equal(9, SelectedTiles(selection).Count);
    }

    [Fact]
    public void Deactivating_DropsMask()
    {
        var selection = new Selection();
        selection.SetTiles(new[] { P(0, 0), P(2, 2) }, selected: true);

        selection.IsActive = false;
        selection.IsActive = true;

        Assert.False(selection.HasMask);
        Assert.Equal(9, SelectedTiles(selection).Count);
    }

    [Fact]
    public void AddingAfterDeactivate_StartsNewSelection()
    {
        var selection = Rectangle(0, 0, 9, 9);
        selection.IsActive = false;

        selection.SetTiles(new[] { P(50, 50) }, selected: true);

        Assert.Equal(new RectangleInt32(50, 50, 1, 1), selection.SelectionArea);
        Assert.Single(SelectedTiles(selection));
    }

    // ── Lasso ───────────────────────────────────────────────────────

    [Fact]
    public void SetLasso_Square_SelectsInteriorAndEdges()
    {
        var selection = new Selection();

        selection.SetLasso(new[] { P(0, 0), P(4, 0), P(4, 4), P(0, 4) }, selected: true);

        Assert.Equal(new RectangleInt32(0, 0, 5, 5), selection.SelectionArea);
        Assert.Equal(25, SelectedTiles(selection).Count);
    }

    [Fact]
    public void SetLasso_Triangle_LeavesOutsideCornerUnselected()
    {
        var selection = new Selection();

        selection.SetLasso(new[] { P(0, 0), P(10, 0), P(0, 10) }, selected: true);

        Assert.True(selection.IsValid(0, 0));
        Assert.True(selection.IsValid(2, 2));
        Assert.True(selection.IsValid(10, 0));
        Assert.True(selection.IsValid(0, 10));
        Assert.False(selection.IsValid(9, 9));
        Assert.False(selection.IsValid(10, 10));
    }

    [Fact]
    public void SetLasso_Subtract_CutsShapeOutOfRectangle()
    {
        var selection = Rectangle(0, 0, 9, 9);

        selection.SetLasso(new[] { P(3, 3), P(6, 3), P(6, 6), P(3, 6) }, selected: false);

        Assert.False(selection.IsValid(4, 4));
        Assert.False(selection.IsValid(3, 3));
        Assert.True(selection.IsValid(2, 2));
        Assert.True(selection.IsValid(7, 7));
        Assert.Equal(100 - 16, SelectedTiles(selection).Count);
    }

    [Fact]
    public void SetLasso_ClippedToWorld()
    {
        var selection = new Selection();

        selection.SetLasso(new[] { P(-5, -5), P(5, -5), P(5, 5), P(-5, 5) }, selected: true,
            clip: new RectangleInt32(0, 0, 100, 100));

        Assert.Equal(new RectangleInt32(0, 0, 6, 6), selection.SelectionArea);
        Assert.Equal(36, SelectedTiles(selection).Count);
    }

    [Fact]
    public void SetLasso_EmptyOrNullPath_IsNoOp()
    {
        var selection = new Selection();

        selection.SetLasso(null, selected: true);
        selection.SetLasso(new List<Vector2Int32>(), selected: true);

        Assert.False(selection.IsActive);
    }

    // ── Notifications ───────────────────────────────────────────────

    [Fact]
    public void SetTiles_RaisesSelectionAreaChanged_WithMaskReadyInHandler()
    {
        var selection = new Selection();
        var seen = new List<bool>();
        selection.PropertyChanged += (_, e) =>
        {
            // Handlers must never observe a mask that does not match the area.
            if (e.PropertyName == nameof(Selection.SelectionArea))
                seen.Add(selection.IsValid(7, 3));
        };

        selection.SetTiles(new[] { P(7, 3), P(9, 9) }, selected: true);

        Assert.Equal(new[] { true }, seen);
    }

    [Fact]
    public void Mode_RaisesPropertyChanged()
    {
        var selection = new Selection();
        var changed = new List<string>();
        selection.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        selection.Mode = SelectionShape.Lasso;

        Assert.Contains(nameof(Selection.Mode), changed);
        Assert.Equal(SelectionShape.Lasso, selection.Mode);
    }

    [Fact]
    public void LargeBrushStroke_StaysConsistent()
    {
        var selection = new Selection();
        var brush = new BrushSettings { Width = 9, Height = 9, Shape = BrushShape.Round };

        var expected = new HashSet<Vector2Int32>();
        for (int x = 0; x < 200; x += 3)
        {
            var stamp = brush.GetShapePoints(P(x, 50 + x % 7));
            expected.UnionWith(stamp);
            selection.SetTiles(stamp, selected: true);
        }

        Assert.Equal(expected, SelectedTiles(selection));
    }
}
