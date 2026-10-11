using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using Shouldly;
using Xunit;

namespace TEdit.Tests.Clipboard;

public class RotationLossTests
{
    [Fact]
    public void DescribeRotationLoss_NoContainers_IsNull()
    {
        new ClipboardBuffer(new Vector2Int32(4, 4)).DescribeRotationLoss().ShouldBeNull();
    }

    [Fact]
    public void DescribeRotationLoss_ListsWhatRotationDrops()
    {
        var buffer = new ClipboardBuffer(new Vector2Int32(4, 4));
        buffer.Chests.Add(new Chest(0, 0));
        buffer.Chests.Add(new Chest(2, 0));
        buffer.Signs.Add(new Sign(0, 2, "hi"));

        buffer.DescribeRotationLoss().ShouldBe("2 chests, 1 sign");
    }
}
