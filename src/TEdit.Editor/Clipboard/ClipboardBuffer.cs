using System.Linq;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Configuration;
using System.Collections.Generic;
using TEdit.Editor.Undo;
using System;

namespace TEdit.Editor.Clipboard;

public partial class ClipboardBuffer : ITileData
{
    private string _name;
    private Vector2Int32 _size;

    public ClipboardBuffer(
        Vector2Int32 size, 
        bool initTiles = false,
        bool[] tileFrameImportant = null)
    {
        Size = size;
        Tiles = new Tile[size.X, size.Y];
        TileFrameImportant = tileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

        if (initTiles)
        {
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    Tiles[x, y] = new Tile();
                }
            }
        }
    }

    public bool[] TileFrameImportant { get; set; }
    public Tile[,] Tiles { get; set; }

    public string Name { get; set; }
    public Vector2Int32 Size
    {
        get { return _size; }
        set
        {
            _size = value;
            Tiles = new Tile[_size.X, _size.Y];
        }
    }

    public double RenderScale { get; set; }
    public List<Chest> Chests { get; } = new();
    public List<Sign> Signs { get; } = new();
    public List<TileEntity> TileEntities { get; } = new();

    // since we are using these functions to add chests into the world we don't need to check all spots, only the anchor spot
    public Chest GetChestAtTile(int x, int y, bool findOrigin = false)
    {
        return Chests.FirstOrDefault(c => (c.X == x) && (c.Y == y));
    }

    public Sign GetSignAtTile(int x, int y, bool findOrigin = false)
    {
        return Signs.FirstOrDefault(c => (c.X == x) && (c.Y == y));
    }

    public TileEntity GetTileEntityAtTile(int x, int y, bool findOrigin = false)
    {
    	return TileEntities.FirstOrDefault(c => (c.PosX == x) && (c.PosY == y));
    }

    public static ClipboardBuffer GetSelectionBuffer(World world, RectangleInt32 area)
    {
        var buffer = new ClipboardBuffer(
            new Vector2Int32(area.Width, area.Height),
            tileFrameImportant: world.TileFrameImportant);

        for (int x = 0; x < area.Width; x++)
        {
            for (int y = 0; y < area.Height; y++)
            {
                Tile curTile = (Tile)world.Tiles[x + area.X, y + area.Y].Clone();

                if (curTile.IsChest())
                {
                    if (buffer.GetChestAtTile(x, y) == null)
                    {
                        var anchor = world.GetAnchor(x + area.X, y + area.Y);
                        if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                        {
                            var data = world.GetChestAtTile(x + area.X, y + area.Y);
                            if (data != null)
                            {
                                var newChest = data.Copy();
                                newChest.X = x;
                                newChest.Y = y;
                                buffer.Chests.Add(newChest);
                            }
                        }
                    }
                }

                if (curTile.IsSign())
                {
                    if (buffer.GetSignAtTile(x, y) == null)
                    {
                        var anchor = world.GetAnchor(x + area.X, y + area.Y);
                        if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                        {
                            var data = world.GetSignAtTile(x + area.X, y + area.Y);
                            if (data != null)
                            {
                                var newSign = data.Copy();
                                newSign.X = x;
                                newSign.Y = y;
                                buffer.Signs.Add(newSign);
                            }
                        }
                    }
                }

                if (curTile.IsTileEntity())
                {
                    if (buffer.GetTileEntityAtTile(x, y) == null)
                    {
                        var anchor = world.GetAnchor(x + area.X, y + area.Y);
                        if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                        {
                            var data = world.GetTileEntityAtTile(x + area.X, y + area.Y);
                            if (data != null)
                            {
                                var newEntity = data.Copy();
                                newEntity.PosX = (short)x;
                                newEntity.PosY = (short)y;
                                buffer.TileEntities.Add(newEntity);
                            }
                        }
                    }
                }

                buffer.Tiles[x, y] = curTile;
            }
        }

        return buffer;
    }

    public void Paste(
        World world,
        Vector2Int32 anchor,
        IUndoManager? undo,
        PasteOptions? pasteOptions = null)
    {
        if (pasteOptions?.HasAction != true) { return; } // nothing to do

        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                int worldX = x + anchor.X;
                int worldY = y + anchor.Y;

                if (!world.ValidTileLocation(new Vector2Int32(worldX, worldY))) { continue; }

                var pasteTile = Tiles[x, y];
                var worldTile = world.Tiles[worldX, worldY];

                if (pasteTile.IsEmpty && !pasteOptions.PasteEmpty)
                {
                    // skip pasting empty tile if paste empty is not set
                    continue;
                }

                if (!worldTile.IsEmpty && !pasteOptions.PasteOverTiles)
                {
                    // skip non-empty tiles if paste over tiles is not set
                    continue;
                }

                // save undo, checks above passed
                undo?.SaveTile(world, worldX, worldY);

                UpdateWorldTileFromBuffer(pasteOptions, worldTile, pasteTile);

                //  Update chest/sign data only if we've pasted tiles
                if (pasteOptions.PasteSprites && pasteOptions.PasteOverTiles)
                {
                    UpdateContainers(world, x, y, worldX, worldY, pasteTile);
                }
            }
        }

        undo?.SaveUndoAsync();
    }

    private void UpdateContainers(World world, int x, int y, int worldX, int worldY, Tile pasteTile)
    {
        // Add new chest data
        if (pasteTile.IsChest())
        {
            var existingChest = world.GetChestAtTile(worldX, worldY);
            if (existingChest != null) { world.Chests.Remove(existingChest); }

            var data = GetChestAtTile(x, y);
            if (data != null) // allow? chest copying may not work...
            {
                // Copied chest
                var newChest = data.Copy();
                newChest.X = worldX;
                newChest.Y = worldY;
                world.Chests.Add(newChest);
            }
        }

        // Add new sign data
        if (pasteTile.IsSign())
        {
            if (world.GetSignAtTile(worldX, worldY) == null)
            {
                var data = GetSignAtTile(x, y);
                if (data != null)
                {
                    // Copied sign
                    var newSign = data.Copy();
                    newSign.X = worldX;
                    newSign.Y = worldY;
                    world.Signs.Add(newSign);
                }
            }
        }

        // Add new tile entity data
        if (pasteTile.IsTileEntity())
        {
            if (world.GetTileEntityAtTile(worldX, worldY) == null)
            {
                var data = GetTileEntityAtTile(x, y);
                if (data != null)
                {
                    // Copied sign
                    var newEntity = data.Copy();
                    newEntity.PosX = (short)(worldX);
                    newEntity.PosY = (short)(worldY);
                    world.TileEntities.Add(newEntity);
                }
            }
        }
    }

    private static void UpdateWorldTileFromBuffer(PasteOptions pasteOptions, Tile worldTile, Tile pasteTile)
    {
        // paste regular tiles or sprites if pasteSprites active
        if ((pasteOptions.PasteTiles && !WorldConfiguration.SettingsTileFrameImportant[pasteTile.Type]) ||
            (pasteOptions.PasteSprites && WorldConfiguration.SettingsTileFrameImportant[pasteTile.Type]))
        {
            worldTile.IsActive        = pasteTile.IsActive;
            worldTile.Type            = pasteTile.Type;
            worldTile.TileColor       = pasteTile.TileColor;
            worldTile.FullBrightBlock = pasteTile.FullBrightBlock;
            worldTile.InvisibleBlock  = pasteTile.InvisibleBlock;
            worldTile.U               = pasteTile.U;
            worldTile.V               = pasteTile.V;
            worldTile.BrickStyle      = pasteTile.BrickStyle;
        }

        // paste over walls only if pasteOverTiles is true
        if (pasteOptions.PasteWalls)
        { 
            worldTile.Wall            = pasteTile.Wall;
            worldTile.WallColor       = pasteTile.WallColor;
            worldTile.FullBrightWall  = pasteTile.FullBrightWall;
            worldTile.InvisibleWall   = pasteTile.InvisibleWall;
        }

        // paste over liquids only if pasteOverTiles is true
        if (pasteOptions.PasteLiquids)
        {
            worldTile.LiquidAmount    = pasteTile.LiquidAmount;
            worldTile.LiquidType      = pasteTile.LiquidType;
        }                             

        if (pasteOptions.PasteWires)
        {
            // if pasting wires is disabled, use any existing wire
            worldTile.WireRed         = pasteTile.WireRed;
            worldTile.WireGreen       = pasteTile.WireGreen;
            worldTile.WireBlue        = pasteTile.WireBlue;
            worldTile.WireYellow      = pasteTile.WireYellow;
            worldTile.Actuator        = pasteTile.Actuator;
            worldTile.InActive        = pasteTile.InActive;
        }
    }

    public ClipboardBuffer FlipX() => Flip(this, true, false);
    public ClipboardBuffer FlipY() => Flip(this, false, false);
    public ClipboardBuffer Rotate() => Flip(this, false, true);

    public static ClipboardBuffer Flip(ClipboardBuffer buffer, bool flipX, bool rotate)
    {
        ClipboardBuffer flippedBuffer = new ClipboardBuffer(buffer.Size);
        //var sprites = new Dictionary<Vector2Int32, Sprite>();
        var spriteSizes = new Dictionary<Vector2Int32, Vector2Short>();
        int maxX = buffer.Size.X - 1;
        int maxY = buffer.Size.Y - 1;
        for (int x = 0; x <= maxX; x++)
        {
            for (int y = 0; y <= maxY; y++)
            {
                int bufferX;
                int bufferY;

                if (flipX)
                {
                    bufferX = maxX - x;
                    bufferY = y;
                }
                else
                {
                    bufferX = x;
                    bufferY = maxY - y;
                }

                Tile tile = (Tile)buffer.Tiles[x, y].Clone();
                var tileProperties = WorldConfiguration.TileProperties[tile.Type];
                flippedBuffer.Tiles[bufferX, bufferY] = (Tile)tile;

                // locate all the sprites and make a list
                if (tileProperties.IsFramed)
                {
                    var loc = new Vector2Int32(x, y);
                    if (tileProperties.IsOrigin(tile.GetUV()))
                    {
                        Vector2Short tileSize = tileProperties.GetFrameSize(tile.V);
                        spriteSizes[loc] = tileSize;
                    }
                }
                else
                {
                    if (flipX)
                    {
                        //  Ignore multi-width objects when flipping on x-axis

                        // Flip brick-style
                        switch (tile.BrickStyle)
                        {
                            case BrickStyle.SlopeTopRight:
                                tile.BrickStyle = BrickStyle.SlopeTopLeft;
                                break;
                            case BrickStyle.SlopeTopLeft:
                                tile.BrickStyle = BrickStyle.SlopeTopRight;
                                break;
                            case BrickStyle.SlopeBottomRight:
                                tile.BrickStyle = BrickStyle.SlopeBottomLeft;
                                break;
                            case BrickStyle.SlopeBottomLeft:
                                tile.BrickStyle = BrickStyle.SlopeBottomRight;
                                break;
                        }
                    }

                    else
                    {
                        //  Ignore multi-height tiles when flipping on y-axis

                        // Flip brick-style
                        switch (tile.BrickStyle)
                        {
                            case BrickStyle.SlopeTopRight:
                                tile.BrickStyle = BrickStyle.SlopeBottomRight;
                                break;
                            case BrickStyle.SlopeTopLeft:
                                tile.BrickStyle = BrickStyle.SlopeBottomLeft;
                                break;
                            case BrickStyle.SlopeBottomRight:
                                tile.BrickStyle = BrickStyle.SlopeTopRight;
                                break;
                            case BrickStyle.SlopeBottomLeft:
                                tile.BrickStyle = BrickStyle.SlopeTopLeft;
                                break;
                        }
                    }


                }
            }
        }

        foreach (var item in spriteSizes)
        {
            var flipOrigin = FlipFramed(buffer.Size, item.Key, item.Value, flipX);

            for (int y = 0; y < item.Value.Y; y++)
            {
                int sourceY = y + item.Key.Y;
                int targetY = y + flipOrigin.Y;

                for (int x = 0; x < item.Value.X; x++)
                {
                    try
                    {
                        int sourceX = x + item.Key.X;
                        int targetX = x + flipOrigin.X;

                        Tile tile = (Tile)buffer.Tiles[sourceX, sourceY].Clone();

                        if (tile.Type == (uint)TileType.JunctionBox)
                        {
                            if (tile.U == 18) { tile.U = 36; }
                            else if (tile.U == 36) { tile.U = 18; }
                        }

                        flippedBuffer.Tiles[targetX, targetY] = (Tile)tile;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        foreach (var chest in buffer.Chests)
        {
            var flipOrigin = FlipFramed(buffer.Size, new Vector2Int32(chest.X, chest.Y), new Vector2Short(2, 2), flipX);
            chest.X = flipOrigin.X;
            chest.Y = flipOrigin.Y;
            flippedBuffer.Chests.Add(chest);
        }

        foreach (var sign in buffer.Signs)
        {
            var flipOrigin = FlipFramed(buffer.Size, new Vector2Int32(sign.X, sign.Y), new Vector2Short(2, 2), flipX);
            sign.X = flipOrigin.X;
            sign.Y = flipOrigin.Y;
            flippedBuffer.Signs.Add(sign);
        }

        foreach (var te in buffer.TileEntities)
        {
            var tileProperties = WorldConfiguration.TileProperties[(int)te.TileType];
            Vector2Short tileSize = tileProperties.FrameSize[0];

            var flipOrigin = FlipFramed(buffer.Size, new Vector2Int32(te.PosX, te.PosY), tileSize, flipX);
            te.PosX = (short)flipOrigin.X;
            te.PosY = (short)flipOrigin.Y;
            flippedBuffer.TileEntities.Add(te);
        }


            if (rotate)
            {
                ClipboardBuffer rotatedBuffer = new ClipboardBuffer(new Vector2Int32(flippedBuffer.Size.Y, flippedBuffer.Size.X));
                // Attempt to make a new buffer
                int FlipmaxX = flippedBuffer.Size.X - 1;
                int FlipmaxY = flippedBuffer.Size.Y - 1;

                // Get buffer horizontal
                for (int x = 0; x <= FlipmaxX; x++)
                {
                    // Get buffer vertical
                    for (int y = 0; y <= FlipmaxY; y++)
                    {
                        // Offet tiles 90
                        Tile tile = (Tile)flippedBuffer.Tiles[x, y].Clone();
                        var tileProperties = WorldConfiguration.TileProperties[tile.Type];

                        // kill sprites
                        if (tileProperties.IsFramed)
                        {
                            tile.IsActive = false;
                        }
                        rotatedBuffer.Tiles[y, x] = (Tile)tile; // Flipping x & y causes a rotation of 90 to the right
                    }
                }

                // Replace Buffers
                return rotatedBuffer;
            }
            else
            {
                return flippedBuffer;
            }
        }
    

    private static Vector2Int32 FlipFramed(Vector2Int32 totalSize, Vector2Int32 origin, Vector2Short spriteSize, bool flipX)
    {
        var maxX = totalSize.X - 1;
        var maxY = totalSize.Y - 1;

        int bufferX;
        int bufferY;

        if (flipX)
        {
            // flip
            bufferX = maxX - origin.X - (spriteSize.X - 1);
            bufferY = origin.Y;
        }
        else
        {
            bufferX = origin.X;
            bufferY = maxY - origin.Y - (spriteSize.Y - 1);
        }

        return new Vector2Int32(bufferX, bufferY);
    }
}
