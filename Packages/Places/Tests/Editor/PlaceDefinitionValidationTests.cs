// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceDefinitionValidationTests {
        private PlaceDefinition _place;

        [SetUp]
        public void CreatePlace() {
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
        }

        [TearDown]
        public void DestroyPlace() {
            Object.DestroyImmediate(_place);
        }

        [Test]
        public void ReportsLayerNameEmptyAtItsSlot() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(new ArrangementLayer { LayerName = " " });

            var finding = Single(Validate(), PlaceFindingCodes.LayerNameEmpty);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[1]", finding.Location);
        }

        [Test]
        public void ReportsLayerNameDuplicateAtItsSlot() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());

            var finding = Single(Validate(), PlaceFindingCodes.LayerNameDuplicate);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[1]", finding.Location);
            StringAssert.Contains("arrangement", finding.Message);
        }

        [Test]
        public void ANullSlotIsSilent() {
            ((List<PlaceLayer>)_place.Layers).Add(null);
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());

            Assert.IsTrue(Validate().IsEmpty);
        }

        [Test]
        public void EachPresentLayerValidatesAtItsSlot() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            var second = _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            second.LayerName = "second";
            ArrangementLayerValidationTests.SetGridSize(second, 2);

            var finding = Single(Validate(), PlaceFindingCodes.GridSizeEven);

            Assert.AreEqual("layers[1].gridSize", finding.Location);
        }

        private ValidationReport Validate() {
            var report = new ValidationReport("place");
            _place.Validate(report);
            return report;
        }

        private static ValidationFinding Single(ValidationReport report, string code) {
            var findings = report.Findings.Where(finding => finding.Code == code).ToArray();
            Assert.AreEqual(1, findings.Length, report.ToText());
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            return findings[0];
        }
    }
}
