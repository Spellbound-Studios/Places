// Copyright (c) Spellbound Studios. All rights reserved.

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Authoring;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceFindingCodesTests {
        [TestCase(PlaceFindingCodes.LayerNameEmpty, "places.layer-name-empty")]
        [TestCase(PlaceFindingCodes.LayerNameDuplicate, "places.layer-name-duplicate")]
        [TestCase(PlaceFindingCodes.LayerEmpty, "places.layer-empty")]
        [TestCase(PlaceFindingCodes.LayerCarried, "places.layer-carried")]
        [TestCase(PlaceFindingCodes.LayerNotInstalled, "places.layer-not-installed")]
        [TestCase(PlaceFindingCodes.LayerOwnerInstalled, "places.layer-owner-installed")]
        [TestCase(PlaceFindingCodes.GridSizeEven, "places.grid-size-even")]
        [TestCase(PlaceFindingCodes.PartNameEmpty, "places.part-name-empty")]
        [TestCase(PlaceFindingCodes.PartNameDuplicate, "places.part-name-duplicate")]
        [TestCase(PlaceFindingCodes.PoolPartUnknown, "places.pool-part-unknown")]
        [TestCase(PlaceFindingCodes.RulePartUnknown, "places.rule-part-unknown")]
        [TestCase(PlaceFindingCodes.RuleCellOutsideGrid, "places.rule-cell-outside-grid")]
        [TestCase(PlaceFindingCodes.WeightNegative, "places.weight-negative")]
        [TestCase(PlaceFindingCodes.NestingPlaceEmpty, "places.nesting-place-empty")]
        [TestCase(PlaceFindingCodes.NestingCycle, "places.nesting-cycle")]
        [TestCase(PlaceFindingCodes.NestingDepth, "places.nesting-depth")]
        [TestCase(PlaceFindingCodes.NestingLayerUnknown, "places.nesting-layer-unknown")]
        [TestCase(PlaceFindingCodes.NestingCellOutsideGrid, "places.nesting-cell-outside-grid")]
        [TestCase(PlaceFindingCodes.EntranceLayerUnknown, "places.entrance-layer-unknown")]
        [TestCase(PlaceFindingCodes.EntranceCellOutsideGrid, "places.entrance-cell-outside-grid")]
        public void EachCodeReadsAsWritten(string code, string expected) {
            Assert.AreEqual(expected, code);
        }

        [Test]
        public void EveryPublicConstantIsAFindingCode() {
            var codes = typeof(PlaceFindingCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral)
                .Select(field => (string)field.GetRawConstantValue())
                .ToList();

            Assert.AreEqual(20, codes.Count);
            Assert.AreEqual(codes.Count, codes.Distinct().Count());

            foreach (var code in codes) {
                Assert.IsTrue(ValidationFinding.IsCode(code), code);
                StringAssert.StartsWith("places.", code);
            }
        }
    }
}
