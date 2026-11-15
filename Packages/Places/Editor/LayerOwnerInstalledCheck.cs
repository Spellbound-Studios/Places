// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
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

            var carried = CarriedBySlot(definition);

            for (var index = 0; index < carried.Count; index++) {
                var location = LayersField + "[" + index + "]";

                if (carried[index] != null) {
                    report.Information(PlaceFindingCodes.LayerCarried, CarriedMessage(carried[index].Value), location);
                } else if (definition.Layers[index] == null) {
                    report.Error(PlaceFindingCodes.LayerEmpty,
                        location + " holds no layer; assign a layer or remove the slot", location);
                }
            }
        }

        public IReadOnlyList<string> PreviewLines(PlaceDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            var count = 0;

            foreach (var missingType in CarriedBySlot(definition)) {
                if (missingType != null) {
                    count++;
                }
            }

            return new[] { count + " layers carried" };
        }

        private static List<ManagedReferenceMissingType?> CarriedBySlot(PlaceDefinition definition) {
            var missingTypes = new List<ManagedReferenceMissingType>(
                SerializationUtility.GetManagedReferencesWithMissingTypes(definition));
            var carried = new List<ManagedReferenceMissingType?>();
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

                if (matched < 0 && missingTypes.Count > 0) {
                    matched = 0;
                }

                if (matched < 0) {
                    carried.Add(null);
                    continue;
                }

                carried.Add(missingTypes[matched]);
                missingTypes.RemoveAt(matched);
            }

            return carried;
        }

        private static string CarriedMessage(ManagedReferenceMissingType missingType) {
            var package = LayerAssemblyPackage.Of(missingType.assemblyName) ?? "none";
            return "stored type " + missingType.className + " from assembly " + missingType.assemblyName
                   + " (package " + package + ") is not installed; the layer is carried and its data kept";
        }
    }
}
