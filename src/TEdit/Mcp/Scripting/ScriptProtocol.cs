using System;
using System.Buffers.Binary;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TEdit.Mcp.Scripting;

internal sealed record ScriptInput(string Language, string Source);
internal sealed record ScriptDiagnostic(string Code, string? Message = null, int? Line = null, int? Column = null);
internal sealed record ScriptMessage(string Method, JsonElement[]? Arguments = null, ScriptDiagnostic? Error = null);
internal sealed record ScriptOutput(string[] Log, ScriptDiagnostic? Diagnostic);

internal static class ScriptProtocol
{
    internal const string WorkerArgument = "--mcp-script-worker";
    internal const int SourceBytes = 8192;
    internal const int FrameBytes = 65536; // Includes worst-case JSON escaping of the source.
    internal const int StartupMs = 10000;
    internal const int TimeoutMs = 5000;
    internal const int DeadlineGraceMs = 250;
    internal const int CallLimit = 256;
    internal const int WorkLimit = 65536;
    internal const int MaxLogEntries = 32;
    internal const int MaxLogChars = 1024;
    internal const int RecursionLimit = 64;
    internal const int InstructionLimit = 100_000;
    internal const int EngineMemoryBytes = 16 * 1024 * 1024;
    internal const int ProcessMemoryBytes = 256 * 1024 * 1024;

    // Both endpoints validate length before allocating storage for a frame.
    internal static async Task<T> Read<T>(Stream pipe, CancellationToken token)
    {
        var header = new byte[4];
        await pipe.ReadExactlyAsync(header, token).ConfigureAwait(false);
        int size = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (size is < 1 or > FrameBytes) throw new InvalidDataException();
        var body = new byte[size];
        await pipe.ReadExactlyAsync(body, token).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(body) ?? throw new InvalidDataException();
    }

    internal static async Task Write<T>(Stream pipe, T value, CancellationToken token)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(value);
        if (body.Length > FrameBytes) throw new InvalidDataException();
        var header = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(header, body.Length);
        await pipe.WriteAsync(header, token).ConfigureAwait(false);
        await pipe.WriteAsync(body, token).ConfigureAwait(false);
        await pipe.FlushAsync(token).ConfigureAwait(false);
    }
}

internal sealed class ScriptLimitException(string code) : Exception(code);
