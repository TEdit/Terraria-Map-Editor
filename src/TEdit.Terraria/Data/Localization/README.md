# Terraria localization resources

The files in this folder are embedded in `TEdit.Terraria` and loaded by
`LocalizationLoader`. There are three independent layers.

| File | Owner | Purpose |
| --- | --- | --- |
| `{locale}.json` | `LocalizationImportTests.GenerateLocalizationFiles` | Official mirror of the game's localization files. Generated only; never edit by hand. Empty values are intentional source-name fallbacks. |
| `LocalizationOverrides.{locale}.json` | Wiki reviewers / maintainers | Optional hand-verified corrections and fills. Missing files are normal. |
| `supplementalNames.{locale}.json` | Crowdin | Optional names that the game resources cannot express: frame style/variety labels, frame anchors, wall natural/unused markers and biome-variant labels. |

## Load order

1. Load the official `{locale}.json` resource.
2. Merge `LocalizationOverrides.{locale}.json` (blank keys and values are ignored).
3. Merge `supplementalNames.{locale}.json`.
4. Resolve `{$Category.Key}` references against the merged data.
5. Restore the official value when an override resolves to an empty label, so an
   invalid or untranslated correction can never hide a valid official name.
6. Apply the merged names to the display models.

## Overrides format

Keys are the stable `key`/`constName` values from `items.json`, `tiles.json`,
`walls.json`, `npcs.json` and `prefixes.json`, grouped by section. Frame labels
use the parent tile key plus the complete original frame label
(`frame.ToString()`, including `Name`, `Variety` and `Anchor`):

```json
{
  "items": { "ManaCloakStar": "魔力斗篷星星" },
  "tiles": { "PlanteraBulb": "世纪之花球茎" },
  "walls": { "CrimsonGrassUnsafe": "猩红草墙（天然）" },
  "frames": {
    "Bottles": { "Bottle": "玻璃瓶" },
    "Containers": { "Gold Chest": "金箱" }
  }
}
```

Only add a frame entry when generic item/tile/supplemental rules cannot produce
the correct complete label. Run `TEdit.Terraria.Tests` after editing; the
integrity tests validate categories, keys, blank values, duplicate JSON
properties and frame mappings for every locale.
