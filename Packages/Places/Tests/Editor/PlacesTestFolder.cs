// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEditor;

namespace Spellbound.Places.Editor.Tests {
    public sealed class PlacesTestFolder : IDisposable {
        public const string Root = "Assets/SpellboundPlacesTestsAuthoring";

        public string Folder { get; }

        public PlacesTestFolder() {
            if (!AssetDatabase.IsValidFolder(Root)) {
                AssetDatabase.CreateFolder("Assets", Root.Substring("Assets/".Length));
            }

            var name = "Run" + Guid.NewGuid().ToString("N").Substring(0, 8);
            AssetDatabase.CreateFolder(Root, name);
            Folder = Root + "/" + name;
        }

        public string Path(string fileName) {
            return Folder + "/" + fileName;
        }

        public void Dispose() {
            AssetDatabase.DeleteAsset(Folder);

            if (AssetDatabase.IsValidFolder(Root)
                && AssetDatabase.FindAssets(string.Empty, new[] { Root }).Length == 0) {
                AssetDatabase.DeleteAsset(Root);
            }
        }
    }
}
