# Video Transcript Fix List

Source: user-provided TEdit tutorial transcript, triaged against the repository on 2026-08-12.

This is a product backlog extracted from the creator's firsthand use of TEdit while building and transforming large custom worlds. It separates reproducible data-loss and compatibility risks from usability complaints and items that are already addressed in the current codebase.

## Priority order

### P0 - Prevent world corruption or silent data loss

- [ ] **Preserve chest contents through every clipboard paste and undo/redo path.**
  - Report: pasted chests sometimes become empty, especially in very large pastes or when paste-layer options are changed; undo can also remove chest contents.
  - Current evidence: local worktree changes in `src/TEdit.Editor/Clipboard/ClipboardBuffer.cs` and `src/TEdit.Tests/Clipboard/ChestPasteUndoRegressionTests.cs` address one `PasteOverTiles = false` metadata path. Treat this item as **in progress**, not complete.
  - Implementation areas: `TEdit.Editor/Clipboard/ClipboardBuffer*.cs`, `TEdit.Editor/Undo/UndoManager.cs`, and container lookup/removal helpers on `World`.
  - Acceptance:
    - Copy/paste a chest into empty space and over another chest with each relevant paste option; name, item IDs, prefixes, stacks, and mod data survive.
    - Undo restores the exact destination chest and redo restores the pasted chest.
    - A paste containing many chests has the same result as a small paste.
    - Sign and tile-entity metadata receive equivalent coverage because they share the container update path.
    - Saving and reopening the resulting world preserves the same metadata.

- [ ] **Repair unsafe world-edge half-blocks during save and report the repair.**
  - Report: Terraria crashes when the final tile at a world edge is a half-block.
  - Current evidence: tools commonly call `World.ValidTileLocation`, but no save-time rule was found that scans edge tiles for unsafe `BrickStyle` values.
  - Preferred behavior: save-time validation converts unsafe edge half-blocks to full blocks, completes the save, and tells the user what was repaired. Apply the same rule to slopes only if Terraria compatibility testing proves they are unsafe too.
  - Implementation areas: world validation/save flow and its user-visible validation report; tool-level prevention in hammer, clipboard, and scripting paths is optional defense in depth.
  - Acceptance:
    - Immediately before serialization, validation scans every cell on all four world edges.
    - Each unsafe edge half-block is normalized to `BrickStyle.Full` in both the in-memory world and the saved file.
    - Saving continues after a successful repair and shows a clear summary with the repair count and affected edge/location details.
    - The validation report distinguishes automatic repairs from errors that still block saving.
    - Regression fixtures cover all four edges, multiple repairs in one save, a clean world with no notification, and save/reload consistency.

### P1 - Make large transforms trustworthy

- [ ] **Make horizontal and vertical clipboard flips sprite-aware.**
  - Report: tree branches face the wrong way after mirroring; stairs and minecart tracks also become invalid, and vertical flips leave many objects unusable.
  - Current evidence: `ClipboardBuffer.Flip` contains partial frame-name and anchor remapping, but comments still say multi-width/multi-height objects are ignored and rotation deletes framed sprites.
  - Tree-branch requirement:
    - Horizontal mirroring requires finding the exact mirrored sprite paired with each source UV coordinate and swapping to that paired UV.
    - Vertical mirroring cannot be implemented as a generic UV inversion because branch growth is usually directional. Define explicit per-branch transformation rules and classify branch variants that have no valid vertical counterpart.
  - Implementation areas: `TEdit.Editor/Clipboard/ClipboardBuffer.cs`, sprite/frame metadata, track framing, and platform/stair framing.
  - Acceptance:
    - Build a fixture matrix for every paired tree-branch UV plus doors, furniture, platforms/stairs, tracks, chests, signs, and tile entities.
    - Horizontal branch flips replace every source UV with its exact paired mirrored UV and round-trip back to the original after a second flip.
    - Vertical branch flips follow explicit directional-growth rules; variants without a valid counterpart are reported rather than silently rewritten.
    - Horizontal and vertical flips produce Terraria-valid frames and correct support/anchor direction.
    - A transform never mutates the source clipboard buffer or its metadata objects.
    - Unsupported sprites are preserved unchanged with a warning or explicitly omitted with a preflight summary; they are never silently corrupted.

- [ ] **Turn biome morphing into a deterministic, non-destructive conversion matrix.**
  - Report: biome results depend on the conversion path; jungle grass can become moss or disappear, and orbs/altars can become unrelated sprites or vanish.
  - Confirmed cause: Hallow, Snow, and Desert explicitly map jungle grass variants to moss `179`, while Forest maps those same variants to grass `2`. The current data also contains 79 tile deletion rules and 64 deleting sprite-UV entries.
  - Safety contract: convert only through a complete, exact source-to-target mapping. If a target or sprite UV mapping is missing, preserve the original unchanged. Normal biome morphing must never delete as a fallback.
  - Detailed audit and plan: continue `docs/todo/morph-tool.md` rather than opening a parallel implementation effort.
  - Implementation areas: `MorphBiomeDataApplier`, `morphBiomes.json` (including its `morphGroups` section), and `MorphTool`.
  - Acceptance:
    - Add source-by-target tests for grass/mud, biome walls, vines/thorns, trees, altars, orbs/hearts, torches, moss/edges, and decorations at each depth.
    - Every framed sprite style/UV has one exact valid target mapping or is preserved byte-for-byte; partial lists, default-style fallback, and deletion are rejected.
    - Converting A to B gives the same result regardless of prior conversions unless a documented lossy rule applies.
    - Multi-tile sprites are replaced from their anchor as one object; no partial or unrelated frame is emitted.
    - Unsupported conversions remain unchanged. Destructive behavior is limited to separately named, explicitly selected removal operations.

- [ ] **Add transform and paste preflight validation for large selections.**
  - Report: failures are most likely during whole-world clipboard operations, where checking every chest and sprite manually is impractical.
  - Acceptance:
    - Before commit, summarize counts of chests, signs, tile entities, unsupported framed sprites, clipped tiles, and invalid edge shapes.
    - After commit, validate that every placed container tile has exactly one matching metadata record and vice versa.
    - Validation scales to a full-world selection without freezing the UI and can be cancelled safely.

### P2 - Remove hidden state and destructive surprises

- [ ] **Make active masks unmistakable and easy to clear.**
  - Report: a tile mask continues filtering wall-only edits, which looks like the brush has stopped working.
  - Triage: cross-layer masks are intentional and documented in `docs/todo/masking-system.md`; the defect is weak feedback, not the filtering semantics.
  - Acceptance:
    - Show a persistent active-mask badge/count near the canvas and affected tool controls.
    - Provide one-click **Clear all masks** and a discoverable shortcut.
    - When a stroke changes zero cells because of masks, explain which mask rejected it.
    - Preview affected cells before applying a masked stroke or bulk action.

- [ ] **Add guardrails to flood fill, replace-all, and Cleanse World.**
  - Report: filling open space or choosing the wrong global replacement can unintentionally alter most of a world.
  - Existing plan: the Cleanse World preview/confirmation belongs in `docs/todo/morph-tool.md`.
  - Acceptance:
    - Estimate the affected tile count before a large operation and require confirmation above a configurable threshold.
    - Offer selection-only scope prominently and display the current scope in the confirmation.
    - Allow cancellation with no partial edit and commit the result as one reliable undo unit.

- [ ] **Distinguish functional placeable sprites from replicas and Rubblemaker variants.**
  - Report: search results mix functional chests, pots, altars, and evil orbs with trapped, replica, or Rubblemaker-only versions; internal names such as `Demon Heart` make expected items hard to find.
  - Implementation areas: sprite picker metadata and aliases in `SpriteView2`/shared sprite picker view models.
  - Acceptance:
    - Label results with functional category badges such as **Container**, **Trapped**, **Replica**, and **Rubblemaker**.
    - Default to functional placeables, with an explicit option to include decorative/technical variants.
    - Add search aliases for player-facing names such as Crimson Heart/Evil Orb.

- [ ] **Prevent silent loss of unsaved chest-editor changes.**
  - Report: chest edits are discarded unless the user presses a separate Save button.
  - Implementation areas: `ChestEditorView` and its view model.
  - Acceptance:
    - Prefer immediate model updates with normal document undo, or clearly show dirty state and prompt before selection/world changes.
    - Closing, changing selection, undoing, or saving the world cannot silently discard edited item, prefix, quantity, or chest name fields.

- [ ] **Clarify or remove paint "sprite mode" if it has no supported behavior.**
  - Report: an experienced user could not tell whether the mode did anything.
  - Acceptance:
    - Document its exact effect in the UI and add an observable integration test, or remove/disable the option until implemented.

### P3 - Plugin quality and resilience

- [ ] **Verify image-to-pixel-art conversion quality and lifecycle.**
  - Report: conversion quality was poor and the plugin window could not be closed.
  - Current evidence: the current window registers a close interaction and calls `Close()`, so the close defect appears addressed but lacks a targeted regression test.
  - Acceptance:
    - Reopening, closing from window chrome, closing from the command, and closing the main window leave no orphan window/event handler.
    - Golden-image tests verify palette mapping, transparency, dimensions, and schematic export on small representative images.
    - The preview explains scaling/dithering choices before export.

- [ ] **Improve crash recovery and save confidence for long editing sessions.**
  - Report: the tutorial repeatedly advises manual saving because large edits may end abruptly.
  - Acceptance:
    - Confirm autosave/recovery behavior in the UI, show the last successful save time, and surface backup health.
    - Recovery preserves container and tile-entity metadata and never overwrites the last known-good world without confirmation.

## Already addressed or not a TEdit fix

- [x] **Journey spawn-rate editing** - no fix. Spawn rate is not stored in the world file, so it is not a TEdit world-editing capability to add.
- [x] **Image converter close command** - present in `ImageToPixelartEditorView.xaml.cs`; retain the lifecycle regression task above.
- [x] **Cross-layer mask evaluation** - current behavior is intentional per `docs/todo/masking-system.md`; only feedback/reset UX remains.
- [ ] **Nonstandard world dimensions render badly on Terraria's in-game map** - documented by the speaker as an engine limitation. TEdit may warn before creating/saving unusual dimensions, but cannot claim to fix Terraria's map renderer.

## Suggested implementation sequence

1. Finish and verify clipboard container metadata plus undo/redo.
2. Add save-time edge validation and repair.
3. Build the transform fixture matrix, then fix sprite/frame remapping.
4. Complete the existing biome morph plan with deterministic conversion tests.
5. Add preflight/confirmation infrastructure shared by paste, fill, replace-all, and cleanse.
6. Address mask, sprite picker, chest editor, and plugin UX items.
