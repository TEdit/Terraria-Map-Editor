using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace TerrariaWorld.Game
{
    partial class World
    {
        public void SaveFile(string filename)
        {
            if (!TileProperties.IsInitialized)
                TileProperties.InitializeTileProperties();

            string backupFileName = filename + ".Tedit";
            if (File.Exists(filename))
            {
                File.Copy(filename, backupFileName, true);
            }
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(this.Header.FileVersion);
                    writer.Write(this.Header.WorldName);
                    writer.Write(this.Header.WorldID);
                    writer.Write((int)this.Header.WorldBounds.TopLeft.X);
                    writer.Write((int)this.Header.WorldBounds.BottomRight.X);
                    writer.Write((int)this.Header.WorldBounds.TopLeft.Y);
                    writer.Write((int)this.Header.WorldBounds.BottomRight.Y);
                    writer.Write(this.Header.MaxTiles.Y);
                    writer.Write(this.Header.MaxTiles.X);
                    writer.Write(this.Header.SpawnTile.X);
                    writer.Write(this.Header.SpawnTile.Y);
                    writer.Write(this.Header.WorldSurface);
                    writer.Write(this.Header.WorldRockLayer);
                    writer.Write(this.Header.Time);
                    writer.Write(this.Header.IsDayTime);
                    writer.Write(this.Header.MoonPhase);
                    writer.Write(this.Header.IsBloodMoon);
                    writer.Write(this.Header.DungeonEntrance.X);
                    writer.Write(this.Header.DungeonEntrance.Y);
                    writer.Write(this.Header.IsBossDowned1);
                    writer.Write(this.Header.IsBossDowned2);
                    writer.Write(this.Header.IsBossDowned3);
                    writer.Write(this.Header.IsShadowOrbSmashed);
                    writer.Write(this.Header.IsSpawnMeteor);
                    writer.Write((byte)this.Header.ShadowOrbCount);
                    writer.Write(this.Header.InvasionDelay);
                    writer.Write(this.Header.InvasionSize);
                    writer.Write(this.Header.InvasionType);
                    writer.Write(this.Header.InvasionX);

                    for (int x = 0; x < this.Header.MaxTiles.X; x++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)x / (double)this.Header.MaxTiles.X * 100.0), "Saving World"));
                        //float num2 = ((float) i) / ((float) this.MaxTiles.X);
                        //string statusText = "Saving world data: " + ((int) ((num2 * 100f) + 1f)) + "%";
                        for (int y = 0; y < this.Header.MaxTiles.Y; y++)
                        {
                            writer.Write(this.Tiles[x, y].IsActive);
                            if (this.Tiles[x, y].IsActive)
                            {
                                writer.Write(this.Tiles[x, y].Type);
                                if (TileProperties.IsFrameImportant[this.Tiles[x, y].Type])
                                {
                                    writer.Write(this.Tiles[x, y].Frame.X);
                                    writer.Write(this.Tiles[x, y].Frame.Y);
                                }
                            }
                            writer.Write(this.Tiles[x, y].IsLighted);
                            if (this.Tiles[x, y].Wall > 0)
                            {
                                writer.Write(true);
                                writer.Write(this.Tiles[x, y].Wall);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                            if (this.Tiles[x, y].Liquid > 0)
                            {
                                writer.Write(true);
                                writer.Write(this.Tiles[x, y].Liquid);
                                writer.Write(this.Tiles[x, y].IsLava);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                        }
                    }
                    for (int chestCount = 0; chestCount < World.MAXCHESTS; chestCount++)
                    {
                        if (this.Chests[chestCount] == null)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(this.Chests[chestCount].Location.X);
                            writer.Write(this.Chests[chestCount].Location.Y);
                            for (int slot = 0; slot < Chest.MAXITEMS; slot++)
                            {
                                writer.Write((byte)this.Chests[chestCount].Items[slot].Stack);
                                if (this.Chests[chestCount].Items[slot].Stack > 0)
                                {
                                    writer.Write(this.Chests[chestCount].Items[slot].Name);
                                }
                            }
                        }
                    }
                    for (int signCount = 0; signCount < World.MAXSIGNS; signCount++)
                    {
                        if ((this.Signs[signCount] == null) || (this.Signs[signCount].Text == null))
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(this.Signs[signCount].Text);
                            writer.Write(this.Signs[signCount].Location.X);
                            writer.Write(this.Signs[signCount].Location.Y);
                        }
                    }
                    for (int npcCount = 0; npcCount < World.MAXNPCS; npcCount++)
                    {
                        if (this.NPCs[npcCount] == null)
                            break;

                        writer.Write(true);
                        writer.Write(this.NPCs[npcCount].Name);
                        writer.Write(this.NPCs[npcCount].Position.X);
                        writer.Write(this.NPCs[npcCount].Position.Y);
                        writer.Write(this.NPCs[npcCount].IsHomeless);
                        writer.Write(this.NPCs[npcCount].HomeTile.X);
                        writer.Write(this.NPCs[npcCount].HomeTile.Y);
                    }
                    writer.Write(false);
                    writer.Close();
                }
            }
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
        }
    }
}
