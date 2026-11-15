# Spellbound.Places

Places describes a place as a Unity asset made of typed layers and composes its layout from adjacency rules. A `PlaceDefinition` holds its layers by reference, so a package can add a layer type of its own and an asset keeps a layer even while the package that owns it is not installed. The arrangement layer that ships here lays parts out on a square grid of stacked strata, matching the socket on each face of a part against its neighbours, and the same seed always gives the same layout. Validation and preview run through Core's authoring contract, so `core validate` and `core preview` read a place like any other authored definition.

## Assemblies

- `Spellbound.Places`, in `Runtime`: `PlacesKit`, the front door, with `PlaceDefinition`, `PlaceLayer`, the finding codes in `PlaceFindingCodes`, and the arrangement types under `Spellbound.Places.Arrangement`. It references Core.
- `Spellbound.Places.Editor`, in `Editor`: editor only. `LayerOwnerInstalledCheck` reports a layer whose owning package is missing, and `PlaceDefinitionPreviewer` lists every layer and draws the composed grid of each arrangement layer, one stratum at a time.
- `Spellbound.Places.Bootstrap`, in `Bootstrap`: editor only. While Core is absent it offers to install it, and it compiles nothing once Core is present.
- `Spellbound.Places.Editor.Tests` and `Spellbound.Places.Tests`, in `Tests`: the edit-mode and play-mode tests, with the fixture assembly `Spellbound.Places.Tests.CarriedFixture` beside them.

## A place and its layers

Create a place from **Create > Spellbound > Places > Place**. Each entry in its layer list is a `PlaceLayer` with a name that is unique within the place. A layer type of your own derives from `PlaceLayer`, is marked `[Serializable]`, and reports its own problems in `Validate(report, location)`.

When the assembly that defines a layer type is removed, Unity keeps that layer's data in the asset and the slot reads as empty. Saving the asset does not lose the data, and reinstalling the package brings the layer back. The editor check reports such a slot as `places.layer-carried`, with the class, the assembly and the package it came from, and reports a slot that holds nothing at all as the error `places.layer-empty`.

## Composing an arrangement

An `ArrangementLayer` holds the parts, the strata, the centre pool and the cell rules:

- A part has a name and a `SocketKind` on each of its six faces: `NoConnection`, `Connection` or `SealedConnection`. Two faces fit when their kinds share a flag, and a part may be rotated in quarter turns about the vertical axis.
- Each stratum has a weighted pool of parts for its ordinary cells and a list of one-of parts, each placed once somewhere in that stratum.
- The centre pool picks the part for the centre of the grid. With an empty centre pool the centre is left void in every stratum.
- `FixedCellRule` pins a part to a cell, with a rotation or -1 for any. `WeightedPoolRule` gives one cell its own pool. `AllowedRegionRule` keeps a part inside a region of cells.

`PlacesKit.Compose(arrangement, seed)` composes a layout from an `ArrangementDescription`, which `ArrangementLayer.ToDescription()` builds from the asset:

```csharp
var result = PlacesKit.Compose(layer.ToDescription(), layer.Seed);
Debug.Log(result.ToText());
```

The composer places the centre, the fixed cells and the one-of parts first, then fills the ordinary cells, always choosing the cell with the fewest parts left that fit and narrowing its neighbours after each choice. Pool weights are relative and must be zero or more. The composer never throws on a layout it cannot complete: a cell that nothing fits stays empty, and the reason is recorded as a `CellContradiction` with its `CellAddress`. A `CompositionResult` gives every cell through `Cell(address)` with its state, part name and rotation, the contradictions, and the count of ordinary cells attempted and left empty. `ToText()` writes the grid one stratum at a time, a part as `name@rotation`, an empty cell as `.` and a void centre as `#`.

## Validation

`PlaceDefinition.Validate` and `ArrangementLayer.Validate` report these findings, each at the field it concerns:

- `places.layer-name-empty` and `places.layer-name-duplicate` for layer names.
- `places.grid-size-even` when the grid size is not an odd number of at least 1.
- `places.part-name-empty` and `places.part-name-duplicate` for part names.
- `places.pool-part-unknown` and `places.rule-part-unknown` when a pool or a rule names a part the layer does not have.
- `places.rule-cell-outside-grid` when a rule addresses a cell outside the grid or its strata.
- `places.weight-negative` for a pool weight below zero.

## Samples

- Places Sample: `PlacesKit` in use through the public surface alone. Import it from the Package Manager.
