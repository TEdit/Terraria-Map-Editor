using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.Terraria;

namespace TEdit.Export;

/// <summary>
/// Exports a world region as a set of 256×256 PNG tiles for use with Leaflet.
/// Generates pixel-map tiles (z=-2, -1, 0) on a background thread and
/// delegates textured tile rendering (z=2, 3, 4) to the XNA renderer via callback.
/// Also writes index.html, .gitignore, GitHub Pages workflow, and README.
/// </summary>
internal static class LeafletTileExporter
{
    private const int TileSize = 256;

    // Leaflet CRS.Simple coordinate scale: 1 unit = 4 world tiles.
    // This aligns tile indices with Leaflet's 256/2^z units-per-tile formula.
    private const int CoordScale = 4;

    // Pixel map zoom levels: (leafletZoom, downsampleFactor) — world tiles per pixel
    public static readonly (int leafletZoom, int downsample)[] PixelMapZoomLevels =
    [
        (0, 4),   // z=0: 0.25x (1 pixel = 4×4 world tiles, 256px = 1024 tiles = 256 units)
        (1, 2),   // z=1: 0.5x  (1 pixel = 2×2 world tiles, 256px = 512 tiles = 128 units)
        (2, 1),   // z=2: 1x    (1 pixel = 1 world tile,     256px = 256 tiles = 64 units)
    ];

    // Textured zoom levels: (leafletZoom, scale) — pixels per world tile
    public static readonly (int leafletZoom, int scale)[] TexturedZoomLevels =
    [
        (4, 4),   // z=4:  4x  (256px = 64 tiles = 16 units)
        (5, 8),   // z=5:  8x  (256px = 32 tiles = 8 units)
        (6, 16),  // z=6:  16x (256px = 16 tiles = 4 units)
    ];

    /// <summary>
    /// Returns the background color for a given world Y coordinate based on depth zone.
    /// Matches the gradient zones used in the minimap and game rendering.
    /// </summary>
    private static Color GetBackgroundColor(World w, int worldY)
    {
        var globalColors = WorldConfiguration.GlobalColors;

        string key;
        if (worldY < 80)
            key = "Space";
        else if (worldY > w.TilesHigh - 192)
            key = "Hell";
        else if (worldY > w.RockLevel)
            key = "Rock";
        else if (worldY > w.GroundLevel)
            key = "Earth";
        else
            key = "Sky";

        if (globalColors.TryGetValue(key, out var zoneColor))
            return new Color(zoneColor.R, zoneColor.G, zoneColor.B, zoneColor.A);

        return new Color(128, 128, 128, 255);
    }

    /// <summary>
    /// Captured pixel-map rendering state (thread-safe snapshot from the UI thread).
    /// </summary>
    public readonly record struct PixelMapState(
        World World,
        Color BackgroundColor,
        bool ShowWalls, bool ShowTiles, bool ShowLiquid,
        bool ShowRedWires, bool ShowBlueWires, bool ShowGreenWires, bool ShowYellowWires);

    /// <summary>
    /// Delegate for rendering textured tiles at a given zoom level.
    /// Called on the UI thread. Must render tiles to the zoom directory and update tilesCompleted.
    /// </summary>
    public delegate Task<int> ExportTexturedTilesDelegate(
        string outputDir, RectangleInt32 area, int tileSize,
        int leafletZoom, int scale,
        IProgress<ProgressChangedEventArgs>? progress,
        int tilesCompleted, int totalTiles);

    /// <summary>
    /// Counts total tiles across all zoom levels for progress reporting.
    /// </summary>
    public static int CountTotalTiles(RectangleInt32 area)
    {
        int total = 0;
        foreach (var (_, downsample) in PixelMapZoomLevels)
        {
            int outW = (area.Width + downsample - 1) / downsample;
            int outH = (area.Height + downsample - 1) / downsample;
            total += ((outW + TileSize - 1) / TileSize) * ((outH + TileSize - 1) / TileSize);
        }
        foreach (var (_, scale) in TexturedZoomLevels)
        {
            int outW = area.Width * scale;
            int outH = area.Height * scale;
            total += ((outW + TileSize - 1) / TileSize) * ((outH + TileSize - 1) / TileSize);
        }
        return total;
    }

    public static async Task ExportAsync(
        string outputDir, RectangleInt32 area, string worldName,
        PixelMapState pixelMap,
        ExportTexturedTilesDelegate exportTexturedTiles,
        IProgress<ProgressChangedEventArgs>? progress,
        Action<float>? layerProgress = null,
        CancellationToken cancellationToken = default)
    {
        // Write support files first
        progress?.Report(new ProgressChangedEventArgs(0, "Writing support files..."));
        WriteLeafletHtml(outputDir, area, worldName, pixelMap.World);
        WriteGitIgnore(outputDir);
        WriteGitHubPagesAction(outputDir);
        WriteReadme(outputDir, worldName);

        int totalTiles = CountTotalTiles(area);
        int tilesCompleted = 0;

        // Export pixel map tiles (z=0, z=1, z=2) on background thread
        foreach (var (leafletZoom, downsample) in PixelMapZoomLevels)
        {
            cancellationToken.ThrowIfCancellationRequested();
            layerProgress?.Invoke(0f);
            progress?.Report(new ProgressChangedEventArgs(
                totalTiles > 0 ? tilesCompleted * 99 / totalTiles : 0,
                $"Exporting pixel map tiles (zoom {leafletZoom}, 1:{downsample})..."));
            tilesCompleted = await ExportPixelMapTilesAsync(
                outputDir, area, leafletZoom, downsample,
                pixelMap, progress, layerProgress, tilesCompleted, totalTiles,
                cancellationToken);
        }

        // Export textured tiles via renderer callback (UI thread)
        foreach (var (leafletZoom, scale) in TexturedZoomLevels)
        {
            cancellationToken.ThrowIfCancellationRequested();
            layerProgress?.Invoke(0f);
            progress?.Report(new ProgressChangedEventArgs(
                tilesCompleted * 99 / totalTiles,
                $"Exporting textured tiles (zoom {leafletZoom}, scale {scale}x)..."));
            tilesCompleted = await exportTexturedTiles(
                outputDir, area, TileSize, leafletZoom, scale,
                progress, tilesCompleted, totalTiles);
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Map tiles export complete."));
    }

    private static Task<int> ExportPixelMapTilesAsync(
        string outputDir, RectangleInt32 area,
        int leafletZoom, int downsample,
        PixelMapState pm,
        IProgress<ProgressChangedEventArgs>? progress,
        Action<float>? layerProgress,
        int tilesCompleted, int totalTiles,
        CancellationToken cancellationToken = default)
    {
        int outW = (area.Width + downsample - 1) / downsample;
        int outH = (area.Height + downsample - 1) / downsample;
        int tilesX = (outW + TileSize - 1) / TileSize;
        int tilesY = (outH + TileSize - 1) / TileSize;
        int layerTotalTiles = tilesX * tilesY;

        string zoomDir = Path.Combine(outputDir, "tiles", leafletZoom.ToString());
        Directory.CreateDirectory(zoomDir);

        int completedCapture = tilesCompleted;
        int totalCapture = totalTiles;

        return Task.Run(() =>
        {
            int rowStride = 1 + TileSize * 4;
            var rawImageData = new byte[TileSize * rowStride];

            for (int ty = 0; ty < tilesY; ty++)
            {
                for (int tx = 0; tx < tilesX; tx++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int pixelX0 = tx * TileSize;
                    int pixelY0 = ty * TileSize;
                    int tilePxW = Math.Min(TileSize, outW - pixelX0);
                    int tilePxH = Math.Min(TileSize, outH - pixelY0);

                    Array.Clear(rawImageData, 0, rawImageData.Length);

                    for (int row = 0; row < TileSize; row++)
                    {
                        if (row < tilePxH)
                        {
                            int rawOffset = row * rowStride;

                            for (int col = 0; col < tilePxW; col++)
                            {
                                int worldX = area.Left + (pixelX0 + col) * downsample;
                                int worldY = area.Top + (pixelY0 + row) * downsample;

                                if (worldX >= 0 && worldX < pm.World.TilesWide &&
                                    worldY >= 0 && worldY < pm.World.TilesHigh)
                                {
                                    var bgColor = GetBackgroundColor(pm.World, worldY);

                                    var tileColor = PixelMap.GetTileColor(
                                        pm.World.Tiles[worldX, worldY], bgColor,
                                        showWall: pm.ShowWalls, showTile: pm.ShowTiles,
                                        showLiquid: pm.ShowLiquid, showRedWire: pm.ShowRedWires,
                                        showBlueWire: pm.ShowBlueWires, showGreenWire: pm.ShowGreenWires,
                                        showYellowWire: pm.ShowYellowWires);

                                    if (tileColor.A < 255)
                                        tileColor = bgColor.AlphaBlend(tileColor);

                                    int i = rawOffset + 1 + col * 4;
                                    rawImageData[i] = tileColor.R;
                                    rawImageData[i + 1] = tileColor.G;
                                    rawImageData[i + 2] = tileColor.B;
                                    rawImageData[i + 3] = tileColor.A;
                                }
                            }
                        }
                    }

                    TilePngWriter.Write(Path.Combine(zoomDir, $"{tx}_{ty}.png"), TileSize, TileSize, rawImageData);

                    completedCapture++;
                    int layerDone = completedCapture - (tilesCompleted); // relative to layer start
                    if (completedCapture % 10 == 0)
                    {
                        layerProgress?.Invoke(layerTotalTiles > 0 ? (float)layerDone / layerTotalTiles : 0f);
                        progress?.Report(new ProgressChangedEventArgs(
                            completedCapture * 99 / totalCapture,
                            string.Format(
                                Properties.Language.export_tiles_progress ?? "Exporting tiles: zoom {0}, tile {1}/{2}...",
                                leafletZoom, completedCapture, totalCapture)));
                    }
                }
            }
            return completedCapture;
        });
    }

    #region File Generation

    private static void WriteLeafletHtml(string outputDir, RectangleInt32 area, string worldName, World world)
    {
        // Leaflet bounds in coordinate units (1 unit = CoordScale world tiles)
        double w = (double)area.Width / CoordScale;
        double h = (double)area.Height / CoordScale;

        // Build marker data
        var markers = new System.Text.StringBuilder();
        BuildMarkerScript(markers, world, area);

        string html = $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>{System.Security.SecurityElement.Escape(worldName)} - Map</title>
<link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css""
  integrity=""sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="" crossorigin="""" />
<script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""
  integrity=""sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="" crossorigin=""""></script>
<style>
  html, body {{ margin: 0; padding: 0; height: 100%; }}
  #map {{ width: 100%; height: 100%; background: #000; }}
  .leaflet-tile {{ image-rendering: pixelated; image-rendering: -moz-crisp-edges; }}
  .leaflet-control-attribution a.tedit-logo {{
    display: inline-block;
    vertical-align: middle;
  }}
  .leaflet-control-attribution a.tedit-logo img {{
    width: 48px;
    height: 48px;
    object-fit: contain;
    vertical-align: middle;
    margin-right: 4px;
  }}
  .leaflet-control-attribution {{
    background: rgba(30, 30, 30, 0.8) !important;
    color: #ccc !important;
  }}
  .leaflet-control-attribution a {{
    color: #8cf !important;
  }}
  .leaflet-control-zoom a {{
    background-color: #2a2a2a !important;
    color: #ddd !important;
    border-color: #444 !important;
  }}
  .leaflet-control-zoom a:hover {{
    background-color: #3a3a3a !important;
    color: #fff !important;
  }}
  .leaflet-control-zoom {{
    border-color: #444 !important;
  }}
</style>
</head>
<body>
<div id=""map""></div>
<script>
// Leaflet.ContinuousZoom plugin (Ilya Zverev, WTFPL)
L.TileLayer.mergeOptions({{ nativeZooms: [] }});
L.TileLayer.addInitHook(function () {{
  var opt = this.options, zooms = opt.nativeZooms;
  if (zooms && zooms.length > 0) {{
    var minZoom = Infinity, i;
    for (i = 0; i < zooms.length; i++)
      if (zooms[i] < minZoom) minZoom = zooms[i];
    var current = opt.minZoom != null ? opt.minZoom : 0;
    opt.minZoom = Math.max(current, minZoom);
  }}
}});
L.TileLayer.include({{
  getTileSize: function () {{
    var map = this._map,
      tileSize = L.GridLayer.prototype.getTileSize.call(this),
      zoom = this._tileZoom + this.options.zoomOffset,
      nativeZoom = this._mapNativeZoom(zoom);
    return nativeZoom == zoom ? tileSize :
      tileSize.divideBy(map.getZoomScale(nativeZoom, zoom)).round();
  }},
  _getZoomForUrl: function () {{
    var zoom = this._tileZoom,
      maxZoom = this.options.maxZoom,
      zoomReverse = this.options.zoomReverse,
      zoomOffset = this.options.zoomOffset;
    if (zoomReverse) zoom = maxZoom - zoom;
    zoom += zoomOffset;
    return this._mapNativeZoom(zoom);
  }},
  _mapNativeZoom: function (zoom) {{
    var zooms = this.options.nativeZooms,
      minNativeZoom = this.options.minNativeZoom,
      maxNativeZoom = this.options.maxNativeZoom;
    if (zooms && zooms.length > 0) {{
      var prevZoom = -Infinity, minZoom = Infinity, i;
      for (i = 0; i < zooms.length; i++) {{
        if (zooms[i] <= zoom && zooms[i] > prevZoom) prevZoom = zooms[i];
        if (zooms[i] < minZoom) minZoom = zooms[i];
      }}
      zoom = prevZoom === -Infinity ? minZoom : prevZoom;
    }} else if (maxNativeZoom !== null && zoom > maxNativeZoom) {{
      zoom = maxNativeZoom;
    }} else if (minNativeZoom !== null && zoom < minNativeZoom) {{
      zoom = minNativeZoom;
    }}
    return zoom;
  }}
}});

// Map setup
var bounds = L.latLngBounds(
  L.latLng(0, 0),
  L.latLng(-{h}, {w})
);

var map = L.map('map', {{
  crs: L.CRS.Simple,
  minZoom: 0,
  maxZoom: 8,
  maxBounds: bounds.pad(0.1),
  maxBoundsViscosity: 1.0,
  attributionControl: false
}});

L.control.attribution({{ prefix: '<a class=""tedit-logo"" href=""https://docs.tedit.dev/"" target=""_blank""><img src=""data:image/png;base64,{TEditLogoBase64}"" alt=""TEdit"" />TEdit</a>' }}).addTo(map);

L.tileLayer('tiles/{{z}}/{{x}}_{{y}}.png', {{
  tileSize: 256,
  nativeZooms: [0, 1, 2, 4, 5, 6],
  maxZoom: 8,
  noWrap: true,
  bounds: bounds,
  errorTileUrl: '',
  keepBuffer: 8,
  updateWhenZooming: false
}}).addTo(map);

map.fitBounds(bounds);

{markers}
</script>
</body>
</html>";

        File.WriteAllText(Path.Combine(outputDir, "index.html"), html);
    }

    private static string EscapeJs(string s) =>
        s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");

    private static void BuildMarkerScript(System.Text.StringBuilder sb, World world, RectangleInt32 area)
    {
        // Helper: convert world tile coords to Leaflet CRS.Simple coords
        // lat = -(tileY - area.Top) / CoordScale, lng = (tileX - area.Left) / CoordScale
        string ToLatLng(double tileX, double tileY) =>
            $"[{-(tileY - area.Top) / CoordScale:F2}, {(tileX - area.Left) / CoordScale:F2}]";

        // Icon factory function
        sb.AppendLine(@"
function mkIcon(emoji, size) {
  return L.divIcon({
    html: '<span style=""font-size:' + size + 'px"">' + emoji + '</span>',
    className: '',
    iconSize: [size, size],
    iconAnchor: [size/2, size/2]
  });
}");

        // -- World markers (always visible) --
        sb.AppendLine("var worldMarkers = L.layerGroup().addTo(map);");

        // Spawn point
        sb.AppendLine($"L.marker({ToLatLng(world.SpawnX, world.SpawnY)}, {{icon: mkIcon('\ud83c\udfe0', 24)}}).bindPopup('Spawn').addTo(worldMarkers);");

        // Dungeon
        sb.AppendLine($"L.marker({ToLatLng(world.DungeonX, world.DungeonY)}, {{icon: mkIcon('\ud83d\udc80', 24)}}).bindPopup('Dungeon').addTo(worldMarkers);");

        // Team spawns (if present and different from main spawn)
        if (world.TeamBasedSpawnsSeed && world.TeamSpawns.Count > 0)
        {
            string[] teamColors = ["#e74c3c", "#2ecc71", "#3498db", "#f1c40f", "#e91e9e", "#ecf0f1"];
            for (int i = 0; i < world.TeamSpawns.Count && i < World.TeamCount; i++)
            {
                var ts = world.TeamSpawns[i];
                if (ts.X == world.SpawnX && ts.Y == world.SpawnY) continue;
                if (ts.X == 0 && ts.Y == 0) continue;
                string color = i < teamColors.Length ? teamColors[i] : "#ccc";
                string teamName = i < World.TeamNames.Length ? World.TeamNames[i] : $"Team {i + 1}";
                sb.AppendLine($@"L.marker({ToLatLng(ts.X, ts.Y)}, {{icon: L.divIcon({{html: '<span style=""font-size:20px;color:{color};text-shadow:0 0 3px #000"">\u2691</span>', className: '', iconSize: [20, 20], iconAnchor: [10, 10]}}) }}).bindPopup('{EscapeJs(teamName)} Spawn').addTo(worldMarkers);");
            }
        }

        // -- NPC markers (only visible at textured zoom levels) --
        sb.AppendLine("var npcMarkers = L.layerGroup();");

        foreach (var npc in world.NPCs)
        {
            // NPC position is in world pixels (16px per tile)
            double tileX = npc.Position.X / 16.0;
            double tileY = npc.Position.Y / 16.0;
            string label = EscapeJs(string.IsNullOrEmpty(npc.DisplayName) ? npc.Name : npc.DisplayName);
            sb.AppendLine($"L.marker({ToLatLng(tileX, tileY)}, {{icon: mkIcon('\ud83e\uddd1', 20)}}).bindPopup('{label}').addTo(npcMarkers);");
        }

        // Show NPC markers only at textured zoom levels (>= 4)
        sb.AppendLine(@"
map.on('zoomend', function() {
  if (map.getZoom() >= 4) {
    if (!map.hasLayer(npcMarkers)) npcMarkers.addTo(map);
  } else {
    if (map.hasLayer(npcMarkers)) map.removeLayer(npcMarkers);
  }
});
if (map.getZoom() >= 4) npcMarkers.addTo(map);");

        // Layer control
        sb.AppendLine(@"
L.control.layers(null, {
  'World Markers': worldMarkers,
  'NPCs': npcMarkers
}, {collapsed: false}).addTo(map);");
    }

    private static void WriteGitIgnore(string outputDir)
    {
        const string content = @"# OS
.DS_Store
Thumbs.db
desktop.ini

# Editor
*.swp
*.swo
*~
.vscode/
.idea/
";
        File.WriteAllText(Path.Combine(outputDir, ".gitignore"), content);
    }

    private static void WriteGitHubPagesAction(string outputDir)
    {
        string workflowDir = Path.Combine(outputDir, ".github", "workflows");
        Directory.CreateDirectory(workflowDir);

        const string workflow = @"name: Deploy to GitHub Pages

on:
  push:
    branches: [main, master]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: false

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/configure-pages@v5
      - uses: actions/upload-pages-artifact@v3
        with:
          path: .
      - id: deployment
        uses: actions/deploy-pages@v4
";
        File.WriteAllText(Path.Combine(workflowDir, "pages.yml"), workflow);
    }

    private static void WriteReadme(string outputDir, string worldName)
    {
        string escaped = worldName.Replace("`", "");
        string content = $@"# {escaped} — Interactive Map

Interactive [Leaflet](https://leafletjs.com/) tile map exported from [TEdit](https://docs.tedit.dev/).

## View Locally

Open `index.html` in a browser. Some browsers block local tile loading; use a local server:

```bash
# Python
python -m http.server 8000

# Node
npx serve .

# PHP
php -S localhost:8000
```

Then open `http://localhost:8000`.

## Deploy to GitHub Pages

1. Create a new GitHub repository
2. Push this folder as the repo contents:
   ```bash
   git init
   git add .
   git commit -m ""Initial map export""
   git branch -M main
   git remote add origin https://github.com/YOUR_USER/YOUR_REPO.git
   git push -u origin main
   ```
3. Go to **Settings → Pages** and set Source to **GitHub Actions**
4. The included workflow (`.github/workflows/pages.yml`) will auto-deploy on push
5. Your map will be live at `https://YOUR_USER.github.io/YOUR_REPO/`

## Zoom Levels

| Zoom | Scale | Description |
|------|-------|-------------|
| 0    | 0.25x | 1 pixel = 4x4 tiles (overview) |
| 1    | 0.5x  | 1 pixel = 2x2 tiles |
| 2    | 1x    | 1 pixel = 1 tile |
| 4    | 4x    | Textured rendering |
| 5    | 8x    | Textured rendering |
| 6    | 16x   | Textured rendering (detail) |

---
*Generated by [TEdit](https://docs.tedit.dev/)*
";
        File.WriteAllText(Path.Combine(outputDir, "README.md"), content);
    }

    private static string? _teditLogoBase64Cache;

    private static string TEditLogoBase64
    {
        get
        {
            if (_teditLogoBase64Cache != null) return _teditLogoBase64Cache;

            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream("TEdit.Images.favicon-96x96.png");
            if (stream != null)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                _teditLogoBase64Cache = Convert.ToBase64String(ms.ToArray());
            }
            else
            {
                _teditLogoBase64Cache = "";
            }
            return _teditLogoBase64Cache;
        }
    }

    #endregion
}
