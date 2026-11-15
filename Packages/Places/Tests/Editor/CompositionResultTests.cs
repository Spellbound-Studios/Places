// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class CompositionResultTests {
        private static CompositionResult TwoByTwoResult() {
            var cells = new[] {
                new CompositionCell(new CellAddress(0, 0, 0), CompositionCellState.Placed, "alpha", 1),
                new CompositionCell(new CellAddress(1, 0, 0), CompositionCellState.Empty, null, 0),
                new CompositionCell(new CellAddress(0, 1, 0), CompositionCellState.VoidCentre, null, 0),
                new CompositionCell(new CellAddress(1, 1, 0), CompositionCellState.Placed, "beta", 0),
            };
            var contradictions = new[] { new CellContradiction(new CellAddress(1, 0, 0), "no part fits") };
            return new CompositionResult(2, 1, cells, contradictions, 3, 1);
        }

        [Test]
        public void ToTextWritesEachStratumTopRowFirstThenTheContradictions() {
            const string expected = "stratum 0\n"
                                    + "# beta@0\n"
                                    + "alpha@1 .\n"
                                    + "contradictions: 1\n"
                                    + "1,0,0: no part fits";

            Assert.AreEqual(expected, TwoByTwoResult().ToText());
        }

        [Test]
        public void TheResultKeepsWhatItWasGiven() {
            var result = TwoByTwoResult();

            Assert.AreEqual(2, result.GridSize);
            Assert.AreEqual(1, result.StratumCount);
            Assert.AreEqual(4, result.Cells.Count);
            Assert.AreEqual(1, result.Contradictions.Count);
            Assert.AreEqual(3, result.OrdinaryCellsAttempted);
            Assert.AreEqual(1, result.OrdinaryCellsEmpty);
        }

        [Test]
        public void CellFindsTheCellAtItsAddress() {
            var cell = TwoByTwoResult().Cell(new CellAddress(0, 0, 0));

            Assert.AreEqual(new CellAddress(0, 0, 0), cell.Address);
            Assert.AreEqual(CompositionCellState.Placed, cell.State);
            Assert.AreEqual("alpha", cell.PartName);
            Assert.AreEqual(1, cell.RotationSteps);
            Assert.AreEqual(CompositionCellState.VoidCentre, TwoByTwoResult().Cell(new CellAddress(0, 1, 0)).State);
        }

        [Test]
        public void CellOutsideTheGridThrows() {
            Assert.Throws<ArgumentOutOfRangeException>(() => TwoByTwoResult().Cell(new CellAddress(2, 0, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => TwoByTwoResult().Cell(new CellAddress(0, 0, 1)));
        }

        [Test]
        public void ACellTheResultWasNotGivenIsEmpty() {
            var result = new CompositionResult(3, 1, null, null, 0, 0);

            Assert.AreEqual(CompositionCellState.Empty, result.Cell(new CellAddress(2, 2, 0)).State);
            Assert.AreEqual(0, result.Cells.Count);
            Assert.AreEqual(0, result.Contradictions.Count);
        }

        [Test]
        public void AContradictionKeepsItsCellAndReason() {
            var contradiction = TwoByTwoResult().Contradictions[0];

            Assert.AreEqual(new CellAddress(1, 0, 0), contradiction.Cell);
            Assert.AreEqual("no part fits", contradiction.Reason);
        }
    }
}
