# Console round-trip investigation

Date: 2026-10-12. Investigated application code: `83179d9e25a821a012f093454379817a225f8cfd` on the draft bugfix branch. No serializer changes were made during this investigation.

## Conclusion

None of these round trips upgrades the world format. All three inputs and outputs contain version **279**, retain their console compression wrapper, and use the same 693-entry tile-frame table.

The failures have different causes:

| Fixture | Decompressed bytes, before → after | Finding |
| --- | --- | --- |
| `Challenge.wld` | 5,571,077 → 171,339 | Lossless tile run-length re-encoding. Every non-tile section and every expanded tile payload is preserved. |
| `console.wld` | 3,032,525 → 3,032,459 | Tile normalization consistent with the format-279 game reader, plus an actual jungle-background setting change. |
| `MAINWORLD.wld` | 7,144,533 → 7,144,533 | Tiles are byte-identical. Two header fields change: a cave-background boundary and the jungle-background style. |

The latter two are not purely encoding differences. TEdit changes meaningful world settings. The investigation found no lost chests, signs, NPCs, tile entities, pressure plates, town-room data, bestiary, creative powers, or footer data in these fixtures: those sections are byte-identical after aligning their section pointers.

## Version and platform checks

The three failing fixtures use the console wrapper signature `0x1AA2227E` and a zlib payload. Both the original and saved wrapper lengths match the decompressed payload lengths. Their version, metadata, section count, frame-table length, and frame-table bytes are preserved. Only relocated section-pointer values change in the file-format header.

`Console_FullRoundTrip_RawBytesMatch` calls `World.Save(..., incrementRevision: false)` without `versionOverride`. The writer therefore uses the loaded version, 279. It does not select TEdit's latest supported version.

The format reference was the local TerrariaServer **1.4.4.9** decompilation under `../Windows/decom1449/TerrariaServer/Terraria`. Its `Main.curRelease` is 279, and `WorldFile.SaveFileFormatHeader` writes 279. The newer 1.4.5 serializer was not used as the authority for these files.

This is a desktop/server decompilation of the matching save format, not a decompiled PlayStation/Xbox/Switch executable. The file contents do not establish an exact console build or prove hardware acceptance. Original `console.wld` and `MAINWORLD.wld` already contain the same one-byte and two-byte run-count encodings used by the saved output, so RLE is not a newly introduced encoding in this console-format sample set.

The passing `ConsoleCookie.wld` fixture is uncompressed version 315. `ConsoleWorld6.wld` is uncompressed version 319. Neither was treated as a version-279 control.

`console.wld` and `MAINWORLD.wld` retain generator metadata `0x11700000001`; `Challenge.wld` retains zero. Unknown earlier editing history does not change the observed 279 → 279 round trip.

## Exact changes

### Challenge: RLE only

The tile section shrinks from **5,560,195 to 160,457 bytes**. The original has 5,040,000 individual tile records and no RLE records. The saved output has 27,729 records representing the same 5,040,000 tile positions.

For example, the first column starts with 1,200 separate `00` empty-tile records. TEdit writes `80 AF 04`: the same empty tile plus a 16-bit repeat count of 1,199. The game reader expands the record to 1,200 tiles.

An independent wire parser compared every payload after expanding only the repeat counts. It found zero non-RLE payload differences and exact section-boundary consumption. Every other section is byte-identical. The large size reduction does not represent deleted tiles.

### console: ignored shape flags and a background change

The tile section shrinks from **3,006,921 to 3,006,855 bytes**. Across 5,040,000 tile positions, 32 have shape bits removed from the second tile header:

| Tile type | ID | Changed positions |
| --- | --- | --- |
| Vines | 52 | 4 |
| Jungle vines | 62 | 5 |
| Stalactites | 165 | 7 |
| Exposed gems | 178 | 1 |
| Palm trees | 323 | 3 |
| Water drips | 373 | 2 |
| Honey drips | 375 | 7 |
| Ash vines | 638 | 3 |

The format-279 `WorldFile.LoadWorldTiles` only applies shape bits when `Main.tileSolid[type]` or `TileID.Sets.NonSolidSaveSlopes[type]` is true. These eight types satisfy neither condition. The exceptions in `NonSolidSaveSlopes` are 131 and 351. The game reader therefore ignores these 32 shape values too.

Example: tile `(38,452)` at decompressed offset `0x4CB9` changes from `03 50 A5 5A 00 24 00` to `02 A5 5A 00 24 00`. The type and frame coordinates stay intact; only the unsupported shape and its now-unnecessary second header disappear.

Removing empty second headers saves 31 bytes. Combining 12 additional adjacent records saves a further net 35 bytes, producing the 66-byte reduction. Independent re-encoding using only these shape-normalization and RLE rules reproduces the entire saved tile section byte-for-byte. There are no other tile-field changes.

However, decompressed offset **`0x130`** contains `JungleBackStyle`, which changes from **0 to 4**. That is a separate world-header defect.

### MAINWORLD: two background fields, no tile changes

All 11,520,000 tile positions and the complete 7,072,312-byte tile section are unchanged. The only changed bytes in the decompressed file are:

| Offset | Field | Before → after |
| --- | --- | --- |
| `0x11D` | `CaveBackX0` | 3236 → 3074 |
| `0x13D` | `JungleBackStyle` | 0 → 1 |

The original cave boundaries are `[3236, 3074, 6400]`, with styles `[1, 3, 2, 4]`. TEdit clamps the first boundary to the second. The reference renderer uses the stored boundaries directly. At horizontal coordinates 3075–3236, that changes the selected cave style from 1 to 2. This is a meaningful background change, even though the original boundaries are not sorted.

## Causes in TEdit

### Jungle background and tree-top state are conflated

`World.FileV2.cs`, `LoadHeaderFlags`, reads the underground `JungleBackStyle` field, then overwrites it with `TreeTopVariations[5]` around line 2304. Saving writes this overwritten value back into the earlier background field. The save path also synchronizes `TreeTopVariations[5]` from `JungleBackStyle` around line 1225.

In the format-279 game code, `WorldFile.LoadHeader` reads `Main.jungleBackStyle` independently. `TreeTopsInfo.Load` reads tree-top variations into its own array without overwriting that setting. `CopyExistingWorldInfo` associates variation 5 with `WorldGen.jungleBG`, not `Main.jungleBackStyle`, and is only used when loading pre-211 data. These worlds are version 279.

This is not an old-format migration. The reference background renderer distinguishes `jungleBackStyle == 0` from nonzero values, so both observed 0 → nonzero changes select a different underground jungle background. Its generator chooses this background with `Next(2)`; the value 4 copied from tree-top state is not one of those generated background variants.

### Cave-boundary validation runs during deserialization

`LoadHeaderFlags` reads the original cave-boundary array, then assigns `CaveBackX2`, `CaveBackX1`, and `CaveBackX0`. The `CaveBackX0` setter in `World.Properties.cs` clamps values above `CaveBackX1`. Saving writes the clamped properties rather than the original array.

The format-279 reference reader directly loads its three cave-boundary integers. It does not perform this clamp. These TEdit paths predate the reviewed bugfix branch.

## Evidence and limits

An independent C# wire reader checked all **21,600,000 tile positions** across the three fixtures. It did not use TEdit's tile decoder or `Tile.Equals` to compare payloads. It checked column bounds, complete tile-section consumption, file metadata, frame flags, and every non-tile section. A separate Python analysis isolated every changed shape flag and independently reproduced the 66-byte tile-section reduction.

Local evidence is under `../contribution-review-evidence/console-format-279`: original/saved decompressed files, `tile-flag-differences.json`, `independent-reencoding.json`, and the investigation harness `ConsoleTileEncodingInvestigation.cs`. The detailed test log and TRX are `console-format-279-detailed.log` and `test-results/console-format-279-detailed.trx`. The strict preservation audit passes Challenge and fails the other two, as expected from the field changes described above. The harness is kept outside the normal test project; the existing three failing tests remain enabled and unchanged.

Reference locations in the local 1.4.4.9 decompilation:

- `IO/WorldFile.cs`: writer at line 1038, reader at line 1834, header background fields around lines 1515–1523, tree-top loading around line 1728.
- `GameContent/TreeTopsInfo.cs`: `Load` at line 27 and `CopyExistingWorldInfo` around line 152.
- `ID/TileID.cs`: `AllowsSaveCompressionBatching` at line 716 and `NonSolidSaveSlopes` at line 727.
- `Main.cs`: cave-background selection around line 41326 and jungle-background selection around line 41485.

Keep the PR in draft. Separate background settings from tree-top state and preserve cave boundaries during loading before dismissing these two failures. A future console test should permit proven lossless RLE changes while still asserting independent preservation of meaningful tile fields and world metadata. Console hardware testing remains necessary for end-to-end acceptance.
