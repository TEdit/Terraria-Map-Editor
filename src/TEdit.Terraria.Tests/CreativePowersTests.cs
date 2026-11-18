using System.IO;
using Xunit;

namespace TEdit.Terraria.Tests;

public class CreativePowersTests
{
    [Theory]
    [InlineData(CreativePowerId.biomespread_setfrozen)]
    [InlineData(CreativePowerId.time_setfrozen)]
    [InlineData(CreativePowerId.rain_setfrozen)]
    [InlineData(CreativePowerId.wind_setfrozen)]
    public void FreezePower_MissingFromWorld_CanBeEnabledAndDisabledAcrossReloads(CreativePowerId id)
    {
        var powers = new CreativePowers();
        Assert.Null(powers.GetPowerBool(id));

        powers.SetPowerStateSafe(id, isEnabled: true);
        powers = Reload(powers);
        Assert.True(powers.GetPowerBool(id));

        powers.SetPowerStateSafe(id, isEnabled: false);
        powers = Reload(powers);
        Assert.False(powers.GetPowerBool(id));
    }

    [Fact]
    public void FreezePower_NullValue_DoesNotAddOrChangeSetting()
    {
        var powers = new CreativePowers();
        powers.SetPowerStateSafe(CreativePowerId.biomespread_setfrozen);
        Assert.Null(Reload(powers).GetPowerBool(CreativePowerId.biomespread_setfrozen));

        powers.SetPowerState(new CreativePowerArgs
        {
            Id = CreativePowerId.biomespread_setfrozen,
            IsActive = true
        });
        powers.SetPowerStateSafe(CreativePowerId.biomespread_setfrozen);
        Assert.True(Reload(powers).GetPowerBool(CreativePowerId.biomespread_setfrozen));
    }

    private static CreativePowers Reload(CreativePowers powers)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            World.SaveCreativePowers(powers, writer);
        }

        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var world = new World { Version = 326 };
        World.LoadCreativePowers(reader, world);
        Assert.Equal(stream.Length, stream.Position);
        return world.CreativePowers;
    }
}
