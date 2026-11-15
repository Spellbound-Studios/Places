// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class ArrangementDescriptionTests {
        [Test]
        public void NullListsBecomeEmpty() {
            var arrangement = new ArrangementDescription(7, null, null, null, null);

            Assert.AreEqual(7, arrangement.GridSize);
            Assert.AreEqual(0, arrangement.Parts.Count);
            Assert.AreEqual(0, arrangement.Strata.Count);
            Assert.AreEqual(0, arrangement.CentrePool.Count);
            Assert.AreEqual(0, arrangement.Rules.Count);
        }

        [Test]
        public void TheDescriptionKeepsWhatItWasGiven() {
            var part = new PartDescription { partName = "alpha", faces = default };
            var stratum = new StratumDescription();
            var centre = new PoolEntry { partName = "alpha", weight = 2f };
            var rule = new FixedCellRule { cell = new CellAddress(1, 1, 0), partName = "alpha" };

            var arrangement = new ArrangementDescription(5, new[] { part }, new[] { stratum }, new[] { centre },
                new CellRule[] { rule });

            Assert.AreSame(part, arrangement.Parts[0]);
            Assert.AreSame(stratum, arrangement.Strata[0]);
            Assert.AreSame(centre, arrangement.CentrePool[0]);
            Assert.AreSame(rule, arrangement.Rules[0]);
        }

        [Test]
        public void TryFindPartFindsAPartByItsName() {
            var alpha = new PartDescription { partName = "alpha" };
            var beta = new PartDescription { partName = "beta" };
            var arrangement = new ArrangementDescription(3, new[] { alpha, beta }, null, null, null);

            Assert.IsTrue(arrangement.TryFindPart("beta", out var found));
            Assert.AreSame(beta, found);
            Assert.IsFalse(arrangement.TryFindPart("gamma", out var missing));
            Assert.IsNull(missing);
            Assert.IsFalse(arrangement.TryFindPart(null, out _));
        }

        [Test]
        public void ThePoolAndRuleDefaultsAreTheAuthoringDefaults() {
            var entry = new PoolEntry();
            var stratum = new StratumDescription();
            var fixedCell = new FixedCellRule();
            var weightedPool = new WeightedPoolRule();
            var allowedRegion = new AllowedRegionRule { partName = "alpha", region = new CellRegion() };

            Assert.AreEqual(1f, entry.weight);
            Assert.IsNull(entry.partName);
            Assert.IsNotNull(stratum.pool);
            Assert.IsNotNull(stratum.oneOfParts);
            Assert.AreEqual(-1, fixedCell.rotationSteps);
            Assert.AreEqual(default(CellAddress), fixedCell.cell);
            Assert.IsNotNull(weightedPool.pool);
            Assert.AreEqual(default(CellAddress), weightedPool.cell);
            Assert.IsInstanceOf<CellRule>(fixedCell);
            Assert.IsInstanceOf<CellRule>(weightedPool);
            Assert.IsInstanceOf<CellRule>(allowedRegion);
            Assert.AreEqual("alpha", allowedRegion.partName);
            Assert.AreEqual(default(CellRegion), allowedRegion.region);
        }
    }
}
