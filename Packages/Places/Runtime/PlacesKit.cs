// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Places.Arrangement;
using Spellbound.Places.Owners;

namespace Spellbound.Places {
    public static class PlacesKit {
        public static CompositionResult Compose(ArrangementDescription arrangement, ulong seed) {
            return ArrangementComposer.Compose(arrangement, seed);
        }

        public static bool Stamp(PlaceDefinition place, PlaceStampContext context) {
            if (place == null) {
                throw new ArgumentNullException(nameof(place));
            }

            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            var layers = place.Layers;
            var owners = context.Owners;
            var refused = false;

            for (var index = 0; index < layers.Count; index++) {
                var layer = layers[index];
                var location = "layers[" + index + "]";

                if (layer == null) {
                    context.Report.Error(PlaceFindingCodes.LayerNotInstalled,
                        "holds no readable layer at runtime (its type is not installed or the slot is empty); "
                        + "nothing is stamped", location);
                    refused = true;
                } else if (!owners.Has(layer.GetType())) {
                    context.Report.Error(PlaceFindingCodes.LayerNotInstalled,
                        "layer '" + layer.LayerName + "' of type " + layer.GetType().Name
                        + " has no installed owner; nothing is stamped", location);
                    refused = true;
                }
            }

            if (refused) {
                return false;
            }

            foreach (var layer in layers) {
                owners.TryStamp(layer, context);
            }

            return !context.Report.HasErrors;
        }
    }
}
