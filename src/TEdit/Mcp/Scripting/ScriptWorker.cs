using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Lua;

namespace TEdit.Mcp.Scripting;

internal static class ScriptWorker
{
    internal static readonly (string Module, string[] Methods)[] Api =
    [
        ("tile", ["isActive", "getTileType", "getWall"]),
        ("draw", ["line", "rectangle", "ellipse", "fillRegion", "placeSprite"]),
        ("log", ["print"]),
        ("world", ["get", "set"])
    ];

    // This entry point runs before any editor, settings, updater or WPF initialization.
    internal static void MainLoop(Stream input, Stream output)
    {
        ScriptProtocol.Write(output, "ready", CancellationToken.None).GetAwaiter().GetResult();
        var request = ScriptProtocol.Read<ScriptInput>(input, CancellationToken.None).GetAwaiter().GetResult();
        using var timeout = new CancellationTokenSource(ScriptProtocol.TimeoutMs);
        var token = timeout.Token;
        // The editor enforces the aggregate call budget across all modules.
        JsonElement Invoke(string method, object[] arguments)
        {
            token.ThrowIfCancellationRequested();
            var args = arguments.Select(value => JsonSerializer.SerializeToElement(value)).ToArray();
            ScriptProtocol.Write(output, new ScriptMessage(method, args), token).GetAwaiter().GetResult();
            return ScriptProtocol.Read<JsonElement>(input, token).GetAwaiter().GetResult();
        }

        ScriptDiagnostic? error = null;
        try
        {
            if (request.Language == "javascript") JavaScript(request.Source, Invoke, token);
            else if (request.Language == "lua") LuaScript(request.Source, Invoke, token);
            else throw new ScriptLimitException("invalid_language");
        }
        catch (JavaScriptException e)
        {
            var name = e.Error.IsObject() ? e.Error.AsObject().Get("name") : JsValue.Undefined;
            error = new(name.IsString() && name.AsString() == "SyntaxError" ? "syntax_error" : "runtime_error",
                Short(e.Message), e.Location.Start.Line, e.Location.Start.Column + 1);
        }
        catch (Acornima.ParseErrorException e) { error = new("syntax_error", Short(e.Description), e.LineNumber, e.Column); }
        catch (LuaParseException e) { error = new("syntax_error", Short(e.Message), e.Position.Line, e.Position.Column); }
        catch (LuaCompileException e) { error = new("syntax_error", Short(e.Message), e.Position.Line, e.Position.Column); }
        catch (LuaStackOverflowException) { error = new("recursion_limit"); }
        catch (LuaRuntimeException e)
        {
            // LuaCSharp represents VM depth exhaustion as a Lua runtime error.
            var code = e.Message == "stack overflow" ? "recursion_limit" : "runtime_error";
            error = new(code, Short(e.Message), e.LuaTraceback?.LastLine);
        }
        catch (RecursionDepthOverflowException) { error = new("recursion_limit"); }
        catch (StatementsCountOverflowException) { error = new("instruction_limit"); }
        catch (Exception e) when (e is MemoryLimitExceededException or OutOfMemoryException) { error = new("memory_limit"); }
        catch (Exception e) when (e is OperationCanceledException or ExecutionCanceledException or TimeoutException) { error = new("timeout"); }
        catch (ScriptLimitException e) { error = new(e.Message); }
        catch (Exception) { error = new("runtime_error"); }
        ScriptProtocol.Write(output, new ScriptMessage("finished", Error: error), CancellationToken.None).GetAwaiter().GetResult();
    }

    private static string Short(string text) => text.Length <= 192 ? text : text[..192];

    private static string BoundedString(string value) => value.Length <= ScriptProtocol.MaxLogChars
        ? value : throw new ScriptLimitException("argument_limit");

    private static void JavaScript(string source, Func<string, object[], JsonElement> invoke, CancellationToken token)
    {
        var engine = new Engine(options => options.Strict()
            .DisableStringCompilation().LimitRecursion(ScriptProtocol.RecursionLimit)
            .MaxStatements(ScriptProtocol.InstructionLimit).LimitMemory(ScriptProtocol.EngineMemoryBytes)
            .CancellationToken(token));
        foreach (var (module, names) in Api)
        {
            var table = engine.Intrinsics.Object.Construct([]);
            foreach (var name in names)
            {
                var method = module + "." + name;
                table.Set(name, new ClrFunction(engine, name, (_, args) =>
                {
                    if (args.Length > 10) throw new ScriptLimitException("invalid_arguments");
                    object Value(JsValue v) => v.IsNumber() && double.IsFinite(v.AsNumber()) ? v.AsNumber()
                        : v.IsBoolean() ? v.AsBoolean() : v.IsString() ? BoundedString(v.AsString())
                        : throw new ScriptLimitException("invalid_arguments");
                    var reply = invoke(method, args.Select(Value).ToArray());
                    return reply.ValueKind switch
                    {
                        JsonValueKind.Number => (JsValue)reply.GetDouble(),
                        JsonValueKind.String => reply.GetString()!,
                        _ => reply.GetBoolean()
                    };
                }), true);
            }
            engine.SetValue(module, table);
        }
        engine.Execute(source, "world.js");
    }

    private static void LuaScript(string source, Func<string, object[], JsonElement> invoke, CancellationToken token)
    {
        var state = LuaState.Create();
        state.MaxCallDepth = ScriptProtocol.RecursionLimit;
        long start = GC.GetTotalAllocatedBytes(false);
        ScriptLimitException? exhausted = null;
        int instructions = 0;
        state.SetHook(new LuaFunction((_, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            if ((instructions += 128) >= ScriptProtocol.InstructionLimit) throw exhausted = new ScriptLimitException("instruction_limit");
            if (GC.GetTotalAllocatedBytes(false) - start > ScriptProtocol.EngineMemoryBytes) throw exhausted = new ScriptLimitException("memory_limit");
            return new ValueTask<int>(0);
        }), "", 128);
        foreach (var (module, names) in Api)
        {
            var table = new LuaTable();
            foreach (var name in names)
            {
                var method = module + "." + name;
                table[name] = new LuaFunction((ctx, ct) =>
                {
                    try
                    {
                        ct.ThrowIfCancellationRequested();
                        if (ctx.ArgumentCount > 10) throw new ScriptLimitException("invalid_arguments");
                        var args = new object[ctx.ArgumentCount];
                        for (int i = 0; i < args.Length; i++)
                        {
                            var v = ctx.GetArgument(i);
                            args[i] = v.Type switch
                            {
                                LuaValueType.Number when double.IsFinite(v.Read<double>()) => v.Read<double>(),
                                LuaValueType.Boolean => v.Read<bool>(),
                                LuaValueType.String => BoundedString(v.Read<string>()),
                                _ => throw new ScriptLimitException("invalid_arguments")
                            };
                        }
                        var reply = invoke(method, args);
                        LuaValue value = reply.ValueKind switch
                        {
                            JsonValueKind.Number => reply.GetDouble(),
                            JsonValueKind.String => reply.GetString()!,
                            _ => reply.GetBoolean()
                        };
                        return new ValueTask<int>(ctx.Return(value));
                    }
                    catch (ScriptLimitException limit) { exhausted = limit; throw; }
                });
            }
            state.Environment[module] = table;
        }
        try { state.DoStringAsync(source, chunkName: "world.lua", cancellationToken: token).AsTask().GetAwaiter().GetResult(); }
        catch (LuaRuntimeException) when (exhausted != null) { throw exhausted; }
    }
}
