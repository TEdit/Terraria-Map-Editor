using System.Text;

namespace TEdit.Scripting;

public static class ScriptApiDocGenerator
{
    public static string GenerateMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# TEdit Scripting API Reference");
        sb.AppendLine();
        sb.AppendLine("Auto-generated reference for the TEdit scripting API.");
        sb.AppendLine("Available in both JavaScript and Lua engines.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var module in ScriptApiMetadata.Modules)
        {
            sb.AppendLine($"## `{module.Name}` — {module.Description}");
            sb.AppendLine();

            foreach (var method in module.Methods)
            {
                sb.AppendLine($"### `{module.Name}.{method.Signature}`");
                sb.AppendLine();
                sb.AppendLine(method.Description);
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
