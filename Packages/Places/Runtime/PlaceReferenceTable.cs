// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places {
    public sealed class PlaceReferenceTable {
        private const string EntranceLocation = "entrance";

        private readonly List<string> _layerNames;
        private readonly List<PlaceLayerReference> _layerReferences;

        private PlaceReferenceTable(List<string> layerNames, List<PlaceLayerReference> layerReferences) {
            _layerNames = layerNames;
            _layerReferences = layerReferences;
        }

        public IReadOnlyList<string> LayerNames => _layerNames;

        public IReadOnlyList<PlaceLayerReference> LayerReferences => _layerReferences;

        public static PlaceReferenceTable Of(PlaceDefinition place) {
            if (place == null) {
                throw new ArgumentNullException(nameof(place));
            }

            var layerNames = new List<string>();
            var layerReferences = new List<PlaceLayerReference>();

            for (var index = 0; index < place.Layers.Count; index++) {
                var layer = place.Layers[index];

                if (layer == null) {
                    continue;
                }

                layerNames.Add(layer.LayerName);
                layer.CollectLayerReferences(layerReferences, "layers[" + index + "]");
            }

            if (place.Entrance != null && place.Entrance.IsSet) {
                layerReferences.Add(new PlaceLayerReference(EntranceLocation, place.Entrance.LayerName));
            }

            return new PlaceReferenceTable(layerNames, layerReferences);
        }

        public IReadOnlyList<PlaceLayerReference> ReferencesTo(string layerName) {
            var references = new List<PlaceLayerReference>();

            foreach (var reference in _layerReferences) {
                if (string.Equals(reference.TargetLayerName, layerName, StringComparison.Ordinal)) {
                    references.Add(reference);
                }
            }

            return references;
        }

        public static void RenameLayer(PlaceDefinition place, string previousName, string newName) {
            if (place == null) {
                throw new ArgumentNullException(nameof(place));
            }

            if (string.IsNullOrWhiteSpace(previousName)) {
                throw new ArgumentException("A layer rename needs the previous layer name", nameof(previousName));
            }

            if (string.IsNullOrWhiteSpace(newName)) {
                throw new ArgumentException("A layer rename needs a new layer name", nameof(newName));
            }

            if (place.FindLayer(previousName) == null) {
                throw new ArgumentException("The place '" + place.name + "' holds no layer named '" + previousName
                                            + "'", nameof(previousName));
            }

            foreach (var layer in place.Layers) {
                if (layer != null && string.Equals(layer.LayerName, previousName, StringComparison.Ordinal)) {
                    layer.LayerName = newName;
                }
            }

            foreach (var layer in place.Layers) {
                layer?.RenameLayerReferences(previousName, newName);
            }

            if (place.Entrance != null
                && string.Equals(place.Entrance.LayerName, previousName, StringComparison.Ordinal)) {
                place.Entrance.LayerName = newName;
            }
        }
    }
}
