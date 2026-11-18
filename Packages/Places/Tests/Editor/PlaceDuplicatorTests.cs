// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceDuplicatorTests {
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
        public void TheDuplicateSitsBesideItsSourceWithEveryLayerRenamed() {
            var source = CreateHall();

            var report = PlaceDuplicator.Duplicate(source, "Hall Copy");

            Assert.AreEqual(_folder.Path("Hall Copy.asset"), report.CopyPath);
            var copy = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(report.CopyPath);
            Assert.IsNotNull(copy);
            CollectionAssert.AreEqual(new[] { "Hall Copy.arrangement", "Hall Copy.cellar" },
                copy.Layers.Select(layer => layer.LayerName));
            var nested = (NestedPlaceLayer)copy.Layers[1];
            Assert.AreEqual("Hall Copy.arrangement", nested.ArrangementLayerName);
            Assert.AreEqual(new CellAddress(1, 1, 0), nested.Cell);
            Assert.AreEqual("Hall Copy.arrangement", copy.Entrance.LayerName);
            Assert.AreEqual(2, report.Renames.Count);
            Assert.AreEqual("arrangement", report.Renames[0].PreviousName);
            Assert.AreEqual("Hall Copy.arrangement", report.Renames[0].NewName);
            Assert.AreEqual("cellar", report.Renames[1].PreviousName);
            Assert.AreEqual("Hall Copy.cellar", report.Renames[1].NewName);
            CollectionAssert.IsEmpty(report.CarriedLayers);
            CollectionAssert.AreEqual(new[] { "arrangement", "cellar" },
                source.Layers.Select(layer => layer.LayerName));
            Assert.AreEqual("arrangement", source.Entrance.LayerName);
        }

        [Test]
        public void TheSourceFileIsUnchangedByteForByte() {
            var source = CreateHall();
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var before = Hash(sourcePath);

            PlaceDuplicator.Duplicate(source, "Hall Copy");

            CollectionAssert.AreEqual(before, Hash(sourcePath));
            Assert.IsFalse(EditorUtility.IsDirty(source));
        }

        [Test]
        public void PartNamesAreNotRenamed() {
            var source = CreateHall();

            var report = PlaceDuplicator.Duplicate(source, "Hall Copy");

            var copy = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(report.CopyPath);
            var arrangement = (ArrangementLayer)copy.Layers[0];
            CollectionAssert.AreEqual(new[] { "corridor", "end" }, arrangement.Parts.Select(part => part.partName));
            CollectionAssert.AreEqual(new[] { "corridor", "end" },
                arrangement.Strata[0].pool.Select(entry => entry.partName));
        }

        [Test]
        public void ALayerAlreadyNamedLikeACopyIsRenamedOnce() {
            var source = ScriptableObject.CreateInstance<PlaceDefinition>();
            source.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            var copied = ArrangementLayerValidationTests.CleanLayer();
            copied.LayerName = "Hall Copy.arrangement";
            source.AddLayer(copied);
            source.Entrance.LayerName = "arrangement";
            AssetDatabase.CreateAsset(source, _folder.Path("Hall.asset"));
            AssetDatabase.SaveAssets();

            var report = PlaceDuplicator.Duplicate(source, "Hall Copy");

            var copy = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(report.CopyPath);
            CollectionAssert.AreEqual(new[] { "Hall Copy.arrangement", "Hall Copy.Hall Copy.arrangement" },
                copy.Layers.Select(layer => layer.LayerName));
            Assert.AreEqual("Hall Copy.arrangement", copy.Entrance.LayerName);
            CollectionAssert.AreEqual(new[] { "arrangement", "Hall Copy.arrangement" },
                report.Renames.Select(rename => rename.PreviousName));
        }

        [Test]
        public void ACarriedLayerIsKeptInTheCopyAndListedWithItsPackage() {
            var source = CarriedPlaces.CreateCarriedPlace(_folder.Path("Hall.asset"), carriedFirst: true);

            var report = PlaceDuplicator.Duplicate(source, "Hall Copy");

            var text = File.ReadAllText(report.CopyPath);
            StringAssert.Contains("FixtureCarriedLayer", text);
            StringAssert.Contains("Spellbound.Places.Tests.CarriedFixture.Absent", text);
            StringAssert.Contains("label: ore", text);
            StringAssert.Contains("weight: 7", text);
            var copy = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(report.CopyPath);
            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(copy));
            Assert.AreEqual(1, report.CarriedLayers.Count);
            StringAssert.Contains("FixtureCarriedLayer", report.CarriedLayers[0]);
            StringAssert.Contains("Spellbound.Places.Tests.CarriedFixture.Absent", report.CarriedLayers[0]);
            StringAssert.Contains("(package Places)", report.CarriedLayers[0]);
            Assert.IsNull(copy.Layers[0]);
            Assert.AreEqual("Hall Copy.arrangement", copy.Layers[1].LayerName);
            Assert.AreEqual(1, report.Renames.Count);
        }

        [Test]
        public void AnExistingTargetIsRefused() {
            var source = CreateHall();
            var existingPath = _folder.Path("Hall Copy.asset");
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PlaceDefinition>(), existingPath);
            AssetDatabase.SaveAssets();
            var before = Hash(existingPath);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                PlaceDuplicator.Duplicate(source, "Hall Copy"));

            StringAssert.Contains(existingPath, exception.Message);
            CollectionAssert.AreEqual(before, Hash(existingPath));
        }

        [Test]
        public void ASourceThatIsNotAnAssetIsRefused() {
            var source = ScriptableObject.CreateInstance<PlaceDefinition>();

            try {
                Assert.Throws<ArgumentException>(() => PlaceDuplicator.Duplicate(source, "Hall Copy"));
            } finally {
                Object.DestroyImmediate(source);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void AnEmptyNameIsRefused(string newName) {
            var source = CreateHall();

            Assert.Throws(Is.InstanceOf<ArgumentException>(), () => PlaceDuplicator.Duplicate(source, newName));
            Assert.AreEqual(1, AssetDatabase.FindAssets("t:PlaceDefinition", new[] { _folder.Folder }).Length);
        }

        private PlaceDefinition CreateHall() {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            place.AddLayer(new NestedPlaceLayer {
                LayerName = "cellar",
                ArrangementLayerName = "arrangement",
                Cell = new CellAddress(1, 1, 0)
            });
            place.Entrance.LayerName = "arrangement";
            AssetDatabase.CreateAsset(place, _folder.Path("Hall.asset"));
            AssetDatabase.SaveAssets();
            return place;
        }

        private static byte[] Hash(string path) {
            using var sha = SHA256.Create();
            return sha.ComputeHash(File.ReadAllBytes(path));
        }
    }
}
