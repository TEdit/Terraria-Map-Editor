using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using Shouldly;
using TEdit.Configuration;
using TEdit.ViewModel;
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

    [Fact]
    public void TextureSettings_DefaultAndRoundTripThroughJson()
    {
        var defaults = JsonSerializer.Deserialize<UserSettings>("{}");

        defaults.ShouldNotBeNull();
        defaults.TextureVisibilityZoomLevel.ShouldBe(6f);
        defaults.ShowTextures.ShouldBeTrue();

        var json = JsonSerializer.Serialize(new UserSettings
        {
            TextureVisibilityZoomLevel = 8f,
            ShowTextures = false
        });
        var settings = JsonSerializer.Deserialize<UserSettings>(json);

        settings.ShouldNotBeNull();
        settings.TextureVisibilityZoomLevel.ShouldBe(8f);
        settings.ShowTextures.ShouldBeFalse();
    }

    [Fact]
    public void TextureVisibilityZoomLevel_ViewModelChangesPersistToUserSettings()
    {
        var userSettings = UserSettingsService.Current;
        var originalValue = userSettings.TextureVisibilityZoomLevel;
        var viewModel = (WorldViewModel)RuntimeHelpers.GetUninitializedObject(typeof(WorldViewModel));
        var zoomField = typeof(WorldViewModel).GetField("_textureVisibilityZoomLevel", BindingFlags.Instance | BindingFlags.Public)!;
        zoomField.SetValue(viewModel, originalValue);

        try
        {
            var changedValue = originalValue == 8f ? 7f : 8f;
            viewModel.TextureVisibilityZoomLevel = changedValue;

            userSettings.TextureVisibilityZoomLevel.ShouldBe(changedValue);
        }
        finally
        {
            userSettings.TextureVisibilityZoomLevel = originalValue;
        }
    }
}
