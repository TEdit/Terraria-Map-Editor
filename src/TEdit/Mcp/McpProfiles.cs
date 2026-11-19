using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace TEdit.Mcp;

// Only the process holding the per-user host lock may modify the profile store.
internal sealed class McpProfiles(string directory)
{
    private sealed record Entry(string Name, byte[] Token);
    private readonly Dictionary<Guid, Entry> _profiles = new();
    private readonly object _sync = new();
    private bool _writable;
    public void Close()
    {
        lock (_sync)
        {
            _writable = false;
            foreach (var p in _profiles.Values) CryptographicOperations.ZeroMemory(p.Token);
            _profiles.Clear();
        }
    }

    public void Load()
    {
        lock (_sync)
        {
            Close();
            foreach (var path in Directory.EnumerateFiles(directory, "*.profile"))
            {
                _profiles.Add(Guid.Parse(Path.GetFileNameWithoutExtension(path)), ReadEntry(path));
            }
            _writable = true;
        }
    }

    private static Entry ReadEntry(string path)
    {
        var clear = ProtectedData.Unprotect(File.ReadAllBytes(path), null, DataProtectionScope.CurrentUser);
        try { return JsonSerializer.Deserialize<Entry>(clear)!; }
        finally { CryptographicOperations.ZeroMemory(clear); }
    }

    internal static string ReadCredential(string directory, Guid id)
    {
        var entry = ReadEntry(Path.Combine(directory, id + ".profile"));
        try { return id.ToString("N") + "." + Convert.ToHexString(entry.Token); }
        finally { CryptographicOperations.ZeroMemory(entry.Token); }
    }

    public KeyValuePair<Guid, string>[] List()
    {
        lock (_sync) return _profiles.Select(p => KeyValuePair.Create(p.Key, p.Value.Name)).ToArray();
    }

    public string Issue(string name)
    {
        lock (_sync)
        {
            if (!_writable) throw new InvalidOperationException();
            name = name.Trim();
            if (name.Length is < 1 or > 64 || _profiles.Count >= 64 ||
                _profiles.Values.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException(nameof(name));
            var id = Guid.NewGuid();
            var entry = new Entry(name, RandomNumberGenerator.GetBytes(32));
            var clear = JsonSerializer.SerializeToUtf8Bytes(entry);
            try
            {
                var encrypted = ProtectedData.Protect(clear, null, DataProtectionScope.CurrentUser);
                var path = Path.Combine(directory, id + ".profile");
                File.WriteAllBytes(path + ".tmp", encrypted);
                File.Move(path + ".tmp", path);
            }
            finally { CryptographicOperations.ZeroMemory(clear); }
            _profiles.Add(id, entry);
            return id.ToString("N") + "." + Convert.ToHexString(entry.Token);
        }
    }

    internal bool Exists(Guid id) { lock (_sync) return _profiles.ContainsKey(id); }

    public Guid? Authenticate(string bearer)
    {
        if (bearer.Length != 97 || bearer[32] != '.' || !Guid.TryParseExact(bearer[..32], "N", out var id)) return null;
        Span<byte> secret = stackalloc byte[32];
        if (Convert.FromHexString(bearer.AsSpan(33), secret, out _, out _) != System.Buffers.OperationStatus.Done) return null;
        lock (_sync) return _profiles.TryGetValue(id, out var p) && CryptographicOperations.FixedTimeEquals(secret, p.Token) ? id : null;
    }

    public void Revoke(Guid id)
    {
        lock (_sync)
        {
            if (!_writable) throw new InvalidOperationException();
            File.Delete(Path.Combine(directory, id + ".profile"));
            if (_profiles.Remove(id, out var entry)) CryptographicOperations.ZeroMemory(entry.Token);
        }
    }
}
