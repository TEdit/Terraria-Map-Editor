/* 
 * Copyright (c) 2023 pharuxtan
 * Copyright (c) 2023 RussDev7
 * 
 * This source is subject to the Microsoft Public License.
 * See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
 * All other rights reserved.
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.IO;
using System.IO.Compression;

namespace TEdit.Terraria.IO;

public static class ConsoleCompressor
{
    private static int BUFFER_SIZE = 16384;

    public static void CompressStream(FileStream stream)
    {
        stream.Position = 0;
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            stream.SetLength(0);

            using (var b = new BinaryWriter(stream))
            {
                b.Write(0x1AA2227E); // Signature
                b.Write((int)ms.Length);

                // TODO: use net7.0 ZLibStream
                using (var compressor = new ZLibStream(stream, CompressionLevel.SmallestSize))
                {
                    ms.Position = 0;

                    byte[] buffer = new byte[BUFFER_SIZE];
                    int n;
                    while ((n = ms.Read(buffer, 0, BUFFER_SIZE)) != 0)
                    {
                        compressor.Write(buffer, 0, n);
                    }
                }
            }
        }
    }

    public static MemoryStream DecompressStream(FileStream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            int signature = br.ReadInt32(); // 0x1AA2227E
            int outputSize = br.ReadInt32();

            var ms = new MemoryStream();

            var decompressor = new ZLibStream(ms, CompressionMode.Decompress);

            byte[] buffer = new byte[BUFFER_SIZE];
            int n;
            while ((n = stream.Read(buffer, 0, BUFFER_SIZE)) != 0)
            {
                decompressor.Write(buffer, 0, n);
            }

            ms.Position = 0;
            return ms;
        }
    }

    public static bool IsCompressed(FileStream stream)
    {
        byte[] magic = new byte[4];
        stream.Read(magic, 0, 4);
        stream.Position = 0;
        return BitConverter.ToInt32(magic, 0) == 0x1AA2227E;
    }
}
