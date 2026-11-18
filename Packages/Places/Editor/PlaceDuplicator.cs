// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Spellbound.Places.Editor {
    public static class PlaceDuplicator {
        private const string AssetExtension = ".asset";

        public static PlaceDuplicateReport Duplicate(PlaceDefinition source, string newName) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(newName)) {
                throw new ArgumentException("A duplicate needs a name", nameof(newName));
            }

            var sourcePath = AssetDatabase.GetAssetPath(source);

            if (string.IsNullOrEmpty(sourcePath) || !AssetDatabase.IsMainAsset(source)) {
                throw new ArgumentException("The place '" + source.name
                                            + "' is not the main asset of a file, so there is no file to duplicate",
                    nameof(source));
            }

            var copyPath = sourcePath.Substring(0, sourcePath.LastIndexOf('/') + 1) + newName + AssetExtension;

            if (File.Exists(copyPath)) {
                throw new InvalidOperationException("The duplicate's path " + copyPath
                                                    + " already holds a file; choose another name");
            }

            if (!AssetDatabase.CopyAsset(sourcePath, copyPath)) {
                throw new InvalidOperationException("Copying " + sourcePath + " to " + copyPath + " failed");
            }

            var copy = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(copyPath);

            if (copy == null) {
                throw new InvalidOperationException("The copy at " + copyPath + " does not load as a place");
            }

            var renames = Renames(PlaceReferenceTable.Of(copy), newName);

            foreach (var rename in renames.OrderByDescending(rename => rename.PreviousName.Length)) {
                PlaceReferenceTable.RenameLayer(copy, rename.PreviousName, rename.NewName);
            }

            EditorUtility.SetDirty(copy);
            AssetDatabase.SaveAssetIfDirty(copy);

            return new PlaceDuplicateReport(copyPath, renames, CarriedLayers(copy));
        }

        private static List<LayerRename> Renames(PlaceReferenceTable table, string newName) {
            var renames = new List<LayerRename>();
            var renamed = new HashSet<string>(StringComparer.Ordinal);

            foreach (var layerName in table.LayerNames) {
                if (string.IsNullOrWhiteSpace(layerName) || !renamed.Add(layerName)) {
                    continue;
                }

                renames.Add(new LayerRename(layerName, newName + "." + layerName));
            }

            return renames;
        }

        private static List<string> CarriedLayers(PlaceDefinition copy) {
            var carriedLayers = new List<string>();

            foreach (var missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(copy)) {
                var package = LayerAssemblyPackage.Of(missingType.assemblyName) ?? "none";
                carriedLayers.Add("class " + missingType.className + " from assembly " + missingType.assemblyName
                                  + " (package " + package + ")");
            }

            return carriedLayers;
        }
    }
}
