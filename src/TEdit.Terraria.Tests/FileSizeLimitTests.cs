using System.IO;
using System.Linq;
using TEdit.Common.Exceptions;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class FileSizeLimitTests
{
    [Fact]
    public void LoadChestData_HugeMaxItems_ThrowsInsteadOfAllocating()
    {
        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            bw.Write((short)1);       // one chest
            bw.Write(10);             // x
            bw.Write(20);             // y
            bw.Write("");             // name
            bw.Write(int.MaxValue);   // corrupt MaxItems
        }
        ms.Position = 0;
        using var br = new BinaryReader(ms);

        Should.Throw<TEditFileFormatException>(() => World.LoadChestData(br, 294).ToList());
    }
}
