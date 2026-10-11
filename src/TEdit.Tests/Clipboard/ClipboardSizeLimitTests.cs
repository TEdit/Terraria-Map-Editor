using System.IO;
using TEdit.Editor.Clipboard;
using Shouldly;
using Xunit;

namespace TEdit.Tests.Clipboard;

public class ClipboardSizeLimitTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(60000, 60000)]
    public void ValidateSize_CorruptSize_Throws(int x, int y)
    {
        Should.Throw<InvalidDataException>(() => ClipboardBuffer.ValidateSize(x, y));
    }

    [Fact]
    public void ValidateSize_LargeWorldSize_IsAllowed()
    {
        Should.NotThrow(() => ClipboardBuffer.ValidateSize(8400, 2400));
    }
}
