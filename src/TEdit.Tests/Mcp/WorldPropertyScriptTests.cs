using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TEdit.Mcp.Scripting;
using TEdit.Scripting.Api;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Mcp;

public class WorldPropertyExposureTests
{
    private static readonly HashSet<(string Element, string Attribute)> EditableBindings =
    [
        ("CheckBox", "IsChecked"), ("ComboBox", "SelectedValue"), ("Slider", "Value"),
        ("ui:NumberBox", "Value"), ("TextBox", "Text")
    ];

    private static string Source(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "TEdit.slnx"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine([dir!.FullName, .. parts]));
    }

    /// <summary>World fields the sidebar binds two-way to an input control that is not read-only.</summary>
    private static SortedSet<string> SidebarEditableFields()
    {
        var xaml = Source("TEdit", "View", "Sidebar", "WorldPropertiesView.xaml");
        var fields = new SortedSet<string>(StringComparer.Ordinal);
        foreach (Match element in Regex.Matches(xaml, @"<([\w:.]+)\b([^>]*?)/?>", RegexOptions.Singleline))
        {
            string attributes = element.Groups[2].Value;
            if (attributes.Contains("IsReadOnly=\"True\"")) continue;
            foreach (Match binding in Regex.Matches(attributes, @"(\w+)=""\{Binding\s+(?:Path=)?CurrentWorld\.(\w+)([^}]*)\}"""))
            {
                if (binding.Groups[3].Value.Contains("OneWay")) continue;
                if (EditableBindings.Contains((element.Groups[1].Value, binding.Groups[1].Value)))
                    fields.Add(binding.Groups[2].Value);
            }
        }
        return fields;
    }

    /// <summary>WorldInfoApi member → the World property its setter writes.</summary>
    private static Dictionary<string, string> WorldInfoApiTargets()
    {
        var source = Source("TEdit", "Scripting", "Api", "WorldInfoApi.cs");
        return Regex.Matches(source, @"public\s+[\w?]+\s+(\w+)\s*\{\s*get\s*=>\s*_world\.\w+[^;]*;\s*set\s*=>\s*_world\.(\w+)\s*=")
            .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
    }

    [Fact]
    public void McpExposesExactlyTheSidebarEditableWorldFields()
    {
        var editable = SidebarEditableFields();
        Assert.True(editable.Count > 100, $"Parsed only {editable.Count} editable fields; the XAML pattern may have changed.");
        var targets = WorldInfoApiTargets();

        var unmapped = McpWorldProperties.Members.Where(m => !targets.ContainsKey(m)).ToArray();
        Assert.True(unmapped.Length == 0, "Exposed members that are not simple WorldInfoApi wrappers: " + string.Join(", ", unmapped));

        var exposed = new SortedSet<string>(McpWorldProperties.Members.Select(m => targets[m]), StringComparer.Ordinal);
        Assert.True(editable.Except(exposed).Count() == 0,
            "Sidebar-editable fields missing from McpWorldProperties (add a WorldInfoApi wrapper and list it): " + string.Join(", ", editable.Except(exposed)));
        Assert.True(exposed.Except(editable).Count() == 0,
            "McpWorldProperties lists fields the sidebar does not edit: " + string.Join(", ", exposed.Except(editable)));
    }

    [Fact]
    public void ExposedMembersAreReadWrite()
    {
        foreach (var name in McpWorldProperties.Members)
        {
            var property = typeof(WorldInfoApi).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.True(property is { CanRead: true, SetMethod.IsPublic: true }, name);
        }
    }
}

public class WorldInfoApiByNameTests
{
    private readonly TEdit.Terraria.World _world = TestWorldFactory.CreateSmallWorld();
    private WorldInfoApi Api => new(_world);

    [Fact]
    public void SetConvertsScriptNumbersAndReturnsStoredValue()
    {
        Assert.Equal(true, Api.Set("hardMode", true));
        Assert.True(_world.HardMode);
        Assert.Equal((byte)4, Api.Set("moonType", 4.0));
        Assert.Equal(1.25f, Api.Set("windSpeedSet", 1.25));
        Assert.Equal("Renamed", Api.Set("Title", "Renamed"));
        Assert.Equal("Renamed", Api.Get("title"));
    }

    [Fact]
    public void GuidRoundTripsAsString()
    {
        var guid = Guid.NewGuid();
        Assert.Equal(guid.ToString(), Api.Set("worldGuid", guid.ToString()));
        Assert.Equal(guid, _world.WorldGUID);
        Assert.Equal("invalid_value", Assert.Throws<ArgumentException>(() => Api.Set("worldGuid", "not-a-guid")).Message);
    }

    [Theory]
    [InlineData("moonType", 256.0)]   // byte overflow
    [InlineData("moonType", -1.0)]
    [InlineData("moonType", 1.5)]     // no rounding
    [InlineData("altarCount", double.NaN)]
    [InlineData("hardMode", 1.0)]     // no truthiness
    [InlineData("title", 3.0)]
    public void SetRejectsValuesThatDoNotFitExactly(string name, object value)
    {
        Assert.Equal("invalid_value", Assert.Throws<ArgumentException>(() => Api.Set(name, value)).Message);
    }

    [Fact]
    public void UnknownAndReadOnlyNamesAreRejected()
    {
        Assert.Equal("unknown_property", Assert.Throws<ArgumentException>(() => Api.Get("noSuchField")).Message);
        Assert.Equal("read_only_property", Assert.Throws<ArgumentException>(() => Api.Set("width", 5.0)).Message);
    }
}

[Collection("Production editor")]
public class WorldPropertyScriptSessionTests
{
    private static Dictionary<string, object?> Args(string language, string source) =>
        new() { ["language"] = language, ["source"] = source };

    [Theory]
    [InlineData("javascript", "log.print(world.set('hardMode', true)); log.print(world.set('moonPhase', 4)); log.print(world.get('title'))")]
    [InlineData("lua", "log.print(world.set('hardMode', true)) log.print(world.set('moonPhase', 4)) log.print(world.get('title'))")]
    public async Task ScriptsReadAndWriteSidebarFields(string language, string source)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        var op = await s.Finish(await s.Start("execute_script", Args(language, source)));
        Assert.True(op.GetProperty("status").GetString() == "succeeded", op.ToString());
        var log = op.GetProperty("script").GetProperty("log");
        Assert.Equal("true", log[0].GetString()!.ToLowerInvariant());
        Assert.Equal("4", log[1].GetString());
        await s.Ui.InvokeAsync(() =>
        {
            Assert.True(s.World.HardMode);
            Assert.Equal(4, s.World.MoonPhase);
            Assert.Equal(s.World.Title, log[2].GetString());
        });
    }

    [Theory]
    [InlineData("javascript", "world.get('fileRevision')", "unknown_property")]
    [InlineData("lua", "world.set('spawnX', 5)", "unknown_property")]
    [InlineData("javascript", "world.set('moonPhase', 2.5)", "invalid_value")]
    [InlineData("lua", "world.set('hardMode', 1)", "invalid_value")]
    public async Task FieldsOutsideTheSidebarAndBadValuesFail(string language, string source, string expected)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        var op = await s.Finish(await s.Start("execute_script", Args(language, source)));
        Assert.True(op.GetProperty("errorCode").GetString() == expected, op.ToString());
    }
}
