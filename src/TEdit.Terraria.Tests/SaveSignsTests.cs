using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class SaveSignsTests
{
    [Fact]
    public void SaveSigns_NullText_WritesEveryCountedSign()
    {
        var signs = new List<Sign>
        {
            new(1, 2, "first"),
            new() { X = 3, Y = 4, Text = null },
            new(5, 6, "last"),
        };

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            World.SaveSigns(signs, bw, 279);
            bw.Write(0x7EADBEEF); // marker for the next section
        }

        ms.Position = 0;
        using var br = new BinaryReader(ms);
        var loaded = World.LoadSignData(br).ToList();

        loaded.Select(s => s.Text).ShouldBe(["first", "", "last"]);
        loaded[2].X.ShouldBe(5);
        br.ReadInt32().ShouldBe(0x7EADBEEF, "the next section must start right after the signs");
    }
}
