using Xunit;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Configuration;
using TEdit.Editor.Undo;
using System.IO;

namespace TEdit.Editor.Undo.Tests;

public class UndoBufferTests : IDisposable
{
    private readonly string _testDir;
    private readonly List<string> _tempFiles = new();

    public UndoBufferTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "TEditUndoTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    private string GetTestFileName()
    {
        var fileName = Path.Combine(_testDir, $"undo_{Guid.NewGuid()}.undo");
        _tempFiles.Add(fileName);
        return fileName;
    }

    [Fact]
    public void UndoBuffer_RoundTrip_SingleTile()
    {
        // Arrange
        var fileName = GetTestFileName();
        var tile = new Tile { IsActive = true, Type = 1, Wall = 5 };
        var location = new Vector2Int32(100, 200);

        // Act - Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(location, tile);
            buffer.Close();
        }

        // Act - Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        // Assert
        Assert.Single(tiles);
        Assert.Equal(location.X, tiles[0].Location.X);
        Assert.Equal(location.Y, tiles[0].Location.Y);
        Assert.True(tiles[0].Tile.Equals(tile));
    }

    [Fact]
    public void UndoBuffer_RoundTrip_MultipleTiles_AllUnique()
    {
        // Arrange
        var fileName = GetTestFileName();
        var testData = new List<(Vector2Int32 loc, Tile tile)>
        {
            (new Vector2Int32(0, 0), new Tile { IsActive = true, Type = 1 }),
            (new Vector2Int32(10, 20), new Tile { IsActive = true, Type = 2, Wall = 3 }),
            (new Vector2Int32(30, 40), new Tile { IsActive = false, Wall = 5 }),
        };

        // Act - Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            foreach (var (loc, tile) in testData)
            {
                buffer.Add(loc, tile);
            }
            buffer.Close();
        }

        // Act - Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        // Assert
        Assert.Equal(testData.Count, tiles.Count);
        for (int i = 0; i < testData.Count; i++)
        {
            Assert.Equal(testData[i].loc.X, tiles[i].Location.X);
            Assert.Equal(testData[i].loc.Y, tiles[i].Location.Y);
            Assert.True(tiles[i].Tile.Equals(testData[i].tile));
        }
    }

    [Fact]
    public void UndoBuffer_RoundTrip_IdenticalTiles_Grouped()
    {
        // Arrange - Same tile at multiple locations (tests compression)
        var fileName = GetTestFileName();
        var tile = new Tile { IsActive = true, Type = 42, Wall = 7, TileColor = 3 };
        var locations = new List<Vector2Int32>
        {
            new Vector2Int32(0, 0),
            new Vector2Int32(100, 100),
            new Vector2Int32(50, 75),
            new Vector2Int32(200, 300),
        };

        // Act - Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            foreach (var loc in locations)
            {
                buffer.Add(loc, (Tile)tile.Clone());
            }
            buffer.Close();
        }

        // Act - Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        // Assert - All tiles restored
        Assert.Equal(locations.Count, tiles.Count);

        // Verify all tiles have correct data
        foreach (var undoTile in tiles)
        {
            Assert.True(undoTile.Tile.Equals(tile), "Tile data should match");
        }

        // Verify all locations are present (order may vary within a tile group)
        var readLocations = tiles.Select(t => t.Location).ToHashSet();
        foreach (var expectedLoc in locations)
        {
            Assert.Contains(expectedLoc, readLocations);
        }
    }

    [Fact]
    public void UndoBuffer_RoundTrip_MixedTiles()
    {
        // Arrange - Mix of unique and repeated tiles
        var fileName = GetTestFileName();
        var tile1 = new Tile { IsActive = true, Type = 1 };
        var tile2 = new Tile { IsActive = true, Type = 2 };
        var tile3 = new Tile { IsActive = true, Type = 3 };

        var testData = new List<(Vector2Int32 loc, Tile tile)>
        {
            (new Vector2Int32(0, 0), tile1),
            (new Vector2Int32(1, 1), tile1),  // Repeat
            (new Vector2Int32(2, 2), tile2),
            (new Vector2Int32(3, 3), tile1),  // Repeat
            (new Vector2Int32(4, 4), tile3),
            (new Vector2Int32(5, 5), tile2),  // Repeat
        };

        // Act - Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            foreach (var (loc, tile) in testData)
            {
                buffer.Add(loc, (Tile)tile.Clone());
            }
            buffer.Close();
        }

        // Act - Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        // Assert
        Assert.Equal(testData.Count, tiles.Count);

        // Verify all locations and tiles are present
        var readData = tiles.ToDictionary(t => t.Location, t => t.Tile);
        foreach (var (loc, expectedTile) in testData)
        {
            Assert.True(readData.ContainsKey(loc), $"Location {loc} should be present");
            Assert.True(readData[loc].Equals(expectedTile), $"Tile at {loc} should match");
        }
    }

    [Fact]
    public void UndoBuffer_Compression_SmallerFileSizeWithRepetition()
    {
        // Arrange
        var fileNameHighRepetition = GetTestFileName();
        var fileNameLowRepetition = GetTestFileName();
        var singleTile = new Tile { IsActive = true, Type = 1, Wall = 1, TileColor = 1 };
        const int tileCount = 1000;

        // Write high repetition file (same tile 1000 times)
        using (var buffer = new UndoBuffer(fileNameHighRepetition, null!))
        {
            for (int i = 0; i < tileCount; i++)
            {
                buffer.Add(new Vector2Int32(i, i), (Tile)singleTile.Clone());
            }
            buffer.Close();
        }

        // Write low repetition file (1000 different tiles)
        using (var buffer = new UndoBuffer(fileNameLowRepetition, null!))
        {
            for (int i = 0; i < tileCount; i++)
            {
                var uniqueTile = new Tile { IsActive = true, Type = (ushort)(i % 500), Wall = (ushort)(i % 255) };
                buffer.Add(new Vector2Int32(i, i), uniqueTile);
            }
            buffer.Close();
        }

        // Assert - High repetition file should be smaller
        var highRepSize = new FileInfo(fileNameHighRepetition).Length;
        var lowRepSize = new FileInfo(fileNameLowRepetition).Length;

        Assert.True(highRepSize < lowRepSize,
            $"High repetition file ({highRepSize} bytes) should be smaller than low repetition file ({lowRepSize} bytes)");
    }

    [Fact]
    public void UndoBuffer_EmptyBuffer_NoTiles()
    {
        // Arrange
        var fileName = GetTestFileName();

        // Act - Write empty buffer
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Close();
        }

        // Act - Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        // Assert
        Assert.Empty(tiles);
    }

    [Fact]
    public void UndoBuffer_TileWithAllProperties()
    {
        // Arrange - Tile with many properties set
        var fileName = GetTestFileName();
        var tile = new Tile
        {
            IsActive = true,
            Type = 123,
            Wall = 45,
            TileColor = 7,
            WallColor = 3,
            LiquidAmount = 128,
            LiquidType = LiquidType.Lava,
            WireRed = true,
            WireBlue = true,
            WireGreen = false,
            WireYellow = true,
            BrickStyle = BrickStyle.SlopeTopLeft,
            Actuator = true,
            InActive = false,
            U = 18,
            V = 36,
        };
        var location = new Vector2Int32(999, 888);

        // Act - Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(location, tile);
            buffer.Close();
        }

        // Act - Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        // Assert
        Assert.Single(tiles);
        var readTile = tiles[0].Tile;

        Assert.Equal(tile.IsActive, readTile.IsActive);
        Assert.Equal(tile.Type, readTile.Type);
        Assert.Equal(tile.Wall, readTile.Wall);
        Assert.Equal(tile.TileColor, readTile.TileColor);
        Assert.Equal(tile.WallColor, readTile.WallColor);
        Assert.Equal(tile.LiquidAmount, readTile.LiquidAmount);
        Assert.Equal(tile.LiquidType, readTile.LiquidType);
        Assert.Equal(tile.WireRed, readTile.WireRed);
        Assert.Equal(tile.WireBlue, readTile.WireBlue);
        Assert.Equal(tile.WireGreen, readTile.WireGreen);
        Assert.Equal(tile.WireYellow, readTile.WireYellow);
        Assert.Equal(tile.BrickStyle, readTile.BrickStyle);
        Assert.Equal(tile.Actuator, readTile.Actuator);
        Assert.Equal(tile.InActive, readTile.InActive);
        Assert.Equal(tile.U, readTile.U);
        Assert.Equal(tile.V, readTile.V);
    }
}
