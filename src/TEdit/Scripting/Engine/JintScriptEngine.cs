using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Jint;
using Jint.Runtime;
using TEdit.Scripting.Api;

namespace TEdit.Scripting.Engine;

public class JintScriptEngine : IScriptEngine
{
    public string Name => "JavaScript";
    public string FileExtension => ".js";

    public ScriptResult Execute(string code, ScriptApi api, ScriptExecutionContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var engine = new Jint.Engine(options =>
            {
                options.TimeoutInterval(TimeSpan.FromMilliseconds(context.TimeoutMs));
                options.CancellationToken(context.CancellationToken);
                options.Strict(false);
                options.Interop.TypeResolver.MemberNameCreator = ToCamelCase;
            });

            // Block dangerous globals
            engine.SetValue("require", Jint.Native.JsValue.Undefined);
            engine.SetValue("eval", Jint.Native.JsValue.Undefined);

            // Expose API surfaces
            engine.SetValue("tile", api.Tile);
            engine.SetValue("geometry", api.Geometry);
            engine.SetValue("chests", api.Chests);
            engine.SetValue("signs", api.Signs);
            engine.SetValue("npcs", api.Npcs);
            engine.SetValue("world", api.World);
            engine.SetValue("selection", api.Selection);
            engine.SetValue("metadata", api.Metadata);
            engine.SetValue("log", api.Log);
            engine.SetValue("batch", api.Batch);
            engine.SetValue("tools", api.Tools);
            engine.SetValue("finder", api.Finder);
            engine.SetValue("sprites", api.Sprites);
            engine.SetValue("draw", api.Draw);
            engine.SetValue("tileEntities", api.TileEntities);

            // print() convenience
            engine.SetValue("print", new Action<object>(msg =>
                context.OnLog?.Invoke(msg?.ToString() ?? "nil")));

            engine.Execute(code);

            sw.Stop();
            return new ScriptResult(true, null, sw.ElapsedMilliseconds);
        }
        catch (TimeoutException)
        {
            sw.Stop();
            return new ScriptResult(false, $"Script timed out after {context.TimeoutMs}ms", sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            return new ScriptResult(false, "Script was cancelled", sw.ElapsedMilliseconds);
        }
        catch (JavaScriptException ex)
        {
            sw.Stop();
            return new ScriptResult(false, $"JavaScript error: {ex.Message} (line {ex.Location.Start.Line})", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ScriptResult(false, $"Error: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }

    private static IEnumerable<string> ToCamelCase(MemberInfo member)
    {
        var name = member.Name;
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
        {
            yield return name;
            yield break;
        }

        var camel = char.ToLowerInvariant(name[0]) + name[1..];
        yield return camel;
        yield return name; // also allow PascalCase
    }
}
