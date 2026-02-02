using Terraria.Enums;
using Terraria.ID;
using Terraria;

namespace SettingsFileUpdater.TerrariaHost
{
    /// <summary>
    /// World-based BuildSafe test.
    ///
    /// Notes:
    /// - Emulates MELEE tile cutting (plants/vines/etc). Does NOT simulate pickaxe mining.
    /// - CanCutTile() is coordinate-dependent, so we place a controlled pad and test a 3x3 area.
    /// - BuildSafe=true only if all 9 tiles remain after the cut pass.
    /// </summary>
    public static class BuildSafeWorldTester
    {
        #region Cache

        /// <summary>
        /// Per-tile cache so we only run the world test once per TileID.
        /// </summary>
        private static bool[] _has;

        /// <summary>
        /// Cached results: true = BuildSafe, false = not BuildSafe.
        /// </summary>
        private static bool[] _safe;

        #endregion

        #region Test Location

        /// <summary>
        /// Choose one test coordinate and reuse it.
        /// </summary>
        private static int _tx = -1;

        private static int _ty = -1;

        #endregion

        #region Public API

        /// <summary>
        /// Returns cached BuildSafe for a tileId, or runs the world test once and stores it.
        /// </summary>
        public static bool GetOrTest(int tileId)
        {
            _has ??= new bool[TileID.Count];
            _safe ??= new bool[TileID.Count];

            if (_has[tileId])
                return _safe[tileId];

            bool safe = TestTileByMeleeCut(tileId);

            _has[tileId] = true;
            _safe[tileId] = safe;
            return safe;
        }
        #endregion

        #region World Test

        /// <summary>
        /// World-based test:
        /// - Places a forced 3x3 block of the tile
        /// - Simulates a melee "cut" pass over the 3x3 using CanCutTile + KillTile
        /// - BuildSafe is true only if the entire 3x3 still exists afterward
        /// </summary>
        private static bool TestTileByMeleeCut(int tileId)
        {
            // NOTE:
            // If no world is loaded, we can’t run a placement/cut test.
            // Current behavior: treat as safe to avoid false negatives.
            if (Main.tile == null || Main.maxTilesX <= 0 || Main.maxTilesY <= 0)
                return true;

            EnsureTestSpot();

            // Choose top-left for the 3x3.
            int x = _tx;
            int y = _ty;

            // NOTE:
            // We clear an area and place a stable stone floor so CanCutTile()
            // doesn't fail due to strange surrounding conditions.
            PrepareTestPad(x, y);

            // Force-place the tile as a 3x3 sample.
            bool placed = ForcePlaceTile3x3ForTest(x, y, tileId);
            if (!placed)
                return false;

            // ---- "Weapon swing" style cut test ----
            // NOTES:
            // - This is meant to emulate melee "cutting" (plants/vines/etc), not pickaxe mining.
            // - The logic is applied to each cell because CanCutTile() is coordinate-dependent.
            bool cutCandidate = Main.tileCut[tileId] || TileID.Sets.bonusCutTiles[tileId];

            if (cutCandidate && WorldGen.CanCutTile(x + 1, y + 1, TileCuttingContext.AttackMelee))
            {
                // IMPORTANT: Cut logic typically operates per tile.
                // If you want to simulate a swing across the entire 3x3,
                // apply the cut attempt to each tile in the 3x3.
                for (int yy = y; yy < y + 3; yy++)
                    for (int xx = x; xx < x + 3; xx++)
                    {
                        if (!WorldGen.InWorld(xx, yy, 10))
                            continue;

                        // Use CanCutTile per coordinate (it depends on neighbors/walls/etc).
                        if (WorldGen.CanCutTile(xx, yy, TileCuttingContext.AttackMelee))
                            WorldGen.KillTile(xx, yy, fail: false, effectOnly: false, noItem: true);
                    }

                WorldGen.RangeFrame(x - 2, y - 2, x + 4, y + 4);
            }

            // Final rule:
            // If any of the 3x3 tiles are missing/changed => NOT safe.
            return IsTile3x3Intact(x, y, tileId);
        }
        #endregion

        #region Placement + Validation

        /// <summary>
        /// Force-places a 3x3 sample of a tile at the given top-left coordinate.
        /// Returns true only if all 9 cells exist and match the requested tileId.
        /// </summary>
        private static bool ForcePlaceTile3x3ForTest(int x, int y, int tileId)
        {
            // x,y is the TOP-LEFT of the 3x3.
            for (int yy = y; yy < y + 3; yy++)
                for (int xx = x; xx < x + 3; xx++)
                {
                    if (!WorldGen.InWorld(xx, yy, 10))
                        return false;

                    Main.tile[xx, yy].ClearEverything();
                    Main.tile[xx, yy].active(true);
                    Main.tile[xx, yy].type = (ushort)tileId;

                    // Keep consistent.
                    Main.tile[xx, yy].halfBrick(false);
                    Main.tile[xx, yy].slope(0);
                    Main.tile[xx, yy].liquid = 0;
                    Main.tile[xx, yy].frameX = 0;
                    Main.tile[xx, yy].frameY = 0;
                }

            // Frame around the block so the game updates neighbors/state.
            int cx = x + 1;
            int cy = y + 1;
            WorldGen.RangeFrame(x - 2, y - 2, x + 4, y + 4);
            WorldGen.SquareTileFrame(cx, cy, true);

            // Confirm the 3x3 exists right after placement.
            return IsTile3x3Intact(x, y, tileId);
        }

        /// <summary>
        /// Returns true if the full 3x3 area is active and matches the expected tileId.
        /// </summary>
        private static bool IsTile3x3Intact(int x, int y, int tileId)
        {
            for (int yy = y; yy < y + 3; yy++)
                for (int xx = x; xx < x + 3; xx++)
                {
                    if (!WorldGen.InWorld(xx, yy, 10))
                        return false;

                    if (!Main.tile[xx, yy].active())
                        return false;

                    if (Main.tile[xx, yy].type != (ushort)tileId)
                        return false;
                }

            return true;
        }
        #endregion

        #region Test Pad Setup

        /// <summary>
        /// Chooses a stable test location in the current world (center-ish, below surface).
        /// </summary>
        private static void EnsureTestSpot()
        {
            if (_tx != -1) return;

            // Middle-ish of the world, safely away from edges.
            _tx = Main.maxTilesX / 2;
            _ty = (int)Main.worldSurface + 50;

            if (!WorldGen.InWorld(_tx, _ty, 50))
            {
                _tx = 300;
                _ty = 300;
            }
        }

        /// <summary>
        /// Clears a small area and builds a stone floor under the test zone.
        /// This makes placement/cut checks more consistent.
        /// </summary>
        private static void PrepareTestPad(int x, int y)
        {
            // Clear a small area.
            for (int yy = y - 6; yy <= y + 6; yy++)
                for (int xx = x - 6; xx <= x + 6; xx++)
                {
                    if (!WorldGen.InWorld(xx, yy, 10)) continue;
                    var t = Main.tile[xx, yy];
                    if (t == null) continue;

                    t.ClearEverything();
                }

            // Build a simple solid floor under the test spot.
            // CanCutTile() checks the tile below and rejects certain types;
            // Stone avoids those special cases.
            for (int xx = x - 6; xx <= x + 6; xx++)
            {
                if (!WorldGen.InWorld(xx, y + 2, 10)) continue;

                PlaceSolid(xx, y + 1, TileID.Stone);
                PlaceSolid(xx, y + 2, TileID.Stone);
            }

            WorldGen.RangeFrame(x - 8, y - 8, x + 8, y + 8);
        }

        /// <summary>
        /// Places a single solid tile (used for the test pad floor).
        /// </summary>
        private static void PlaceSolid(int x, int y, int tileId)
        {
            var t = Main.tile[x, y];
            t.ClearEverything();
            t.active(true);
            t.type = (ushort)tileId;
            t.slope(0);
            t.halfBrick(false);
            WorldGen.SquareTileFrame(x, y, true);
        }
        #endregion
    }
}
