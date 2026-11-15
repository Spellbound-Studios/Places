// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Core.Authoring.Editor;
using Spellbound.Places.Tests.CarriedFixture;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Places.Editor.Tests {
    public class LayerOwnerInstalledCheckTests {
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
        public void TheRegistryListsTheCheckAndTheOwnValidationWithThePlacesLabel() {
            var check = DefinitionCheckRegistry.Default.Find(PlaceFindingCodes.LayerOwnerInstalled);

            Assert.IsNotNull(check);
            Assert.AreEqual("places.layer-owner-installed (Places)", check.Label);
            Assert.AreEqual(typeof(PlaceDefinition), check.DefinitionType);
            Assert.AreEqual(typeof(LayerOwnerInstalledCheck), check.CheckType);
            Assert.IsTrue(check.RunsWhileEditing);
            CollectionAssert.Contains(DefinitionCheckRegistry.Default.For(typeof(PlaceDefinition)), check);
            Assert.AreEqual("PlaceDefinition (Places)", PackageLabel.OwnValidation(typeof(PlaceDefinition)));
        }

        [Test]
        public void ACarriedLayerIsReportedAtItsSlot() {
            var place = CreateCarriedPlace(_folder.Path("CarriedFirst.asset"), true);

            var report = Check(place);

            var finding = Single(report, PlaceFindingCodes.LayerCarried);
            Assert.AreEqual(FindingSeverity.Information, finding.Severity);
            Assert.AreEqual("layers[0]", finding.Location);
            Assert.AreEqual(0, report.Count(FindingSeverity.Error), report.ToText());
        }

        [Test]
        public void ReportsLayerCarriedAtItsSlot() {
            var place = CreateCarriedPlace(_folder.Path("CarriedSecond.asset"), false);

            var report = Check(place);

            var finding = Single(report, PlaceFindingCodes.LayerCarried);
            Assert.AreEqual(FindingSeverity.Information, finding.Severity);
            Assert.AreEqual("layers[1]", finding.Location);
            Assert.AreEqual(
                "stored type FixtureCarriedLayer from assembly Spellbound.Places.Tests.CarriedFixture.Absent "
                + "(package Places) is not installed; the layer is carried and its data kept",
                finding.Message);
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
        }

        [Test]
        public void PreviewLinesSayHowManyLayersAreCarried() {
            var place = CreateCarriedPlace(_folder.Path("CarriedPreview.asset"), false);

            var lines = new LayerOwnerInstalledCheck().PreviewLines(place);

            CollectionAssert.AreEqual(new[] { "1 layers carried" }, lines);
        }

        [Test]
        public void ReportsLayerEmptyAtItsSlot() {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            ((List<PlaceLayer>)place.Layers).Add(null);

            var report = Check(place);

            var finding = Single(report, PlaceFindingCodes.LayerEmpty);
            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[1]", finding.Location);
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            Object.DestroyImmediate(place);
        }

        [Test]
        public void TheNullSlotIsSilentAtRuntimeAndAnErrorInTheCheck() {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            ((List<PlaceLayer>)place.Layers).Add(null);
            place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            var runtime = new ValidationReport("place");

            place.Validate(runtime);
            var report = Check(place);

            Assert.IsTrue(runtime.IsEmpty, runtime.ToText());
            var finding = Single(report, PlaceFindingCodes.LayerEmpty);
            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[0]", finding.Location);
            CollectionAssert.AreEqual(new[] { "0 layers carried" }, new LayerOwnerInstalledCheck().PreviewLines(place));
            Object.DestroyImmediate(place);
        }

        [Test]
        public void APlaceWithEveryLayerPresentReportsNothing() {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            place.AddLayer(ArrangementLayerValidationTests.CleanLayer());

            Assert.IsTrue(Check(place).IsEmpty);
            Object.DestroyImmediate(place);
        }

        private static ValidationReport Check(PlaceDefinition place) {
            var report = new ValidationReport("place");
            new LayerOwnerInstalledCheck().Check(place, report);
            return report;
        }

        private static ValidationFinding Single(ValidationReport report, string code) {
            var findings = report.Findings.Where(finding => finding.Code == code).ToArray();
            Assert.AreEqual(1, findings.Length, report.ToText());
            return findings[0];
        }

        private static PlaceDefinition CreateCarriedPlace(string path, bool carriedFirst) {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            var carried = new FixtureCarriedLayer { LayerName = "carried", label = "ore", weight = 7 };
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
            Assert.IsNull(loaded.Layers[carriedFirst ? 0 : 1]);
            return loaded;
        }
    }
}
