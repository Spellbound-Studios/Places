// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEditor;

namespace Spellbound.Places.Editor {
    public sealed class PlaceLayerInspectorActionRegistry {
        private static PlaceLayerInspectorActionRegistry _default;

        private readonly List<IPlaceLayerInspectorActions> _providers = new();

        public PlaceLayerInspectorActionRegistry(IEnumerable<IPlaceLayerInspectorActions> providers) {
            if (providers == null) {
                throw new ArgumentNullException(nameof(providers));
            }

            foreach (var provider in providers) {
                if (provider == null) {
                    throw new ArgumentException("The providers hold a null provider", nameof(providers));
                }

                _providers.Add(provider);
            }
        }

        public static PlaceLayerInspectorActionRegistry Default => _default ??= Scan();

        public static void Reset() {
            _default = null;
        }

        public IReadOnlyList<PlaceLayerInspectorAction> ActionsFor(PlaceDefinition place, int layerIndex) {
            if (place == null) {
                throw new ArgumentNullException(nameof(place));
            }

            if (layerIndex < 0 || layerIndex >= place.Layers.Count) {
                throw new ArgumentOutOfRangeException(nameof(layerIndex), layerIndex,
                    "The place '" + place.name + "' has " + place.Layers.Count + " layer slots");
            }

            var actions = new List<PlaceLayerInspectorAction>();
            var layer = place.Layers[layerIndex];

            if (layer == null) {
                return actions;
            }

            var layerType = layer.GetType();

            foreach (var provider in _providers) {
                if (provider.LayerType != layerType) {
                    continue;
                }

                var provided = provider.Actions(place, layerIndex);

                if (provided == null) {
                    continue;
                }

                foreach (var action in provided) {
                    if (action != null) {
                        actions.Add(action);
                    }
                }
            }

            return actions;
        }

        private static PlaceLayerInspectorActionRegistry Scan() {
            var providers = new List<IPlaceLayerInspectorActions>();

            foreach (var providerType in TypeCache.GetTypesWithAttribute<PlaceLayerInspectorActionAttribute>()) {
                if (!typeof(IPlaceLayerInspectorActions).IsAssignableFrom(providerType)) {
                    Log.Error("The inspector actions " + providerType.FullName
                        + " were not registered: the type does not implement " + nameof(IPlaceLayerInspectorActions));
                    continue;
                }

                if (providerType.IsAbstract || providerType.GetConstructor(Type.EmptyTypes) == null) {
                    Log.Error("The inspector actions " + providerType.FullName
                        + " were not registered: the type has no public parameterless constructor");
                    continue;
                }

                try {
                    providers.Add((IPlaceLayerInspectorActions)Activator.CreateInstance(providerType));
                }
                catch (Exception exception) {
                    Log.Error("The inspector actions " + providerType.FullName + " were not registered: "
                        + exception.Message);
                }
            }

            return new PlaceLayerInspectorActionRegistry(providers);
        }
    }
}
