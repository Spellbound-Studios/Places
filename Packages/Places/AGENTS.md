# Spellbound.Places

## Purpose

A place is an authored unit of content: a `PlaceDefinition` asset holding typed layers in Unity serialization, each layer stamped by the package that owns it. Places composes an arrangement layer from parts by adjacency rules at a seed, validates and previews a place through Core's authoring seam, nests places and duplicates them with every reference updated. A layer whose owner is not installed is carried as it is, kept through every save and reported, never stamped.

## Front door

`PlacesKit`: `Compose(arrangement, seed)` composes an arrangement description at a seed and returns the grid with its contradictions; `Stamp(place, context)` stamps every layer of a place through its installed owner, all or nothing. `PlaceSections`: `TryPack(place, report, out section)` packs a place as the `places.place` section, each layer packing itself through Core's `IPacker` with its layout fingerprint, and `TryUnpack(bytes, into, report)` rebuilds the layers into a place, refusing an empty slot, a layer that does not pack itself, a layer type that is not installed and a layer packed with another layout. The `places` operations below read a place from the command line and write nothing.

<!-- spellbound:operations 10839e05 -->
com.spellboundstudios.places
Operations of the group `places`, run as `<group> <operation> [arguments]`:
- `places compose --target <place-definition> --seed <integer>`: Composes every arrangement layer of a place at a seed and prints each stratum's grid, the contradictions count and one line per contradiction with its reason; the asset's own seed is left as it is. [reads, developer]
  - `--target`: The place, as asset:<path> or asset:<name>.
  - `--seed`: The seed to compose with.
- `places parts --target <place-definition> [--offset <integer> = 0] [--limit <integer> = 0]`: Lists the parts of every arrangement layer of a place with their six faces: east, west, up, down, north and south. [reads, developer]
  - `--target`: The place, as asset:<path> or asset:<name>.
  - `--offset`: How many parts to skip from the start of the list. Default: 0.
  - `--limit`: How many parts to list at most; zero lists every one. Default: 0.
<!-- /spellbound:operations -->

<!-- spellbound:samples 933ff23a -->
Samples in `Samples~`, imported from the Package Manager's Samples tab:
- Places Sample: Shows the package's front door in use. (`Samples~/PlacesSample`)
<!-- /spellbound:samples -->

<!-- spellbound:depends 64c552e8 -->
Depends on:
- `com.spellboundstudios.core` in [1.1.9,3.0.0)
<!-- /spellbound:depends -->

<!-- spellbound:pipeline 433e4b11 -->
Agents run these operation lines from the Unity command line when `com.unity.pipeline` is installed in [0.0.0,1.0.0): find the command with `unity command --query spellbound` and pass it one line. The ranges Core's Pipeline bridge accepts are in Core's AGENTS.md.
<!-- /spellbound:pipeline -->

## Rules

The binding rules are the line text at https://github.com/Spellbound-Studios/.github/tree/main/line

## Not for

- A game's own parts, pools and rules: a game authors them as data in its own assets.
- Rendering: Places draws nothing in a scene and holds no material.
- Stamping an arrangement into terrain or into the world: that belongs to the package that owns the arrangement layer.
- Any layer's data format or editor: the package that owns a layer type owns both.

## Gaps

- `ArrangementLayer` has no owner type yet, so `PlacesKit.Stamp` refuses any place that holds an arrangement layer until a package registers an owner for it.
- Pool weights are floats drawn through Places' own pick, not through Core's integer `WeightedTable`.
- A carried layer cannot be mapped to its slot from Unity's own data, because `managedReferenceId` reads -2 for a slot whose type is missing; the check maps carried layers to slots by their order.
- `ArrangementLayer.GridSize` has no setter, so a grid's size is set only in the inspector or in the asset.
- `places compose` and the Places window compose every arrangement layer of a place at one seed; a place with two arrangement layers cannot compose each at its own seed from either.

## Ledger

| Id | Ours | Ours (type) | Unity item | Verdict | Judged at | Reason | Evidence | Watch packages | Watch words |
|---|---|---|---|---|---|---|---|---|---|
| U-001 | Carried layers | `Spellbound.Places.PlaceLayer` | SerializeReference, managed references of a missing type | Builds on | 6000.4 | Unity keeps the type and data blocks of a managed reference whose type is missing when the asset is saved again, and reads the slot back as its type once the assembly returns, so Places carries a layer of an absent package without a format of its own. SerializationUtility.ClearManagedReferenceWithMissingType followed by a save drops the block. | CarriedLayerSurvivalTests.cs, ACarriedLayerSurvivesASaveOfTheAsset and ClearingTheCarriedLayerThenSavingDropsItsBlock | | SerializeReference; missing type; ClearManagedReferenceWithMissingType |
| U-002 | Place duplication | `Spellbound.Places.Editor.PlaceDuplicator` | AssetDatabase.CopyAsset | Builds on | 6000.4 | AssetDatabase.CopyAsset copies the asset's file, so a carried layer's type and data blocks stay in the copy, where Object.Instantiate and EditorUtility.CopySerialized drop them. | PlaceDuplicatorTests.cs, ACarriedLayerIsKeptInTheCopyAndListedWithItsPackage | | CopyAsset; Instantiate; CopySerialized |
| U-003 | Place inspector guard | `Spellbound.Places.Editor.PlaceDefinitionEditor` | SerializationUtility.HasManagedReferencesWithMissingTypes | Builds on | 6000.4 | The call is true for a carried layer and for a carried rule nested in a present layer, so the inspector greys the whole layer list either way and an edit cannot drop carried data. | PlaceDefinitionEditorTests.cs, TheLayerListIsDisabledOnACarriedAsset | | HasManagedReferencesWithMissingTypes; managedReferenceId |
