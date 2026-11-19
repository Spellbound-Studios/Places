// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.IO;
using Spellbound.Core;
using Spellbound.Core.Hashing;
using Spellbound.Core.Packing;

namespace Spellbound.Places {
    internal static class PlaceLayerLayouts {
        private static readonly ConcurrentDictionary<Type, uint> Fingerprints = new();

        public static uint Fingerprint(Type layerType) => Fingerprints.GetOrAdd(layerType, ReadFingerprint);

        public static bool IsPackable(Type layerType) =>
            typeof(IPacker).IsAssignableFrom(layerType)
            && !layerType.IsAbstract
            && layerType.GetConstructor(Type.EmptyTypes) != null;

        public static Type FindLayerType(string assemblyQualifiedName) {
            Type layerType;

            try {
                layerType = Type.GetType(assemblyQualifiedName, false);
            } catch (Exception exception) when (exception is ArgumentException or FileLoadException
                                                    or BadImageFormatException) {
                return null;
            }

            return layerType != null && typeof(PlaceLayer).IsAssignableFrom(layerType) ? layerType : null;
        }

        private static uint ReadFingerprint(Type layerType) {
            var description = CoreKit.Packing.Describe(layerType);
            var firstLineEnd = description.IndexOf('\n');
            return StableHash.Fnv1A32(firstLineEnd < 0 ? string.Empty : description.Substring(firstLineEnd + 1));
        }
    }
}
