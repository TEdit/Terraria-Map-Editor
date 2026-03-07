using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TEdit.Common.Utility;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.Terraria;

namespace TEdit.Export;

/// <summary>
/// Exports a world region to a single PNG file.
/// Pixel map mode (scale 1): renders 1 pixel per tile using PixelMap colors (CPU-only).
/// Textured mode (scale > 1): renders via XNA using a callback delegate for chunk rendering.
/// </summary>
internal static class PngExporter
{
    /// <summary>
    /// Callback that renders a world-space chunk to RGBA pixel data.
    /// Parameters: worldX, worldY (top-left tile coords), pixelW, pixelH (output size), scale.
    /// Returns Color[] of size pixelW × pixelH.
    /// </summary>
    public delegate Color[] RenderChunkDelegate(int worldX, int worldY, int pixelW, int pixelH, int scale);

    public static Task ExportPixelMapAsync(
        string filename, RectangleInt32 area,
        World world, Color backgroundColor,
        bool showWalls, bool showTiles, bool showLiquid,
        bool showRedWires, bool showBlueWires, bool showGreenWires, bool showYellowWires,
        IProgress<ProgressChangedEventArgs>? progress)
    {
        return Task.Run(() =>
        {
            var rowBytes = new byte[area.Width * 4]; // RGBA

            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 65536);
            using var png = new StreamingPngWriter(fs, area.Width, area.Height);

            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    var tileColor = PixelMap.GetTileColor(
                        world.Tiles[x, y], backgroundColor,
                        showWall: showWalls,
                        showTile: showTiles,
                        showLiquid: showLiquid,
                        showRedWire: showRedWires,
                        showBlueWire: showBlueWires,
                        showGreenWire: showGreenWires,
                        showYellowWire: showYellowWires);

                    if (tileColor.A < 255)
                        tileColor = backgroundColor.AlphaBlend(tileColor);

                    int i = (x - area.Left) * 4;
                    rowBytes[i] = tileColor.R;
                    rowBytes[i + 1] = tileColor.G;
                    rowBytes[i + 2] = tileColor.B;
                    rowBytes[i + 3] = tileColor.A;
                }

                png.WriteScanline(rowBytes);

                int rowsDone = y - area.Top + 1;
                if (rowsDone % 50 == 0)
                    progress?.Report(new ProgressChangedEventArgs(
                        rowsDone * 100 / area.Height,
                        $"Exporting row {rowsDone}/{area.Height}..."));
            }

            progress?.Report(new ProgressChangedEventArgs(99, "Finalizing PNG..."));
            png.Finish();
        });
    }

    public static async Task ExportTexturedAsync(
        string filename, RectangleInt32 area, int scale,
        Color backgroundColor, RenderChunkDelegate renderChunk,
        IProgress<ProgressChangedEventArgs>? progress)
    {
        int outputW = area.Width * scale;
        int outputH = area.Height * scale;

        const int maxChunkPx = 4096;
        const long memoryBudget = 256L * 1024 * 1024; // 256 MB for chunk data
        int maxStripPxH = Math.Max(16, (int)(memoryBudget / ((long)outputW * 4)));
        maxStripPxH = Math.Min(maxStripPxH, maxChunkPx);
        int chunkTilesW = Math.Max(1, maxChunkPx / scale);
        int stripTilesH = Math.Max(1, maxStripPxH / scale);

        int totalStrips = (area.Height + stripTilesH - 1) / stripTilesH;
        int totalChunksX = (area.Width + chunkTilesW - 1) / chunkTilesW;
        int stripIndex = 0;

        var rowBytes = new byte[outputW * 4];

        using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 65536);
        using var png = new StreamingPngWriter(fs, outputW, outputH);

        var chunkDataList = new Color[totalChunksX][];
        var chunkWidths = new int[totalChunksX];

        for (int cy = 0; cy < area.Height; cy += stripTilesH)
        {
            int thisTilesH = Math.Min(stripTilesH, area.Height - cy);
            int thisPxH = thisTilesH * scale;

            progress?.Report(new ProgressChangedEventArgs(
                stripIndex * 99 / totalStrips,
                $"Rendering strip {stripIndex + 1}/{totalStrips}..."));
            stripIndex++;

            for (int cxi = 0; cxi < totalChunksX; cxi++)
            {
                int cx = cxi * chunkTilesW;
                int thisTilesW = Math.Min(chunkTilesW, area.Width - cx);
                int thisPxW = thisTilesW * scale;

                chunkDataList[cxi] = renderChunk(area.Left + cx, area.Top + cy, thisPxW, thisPxH, scale);
                chunkWidths[cxi] = thisPxW;
            }

            for (int row = 0; row < thisPxH; row++)
            {
                int destX = 0;
                for (int cxi = 0; cxi < totalChunksX; cxi++)
                {
                    var data = chunkDataList[cxi];
                    int thisPxW = chunkWidths[cxi];
                    int srcOffset = row * thisPxW;

                    for (int col = 0; col < thisPxW; col++)
                    {
                        var c = data[srcOffset + col];
                        int d = (destX + col) * 4;
                        rowBytes[d] = c.R;
                        rowBytes[d + 1] = c.G;
                        rowBytes[d + 2] = c.B;
                        rowBytes[d + 3] = c.A;
                    }
                    destX += thisPxW;
                }
                png.WriteScanline(rowBytes);
            }

            // Yield to the UI thread so progress updates can render
            await Task.Delay(1);
        }

        progress?.Report(new ProgressChangedEventArgs(99, "Finalizing PNG..."));
        png.Finish();
    }
}
