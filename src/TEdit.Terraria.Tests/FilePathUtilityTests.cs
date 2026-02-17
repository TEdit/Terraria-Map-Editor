using Shouldly;
using TEdit.Common;

namespace TEdit.Terraria.Tests;

public class FilePathUtilityTests
{
    [Theory]
    [InlineData("MyWorld.wld", "MyWorld.wld")]
    [InlineData("MyWorld.wld.bak", "MyWorld.wld")]
    [InlineData("MyWorld.wld.TEdit", "MyWorld.wld")]
    [InlineData("MyWorld.wld.bak.TEdit", "MyWorld.wld")]
    [InlineData("MyWorld.wld.bak.TEdit.TEdit.TEdit", "MyWorld.wld")]
    [InlineData("MyWorld.wld.autosave", "MyWorld.wld")]
    [InlineData("MyWorld.wld.autosave.tmp", "MyWorld.wld")]
    [InlineData("MyWorld.wld.tmp", "MyWorld.wld")]
    [InlineData("SomeFile.txt", "SomeFile.txt")]
    [InlineData("NoExtension", "NoExtension")]
    public void NormalizeWorldFilePath_StripBackupExtensions(string input, string expected)
    {
        FilePathUtility.NormalizeWorldFilePath(input).ShouldBe(expected);
    }

    [Fact]
    public void NormalizeWorldFilePath_Null_ReturnsNull()
    {
        FilePathUtility.NormalizeWorldFilePath(null).ShouldBeNull();
    }

    [Fact]
    public void NormalizeWorldFilePath_Empty_ReturnsEmpty()
    {
        FilePathUtility.NormalizeWorldFilePath("").ShouldBe("");
    }

    [Theory]
    [InlineData("MyWorld.wld.bak", true)]
    [InlineData("MyWorld.wld.TEdit", true)]
    [InlineData("MyWorld.wld.autosave", true)]
    [InlineData("MyWorld.wld.tmp", true)]
    [InlineData("MyWorld.wld", false)]
    [InlineData("MyWorld.txt", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsBackupFile_DetectsCorrectly(string input, bool expected)
    {
        FilePathUtility.IsBackupFile(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(0L, "0 B")]
    [InlineData(1023L, "1023 B")]
    [InlineData(1024L, "1 KB")]
    [InlineData(1536L, "1.5 KB")]
    [InlineData(1048576L, "1 MB")]
    [InlineData(1073741824L, "1 GB")]
    public void FormatFileSize_FormatsCorrectly(long bytes, string expected)
    {
        FilePathUtility.FormatFileSize(bytes).ShouldBe(expected);
    }
}
