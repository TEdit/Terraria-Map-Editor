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
        var denial = Execution.ScriptAccessPolicy.GetDenial(context.AccessProfile);
        if (denial != null) return new ScriptResult(false, denial, 0);
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
            RegisterLiveApi(engine, "tile", () => api.Tile, api);
            RegisterLiveApi(engine, "geometry", () => api.Geometry, api);
            RegisterLiveApi(engine, "chests", () => api.Chests, api);
            RegisterLiveApi(engine, "signs", () => api.Signs, api);
            RegisterLiveApi(engine, "npcs", () => api.Npcs, api);
            RegisterLiveApi(engine, "world", () => api.World, api);
            RegisterLiveApi(engine, "selection", () => api.Selection, api);
            RegisterLiveApi(engine, "metadata", () => api.Metadata, api);
            RegisterLiveApi(engine, "log", () => api.Log, api);
            RegisterLiveApi(engine, "batch", () => api.Batch, api);
            RegisterLiveApi(engine, "tools", () => api.Tools, api);
            RegisterLiveApi(engine, "finder", () => api.Finder, api);
            RegisterLiveApi(engine, "sprites", () => api.Sprites, api);
            RegisterLiveApi(engine, "draw", () => api.Draw, api);
            RegisterLiveApi(engine, "tileEntities", () => api.TileEntities, api);
            RegisterLiveApi(engine, "generate", () => api.Generate, api);

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
            return new ScriptResult(false, string.Format(Properties.Language.scripting_timeout, context.TimeoutMs), sw.ElapsedMilliseconds);
        }
        catch (Exception ex) when (ex is OperationCanceledException or ExecutionCanceledException)
        {
            sw.Stop();
            return new ScriptResult(false, Properties.Language.scripting_cancel, sw.ElapsedMilliseconds);
        }
        catch (JavaScriptException ex)
        {
            sw.Stop();
            return new ScriptResult(false, string.Format(Properties.Language.scripting_js_error, ex.Message, ex.Location.Start.Line), sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ScriptResult(false, string.Format(Properties.Language.scripting_generic_error, ex.Message), sw.ElapsedMilliseconds);
        }
    }

    private static void RegisterLiveApi(Jint.Engine engine, string name, Func<object?> currentModule, ScriptApi api)
    {
        if (currentModule() == null)
        {
            engine.SetValue(name, Jint.Native.JsValue.Null);
            return;
        }
        engine.SetValue("__teditGetModule", currentModule);
        engine.SetValue("__teditEnterApi", new Action(api.EnterApiCall));
        engine.SetValue("__teditExitApi", new Action(api.ExitApiCall));
        // Resolve again when a saved method is invoked: tools.load may have replaced
        // both the world and its undo manager since the method was obtained.
        engine.Execute($$"""
            var {{name}} = (function(getModule, enter, exit) {
                return new Proxy({}, {
                    get: function(_, key) {
                        var value = getModule()[key];
                        if (typeof value !== 'function') return value;
                        return function(...args) {
                            enter();
                            try { return getModule()[key](...args); }
                            finally { exit(); }
                        };
                    },
                    set: function(_, key, value) { getModule()[key] = value; return true; }
                });
            })(__teditGetModule, __teditEnterApi, __teditExitApi);
            delete __teditGetModule;
            delete __teditEnterApi;
            delete __teditExitApi;
            """);
        if (!engine.GetValue("__teditGetModule").IsUndefined() ||
            !engine.GetValue("__teditEnterApi").IsUndefined() ||
            !engine.GetValue("__teditExitApi").IsUndefined())
            throw new InvalidOperationException(Properties.Language.scripting_binding);
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
