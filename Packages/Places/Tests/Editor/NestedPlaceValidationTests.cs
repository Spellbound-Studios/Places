// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class NestedPlaceValidationTests {
        private readonly List<PlaceDefinition> _places = new();

        [TearDown]
        public void DestroyPlaces() {
            foreach (var place in _places) {
                Object.DestroyImmediate(place);
            }

            _places.Clear();
        }

        [Test]
        public void ReportsNestingPlaceEmptyAtThePlace() {
            var root = Place("Hall");
            root.AddLayer(new NestedPlaceLayer { LayerName = "cellar" });

            var finding = Single(Validate(root), PlaceFindingCodes.NestingPlaceEmpty);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[0].place", finding.Location);
        }

        [TestCase("missing")]
        [TestCase("deposits")]
        public void ReportsNestingLayerUnknownAtTheArrangementLayerName(string layerName) {
            var root = Place("Hall");
            root.AddLayer(new HarvestableTestLayer { LayerName = "deposits" });
            root.AddLayer(new NestedPlaceLayer {
                LayerName = "cellar",
                Place = Place("Cellar"),
                ArrangementLayerName = layerName
            });

            var finding = Single(Validate(root), PlaceFindingCodes.NestingLayerUnknown);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[1].arrangementLayerName", finding.Location);
            StringAssert.Contains("'" + layerName + "'", finding.Message);
        }

        [TestCase(3, 0, 0)]
        [TestCase(-1, 0, 0)]
        [TestCase(0, 3, 0)]
        [TestCase(0, 0, 1)]
        public void ReportsNestingCellOutsideGridAtTheCell(int column, int row, int stratum) {
            var root = Place("Hall");
            root.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            var cell = new CellAddress(column, row, stratum);
            root.AddLayer(new NestedPlaceLayer {
                LayerName = "cellar",
                Place = Place("Cellar"),
                ArrangementLayerName = "arrangement",
                Cell = cell
            });

            var finding = Single(Validate(root), PlaceFindingCodes.NestingCellOutsideGrid);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[1].cell", finding.Location);
            StringAssert.Contains(cell.ToString(), finding.Message);
            StringAssert.Contains("3 by 3 grid of 1 strata", finding.Message);
        }

        [Test]
        public void ReportsNestingCycleAtTheRootLayer() {
            var hall = Place("Hall");
            var cellar = Place("Cellar");
            hall.AddLayer(Nest("cellar", cellar));
            cellar.AddLayer(Nest("hall", hall));

            var finding = Single(Validate(hall), PlaceFindingCodes.NestingCycle);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[0]", finding.Location);
            StringAssert.Contains("Hall > Cellar > Hall", finding.Message);
        }

        [Test]
        public void ReportsNestingDepthOverTheLimit() {
            var root = Place("Root");
            var deepest = root;

            for (var depth = 1; depth <= 4; depth++) {
                var nested = Place("Depth" + depth);
                deepest.AddLayer(Nest("nested", nested));
                deepest = nested;
            }

            Assert.AreEqual(4, root.NestingDepthLimit);
            Assert.IsTrue(Validate(root).IsEmpty, Validate(root).ToText());

            deepest.AddLayer(Nest("nested", Place("Depth5")));

            var finding = Single(Validate(root), PlaceFindingCodes.NestingDepth);

            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual("layers[0]", finding.Location);
            StringAssert.Contains("5 deep", finding.Message);
            StringAssert.Contains("limit of 4", finding.Message);
        }

        [Test]
        public void ADiamondIsNotACycle() {
            var hall = Place("Hall");
            var east = Place("East");
            var west = Place("West");
            var cellar = Place("Cellar");
            hall.AddLayer(Nest("east", east));
            hall.AddLayer(Nest("west", west));
            east.AddLayer(Nest("cellar", cellar));
            west.AddLayer(Nest("cellar", cellar));
            east.AddLayer(Nest("west", west));

            var report = Validate(hall);

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        [Test]
        public void ANestedPlaceIsNotValidatedByItsParent() {
            var hall = Place("Hall");
            var cellar = Place("Cellar");
            cellar.AddLayer(new HarvestableTestLayer { LayerName = "" });
            cellar.AddLayer(new NestedPlaceLayer { LayerName = "empty" });
            hall.AddLayer(Nest("cellar", cellar));

            var report = Validate(hall);

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        private PlaceDefinition Place(string name) {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            place.name = name;
            _places.Add(place);
            return place;
        }

        private static NestedPlaceLayer Nest(string layerName, PlaceDefinition place) {
            return new NestedPlaceLayer { LayerName = layerName, Place = place };
        }

        private static ValidationReport Validate(PlaceDefinition place) {
            var report = new ValidationReport("place");
            place.Validate(report);
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
