using System;

namespace TEdit.Png;

/// <summary>
/// Incremental Adler-32 checksum used in the zlib footer.
/// </summary>
internal struct Adler32
{
    private uint _a = 1;
    private uint _b;

    public Adler32() { }

    public void Update(byte value)
    {
        _a = (_a + value) % 65521;
        _b = (_b + _a) % 65521;
    }

    public void Update(ReadOnlySpan<byte> data)
    {
        const int nmax = 5552; // largest n such that 255*n*(n+1)/2 + (n+1)*(65520) <= 2^32-1
        int remaining = data.Length;
        int offset = 0;

        while (remaining > 0)
        {
            int blockLen = Math.Min(remaining, nmax);
            for (int i = 0; i < blockLen; i++)
            {
                _a += data[offset + i];
                _b += _a;
            }
            _a %= 65521;
            _b %= 65521;
            offset += blockLen;
            remaining -= blockLen;
        }
    }

    public readonly uint Value => (_b << 16) | _a;
}
