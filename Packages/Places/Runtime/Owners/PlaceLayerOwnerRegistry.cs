// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Operations;

namespace Spellbound.Places.Owners {
    public sealed class PlaceLayerOwnerRegistry {
        private static PlaceLayerOwnerRegistry _default;

        private readonly Dictionary<Type, PlaceLayerOwnerBinding> _bindings = new();
        private readonly List<Type> _layerTypes = new();

        public PlaceLayerOwnerRegistry(IEnumerable<object> owners) {
            if (owners == null) {
                throw new ArgumentNullException(nameof(owners));
            }

            foreach (var owner in owners) {
                Add(Bind(owner));
            }
        }

        private PlaceLayerOwnerRegistry() {
        }

        public static PlaceLayerOwnerRegistry Default => _default ??= Scan();

        public IReadOnlyList<Type> LayerTypes => _layerTypes;

        public static void Reset() {
            _default = null;
        }

        public bool Has(Type layerType) {
            return layerType != null && _bindings.ContainsKey(layerType);
        }

        public bool TryStamp(PlaceLayer layer, PlaceStampContext context) {
            if (layer == null) {
                throw new ArgumentNullException(nameof(layer));
            }

            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            if (!_bindings.TryGetValue(layer.GetType(), out var binding)) {
                return false;
            }

            binding.Stamp(layer, context);
            return true;
        }

        public PlaceLayer Capture(Type layerType, PlaceCaptureContext context) {
            if (layerType == null) {
                throw new ArgumentNullException(nameof(layerType));
            }

            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            return _bindings.TryGetValue(layerType, out var binding) ? binding.Capture(context) : null;
        }

        private static PlaceLayerOwnerRegistry Scan() {
            var registry = new PlaceLayerOwnerRegistry();

            foreach (var ownerType in MarkedAssemblyScan.TypesWith<PlaceLayerOwnerAttribute>()) {
                try {
                    registry.Add(Bind(Activator.CreateInstance(ownerType)));
                }
                catch (Exception exception) {
                    Log.Error("The layer owner " + ownerType.FullName + " was not registered: " + exception.Message);
                }
            }

            return registry;
        }

        private static PlaceLayerOwnerBinding Bind(object owner) {
            if (owner == null) {
                throw new ArgumentException("The owners hold a null owner", "owners");
            }

            var ownerType = owner.GetType();
            var layerTypes = new List<Type>();

            foreach (var implemented in ownerType.GetInterfaces()) {
                if (implemented.IsGenericType && implemented.GetGenericTypeDefinition() == typeof(IPlaceLayerOwner<>)) {
                    layerTypes.Add(implemented.GetGenericArguments()[0]);
                }
            }

            if (layerTypes.Count != 1) {
                throw new ArgumentException("The layer owner " + ownerType.FullName + " owns " + layerTypes.Count
                    + " layer types through IPlaceLayerOwner; an owner owns exactly one", "owners");
            }

            var bindingType = typeof(TypedPlaceLayerOwnerBinding<>).MakeGenericType(layerTypes[0]);
            return (PlaceLayerOwnerBinding)Activator.CreateInstance(bindingType, owner);
        }

        private void Add(PlaceLayerOwnerBinding binding) {
            if (_bindings.ContainsKey(binding.LayerType)) {
                throw new ArgumentException("The layer type " + binding.LayerType.FullName
                    + " is claimed by a second owner; one owner owns a layer type", "owners");
            }

            _bindings.Add(binding.LayerType, binding);
            _layerTypes.Add(binding.LayerType);
        }
    }
}
