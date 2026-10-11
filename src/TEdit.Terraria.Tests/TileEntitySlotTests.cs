using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace TEdit.Terraria.Tests;

public class TileEntitySlotTests
{
    [Theory]
    [InlineData((int)TileType.HatRack)]
    [InlineData((int)TileType.DisplayDoll)]
    public void CreateForTile_GivesEverySlotItsOwnItem(int tileType)
    {
        var tile = new Tile { IsActive = true, Type = (ushort)tileType };

        var entity = TileEntity.CreateForTile(tile, 10, 10, 0);

        AssertDistinct(entity.Items);
        AssertDistinct(entity.Dyes);

        // Editing one slot must not change the others.
        entity.Items[0].Id = 1;
        entity.Items[1].Id.ShouldBe((short)0);
    }

    private static void AssertDistinct(IList<TileEntityItem> slots)
    {
        slots.Count.ShouldBeGreaterThan(1);
        slots.Distinct(ReferenceEqualityComparer.Instance).Count().ShouldBe(slots.Count);
    }
}
