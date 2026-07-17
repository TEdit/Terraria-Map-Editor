using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Common.Exceptions;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Editor.Plugins;

public class ReplayFile
{
    public const string Magic = "TERP";
    public const int FormatVersion = 1;

    public int Version { get; set; } = FormatVersion;
    public DateTime StartTime { get; set; }
    public long TotalTime { get; set; }
    public World BaselineWorld { get; set; }
    public List<ReplayFrame> Frames { get; set; } = [];

    public void Save(string path)
    {
        string tempFile = Path.GetTempFileName();
        World.Save(BaselineWorld, tempFile);
        byte[] baselineBytes = File.ReadAllBytes(tempFile);
        File.Delete(tempFile);

        using var stream = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        writer.Write(System.Text.Encoding.UTF8.GetBytes(Magic));
        writer.Write(Version);
        writer.Write(new DateTimeOffset(StartTime).ToUnixTimeMilliseconds());
        writer.Write(TotalTime);
        writer.Write(Frames.Count);

        writer.Write(baselineBytes.Length);
        writer.Write(baselineBytes);

        foreach (var frame in Frames)
            frame.Write(writer);

        writer.Write(System.Text.Encoding.UTF8.GetBytes(Magic));
    }

    public void Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open);
        using var reader = new BinaryReader(stream);

        string magic = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
        if (magic != Magic) throw new TEditFileFormatException("Not a replay file.");

        Version = reader.ReadInt32();
        StartTime = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()).DateTime;
        TotalTime = reader.ReadInt64();

        int frameCount = reader.ReadInt32();

        int baselineLen = reader.ReadInt32();
        byte[] baselineBytes = reader.ReadBytes(baselineLen);

        for (int i = 0; i < frameCount; i++)
            Frames.Add(ReplayFrame.Read(reader, i));

        magic = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(4));
        if (magic != Magic) throw new TEditFileFormatException("Replay file footer mismatch.");

        string tempFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, baselineBytes);
        var (world, error) = World.LoadWorld(tempFile);
        File.Delete(tempFile);
        if (error != null) throw error;

        BaselineWorld = world;
    }
}

public class ReplayFrame
{
    public int Index { get; set; }
    public long Time { get; set; }
    public byte[] Data { get; set; }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(Data.Length);
        writer.Write(Data);
    }

    public static ReplayFrame Read(BinaryReader reader, int index)
    {
        return new ReplayFrame
        {
            Index = index,
            Time = reader.ReadInt64(),
            Data = reader.ReadBytes(reader.ReadInt32()),
        };
    }

    public List<UndoTile> ReadTiles(World world)
    {
        using var stream = new MemoryStream(Data);
        using var reader = new BinaryReader(stream);
        return [.. UndoBuffer.ReadUndoTilesFromStream(reader, world.TileFrameImportant)];
    }

    public void WriteTiles(List<UndoTile> tiles, World world)
    {
        // Group tiles by type
        var groups = new Dictionary<Tile, List<Vector2Int32>>();
        var tileOrder = new List<Tile>();

        foreach (var ut in tiles)
        {
            if (!groups.TryGetValue(ut.Tile, out var list))
            {
                list = [];
                groups[ut.Tile] = list;
                tileOrder.Add(ut.Tile);
            }
            list.Add(ut.Location);
        }

        // Serialize each group
        var version = (int)(world?.Version ?? WorldConfiguration.CompatibleVersion);
        var data = WorldConfiguration.SaveConfiguration.GetData((uint)version);
        var tileFrameImportant = world?.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(tileOrder.Count);
        foreach (var tile in tileOrder)
        {
            var locations = groups[tile];
            byte[] tileData = World.SerializeTileData(
                tile,
                version,
                data.MaxTileId,
                data.MaxWallId,
                tileFrameImportant,
                out int dataIndex,
                out int headerIndex);
            bw.Write(tileData, headerIndex, dataIndex - headerIndex);
            bw.Write(locations.Count);
            foreach (var loc in locations)
            {
                bw.Write(loc.X);
                bw.Write(loc.Y);
            }
        }

        Data = ms.ToArray();
    }
}
