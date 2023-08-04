using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public sealed class BlockShufflePlugin : BasePlugin //Originally: HackedPlugin, BlockShuffle
{
    public BlockShufflePlugin(WorldViewModel worldViewModel) : base(worldViewModel) { Name = "Shuffle Block Locations"; }

    public override void Execute()
    {
        //Check if a world is loaded
        if (_wvm.CurrentWorld == null)
            return;

        //Show settings window / get settings
        BlockShufflePluginView settingsView = new(_wvm.Selection.IsActive);
        if (settingsView.ShowDialog() == false)
            return;

        //Get affected area
        var selectionArea = settingsView.OnlySelection ? _wvm.Selection.SelectionArea : new(0, 0, _wvm.CurrentWorld.Size.X, _wvm.CurrentWorld.Size.Y);

        //Randomizer with Seed
        Random rng = new(settingsView.Seed);

        //Select all affected tiles
        List<Tile> selectedTiles = new();
        for (int x = selectionArea.Left; x < selectionArea.Right; x++) //From left to right
            for (int y = selectionArea.Top; y < selectionArea.Bottom; y++) //From top to bottom
                if (!IsSensitiveGroup(_wvm.CurrentWorld.Tiles[x, y].Type))
                {
                    if (!settingsView.IncludeTileEntities)
                        if (WorldConfiguration.GetTileProperties(_wvm.CurrentWorld.Tiles[x, y].Type).IsFramed)
                            continue;
                    if (settingsView.SensitivePlatform)
                        if ((y - 1) > 0 && IsSensitiveGroup(_wvm.CurrentWorld.Tiles[x, y - 1].Type))
                            continue;
                    if ((EmptyCheck(_wvm.CurrentWorld.Tiles[x, y], settingsView.ConsiderWallEmpty, settingsView.ConsiderLiquidEmpty)
                        && settingsView.ReplaceEmptyPercentage != 100) || settingsView.ConsiderEverything)
                        if (settingsView.ReplaceEmptyPercentage == 0) continue;
                        else
                        {
                            if (rng.Next(0, 100) < settingsView.ReplaceEmptyPercentage)
                                selectedTiles.Add(_wvm.CurrentWorld.Tiles[x, y]);
                            else continue;
                        }
                    else selectedTiles.Add(_wvm.CurrentWorld.Tiles[x, y]);
                }
        if (selectedTiles == null || selectedTiles.Count <= 1)
            return;

        //Randomize Tiles (Simple algorithm)
        int shufflesize = selectedTiles.Count;
        while (shufflesize > 1)
        {
            int k = rng.Next(shufflesize--);
            Tile temp = selectedTiles[shufflesize];
            selectedTiles[shufflesize] = selectedTiles[k];
            selectedTiles[k] = temp;
        }

        //Randomize Selection/World
        int lstIndex = 0;
        for (int x = selectionArea.Left; x < selectionArea.Right; x++)
            for (int y = selectionArea.Top; y < selectionArea.Bottom; y++)
                if (!IsSensitiveGroup(_wvm.CurrentWorld.Tiles[x, y].Type))
                {
                    if (!settingsView.IncludeTileEntities)
                        if (WorldConfiguration.GetTileProperties(_wvm.CurrentWorld.Tiles[x, y].Type).IsFramed)
                            continue;
                    if (settingsView.SensitivePlatform)
                        if ((y - 1) > 0 && IsSensitiveGroup(_wvm.CurrentWorld.Tiles[x, y - 1].Type))
                            continue;
                    //Air Replace Options
                    if ((EmptyCheck(_wvm.CurrentWorld.Tiles[x, y], settingsView.ConsiderWallEmpty, settingsView.ConsiderLiquidEmpty)
                        && settingsView.ReplaceEmptyPercentage != 100) || settingsView.ConsiderEverything)
                        if (settingsView.ReplaceEmptyPercentage == 0) continue;
                        else if (rng.Next(0, 100) >= settingsView.ReplaceEmptyPercentage)
                            continue;
                    //Safe Block before updating it!
                    if (settingsView.EnableUndo)
                        _wvm.UndoManager.SaveTile(x, y);
                    //HACK: Edits the World-View (_wvm) directly!
                    _wvm.CurrentWorld.Tiles[x, y] = selectedTiles[lstIndex];
                    //Safety: The worldborder does NOT like visual waterfalls -> Remove all Slopes.
                    if (x <= 20 || x >= _wvm.CurrentWorld.TilesWide - 20 || y <= 20 || y >= _wvm.CurrentWorld.TilesHigh - 20)
                        _wvm.CurrentWorld.Tiles[x, y].BrickStyle = BrickStyle.Full;
                    //Use "Random" Index:
                    lstIndex++;
                    if (lstIndex >= selectedTiles.Count)
                        lstIndex = 0; //Loop back
                }

        if (settingsView.EnableUndo)
            _wvm.UndoManager.SaveUndo();
        _wvm.UpdateRenderRegion(selectionArea); // Re-render map
        _wvm.MinimapImage = Render.RenderMiniMap.Render(_wvm.CurrentWorld); // Update Minimap
    }

    private static bool EmptyCheck(Tile pTile, bool pConsiderWallEmpty, bool pConsiderLiquidEmpty)
    {
        if (pConsiderWallEmpty && pConsiderLiquidEmpty) return !pTile.IsActive;
        else if (pConsiderWallEmpty) return !pTile.IsActive && !pTile.HasLiquid;
        else if (pConsiderLiquidEmpty) return !pTile.IsActive && pTile.Wall == 0 && !pTile.HasWire;
        else return pTile.IsEmpty;
    }

    private static bool IsSensitiveGroup(int pTileType)
    {
        if (  TileTypes.IsChest(pTileType)
           || TileTypes.IsSign(pTileType)
           || TileTypes.IsTileEntity(pTileType)
           || IsSensitive(pTileType)
            ) return true;
        else return false;
    }
    private static bool IsSensitive(int pTileType)
    {
        return pTileType == 12 //Heart Crystal
            || pTileType == 26 //Demon Altar
            || pTileType == 31 //Orb/Heart
            || pTileType == 231 //Larva
            || pTileType == 237 //Lihzahrd Altar
            || pTileType == 238 //Plantera Bulb
            || pTileType == 488 //Fallen Log (Fairys)
            || pTileType == 639 //Mana Crystal
            ;
    }
}
