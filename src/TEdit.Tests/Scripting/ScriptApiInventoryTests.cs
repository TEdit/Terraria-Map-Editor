using System.IO;
using System.Reflection;
using Shouldly;
using TEdit.Scripting.Api;
using Xunit;

namespace TEdit.Tests.Scripting;

public class ScriptApiInventoryTests
{
    [Fact]
    public void EveryDeclaredApiMemberHasAnExplicitBlockedExposureDecision()
    {
        var members = typeof(ScriptApi).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(module => module.PropertyType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(member => member is MethodInfo or PropertyInfo)
                .Select(member => $"BLOCKED | {module.Name} | {member}"))
            .Order(StringComparer.Ordinal).ToArray();
        // Explicit maintenance mode writes an artifact, never updates acceptance.
        if (Environment.GetEnvironmentVariable("TEDIT_GENERATE_API_INVENTORY") == "1")
        {
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "generated-api-members.txt"), members);
            return;
        }
        var recorded = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "mcp-script-api-members.txt"));
        members.ShouldBe(recorded, "New API members require an exposure decision in the Phase 1 inventory.");
    }
}
