// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Places {
    [CreateAssetMenu(fileName = "Place", menuName = "Spellbound/Places/Place")]
    public class PlaceDefinition : ScriptableObject, IAuthoredDefinition {
        [SerializeReference] private List<PlaceLayer> layers = new();
        [SerializeField] private PlaceEntrance entrance = new();
        [SerializeField] private string defaultMovementProfileName = "";
        [SerializeField] private int nestingDepthLimit = 4;

        public IReadOnlyList<PlaceLayer> Layers => layers;

        public PlaceEntrance Entrance => entrance;

        public string DefaultMovementProfileName => defaultMovementProfileName;

        public int NestingDepthLimit => nestingDepthLimit;

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
        }
    }
}
