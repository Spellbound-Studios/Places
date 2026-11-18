// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using Spellbound.Core.Tooling;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places {
    [CreateAssetMenu(fileName = "Place", menuName = "Spellbound/Places/Place")]
    public class PlaceDefinition : ScriptableObject, IAuthoredDefinition {
        [SerializeReference] [SerializeReferenceDropdown] private List<PlaceLayer> layers = new();
        [SerializeField] private PlaceEntrance entrance = new();
        [SerializeField] private string defaultMovementProfileName = "";
        [SerializeField] private int nestingDepthLimit = 4;

        public IReadOnlyList<PlaceLayer> Layers => layers;

        public PlaceEntrance Entrance => entrance;

        public string DefaultMovementProfileName => defaultMovementProfileName;

        public int NestingDepthLimit => nestingDepthLimit;

        public IReadOnlyList<PlaceLayer> HarvestableLayers {
            get {
                var harvestable = new List<PlaceLayer>();

                foreach (var layer in layers) {
                    if (layer != null && layer.IsHarvestable) {
                        harvestable.Add(layer);
                    }
                }

                return harvestable;
            }
        }

        public IReadOnlyList<CellRule> Rules {
            get {
                var rules = new List<CellRule>();

                foreach (var layer in layers) {
                    if (layer is not ArrangementLayer arrangement) {
                        continue;
                    }

                    foreach (var rule in arrangement.Rules) {
                        if (rule != null) {
                            rules.Add(rule);
                        }
                    }
                }

                return rules;
            }
        }

        public IReadOnlyList<PlacementPoint> PlacementPoints {
            get {
                var points = new List<PlacementPoint>();

                foreach (var layer in layers) {
                    if (layer != null) {
                        points.AddRange(layer.PlacementPoints());
                    }
                }

                return points;
            }
        }

        public TLayer AddLayer<TLayer>(TLayer layer) where TLayer : PlaceLayer {
            if (layer == null) {
                throw new ArgumentNullException(nameof(layer));
            }

            layers.Add(layer);
            return layer;
        }

        public PlaceLayer FindLayer(string layerName) {
            if (layerName == null) {
                return null;
            }

            foreach (var layer in layers) {
                if (layer != null && string.Equals(layer.LayerName, layerName, StringComparison.Ordinal)) {
                    return layer;
                }
            }

            return null;
        }

        public void Validate(ValidationReport report) {
            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            var layerNames = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < layers.Count; index++) {
                var layer = layers[index];

                if (layer == null) {
                    continue;
                }

                var location = "layers[" + index + "]";

                if (string.IsNullOrWhiteSpace(layer.LayerName)) {
                    report.Error(PlaceFindingCodes.LayerNameEmpty, location + " has no layer name", location);
                } else if (!layerNames.Add(layer.LayerName)) {
                    report.Error(PlaceFindingCodes.LayerNameDuplicate,
                        location + " repeats the layer name '" + layer.LayerName + "'", location);
                }

                layer.Validate(report, location);
            }

            if (entrance.IsSet) {
                const string entranceLocation = "entrance";

                if (FindLayer(entrance.LayerName) is not ArrangementLayer arrangement) {
                    report.Error(PlaceFindingCodes.EntranceLayerUnknown,
                        entranceLocation + " names the layer '" + entrance.LayerName
                        + "', which is not an arrangement layer of this place", entranceLocation);
                } else if (!arrangement.ContainsCell(entrance.Cell)) {
                    report.Error(PlaceFindingCodes.EntranceCellOutsideGrid,
                        entranceLocation + ".cell is " + entrance.Cell + ", outside the " + arrangement.GridSize
                        + " by " + arrangement.GridSize + " grid of " + arrangement.Strata.Count
                        + " strata of the layer '" + entrance.LayerName + "'", entranceLocation);
                }
            }

            for (var index = 0; index < layers.Count; index++) {
                if (layers[index] is not NestedPlaceLayer nested) {
                    continue;
                }

                var location = "layers[" + index + "]";

                if (!string.IsNullOrEmpty(nested.ArrangementLayerName)) {
                    var arrangementLocation = location + ".arrangementLayerName";
                    var cellLocation = location + ".cell";

                    if (FindLayer(nested.ArrangementLayerName) is not ArrangementLayer arrangement) {
                        report.Error(PlaceFindingCodes.NestingLayerUnknown,
                            arrangementLocation + " names the layer '" + nested.ArrangementLayerName
                            + "', which is not an arrangement layer of this place", arrangementLocation);
                    } else if (!arrangement.ContainsCell(nested.Cell)) {
                        report.Error(PlaceFindingCodes.NestingCellOutsideGrid,
                            cellLocation + " is " + nested.Cell + ", outside the " + arrangement.GridSize + " by "
                            + arrangement.GridSize + " grid of " + arrangement.Strata.Count + " strata of the layer '"
                            + nested.ArrangementLayerName + "'", cellLocation);
                    }
                }

                var depthReported = false;
                WalkNesting(report, location, nested.Place, new List<PlaceDefinition> { this }, ref depthReported);
            }
        }

        private void WalkNesting(ValidationReport report, string rootLocation, PlaceDefinition place,
            List<PlaceDefinition> path, ref bool depthReported) {
            if (place == null) {
                return;
            }

            if (path.Contains(place)) {
                var names = new List<string>();

                foreach (var visited in path) {
                    names.Add(visited.name);
                }

                names.Add(place.name);
                report.Error(PlaceFindingCodes.NestingCycle,
                    rootLocation + " nests a cycle of places: " + string.Join(" > ", names), rootLocation);
                return;
            }

            if (path.Count > nestingDepthLimit) {
                if (!depthReported) {
                    depthReported = true;
                    report.Error(PlaceFindingCodes.NestingDepth,
                        rootLocation + " nests places " + path.Count
                        + " deep below this place, over the nesting depth limit of " + nestingDepthLimit, rootLocation);
                }

                return;
            }

            path.Add(place);

            foreach (var layer in place.Layers) {
                if (layer is NestedPlaceLayer nested) {
                    WalkNesting(report, rootLocation, nested.Place, path, ref depthReported);
                }
            }

            path.RemoveAt(path.Count - 1);
        }
    }
}
