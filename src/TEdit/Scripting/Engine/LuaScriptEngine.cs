using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Lua;
using Lua.Standard;
using TEdit.Scripting.Api;

namespace TEdit.Scripting.Engine;

public class LuaScriptEngine : IScriptEngine
{
    public string Name => "Lua";
    public string FileExtension => ".lua";

    public ScriptResult Execute(string code, ScriptApi api, ScriptExecutionContext context)
    {
        var denial = Execution.ScriptAccessPolicy.GetDenial(context.AccessProfile);
        if (denial != null) return new ScriptResult(false, denial, 0);
        var sw = Stopwatch.StartNew();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            cts.CancelAfter(context.TimeoutMs);

            var state = LuaState.Create();

            // Open safe standard libraries (math, string, table)
            state.OpenStandardLibraries();

            // Remove dangerous globals
            state.Environment["os"] = default(LuaValue);
            state.Environment["io"] = default(LuaValue);
            state.Environment["dofile"] = default(LuaValue);
            state.Environment["loadfile"] = default(LuaValue);
            state.Environment["package"] = default(LuaValue);

            // Expose API surfaces as tables with methods
            RegisterApi(state, "tile", () => api.Tile, api);
            RegisterApi(state, "geometry", () => api.Geometry, api);
            RegisterApi(state, "chests", () => api.Chests, api);
            RegisterApi(state, "signs", () => api.Signs, api);
            RegisterApi(state, "npcs", () => api.Npcs, api);
            RegisterApi(state, "world", () => api.World, api);
            RegisterApi(state, "selection", () => api.Selection, api);
            RegisterApi(state, "metadata", () => api.Metadata, api);
            RegisterApi(state, "log", () => api.Log, api);
            RegisterApi(state, "batch", () => api.Batch, api);
            RegisterApi(state, "tools", () => api.Tools, api);
            RegisterApi(state, "finder", () => api.Finder, api);
            RegisterApi(state, "sprites", () => api.Sprites, api);
            RegisterApi(state, "draw", () => api.Draw, api);
            RegisterApi(state, "tileEntities", () => api.TileEntities, api);
            RegisterApi(state, "generate", () => api.Generate, api);

            // print() convenience
            state.Environment["print"] = new LuaFunction((ctx, ct) =>
            {
                var msg = ctx.GetArgument<string>(0);
                context.OnLog?.Invoke(msg ?? "nil");
                return new ValueTask<int>(0);
            });

            state.DoStringAsync(code, cancellationToken: cts.Token)
                .AsTask()
                .GetAwaiter()
                .GetResult();

            sw.Stop();
            return new ScriptResult(true, null, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            var msg = context.CancellationToken.IsCancellationRequested
                ? Properties.Language.scripting_cancel
                : string.Format(Properties.Language.scripting_timeout, context.TimeoutMs);
            return new ScriptResult(false, msg, sw.ElapsedMilliseconds);
        }
        catch (LuaRuntimeException ex)
        {
            sw.Stop();
            return new ScriptResult(false, string.Format(Properties.Language.scripting_lua_error, ex.Message), sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ScriptResult(false, string.Format(Properties.Language.scripting_generic_error, ex.Message), sw.ElapsedMilliseconds);
        }
    }

    private static void RegisterApi(LuaState state, string name, Func<object?> currentModule, ScriptApi api)
    {
        var apiObject = currentModule();
        if (apiObject == null) return;
        var table = new LuaTable();
        var type = apiObject.GetType();

        foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        {
            var methodInfo = method;
            var methodName = char.ToLower(methodInfo.Name[0]) + methodInfo.Name[1..];

            table[methodName] = new LuaFunction((ctx, ct) =>
            {
                var parameters = methodInfo.GetParameters();
                var args = new object?[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    try
                    {
                        if (paramType == typeof(int))
                            args[i] = (int)ctx.GetArgument<double>(i);
                        else if (paramType == typeof(double))
                            args[i] = ctx.GetArgument<double>(i);
                        else if (paramType == typeof(string))
                            args[i] = ctx.GetArgument<string>(i);
                        else if (paramType == typeof(bool))
                            args[i] = ctx.GetArgument<bool>(i);
                        else if (paramType == typeof(byte))
                            args[i] = (byte)ctx.GetArgument<double>(i);
                        else if (paramType == typeof(ushort))
                            args[i] = (ushort)ctx.GetArgument<double>(i);
                        else if (paramType == typeof(Action<int, int>))
                        {
                            var luaFunc = ctx.GetArgument<LuaFunction>(i);
                            var luaState = ctx.State;
                            args[i] = new Action<int, int>((x, y) =>
                            {
                                luaState.CallAsync(luaFunc, new LuaValue[] { (double)x, (double)y }, ct)
                                    .AsTask().GetAwaiter().GetResult();
                            });
                        }
                        else if (paramType == typeof(Func<int, int, bool>))
                        {
                            var luaFunc = ctx.GetArgument<LuaFunction>(i);
                            var luaState = ctx.State;
                            args[i] = new Func<int, int, bool>((x, y) =>
                            {
                                var results = luaState.CallAsync(luaFunc, new LuaValue[] { (double)x, (double)y }, ct)
                                    .AsTask().GetAwaiter().GetResult();
                                return results.Length > 0 && results[0].TryRead<bool>(out var b) && b;
                            });
                        }
                        else
                            args[i] = null;
                    }
                    catch
                    {
                        args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                    }
                }

                object? result;
                try
                {
                    api.EnterApiCall();
                    try { result = methodInfo.Invoke(currentModule(), args); }
                    finally { api.ExitApiCall(); }
                }
                catch (TargetInvocationException ex) when (ex.InnerException is OperationCanceledException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    throw;
                }

                if (result == null || methodInfo.ReturnType == typeof(void))
                    return new ValueTask<int>(0);

                ctx.Return(ToLuaValue(result));
                return new ValueTask<int>(1);
            });
        }

        // Expose properties as getter functions so they return live values
        foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        {
            var propInfo = prop;
            var propName = char.ToLower(propInfo.Name[0]) + propInfo.Name[1..];

            table[propName] = new LuaFunction((ctx, ct) =>
            {
                var value = propInfo.GetValue(currentModule());
                ctx.Return(ToLuaValue(value));
                return new ValueTask<int>(1);
            });
        }

        state.Environment[name] = table;
    }

    private static LuaValue ToLuaValue(object? value)
    {
        if (value is int intVal) return (double)intVal;
        if (value is double dblVal) return dblVal;
        if (value is bool boolVal) return boolVal;
        if (value is string strVal) return strVal;
        return value?.ToString() ?? "";
    }
}
