using System.Text.Json;
using Shouldly;
using TEdit.Configuration;
using Xunit;

namespace TEdit.Tests.Configuration;

public class UserSettingsTests
{
    [Fact]
    public void ShowSplashScreen_DefaultsToTrue_WhenSettingIsMissing()
    {
        var settings = JsonSerializer.Deserialize<UserSettings>("{}");

        settings.ShouldNotBeNull();
        settings.ShowSplashScreen.ShouldBeTrue();
    }

    [Fact]
    public void ShowSplashScreen_CanBeDisabledAndSerialized()
    {
        var json = JsonSerializer.Serialize(new UserSettings { ShowSplashScreen = false });
        var settings = JsonSerializer.Deserialize<UserSettings>(json);

        settings.ShouldNotBeNull();
        settings.ShowSplashScreen.ShouldBeFalse();
    }
}
