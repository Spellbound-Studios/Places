// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Places.Tests.CarriedFixture;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Places.Editor.Tests {
    public static class CarriedPlaces {
        private const string CarriedAssembly = "asm: Spellbound.Places.Tests.CarriedFixture";
        private const string AbsentAssembly = "asm: Spellbound.Places.Tests.CarriedFixture.Absent";

        public static PlaceDefinition CreateCarriedPlace(string path, bool carriedFirst, bool emptySlotAhead = false) {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            var carried = new FixtureCarriedLayer { LayerName = "carried", label = "ore", weight = 7 };
            if (emptySlotAhead) {
                ((List<PlaceLayer>)place.Layers).Add(null);
            }

            if (carriedFirst) {
                place.AddLayer(carried);
            }

            place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            if (!carriedFirst) {
                place.AddLayer(carried);
            }

            AssetDatabase.CreateAsset(place, path);
            AssetDatabase.SaveAssets();

            var text = File.ReadAllText(path);
            Assert.AreEqual(1, Regex.Matches(text, Regex.Escape(CarriedAssembly)).Count, text);
            File.WriteAllText(path, text.Replace(CarriedAssembly, AbsentAssembly));
            LogAssert.Expect(LogType.Warning,
                new Regex("^Missing types referenced from component PlaceDefinition on game object"));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var loaded = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(path);

            Assert.IsNotNull(loaded, path);
            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(loaded));
            var carriedIndex = (emptySlotAhead ? 1 : 0) + (carriedFirst ? 0 : 1);
            Assert.IsNull(loaded.Layers[carriedIndex]);
            return loaded;
        }
    }
}
