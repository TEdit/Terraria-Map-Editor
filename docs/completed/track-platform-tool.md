# Track & Platform Tool Refactor

Related issues: #2068

## Overview

The track/platform tool places tiles with incorrect or default frame data, making them unusable in-game.

## Problems

1. **Railway tracks**: placed tracks are completely unusable in-game; continuing to place them can corrupt the save
2. **Platforms/stairs**: only place in initial frame state — no way to place downward-sloping stairs or alternate orientations
3. **One-way walls/gates**: placed in initial form, non-functional until hammered in-game

## Requirements

- Track tool must write correct frame values for track connections (straight, curves, junctions, endpoints)
- Platform placement should support frame variants: flat, left slope, right slope (matching hammer states)
- One-way gate placement should write the correct active frame
- Auto-connect: adjacent tracks should update their frame data to form valid connections (similar to how the game places them)
- UI should expose orientation/variant options where applicable
