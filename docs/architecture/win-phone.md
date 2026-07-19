# Windows Phone world format

Status: Windows Phone v49 and v60 containers are detected and decoded through their character-name tables. The native header, metadata primitives, tiles, chests, signs, NPCs, and names are reconstructed by a section-aware serializer. All three fixtures save byte-for-byte through `World.Save`.

The implementation is in `src/TEdit.Terraria/World.FileWinPhone.cs`. Reverse-engineering notes for the local Ghidra setup are in `ghidra-winphone-quick-ref.md`.

## Verified fixtures

| Fixture | Version | Size | Result |
|---|---:|---:|---|
| `_reference_sources/winphone/Terraria v1.0.0.0/Tutorial.world` | 49 | 1,538,009 | Full tile load and byte-identical save |
| `_reference_sources/winphone/Terraria v1.2.4.3/Tutorial.world` | 49 | 1,538,009 | Full tile load and byte-identical save; identical to the v1.0 copy |
| `src/TEdit.Tests/WorldFiles/win-phone.world` | 60 | 4,210,977 | Full tile load and byte-identical save |

The two tutorial files have SHA-256 `2E06CDBDBB29604831AA149A94CDBA8A5C06A20C9DF6D0CE46F0BFEDBC2061F9`.

## Native container header

All integers are little-endian.

| Field | Encoding |
|---|---|
| Version | `uint32` |
| CRC-32 | `uint32`; reflected polynomial `0xEDB88320`, calculated over bytes from offset 8 through EOF |
| Title length | `int32` character/byte count |
| Title | UTF-8 for versions below 50; UTF-16LE for version 50 and newer |
| Cloud flag | One byte for version 53 and newer |
| History | Version 53 has one `uint32`; later versions have `int32 count` followed by that many `uint32` values |

The v49 tutorial CRC field is zero. The v60 fixture contains and passes the native CRC calculation. The value previously described as a fixed magic number is therefore a file-specific CRC, not a format signature.

The body begins with these verified fields:

| Relative offset | Field | Encoding |
|---:|---|---|
| `+0` | World ID | `int32` |
| `+4` | Revision/legacy value | `int32` where present |
| `+8` | Right pixel bound | `int32` |
| `+12` | Bottom pixel bound | `int16` |
| `+14` | Tile height | `int16` |
| `+16` | Tile width | `int16` |
| `+18` | Native horizontal bound/state | `int16` |
| `+20` | Native horizontal bound/state | `int16` |
| `+22` | Surface level | `int16` |
| `+24` | Rock level | `int16` |

Detection validates `right == width * 16` and `bottom == height * 16`. This prevents ordinary desktop V1 files in the same version range from being claimed as Windows Phone files.

The remaining pre-tile metadata has an exact versioned primitive layout:

| Block | v49 | v60 |
|---|---:|---:|
| Common bounds and layers above | 26 bytes | 26 bytes |
| World-state helper (`FUN_0073527c`/`FUN_00735318`) | 9 bytes | 11 bytes |
| Spawn X/Y | 4 bytes | 4 bytes |
| Extra v58 state plus weather (`FUN_00749890`/`FUN_00749d20`) | — | 14 bytes |
| Progression flags (`FUN_006598b4`/`FUN_00659a38`) | 9 bytes | 19 bytes |
| Moon/time/invasion/ore fields | 16 bytes | 24 bytes |
| Two biome-state blocks, packed styles, eight background bytes | — | 29 bytes |
| Total body prefix before the first marker | 64 bytes | 127 bytes |

Fields without a confirmed gameplay name are retained as typed `byte`, `int16`, or `int32` primitives rather than assigned speculative names. The serializer writes the primitive sequence, not copied source bytes.

The native body is divided by the little-endian `int32` marker `0x162E`. Its complete sequence is:

1. Metadata, marker.
2. Column-major tile grid, marker.
3. 1,000 sparse chest slots, marker.
4. 1,000 sparse sign slots, marker.
5. Sentinel-terminated NPC records, marker.
6. Character-name strings: 10 for v49 and 18 for v60.

## Two tile encodings

The v1.0 executable does not use one version-conditional tile reader. Its body loader dispatches versions below 58 to a legacy reader and versions 58 or newer to a compact reader.

### Versions below 58

Recovered from `FUN_00733bac` in the v1.0.0.0 executable. Each record describes one tile followed by zero or more vertical duplicates.

| Condition | Field | Encoding |
|---|---|---|
| Always | Active | `byte`, zero or nonzero |
| Active | Stored tile type | `byte` |
| Active and legacy-framed | Frame U, frame V | Two `int16` values |
| Always | Wall type | `byte` |
| Version > 50 | Wall color | `byte` |
| Always | Liquid amount | `byte` |
| Liquid amount != 0 | Lava flag | `byte`, zero for water and nonzero for lava |
| Always | Tile flags | `byte` |
| Always | Vertical repeat count | `int16` |

The repeat count excludes the first tile, so a value of zero represents one tile. Runs cannot cross a column boundary.

The legacy frame predicate is recovered exactly from `FUN_0072ad80`. It includes IDs 3–5; 10–18 except 19; 20, 21, 24, 26–29, 31, 33–36, 42, 50, 55, 61, 71–74, 77–79, 81–106, 110, 113, 114, 125–129, 132–139, 141–144, 149, and 150.

The native migration maps stored types 35 and 36 to type 34 while shifting their frame V by 54 and 108 respectively. Stored type 150 maps to the internal placeholder type 500. Type 127 is decoded and then deactivated.

### Versions 58 and newer

Recovered from `FUN_00733858`. The v60 fixture uses this encoding.

| Condition | Field | Encoding |
|---|---|---|
| Always | Header 1 | `byte`: active, red/green/blue wire, actuator, inactive flags |
| Active | Tile type | `uint16`; the native tile field is nine bits wide |
| Active and frame-important | Frame U, frame V | Two `int16` values |
| Active | Tile color | `byte` |
| Always | Wall type | `byte` |
| Wall != 0 | Wall color | `byte` |
| Always | Liquid amount | `byte` |
| Liquid amount != 0 | Liquid kind | `byte`; native value plus one maps to TEdit's enum |
| Always | Header 2 | `byte`; includes yellow wire |
| Versions 60–68 | Shape | `byte`; high nibble is the brick style |
| Always | Vertical repeat count | One or two bytes, 7-bit continuation encoding |

Type 500 is a valid framed internal placeholder in the v60 stream, not corrupt input.

The original RLE boundaries are encoding metadata. The v49 tutorial sometimes ends a run before a following semantically identical tile, so maximizing every run changes six bytes even though the decoded grid is unchanged. TEdit retains and validates those native boundaries.

## Chests, signs, NPCs, and names

Each chest slot begins with a presence byte and, when present, `int16 X`, `int16 Y`.

- v49 has 20 items per chest. Every slot stores a one-byte stack; nonempty items then store `int16 net ID` and `byte prefix`.
- v60 has 40 items per chest. A one-byte mask length and five mask bytes select populated slots; each selected item stores `int16 stack`, `int16 net ID`, and `byte prefix`.

Each sign slot begins with a presence byte and, when present, `int16 X`, `int16 Y`. v60 then has a text-present byte. Text uses the container string encoding: `int32 length` plus UTF-8 below version 50 or UTF-16LE at version 50 and newer.

NPC records begin with byte `1`, then contain `byte sprite ID`, two `float32` positions, a homeless byte, and two `int16` home coordinates. A zero presence byte terminates the list.

The character-name IDs are `17, 18, 19, 20, 22, 54, 38, 107, 108, 124, 160, 178, 207, 208, 209, 227, 228, 229`. v49 stores the first 10; v60 stores all 18.

The v49 `Tutorial.world` is a fat bundled resource. The native v49 reader stops at offset 201,842 after its tenth name and ignores the remaining 1,336,167 bytes. TEdit reconstructs the complete parsed world and preserves this ignored bundle suffix verbatim. The v60 file ends at its eighteenth name.

## Save behavior

`World.SaveWinPhoneUnchanged` remains an explicit raw-copy utility. The normal `World.Save` and `World.SaveAsync` paths require a full load and verify that the parsed state is unchanged. They then reconstruct the header, metadata, tiles, section markers, chests, signs, NPCs, and character names. They do not emit the original parsed bytes.

Chests, signs, NPCs, and character names are materialized into TEdit's models. Edited native saves remain disabled until native flag/RLE regeneration and CRC updates are exposed as a supported edit path. A detected edit throws `NotSupportedException`; it cannot be silently discarded.

## Recovered native functions

Addresses below are from the v1.0.0.0 Ghidra project.

| Address | Purpose |
|---|---|
| `00768ed0` | Read the container version/header |
| `0076a604` | Dispatch body loader at version 58 |
| `00767f5c` | Legacy body reader |
| `00768798` | New body reader |
| `00733bac` | Legacy tile reader used by v49 |
| `00733858` | Compact tile reader used by v58+ |
| `0072ad80` | Legacy framed-tile predicate |
| `00733e90` | Current compact tile writer |
| `0076758c` | World body writer and section markers |
| `00605a60` / `00605b9c` | Read/write one chest inventory |
| `006082a4` / `00606768` | Read/write 1,000 chest slots |
| `00726440` / `007260ec` | Read/write one sign slot |
| `00735318` / `0073527c` | Read/write the compact world-state helper |
| `00749d20` / `00749890` | Read/write weather state |
| `00659a38` / `006598b4` | Read/write progression flags |

## Tests

`WinPhoneWorldTests` verifies header dispatch, complete tile allocation, decoded section counts, native validation, edit rejection, and byte-for-byte `World.Save` output for all three fixtures.

The strongest serializer test loads each file, overwrites every cached source byte from offset zero through the end of the parsed world with `0xA5`, and reconstructs it. Output still matches the original byte-for-byte. For v60 this means no original file bytes participate in reconstruction; for v49 only the native-reader-ignored bundle suffix is preserved.
