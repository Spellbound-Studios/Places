// Copyright (c) Spellbound Studios. All rights reserved.

using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Places.Arrangement;
using Spellbound.Places.Tests.CarriedFixture;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Places.Editor.Tests {
    public class CarriedLayerSurvivalTests {
        private const string CarriedAssembly = "asm: Spellbound.Places.Tests.CarriedFixture";
        private const string AbsentAssembly = "asm: Spellbound.Places.Tests.CarriedFixture.Absent";

        private PlacesTestFolder _folder;

        [SetUp]
        public void CreateFolder() {
            _folder = new PlacesTestFolder();
        }

        [TearDown]
        public void DeleteFolder() {
            _folder.Dispose();
        }

        [Test]
        public void ACarriedLayerSurvivesASaveOfTheAsset() {
            var path = _folder.Path("CarriedPlace.asset");
            var place = CreatePlaceWithCarriedLayerMissing(path);

            place.Layers[0].LayerName = "renamed";
            EditorUtility.SetDirty(place);
            AssetDatabase.SaveAssets();

            var saved = File.ReadAllText(path);
            StringAssert.Contains("class: FixtureCarriedLayer", saved);
            StringAssert.Contains(AbsentAssembly, saved);
            StringAssert.Contains("label: ore", saved);
            StringAssert.Contains("weight: 7", saved);

            File.WriteAllText(path, saved.Replace(AbsentAssembly, CarriedAssembly));
            var restored = Reimport(path);

            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(restored));
            Assert.IsInstanceOf<FixtureCarriedLayer>(restored.Layers[1]);
            var carried = (FixtureCarriedLayer)restored.Layers[1];
            Assert.AreEqual("ore", carried.label);
            Assert.AreEqual(7, carried.weight);
            Assert.AreEqual("carried", carried.LayerName);
            Assert.AreEqual("renamed", restored.Layers[0].LayerName);
        }

        [Test]
        public void ClearingTheCarriedLayerThenSavingDropsItsBlock() {
            var path = _folder.Path("ClearedPlace.asset");
            var place = CreatePlaceWithCarriedLayerMissing(path);

            var missing = SerializationUtility.GetManagedReferencesWithMissingTypes(place);
            Assert.AreEqual(1, missing.Length);
            SerializationUtility.ClearManagedReferenceWithMissingType(place, missing[0].referenceId);
            EditorUtility.SetDirty(place);
            AssetDatabase.SaveAssets();

            var saved = File.ReadAllText(path);
            StringAssert.DoesNotContain("label: ore", saved);
        }

        private static PlaceDefinition CreatePlaceWithCarriedLayerMissing(string path) {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            place.AddLayer(new ArrangementLayer { LayerName = "arrangement" });
            place.AddLayer(new FixtureCarriedLayer { LayerName = "carried", label = "ore", weight = 7 });
            AssetDatabase.CreateAsset(place, path);
            AssetDatabase.SaveAssets();

            var text = File.ReadAllText(path);
            Assert.AreEqual(1, Regex.Matches(text, Regex.Escape(CarriedAssembly)).Count, text);
            File.WriteAllText(path, text.Replace(CarriedAssembly, AbsentAssembly));
            LogAssert.Expect(LogType.Warning,
                new Regex("^Missing types referenced from component PlaceDefinition on game object"));
            var loaded = Reimport(path);

            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(loaded));
            Assert.AreEqual(2, loaded.Layers.Count);
            Assert.IsNull(loaded.Layers[1]);
            return loaded;
        }

        private static PlaceDefinition Reimport(string path) {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var place = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(path);
            Assert.IsNotNull(place, path + " did not load as " + nameof(PlaceDefinition));
            return place;
        }
    }
}
