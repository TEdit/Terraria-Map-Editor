using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class LegacyContainerLimitTests
{
    private static Chest[] Chests(int n) => Enumerable.Range(0, n).Select(i => new Chest(i, 0)).ToArray();
    private static Sign[] Signs(int n) => Enumerable.Range(0, n).Select(i => new Sign(i, 0, "s")).ToArray();

    [Fact]
    public void Save_MoreThanTheOldFormatHolds_FailsInsteadOfDropping()
    {
        using var bw = new BinaryWriter(new MemoryStream());

        Should.Throw<InvalidOperationException>(() => World.SaveChests(Chests(1001), bw, 200));
        Should.Throw<InvalidOperationException>(() => World.SaveSigns(Signs(1001), bw, 200));
        Should.Throw<InvalidOperationException>(() => World.WriteChestDataToStreamV1(Chests(1001), bw, 71));
        Should.Throw<InvalidOperationException>(() => World.WriteSignDataToStreamV1(Signs(1001), bw, 71));
    }

    [Fact]
    public void Save_AtTheLimit_Works()
    {
        using var bw = new BinaryWriter(new MemoryStream());

        Should.NotThrow(() => World.SaveSigns(Signs(1000), bw, 200));
        Should.NotThrow(() => World.SaveSigns(Signs(1001), bw, 279));
    }
}
