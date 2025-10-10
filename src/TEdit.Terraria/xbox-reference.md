XBox Reference Implmentation

```csharp
tileFrameImportant[139] = true;

		tileFrameImportant[149] = true;

		tileFrameImportant[142] = true;
		tileFrameImportant[143] = true;
		tileFrameImportant[144] = true;

		tileFrameImportant[136] = true;
		tileFrameImportant[137] = true;
		tileFrameImportant[138] = true;
tileFrameImportant[3] = true;
		tileFrameImportant[4] = true;
		tileFrameImportant[5] = true;
		tileFrameImportant[10] = true;
		tileFrameImportant[11] = true;
		tileFrameImportant[12] = true;
		tileFrameImportant[13] = true;
		tileFrameImportant[14] = true;
		tileFrameImportant[15] = true;
		tileFrameImportant[16] = true;
		tileFrameImportant[17] = true;
		tileFrameImportant[18] = true;
		tileFrameImportant[20] = true;
		tileFrameImportant[21] = true;
		tileFrameImportant[24] = true;
		tileFrameImportant[26] = true;
		tileFrameImportant[27] = true;
		tileFrameImportant[28] = true;
		tileFrameImportant[29] = true;
		tileFrameImportant[31] = true;
		tileFrameImportant[33] = true;
		tileFrameImportant[34] = true;
		tileFrameImportant[35] = true;
		tileFrameImportant[36] = true;
		tileFrameImportant[42] = true;
		tileFrameImportant[50] = true;
		tileFrameImportant[55] = true;
		tileFrameImportant[61] = true;
		tileFrameImportant[71] = true;
		tileFrameImportant[72] = true;
		tileFrameImportant[73] = true;
		tileFrameImportant[74] = true;
		tileFrameImportant[77] = true;
		tileFrameImportant[78] = true;
		tileFrameImportant[79] = true;
		tileFrameImportant[81] = true;
		tileFrameImportant[82] = true;
		tileFrameImportant[83] = true;
		tileFrameImportant[84] = true;
		tileFrameImportant[85] = true;
		tileFrameImportant[86] = true;
		tileFrameImportant[87] = true;
		tileFrameImportant[88] = true;
		tileFrameImportant[89] = true;
		tileFrameImportant[90] = true;
		tileFrameImportant[91] = true;
		tileFrameImportant[92] = true;
		tileFrameImportant[93] = true;
		tileFrameImportant[94] = true;
		tileFrameImportant[95] = true;
		tileFrameImportant[96] = true;
		tileFrameImportant[97] = true;
		tileFrameImportant[98] = true;
		tileFrameImportant[99] = true;
		tileFrameImportant[101] = true;
		tileFrameImportant[102] = true;
		tileFrameImportant[103] = true;
		tileFrameImportant[104] = true;
		tileFrameImportant[105] = true;
		tileFrameImportant[100] = true;
		tileFrameImportant[106] = true;
		tileFrameImportant[110] = true;
		tileFrameImportant[113] = true;
		tileFrameImportant[114] = true;
		tileFrameImportant[125] = true;
		tileFrameImportant[126] = true;
		tileFrameImportant[128] = true;
		tileFrameImportant[129] = true;
		tileFrameImportant[132] = true;
		tileFrameImportant[133] = true;
		tileFrameImportant[134] = true;
		tileFrameImportant[135] = true;
		tileFrameImportant[141] = true;



private unsafe static void loadNewWorld(BinaryReader fileIO, int release, MemoryStream stream)
	{
		CRC32 cRC = new CRC32();
		cRC.Update(stream.GetBuffer(), 8, (int)stream.Length - 8);
		if (cRC.GetValue() != fileIO.ReadUInt32())
		{
			throw new InvalidOperationException("Invalid CRC32");
		}
		fileIO.ReadString();
		int worldID = fileIO.ReadInt32();
		int worldTimestamp = ((release >= 48) ? fileIO.ReadInt32() : 0);
		Main.rightWorld = fileIO.ReadInt32();
		Main.bottomWorld = fileIO.ReadInt16();
		Main.maxTilesY = fileIO.ReadInt16();
		Main.maxTilesX = fileIO.ReadInt16();
		clearWorld();
		Main.worldID = worldID;
		Main.worldTimestamp = worldTimestamp;
		UI.main.FirstProgressStep(4, Lang.gen[51]);
		Main.spawnTileX = fileIO.ReadInt16();
		Main.spawnTileY = fileIO.ReadInt16();
		Main.worldSurface = fileIO.ReadInt16();
		Main.worldSurfacePixels = Main.worldSurface << 4;
		Main.rockLayer = fileIO.ReadInt16();
		Main.rockLayerPixels = Main.rockLayer << 4;
		UpdateMagmaLayerPos();
		tempTime.dayRate = 1f;
		tempTime.time = fileIO.ReadSingle();
		tempTime.dayTime = fileIO.ReadBoolean();
		tempTime.moonPhase = fileIO.ReadByte();
		tempTime.bloodMoon = fileIO.ReadBoolean();
		Main.dungeonX = fileIO.ReadInt16();
		Main.dungeonY = fileIO.ReadInt16();
		NPC.downedBoss1 = fileIO.ReadBoolean();
		NPC.downedBoss2 = fileIO.ReadBoolean();
		NPC.downedBoss3 = fileIO.ReadBoolean();
		NPC.savedGoblin = fileIO.ReadBoolean();
		NPC.savedWizard = fileIO.ReadBoolean();
		NPC.savedMech = fileIO.ReadBoolean();
		NPC.downedGoblins = fileIO.ReadBoolean();
		NPC.downedClown = fileIO.ReadBoolean();
		NPC.downedFrost = fileIO.ReadBoolean();
		shadowOrbSmashed = fileIO.ReadBoolean();
		spawnMeteor = fileIO.ReadBoolean();
		shadowOrbCount = fileIO.ReadByte();
		altarCount = fileIO.ReadInt32();
		Main.hardMode = fileIO.ReadBoolean();
		Main.invasionDelay = fileIO.ReadByte();
		Main.invasionSize = fileIO.ReadInt16();
		Main.invasionType = fileIO.ReadByte();
		Main.invasionX = fileIO.ReadSingle();
		for (int i = 0; i < Main.maxTilesX; i++)
		{
			if ((i & 0x1F) == 0)
			{
				UI.main.progress = (float)i / (float)Main.maxTilesX;
			}
			fixed (Tile* ptr = &Main.tile[i, 0])
			{
				Tile* ptr2 = ptr;
				int num = 0;
				while (num < Main.maxTilesY)
				{
					ptr2->flags = ~(Tile.Flags.WALLFRAME_MASK | Tile.Flags.HIGHLIGHT_MASK | Tile.Flags.VISITED | Tile.Flags.WIRE | Tile.Flags.CHECKING_LIQUID | Tile.Flags.SKIP_LIQUID);
					ptr2->active = fileIO.ReadByte();
					if (ptr2->active != 0)
					{
						ptr2->type = fileIO.ReadByte();
						if (ptr2->type == 127)
						{
							ptr2->active = 0;
						}
						if (Main.tileFrameImportant[ptr2->type])
						{
							ptr2->frameX = fileIO.ReadInt16();
							ptr2->frameY = fileIO.ReadInt16();
							if (ptr2->type == 144)
							{
								ptr2->frameY = 0;
							}
						}
						else
						{
							ptr2->frameX = -1;
							ptr2->frameY = -1;
						}
					}
					ptr2->wall = fileIO.ReadByte();
					ptr2->liquid = fileIO.ReadByte();
					if (ptr2->liquid > 0 && fileIO.ReadBoolean())
					{
						ptr2->lava = 32;
					}
					ptr2->flags |= (Tile.Flags)fileIO.ReadByte();
					if (Main.IsTutorial())
					{
						ptr2->flags &= ~Tile.Flags.VISITED;
					}
					int num2 = fileIO.ReadByte();
					if (((uint)num2 & 0x80u) != 0)
					{
						num2 &= 0x7F;
						num2 |= fileIO.ReadByte() << 7;
					}
					num += num2;
					while (num2 > 0)
					{
						ptr2[1] = *ptr2;
						ptr2++;
						num2--;
					}
					num++;
					ptr2++;
				}
			}
		}
		for (int j = 0; j < 1000; j++)
		{
			if (!fileIO.ReadBoolean())
			{
				continue;
			}
			Main.chest[j] = new Chest();
			Main.chest[j].x = fileIO.ReadInt16();
			Main.chest[j].y = fileIO.ReadInt16();
			for (int k = 0; k < 20; k++)
			{
				byte b = fileIO.ReadByte();
				if (b > 0)
				{
					Main.chest[j].item[k].netDefaults(fileIO.ReadInt16(), b);
					Main.chest[j].item[k].Prefix(fileIO.ReadByte());
				}
			}
		}
		for (int l = 0; l < 1000; l++)
		{
			Main.sign[l].Read(fileIO, release);
		}
		bool flag = fileIO.ReadBoolean();
		int num3 = 0;
		while (flag)
		{
			int type = fileIO.ReadByte();
			Main.npc[num3].SetDefaults(type);
			Main.npc[num3].position.X = fileIO.ReadSingle();
			Main.npc[num3].position.Y = fileIO.ReadSingle();
			Main.npc[num3].aabb.X = (int)Main.npc[num3].position.X;
			Main.npc[num3].aabb.Y = (int)Main.npc[num3].position.Y;
			Main.npc[num3].homeless = fileIO.ReadBoolean();
			Main.npc[num3].homeTileX = fileIO.ReadInt16();
			Main.npc[num3].homeTileY = fileIO.ReadInt16();
			num3++;
			flag = fileIO.ReadBoolean();
		}
		NPC.chrName[17] = fileIO.ReadString();
		NPC.chrName[18] = fileIO.ReadString();
		NPC.chrName[19] = fileIO.ReadString();
		NPC.chrName[20] = fileIO.ReadString();
		NPC.chrName[22] = fileIO.ReadString();
		NPC.chrName[54] = fileIO.ReadString();
		NPC.chrName[38] = fileIO.ReadString();
		NPC.chrName[107] = fileIO.ReadString();
		NPC.chrName[108] = fileIO.ReadString();
		NPC.chrName[124] = fileIO.ReadString();
	}

	public static void saveNewWorld()
	{
		if (saveLock)
		{
			return;
		}
		saveLock = true;
		if (hardLock)
		{
			UI.main.statusText = Lang.gen[48];
			do
			{
				Thread.Sleep(16);
			}
			while (hardLock);
		}
		bool flag = false;
		lock (padlock)
		{
			UI.main.FirstProgressStep(1, Lang.gen[49]);
			using (MemoryStream memoryStream = new MemoryStream(6291456))
			{
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(49);
				binaryWriter.Write(0u);
				binaryWriter.Write(Main.worldName);
				binaryWriter.Write(Main.worldID);
				binaryWriter.Write(Main.worldTimestamp);
				binaryWriter.Write(Main.rightWorld);
				binaryWriter.Write((short)Main.bottomWorld);
				binaryWriter.Write(Main.maxTilesY);
				binaryWriter.Write(Main.maxTilesX);
				binaryWriter.Write(Main.spawnTileX);
				binaryWriter.Write(Main.spawnTileY);
				binaryWriter.Write((short)Main.worldSurface);
				binaryWriter.Write((short)Main.rockLayer);
				binaryWriter.Write(tempTime.time);
				binaryWriter.Write(tempTime.dayTime);
				binaryWriter.Write(tempTime.moonPhase);
				binaryWriter.Write(tempTime.bloodMoon);
				binaryWriter.Write(Main.dungeonX);
				binaryWriter.Write(Main.dungeonY);
				binaryWriter.Write(NPC.downedBoss1);
				binaryWriter.Write(NPC.downedBoss2);
				binaryWriter.Write(NPC.downedBoss3);
				binaryWriter.Write(NPC.savedGoblin);
				binaryWriter.Write(NPC.savedWizard);
				binaryWriter.Write(NPC.savedMech);
				binaryWriter.Write(NPC.downedGoblins);
				binaryWriter.Write(NPC.downedClown);
				binaryWriter.Write(NPC.downedFrost);
				binaryWriter.Write(shadowOrbSmashed);
				binaryWriter.Write(spawnMeteor);
				binaryWriter.Write((byte)shadowOrbCount);
				binaryWriter.Write(altarCount);
				binaryWriter.Write(Main.hardMode);
				binaryWriter.Write((byte)Main.invasionDelay);
				binaryWriter.Write((short)Main.invasionSize);
				binaryWriter.Write((byte)Main.invasionType);
				binaryWriter.Write(Main.invasionX);
				for (int i = 0; i < Main.maxTilesX; i++)
				{
					if ((i & 0x1F) == 0)
					{
						UI.main.progress = (float)i / (float)Main.maxTilesX;
					}
					int num;
					for (num = 0; num < Main.maxTilesY; num++)
					{
						Tile tile = Main.tile[i, num];
						if (tile.type == 127)
						{
							tile.active = 0;
						}
						if (tile.active != 0)
						{
							binaryWriter.Write(value: true);
							binaryWriter.Write(tile.type);
							if (Main.tileFrameImportant[tile.type])
							{
								binaryWriter.Write(tile.frameX);
								binaryWriter.Write(tile.frameY);
							}
						}
						else
						{
							binaryWriter.Write(value: false);
						}
						binaryWriter.Write(tile.wall);
						binaryWriter.Write(tile.liquid);
						if (tile.liquid > 0)
						{
							binaryWriter.Write(tile.lava != 0);
						}
						binaryWriter.Write((byte)(tile.flags & (Tile.Flags.VISITED | Tile.Flags.WIRE)));
						int j;
						for (j = 1; num + j < Main.maxTilesY && tile.isTheSameAs(ref Main.tile[i, num + j]); j++)
						{
						}
						j--;
						num += j;
						if (j < 128)
						{
							binaryWriter.Write((byte)j);
						}
						else
						{
							int num2 = (j & 0x7F) | 0x80;
							j >>= 7;
							binaryWriter.Write((ushort)(num2 | (j << 8)));
						}
					}
				}
				for (int k = 0; k < 1000; k++)
				{
					if (Main.chest[k] == null)
					{
						binaryWriter.Write(value: false);
						continue;
					}
					Chest chest = Main.chest[k];
					binaryWriter.Write(value: true);
					binaryWriter.Write(chest.x);
					binaryWriter.Write(chest.y);
					for (int l = 0; l < 20; l++)
					{
						if (chest.item[l].type == 0)
						{
							chest.item[l].stack = 0;
						}
						binaryWriter.Write((byte)chest.item[l].stack);
						if (chest.item[l].stack > 0)
						{
							binaryWriter.Write(chest.item[l].netID);
							binaryWriter.Write(chest.item[l].prefix);
						}
					}
				}
				for (int m = 0; m < 1000; m++)
				{
					Sign sign = Main.sign[m];
					sign.Write(binaryWriter);
				}
				for (int n = 0; n < 196; n++)
				{
					NPC nPC = Main.npc[n];
					if (nPC.townNPC && nPC.active != 0)
					{
						nPC = (NPC)nPC.Clone();
						binaryWriter.Write(value: true);
						binaryWriter.Write(nPC.type);
						binaryWriter.Write(nPC.position.X);
						binaryWriter.Write(nPC.position.Y);
						binaryWriter.Write(nPC.homeless);
						binaryWriter.Write(nPC.homeTileX);
						binaryWriter.Write(nPC.homeTileY);
					}
				}
				binaryWriter.Write(value: false);
				binaryWriter.Write(NPC.chrName[17]);
				binaryWriter.Write(NPC.chrName[18]);
				binaryWriter.Write(NPC.chrName[19]);
				binaryWriter.Write(NPC.chrName[20]);
				binaryWriter.Write(NPC.chrName[22]);
				binaryWriter.Write(NPC.chrName[54]);
				binaryWriter.Write(NPC.chrName[38]);
				binaryWriter.Write(NPC.chrName[107]);
				binaryWriter.Write(NPC.chrName[108]);
				binaryWriter.Write(NPC.chrName[124]);
				CRC32 cRC = new CRC32();
				cRC.Update(memoryStream.GetBuffer(), 8, (int)memoryStream.Length - 8);
				binaryWriter.Seek(4, SeekOrigin.Begin);
				binaryWriter.Write(cRC.GetValue());
				Main.ShowSaveIcon();
				try
				{
					if (UI.main.TestStorageSpace("Worlds", WorldSelect.worldPathName, (int)memoryStream.Length))
					{
						using StorageContainer storageContainer = UI.main.OpenPlayerStorage("Worlds");
						using Stream stream = storageContainer.OpenFile(WorldSelect.worldPathName, FileMode.Create);
						stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
						stream.Close();
						flag = true;
					}
				}
				catch (IOException)
				{
					UI.main.WriteError();
				}
				catch (Exception)
				{
				}
				binaryWriter.Close();
				Main.HideSaveIcon();
			}
			saveLock = false;
			if (!flag)
			{
				WorldSelect.LoadWorlds();
			}
		}
	}
    ```
