using System;
using System.Diagnostics;
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
            RegisterApi(state, "tile", api.Tile);
            RegisterApi(state, "geometry", api.Geometry);
            RegisterApi(state, "chests", api.Chests);
            RegisterApi(state, "signs", api.Signs);
            RegisterApi(state, "npcs", api.Npcs);
            RegisterApi(state, "world", api.World);
            RegisterApi(state, "selection", api.Selection);
            RegisterApi(state, "metadata", api.Metadata);
            RegisterApi(state, "log", api.Log);
            RegisterApi(state, "batch", api.Batch);
            RegisterApi(state, "tools", api.Tools);
            RegisterApi(state, "finder", api.Finder);

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
                ? "Script was cancelled"
                : $"Script timed out after {context.TimeoutMs}ms";
            return new ScriptResult(false, msg, sw.ElapsedMilliseconds);
        }
        catch (LuaRuntimeException ex)
        {
            sw.Stop();
            return new ScriptResult(false, $"Lua error: {ex.Message}", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ScriptResult(false, $"Error: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }

    private static void RegisterApi(LuaState state, string name, object apiObject)
    {
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
                                luaState.CallAsync(luaFunc, new LuaValue[] { (double)x, (double)y }, CancellationToken.None)
                                    .AsTask().GetAwaiter().GetResult();
                            });
                        }
                        else if (paramType == typeof(Func<int, int, bool>))
                        {
                            var luaFunc = ctx.GetArgument<LuaFunction>(i);
                            var luaState = ctx.State;
                            args[i] = new Func<int, int, bool>((x, y) =>
                            {
                                var results = luaState.CallAsync(luaFunc, new LuaValue[] { (double)x, (double)y }, CancellationToken.None)
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

                var result = methodInfo.Invoke(apiObject, args);

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
                var value = propInfo.GetValue(apiObject);
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
