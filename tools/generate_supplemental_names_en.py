"""Generate the English source for supplemental sprite/style names.

The official localization files mirror the game's own localization resources.
They cannot express frame style names, frame anchors, wall natural/unused
markers or biome-variant labels, so those live in a separate Crowdin-managed
dictionary: supplementalNames.{locale}.json.

Run from the repository root:
    python tools/generate_supplemental_names_en.py
"""

import json
import os
import re

repo_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
tiles_path = os.path.join(repo_root, 'src', 'TEdit.Terraria', 'Data', 'tiles.json')
loc_path = os.path.join(repo_root, 'src', 'TEdit.Terraria', 'Data', 'Localization', 'en-US.json')
output_path = os.path.join(
    repo_root, 'src', 'TEdit.Terraria', 'Data', 'Localization', 'supplementalNames.en-US.json')

with open(tiles_path, 'r', encoding='utf-8') as f:
    tiles = json.load(f)

# Names already resolved by the official game data (item/tile keys and display
# values) are looked up through data.Items / data.Tiles before supplementalNames
# is ever consulted. Only non-empty official values count as covered; keys with
# an empty placeholder still need a supplemental source entry.
covered = set()
if os.path.exists(loc_path):
    with open(loc_path, 'r', encoding='utf-8') as f:
        loc = json.load(f)
    for section in ('items', 'tiles'):
        data = loc.get(section) or {}
        covered.update(str(k) for k, v in data.items() if isinstance(v, str) and v.strip())
        covered.update(str(v) for v in data.values() if isinstance(v, str) and v.strip())

numeric_only = re.compile(r'^\d+$')

keys = set()
for tile in tiles:
    tile_key = tile.get('key')
    if isinstance(tile_key, str) and tile_key.strip():
        keys.add(tile_key.strip())

    # tile.name is deliberately not collected: the runtime looks up tile.Key,
    # frame.Name and frame.Variety, never the tile display name, so those entries
    # would be dead weight in the Crowdin source.
    for frame in tile.get('frames', []):
        fn = frame.get('name')
        if isinstance(fn, str) and fn.strip() and fn.strip() != 'None':
            keys.add(fn.strip())
        fv = frame.get('variety')
        if isinstance(fv, str) and fv.strip() and fv.strip() != 'None':
            keys.add(fv.strip())

    # Biome-variant labels shown in the sprite picker dropdown (e.g. "Forest", "Beach Palm").
    for variant in tile.get('biomeVariants', []):
        vn = variant.get('name')
        if isinstance(vn, str) and vn.strip() and vn.strip() != 'None':
            keys.add(vn.strip())

# Keep only residual style names that need localization: exclude names already
# covered by non-empty official item/tile translations and pure numeric tokens.
result = {k: k for k in sorted(keys) if k not in covered and not numeric_only.match(k)}

# Frame anchor suffixes shown as "[Left]" etc.
result['Anchor.Bottom'] = 'Bottom'
result['Anchor.Center'] = 'Center'
result['Anchor.Left'] = 'Left'
result['Anchor.None'] = 'None'
result['Anchor.Right'] = 'Right'
result['Anchor.Top'] = 'Top'

# Suffix markers appended to natural/unused wall variants (see ApplyLocalization).
result['Wall.Natural'] = '(natural)'
result['Wall.Unused'] = '(unused)'

sorted_result = dict(sorted(result.items()))

with open(output_path, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(sorted_result, f, ensure_ascii=False, indent=2)
    f.write('\n')

print(f"Generated {len(sorted_result)} entries in {output_path}")
