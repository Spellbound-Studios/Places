// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;

namespace Spellbound.Places {
    public static class PlaceSections {
        public static bool TryPack(PlaceDefinition place, ValidationReport report, out PlaceSection section) {
            if (place == null) {
                throw new ArgumentNullException(nameof(place));
            }

            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            section = null;
            var refused = false;

            for (var index = 0; index < place.Layers.Count; index++) {
                var layer = place.Layers[index];
                var location = "layers[" + index + "]";

                if (layer == null) {
                    report.Error(PlaceFindingCodes.LayerNotInstalled,
                        location + " holds a layer whose type is not installed, so the place cannot be packed",
                        location);
                    refused = true;
                    continue;
                }

                var layerType = layer.GetType();

                if (!PlaceLayerLayouts.IsPackable(layerType)) {
                    report.Error(PlaceFindingCodes.LayerNotPackable,
                        location + " '" + layer.LayerName + "' is a " + layerType.FullName
                        + ", which does not pack itself: a packable layer implements IPacker and has a public"
                        + " parameterless constructor", location);
                    refused = true;
                }
            }

            if (refused) {
                return false;
            }

            section = new PlaceSection(place.name, place.Layers);
            return true;
        }

        public static bool TryUnpack(ReadOnlySpan<byte> bytes, PlaceDefinition into, ValidationReport report) {
            if (into == null) {
                throw new ArgumentNullException(nameof(into));
            }

            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            var section = new PlaceSection();

            if (!section.TryRead(ref bytes, report)) {
                return false;
            }

            into.name = section.PlaceName;

            foreach (var layer in section.Layers) {
                into.AddLayer(layer);
            }

            return true;
        }
    }
}
