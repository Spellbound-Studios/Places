// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class ArrangementLayerValidationTests {
        private const string Location = "layers[0]";

        [Test]
        public void ACleanLayerReportsNothing() {
            var report = Validate(CleanLayer());

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        [Test]
        public void ReportsGridSizeEvenAtTheGridSizeWithTheValue() {
            var layer = CleanLayer();
            SetGridSize(layer, 4);

            var finding = Single(Validate(layer), PlaceFindingCodes.GridSizeEven);

            AssertError(finding, Location + ".gridSize");
            StringAssert.Contains("4", finding.Message);
        }

        [Test]
        public void AGridSizeBelowOneIsReportedAsGridSizeEven() {
            var layer = CleanLayer();
            SetGridSize(layer, -1);

            var finding = Single(Validate(layer), PlaceFindingCodes.GridSizeEven);

            AssertError(finding, Location + ".gridSize");
            StringAssert.Contains("-1", finding.Message);
        }

        [Test]
        public void ReportsPartNameEmptyAtThePart() {
            var layer = CleanLayer();
            Parts(layer).Add(new PartDescription { partName = " " });

            var finding = Single(Validate(layer), PlaceFindingCodes.PartNameEmpty);

            AssertError(finding, Location + ".parts[2]");
        }

        [Test]
        public void ReportsPartNameDuplicateAtTheRepeatedPart() {
            var layer = CleanLayer();
            Parts(layer).Add(new PartDescription { partName = "corridor" });

            var finding = Single(Validate(layer), PlaceFindingCodes.PartNameDuplicate);

            AssertError(finding, Location + ".parts[2]");
            StringAssert.Contains("corridor", finding.Message);
        }

        [Test]
        public void ReportsPoolPartUnknownAtThePoolEntry() {
            var layer = CleanLayer();
            ((List<StratumDescription>)layer.Strata)[0].pool.Add(new PoolEntry { partName = "missing" });

            var finding = Single(Validate(layer), PlaceFindingCodes.PoolPartUnknown);

            AssertError(finding, Location + ".strata[0].pool[2]");
            StringAssert.Contains("missing", finding.Message);
        }

        [Test]
        public void APoolPartUnknownIsReportedInTheCentrePoolAndInARulePool() {
            var layer = CleanLayer();
            ((List<PoolEntry>)layer.CentrePool).Add(new PoolEntry { partName = "absent" });
            var rule = new WeightedPoolRule { cell = new CellAddress(0, 0, 0) };
            rule.pool.Add(new PoolEntry { partName = "end" });
            rule.pool.Add(new PoolEntry { partName = "gone" });
            Rules(layer).Add(rule);

            var findings = All(Validate(layer), PlaceFindingCodes.PoolPartUnknown);

            CollectionAssert.AreEquivalent(new[] { Location + ".centrePool[0]", Location + ".rules[0].pool[1]" },
                findings.Select(finding => finding.Location).ToArray());
        }

        [Test]
        public void ReportsRulePartUnknownAtTheRule() {
            var layer = CleanLayer();
            Rules(layer).Add(new FixedCellRule { cell = new CellAddress(0, 0, 0), partName = "missing" });

            var finding = Single(Validate(layer), PlaceFindingCodes.RulePartUnknown);

            AssertError(finding, Location + ".rules[0].partName");
            StringAssert.Contains("missing", finding.Message);
        }

        [Test]
        public void ARulePartUnknownIsReportedForAOneOfPartAndARegionRule() {
            var layer = CleanLayer();
            ((List<StratumDescription>)layer.Strata)[0].oneOfParts.Add("lost");
            Rules(layer).Add(new AllowedRegionRule {
                partName = "elsewhere",
                region = new CellRegion { minimum = new CellAddress(0, 0, 0), maximum = new CellAddress(1, 1, 0) }
            });

            var findings = All(Validate(layer), PlaceFindingCodes.RulePartUnknown);

            CollectionAssert.AreEquivalent(
                new[] { Location + ".strata[0].oneOfParts[0]", Location + ".rules[0].partName" },
                findings.Select(finding => finding.Location).ToArray());
        }

        [Test]
        public void ReportsRuleCellOutsideGridWithTheAddress() {
            var layer = CleanLayer();
            Rules(layer).Add(new FixedCellRule { cell = new CellAddress(3, 0, 0), partName = "corridor" });

            var finding = Single(Validate(layer), PlaceFindingCodes.RuleCellOutsideGrid);

            AssertError(finding, Location + ".rules[0].cell");
            StringAssert.Contains("3,0,0", finding.Message);
        }

        [Test]
        public void ARuleCellOutsideTheStrataOrARegionCornerOutsideIsReported() {
            var layer = CleanLayer();
            var rule = new WeightedPoolRule { cell = new CellAddress(0, 0, 1) };
            rule.pool.Add(new PoolEntry { partName = "end" });
            Rules(layer).Add(rule);
            Rules(layer).Add(new AllowedRegionRule {
                partName = "end",
                region = new CellRegion { minimum = new CellAddress(0, 0, 0), maximum = new CellAddress(2, 5, 0) }
            });

            var findings = All(Validate(layer), PlaceFindingCodes.RuleCellOutsideGrid);

            CollectionAssert.AreEquivalent(new[] { Location + ".rules[0].cell", Location + ".rules[1].region.maximum" },
                findings.Select(finding => finding.Location).ToArray());
        }

        [Test]
        public void ReportsWeightNegativeWithTheValue() {
            var layer = CleanLayer();
            ((List<StratumDescription>)layer.Strata)[0].pool[1].weight = -2f;

            var finding = Single(Validate(layer), PlaceFindingCodes.WeightNegative);

            AssertError(finding, Location + ".strata[0].pool[1].weight");
            StringAssert.Contains("-2", finding.Message);
        }

        [Test]
        public void ANullRuleIsSkipped() {
            var layer = CleanLayer();
            Rules(layer).Add(null);

            Assert.IsTrue(Validate(layer).IsEmpty);
        }

        public static ArrangementLayer CleanLayer() {
            var layer = new ArrangementLayer { LayerName = "arrangement", Seed = 1UL };
            SetGridSize(layer, 3);
            Parts(layer).Add(new PartDescription {
                partName = "corridor",
                faces = new PartFaces(SocketKind.Connection, SocketKind.Connection, SocketKind.NoConnection,
                    SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection)
            });
            Parts(layer).Add(new PartDescription {
                partName = "end",
                faces = new PartFaces(SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection,
                    SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection)
            });
            var stratum = new StratumDescription();
            stratum.pool.Add(new PoolEntry { partName = "corridor", weight = 1f });
            stratum.pool.Add(new PoolEntry { partName = "end", weight = 1f });
            ((List<StratumDescription>)layer.Strata).Add(stratum);
            return layer;
        }

        public static void SetGridSize(ArrangementLayer layer, int gridSize) {
            typeof(ArrangementLayer).GetField("gridSize", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(layer, gridSize);
        }

        private static List<PartDescription> Parts(ArrangementLayer layer) {
            return (List<PartDescription>)layer.Parts;
        }

        private static List<CellRule> Rules(ArrangementLayer layer) {
            return (List<CellRule>)layer.Rules;
        }

        private static ValidationReport Validate(ArrangementLayer layer) {
            var report = new ValidationReport("layer");
            layer.Validate(report, Location);
            return report;
        }

        private static ValidationFinding[] All(ValidationReport report, string code) {
            return report.Findings.Where(finding => finding.Code == code).ToArray();
        }

        private static ValidationFinding Single(ValidationReport report, string code) {
            var findings = All(report, code);
            Assert.AreEqual(1, findings.Length, report.ToText());
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            return findings[0];
        }

        private static void AssertError(ValidationFinding finding, string location) {
            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual(location, finding.Location);
        }
    }
}
