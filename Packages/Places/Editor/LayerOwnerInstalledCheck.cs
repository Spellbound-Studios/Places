// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Spellbound.Core.Authoring;
using Spellbound.Core.Authoring.Editor;
using UnityEditor;

namespace Spellbound.Places.Editor {
    [RegisteredDefinitionCheck("places.layer-owner-installed", RunsWhileEditing = true)]
    public sealed class LayerOwnerInstalledCheck : IDefinitionCheck<PlaceDefinition> {
        private const string LayersField = "layers";

        public void Check(PlaceDefinition definition, ValidationReport report) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            var reading = ReadSlots(definition);

            for (var index = 0; index < reading.CarriedBySlot.Count; index++) {
                var location = LayersField + "[" + index + "]";

                if (reading.CarriedBySlot[index] != null) {
                    report.Information(PlaceFindingCodes.LayerCarried,
                        CarriedMessage(reading.CarriedBySlot[index].Value), location);
                    continue;
                }

                if (definition.Layers[index] != null) {
                    continue;
                }

                if (reading.SharedMissingTypes.Count > 0) {
                    report.Information(PlaceFindingCodes.LayerCarried, SharedMessage(location, reading), location);
                } else {
                    report.Error(PlaceFindingCodes.LayerEmpty,
                        location + " holds no layer; assign a layer or remove the slot", location);
                }
            }
        }

        public IReadOnlyList<string> PreviewLines(PlaceDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            var reading = ReadSlots(definition);
            var count = reading.SharedMissingTypes.Count;

            foreach (var missingType in reading.CarriedBySlot) {
                if (missingType != null) {
                    count++;
                }
            }

            return new[] { count + " layers carried" };
        }

        private static SlotReading ReadSlots(PlaceDefinition definition) {
            var missingTypes = new List<ManagedReferenceMissingType>(
                SerializationUtility.GetManagedReferencesWithMissingTypes(definition));
            var carried = new List<ManagedReferenceMissingType?>();
            var unmatchedSlots = new List<int>();
            using var serialized = new SerializedObject(definition);
            var layers = serialized.FindProperty(LayersField);

            for (var index = 0; index < layers.arraySize; index++) {
                var element = layers.GetArrayElementAtIndex(index);

                if (element.managedReferenceValue != null) {
                    carried.Add(null);
                    continue;
                }

                var matched = missingTypes.FindIndex(missingType =>
                    missingType.referenceId == element.managedReferenceId);

                if (matched < 0) {
                    carried.Add(null);
                    unmatchedSlots.Add(index);
                    continue;
                }

                carried.Add(missingTypes[matched]);
                missingTypes.RemoveAt(matched);
            }

            if (missingTypes.Count < unmatchedSlots.Count) {
                return new SlotReading(carried, unmatchedSlots.Count, missingTypes);
            }

            foreach (var slot in unmatchedSlots) {
                carried[slot] = missingTypes[0];
                missingTypes.RemoveAt(0);
            }

            return new SlotReading(carried, unmatchedSlots.Count, new List<ManagedReferenceMissingType>());
        }

        private static string SharedMessage(string location, SlotReading reading) {
            var storedTypes = reading.SharedMissingTypes.Select(missingType =>
                missingType.className + " from assembly " + missingType.assemblyName);
            return location + " is one of " + reading.UnmatchedSlotCount + " slots without a layer, of which "
                   + reading.SharedMissingTypes.Count + " carry a stored type (" + string.Join(", ", storedTypes)
                   + "); the slots cannot be told apart, so none is reported empty and every slot's data is kept";
        }

        private static string CarriedMessage(ManagedReferenceMissingType missingType) {
            var package = LayerAssemblyPackage.Of(missingType.assemblyName) ?? "none";
            return "stored type " + missingType.className + " from assembly " + missingType.assemblyName
                   + " (package " + package + ") is not installed; the layer is carried and its data kept";
        }

        private sealed class SlotReading {
            public SlotReading(IReadOnlyList<ManagedReferenceMissingType?> carriedBySlot, int unmatchedSlotCount,
                IReadOnlyList<ManagedReferenceMissingType> sharedMissingTypes) {
                CarriedBySlot = carriedBySlot;
                UnmatchedSlotCount = unmatchedSlotCount;
                SharedMissingTypes = sharedMissingTypes;
            }

            public IReadOnlyList<ManagedReferenceMissingType?> CarriedBySlot { get; }

            public int UnmatchedSlotCount { get; }

            public IReadOnlyList<ManagedReferenceMissingType> SharedMissingTypes { get; }
        }
    }
}
