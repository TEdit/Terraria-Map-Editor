# Contribution bugfix review

Review date: 2026-10-12. Baseline: `origin/main` at `cba4d21fc2a5b20909950c5fc06aec27e5da54f4`.

This branch manually implements selected fixes from 50 pull requests and 51 issues submitted by `rodneypantonial`. It does not merge the contributor's branches. Thirteen PRs support the selected changes. The other 37 implementations are excluded. The lasso and furniture feature proposals remain future work, including the dependent terrain-conversion fix.

The branch is for testing. Manual regression, game save/load, and modded-world acceptance remain incomplete. Closing a source issue or PR records this review disposition; it does not establish that an excluded defect was fixed or that an included fix passed manual acceptance.

## Review scope and security

The review used issue bodies, PR metadata, and file patches downloaded through read-only GitHub API requests. All 50 patch inventories matched their reported file counts and addition/deletion counts. Reviewed head SHAs were checked again before publication.

Contributor branches, build scripts, executables, workflows, and dependencies were not run. No submitted package or workflow change is included. Static inspection found no obvious code-execution or exfiltration behavior in the retained changes. This is not a security guarantee. Excluded implementations were rejected when a concrete scope or correctness problem was sufficient; they did not receive exhaustive security clearance.

Raw review evidence and test logs are stored locally outside the repository in `../contribution-review-evidence`. The temporary baseline comparison script was removed at the maintainer's request. Decompiled game sources are local references and are not redistributed here.

## Selected changes

| Source PR | Supporting issue | Applied scope and evidence |
| --- | --- | --- |
| #2408 | #2404 | Isolate the existing shared-state test collection and initialize core test configuration. No additional parallel-test dependency. |
| #2409 | #2405 | Use the existing item-ID dictionary. Tests cover all configured IDs, uniqueness, and preservation of unknown IDs. |
| #2437 | #2436 | Explicitly skip the JSON-reformat maintenance operation in ordinary test runs. This operation rewrites repository data. |
| #2439 | #2438 | Inject an optional random source into GenerateApi and seed its tests. Runtime callers retain normal random behavior. |
| #2440 | #2414 | Implement player equipment favorite flags from version 322 and the reserved flag from version 324. Reference-based wire-layout tests cover 317, 321, 322, 323, 324, and 326. No automatic legacy-player upgrade. |
| #2450 | #2427 | Give empty display-doll and hat-rack slots independent item instances. Tests cover creation, a literal hat-rack wire record, mod-item isolation, and copying. |
| #2453 | #2430 | Continue after unsupported NPCs during downgrade instead of ending the list. Tests compare supported-only output with a list containing an unsupported middle entry. Preserve-all remains supported. |
| #2456 | #2434 | Populate dye choices using dye item IDs, independently of translated names. Exact-membership tests cover English and German data. |
| #2480 | #2462 | Adapt the reported sign-count defect to Terraria's behavior: omit null-text signs from both count and records. Preserve empty strings. Test section boundaries and the legacy valid-record limit. |
| #2483 | #2465 | Format JSON geometry and Leaflet coordinates using invariant culture, preserving existing precision. A non-ASCII negative-sign test reproduces the JSON defect. |
| #2484 | #2466 | Check tile and wall paint indices before lookup. Unknown paint renders without paint while retaining its stored ID. Tests cover the exact upper boundary and ID 255. |
| #2492 | #2475 | Reject an unreadable schematic before constructing its preview. Existing caller error handling displays the failure. |
| #2493 | #2476 | Lock sprite-list enumeration in four lookup paths through FindSprite. This does not claim to make every mutable sprite object thread-safe. |

An additional test-only dispatcher initialization is needed by the ReactiveUI dependency on the updated baseline. Without it, the WPF test module fails before tests execute. It adds no selection-tool behavior.

## Reference validation

The local Terraria 1.4.5.8 server decompilation supplied these format references:

- `Terraria/IO/WorldFile.cs`, `SaveSigns`: exclude null-text signs from the count and records.
- `Terraria/Player.cs`, `Serialize` and `Deserialize`: armor/dye favorite fields and the reserved boolean version gates.
- `Terraria/EquipmentLoadout.cs` and `Terraria/Item.cs`: loadout item serialization, including version-dependent favorite fields.
- `Terraria/GameContent/Tile_Entities/TEHatRack.cs`: item-slot initialization and occupied-slot bitmask layout.

The local tModLoader 2026.06.3.6 source supplied additional checks against `TileIO`, `WorldIO`, and `ItemIO`. It supports concerns about truncated mod data and preserving mod inventory, but does not validate the submitted recovery or save-transaction implementations. Those implementations are excluded.

## Excluded implementations

These are implementation decisions, not assertions that every reported defect is invalid.

| Source PR | Issue | Reason and disposition |
| --- | --- | --- |
| #2400 | #2399 | Keep open for future lasso work. Exclude this implementation: freeform masks are not persisted in schematics, so reloaded copies become rectangles and can paste empty tiles into excluded areas. |
| #2402 | #2401 | Keep open for future furniture conversion. Exclude inherited selection changes and conversion based on English display names without adequate mod/style mapping. The issue body also duplicates the lasso specification. |
| #2410 | #2406 | A fixture produced by the same writer cannot independently prove game-format correctness. Use separate reference-based layout assertions for the retained player fix. |
| #2411 | #2403 | Do not weaken WinPhone fixture coverage. The local reference fixtures are available and the tests execute. |
| #2412 | #2407 | Exclude the additional parallel-test framework package and increased shared-state concurrency. |
| #2441 | #2413 | Exclude CI changes bundled with skipping console regressions. Preserve the failing tests. |
| #2442 | #2435 | Coverage configuration is outside this focused bugfix branch. |
| #2443 | #2416 | Opening prompts do not cover every world-replacement path. Reentrancy and save/cancel outcomes need dedicated lifecycle tests. |
| #2444 | #2417 | Broad file replacement changes lack failure-injection evidence for disk-full, permissions, concurrent saves, and world/sidecar pairing. |
| #2445 | #2418 | The recovery flag is cleared before successful save. Failed saves can lose the warning, and recovered-world overwrite behavior remains insufficiently tested. |
| #2446 | #2419 | Capturing an undo index does not preserve property edits made during a save or solve the mutable-world snapshot race. |
| #2447 | #2420 | Unbounded per-operation coordinate tracking changes undo memory behavior. Flush/clear/remove behavior needs an isolated design and tests. |
| #2448 | #2421 | Second-resolution session paths plus per-manager counters still permit cross-process collisions. Recursive cleanup and pending writes need lifecycle tests. |
| #2449 | #2423 | Excluded pending a clearly scoped world-transition fix. CurrentWorld resets dirty flags after crop/expand transforms, and FinalizeLoad is called only by crop/expand. Ordinary file loads must stay clean. The need is real, but this branch leaves dirty-state behavior unchanged. |
| #2451 | #2428 | Strict truncation detection is consistent with tModLoader, but the patch does not prove safe end-to-end recovery and overwrite prevention after a partial load. |
| #2452 | #2429 | Marking arbitrary dispatcher exceptions handled continues execution after potentially incomplete mutations. No reliable recovery invariant is demonstrated. |
| #2454 | #2431 | Path-hash naming needs backup migration, restore, retention, and collision tests. Unnamed worlds still share a name. |
| #2455 | #2432 | Uses autosave dirty state rather than user-save dirty state. After autosave, unsaved user work can still be replaced. Script references also need lifecycle handling. |
| #2457 | #2422 | Silently upgrades old players and mutates their version instead of preserving an explicit compatibility choice. |
| #2458 | #2424 | Builds on incomplete prompt handling without demonstrated restart/save/cancel behavior. |
| #2459 | #2425 | Restores shared LastSave state across an await without operation/world ownership. Concurrent save behavior remains unsafe. |
| #2460 | #2426 | Reversing world/sidecar replacement order still permits a mismatched pair when the final world replacement fails. No rollback. |
| #2461 | #2433 | An ad hoc script flag misses editing paths and lacks reliable finally cleanup. Interrupting mouse interactions also needs lifecycle coverage. |
| #2481 | #2463 | A potentially valid sync-save concern is bundled with broad save consolidation and replacement-order changes. Needs an isolated fix and real modded fixtures. |
| #2482 | #2464 | Discards unknown bytes without establishing compatibility, inherits recovery problems, and does not consistently use the requested output version. |
| #2485 | #2467 | The arbitrary tile limit still permits large allocations. Bounds are not consistently scoped to the containing section, and tests do not establish full-loader safety. |
| #2486 | #2468 | Script restrictions and recursion limits need compatibility and adversarial tests. The patch does not establish equivalent recursion protection for both engines. |
| #2487 | #2469 | Temporarily changes CurrentFile before the awaited operation and relies on shared success state. Operation-scoped results are still missing. |
| #2488 | #2470 | Existing read-only portable data paths are still selected. Repeated corruption handling can overwrite the preserved backup. |
| #2489 | #2471 | FrameSize[0] does not establish correct dimensions for every style/mod object. Source-buffer object aliasing remains a concern. |
| #2490 | #2472 | Recovery writes to the same failed stream can fail again and silently discard undo history. No convincing failure-path tests. |
| #2491 | #2474 | Incomplete legacy chest/sign guards and inconsistent version coverage. A downgrade policy change needs complete boundary tests. |
| #2494 | #2477 | Cancellation after Save As can leave the selected filename changed. Save outcomes still need explicit propagation. |
| #2495 | #2478 | Includes the excluded undo-deduplication change. A focused in-flight flush race test is needed instead of eventual-completion assertions. |
| #2496 | #2479 | Full sprites can extend outside the requested generation region, and support checks do not establish valid solid support. Tile-count tests miss these defects. |
| #2497 | #2473 | Deferred rotation UX acts on mutable clipboard state after an awaited confirmation. Reentrancy and object-preservation behavior need a separate design. |
| #2499 | #2498 | Keep open with furniture conversion. Fixes behavior in an unaccepted feature and inherits the excluded selection implementation. |

Issue #2415 has no corresponding PR. Console byte-identity tests remain enabled; semantic reload success alone does not establish console-game compatibility. Its unresolved concern is preserved in the validation results and manual acceptance checklist.

## Automated validation

Results on the reviewed branch, against the baseline recorded above:

| Check | Result |
| --- | --- |
| Core suite | 623 passed, 11 skipped, zero failed. |
| WPF suite after dispatcher bootstrap fix | 625 passed, three failed, zero skipped. |
| Baseline console comparison | The same three failing console cases reproduce with unchanged origin/main production code. A fourth case passes. The test-only dispatcher bootstrap remains enabled for this comparison. |
| Release application build | Succeeded with zero errors and 16 warnings. |

The three failures are `Console_FullRoundTrip_RawBytesMatch` for `console.wld`, `Challenge.wld`, and `MAINWORLD.wld`. They remain enabled. The encoding audit, detailed investigation, and background-preservation fix work are tracked separately in [PR #2502](https://github.com/TEdit/Terraria-Map-Editor/pull/2502). Console-game compatibility remains unproven.

The 11 core skips comprise six maintenance/data-generation operations and five Calamity cases. They are not counted as passes. In particular, the missing modded-world coverage remains a manual acceptance requirement.

The initial WPF attempt failed all 628 cases during module initialization because ReactiveUI could not find a dispatcher. The bootstrap correction allowed the suite to execute. No production dependency or selection-tool changes were needed.

## Manual acceptance gates

Use copies of worlds and retain untouched original `.wld`/`.twld` pairs. Automated byte-layout checks do not replace loading files in Terraria or tModLoader.

- [ ] Open an unchanged world and confirm it remains clean. Confirm existing crop/expand dirty-state limitations are understood; this branch does not fix them.
- [ ] Save and reopen representative legacy and current vanilla worlds in TEdit and Terraria. Check signs, NPCs, chests, tile entities, and world settings.
- [ ] Test NPC downgrade with an unsupported NPC between supported NPCs, both with and without preserve-all behavior.
- [ ] Load and save representative player versions, including 322, 324, and 326, in the game. Check equipment, dye, favorite flags, and every loadout. Retain legacy-version behavior.
- [ ] Test modded world/sidecar pairs in tModLoader. Edit separate mannequin/hat-rack slots, including mod items and dyes, then save/reload, copy/paste, and undo/redo.
- [ ] Confirm mod chests, unrelated mod data, and world metadata remain intact. Missing Calamity fixtures are not covered by a passing suite.
- [ ] Check unknown paint IDs render without crashing and remain stored unchanged.
- [ ] Check dye choices and exports under non-English locales, plus ordinary schematic import, picker, statue, and banner previews.
- [ ] Resolve or explicitly accept the existing console byte-identity discrepancy before claiming console-game compatibility.

Manual acceptance must remain unchecked until performed.
