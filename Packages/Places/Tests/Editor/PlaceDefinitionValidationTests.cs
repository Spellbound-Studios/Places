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

        [TestCase("missing")]
        [TestCase("deposits")]
        public void ReportsEntranceLayerUnknownAtTheEntrance(string layerName) {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(new HarvestableTestLayer { LayerName = "deposits" });
            _place.Entrance.LayerName = layerName;

            var finding = Single(Validate(), PlaceFindingCodes.EntranceLayerUnknown);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("entrance", finding.Location);
            StringAssert.Contains("'" + layerName + "'", finding.Message);
        }

        [TestCase(3, 0, 0)]
        [TestCase(-1, 0, 0)]
        [TestCase(0, 3, 0)]
        [TestCase(0, -1, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, -1)]
        public void ReportsEntranceCellOutsideGridAtTheEntrance(int column, int row, int stratum) {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.Entrance.LayerName = "arrangement";
            _place.Entrance.Cell = new CellAddress(column, row, stratum);

            var finding = Single(Validate(), PlaceFindingCodes.EntranceCellOutsideGrid);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("entrance", finding.Location);
            StringAssert.Contains(_place.Entrance.Cell.ToString(), finding.Message);
            StringAssert.Contains("3 by 3 grid of 1 strata", finding.Message);
        }

        [Test]
        public void AnUnsetEntranceIsSilent() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.Entrance.Cell = new CellAddress(99, 99, 99);

            Assert.IsFalse(_place.Entrance.IsSet);
            Assert.IsTrue(Validate().IsEmpty);
        }

        [Test]
        public void AnEntranceInsideTheGridIsSilent() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.Entrance.LayerName = "arrangement";
            _place.Entrance.Cell = new CellAddress(2, 2, 0);

            Assert.IsTrue(Validate().IsEmpty);
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
