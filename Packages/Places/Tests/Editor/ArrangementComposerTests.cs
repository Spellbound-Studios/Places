// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Spellbound.Places.Arrangement;
using static Spellbound.Places.Editor.Tests.ArrangementFixtures;

namespace Spellbound.Places.Editor.Tests {
    public class ArrangementComposerTests {
        private const string EmptiedDomainReason = "no part fits the faces required by its neighbours";

        private static readonly HorizontalDirection[] Directions = {
            HorizontalDirection.North, HorizontalDirection.East, HorizontalDirection.South, HorizontalDirection.West
        };

        private static CellAddress Neighbour(CellAddress cell, HorizontalDirection direction) {
            switch (direction) {
                case HorizontalDirection.North:
                    return new CellAddress(cell.column, cell.row + 1, cell.stratum);
                case HorizontalDirection.East:
                    return new CellAddress(cell.column + 1, cell.row, cell.stratum);
                case HorizontalDirection.South:
                    return new CellAddress(cell.column, cell.row - 1, cell.stratum);
                default:
                    return new CellAddress(cell.column - 1, cell.row, cell.stratum);
            }
        }

        private static HorizontalDirection Opposite(HorizontalDirection direction) {
            return (HorizontalDirection)(((int)direction + 2) % 4);
        }

        private static bool IsInside(CompositionResult result, CellAddress cell) {
            return cell.column >= 0 && cell.column < result.GridSize && cell.row >= 0 && cell.row < result.GridSize;
        }

        private static PartFaces FacesOf(ArrangementDescription arrangement, CompositionCell cell) {
            Assert.IsTrue(arrangement.TryFindPart(cell.PartName, out var part), cell.PartName);
            return part.faces;
        }

        private static void AssertEveryHorizontalFaceFits(ArrangementDescription arrangement,
            CompositionResult result) {
            foreach (var cell in result.Cells.Where(cell => cell.State == CompositionCellState.Placed)) {
                var faces = FacesOf(arrangement, cell);

                foreach (var direction in Directions) {
                    var face = faces.Rotated(cell.RotationSteps, direction);
                    var neighbourAddress = Neighbour(cell.Address, direction);

                    if (!IsInside(result, neighbourAddress)) {
                        Assert.IsTrue(SocketCompatibility.AreCompatible(face, SocketKind.NoConnection),
                            cell.Address + " faces the grid edge with " + face);
                        continue;
                    }

                    var neighbour = result.Cell(neighbourAddress);

                    if (neighbour.State == CompositionCellState.VoidCentre) {
                        Assert.IsTrue(SocketCompatibility.AreCompatible(face, SocketKind.NoConnection),
                            cell.Address + " faces the void centre with " + face);
                    } else if (neighbour.State == CompositionCellState.Placed) {
                        var touching = FacesOf(arrangement, neighbour)
                            .Rotated(neighbour.RotationSteps, Opposite(direction));
                        Assert.IsTrue(SocketCompatibility.AreCompatible(face, touching),
                            cell.Address + " " + face + " against " + neighbourAddress + " " + touching);
                    }
                }
            }
        }

        private static int CountPlaced(CompositionResult result, string partName, int stratum) {
            return result.Cells.Count(cell => cell.State == CompositionCellState.Placed
                                              && cell.Address.stratum == stratum && cell.PartName == partName);
        }

        [Test]
        public void TheSameSeedGivesTheSameText() {
            var arrangement = LiveSized();

            var first = ArrangementComposer.Compose(arrangement, 42UL).ToText();
            var second = PlacesKit.Compose(arrangement, 42UL).ToText();

            Assert.AreEqual(first, second);
        }

        [Test]
        public void ADifferentSeedDiffersOnATwoPartPool() {
            var arrangement = TwoPartPool(5);

            var first = ArrangementComposer.Compose(arrangement, 1UL);
            var second = ArrangementComposer.Compose(arrangement, 2UL);

            Assert.AreNotEqual(first.ToText(), second.ToText());
            Assert.AreEqual(0, first.Contradictions.Count);
            Assert.AreEqual(24, first.OrdinaryCellsAttempted);
        }

        [Test]
        public void AFixedCellRuleIsHonoured() {
            var rules = new CellRule[] {
                new FixedCellRule { cell = new CellAddress(3, 1, 0), partName = SecondClosedPart, rotationSteps = 2 },
                new FixedCellRule { cell = new CellAddress(0, 4, 0), partName = FirstClosedPart, rotationSteps = -1 }
            };

            for (var seed = 0UL; seed < 10UL; seed++) {
                var result = ArrangementComposer.Compose(TwoPartPool(5, rules), seed);

                var pinned = result.Cell(new CellAddress(3, 1, 0));
                Assert.AreEqual(CompositionCellState.Placed, pinned.State);
                Assert.AreEqual(SecondClosedPart, pinned.PartName);
                Assert.AreEqual(2, pinned.RotationSteps);
                Assert.AreEqual(FirstClosedPart, result.Cell(new CellAddress(0, 4, 0)).PartName);
                Assert.AreEqual(0, result.Contradictions.Count);
                Assert.AreEqual(22, result.OrdinaryCellsAttempted);
            }
        }

        [Test]
        public void AFixedRotationIsKeptAndTheClashIsReported() {
            var rules = new CellRule[] {
                new FixedCellRule { cell = new CellAddress(0, 0, 0), partName = FirstClosedPart, rotationSteps = 0 },
                new FixedCellRule { cell = new CellAddress(1, 0, 0), partName = DeadEndPartName, rotationSteps = 0 }
            };
            var arrangement = Describe(3, new[] { ClosedPart(FirstClosedPart), DeadEndPart() },
                new[] { Stratum(new[] { Entry(FirstClosedPart) }) }, null, rules);

            var result = ArrangementComposer.Compose(arrangement, 7UL);

            var clashing = result.Cell(new CellAddress(1, 0, 0));
            Assert.AreEqual(DeadEndPartName, clashing.PartName);
            Assert.AreEqual(0, clashing.RotationSteps);
            Assert.AreEqual(1, result.Contradictions.Count);
            Assert.AreEqual(new CellAddress(1, 0, 0), result.Contradictions[0].Cell);
            Assert.AreEqual(FirstClosedPart, result.Cell(new CellAddress(0, 0, 0)).PartName);
        }

        [Test]
        public void AWeightedPoolRuleReplacesTheCellPool() {
            var rules = new CellRule[] {
                new WeightedPoolRule {
                    cell = new CellAddress(2, 0, 0),
                    pool = { Entry(SecondClosedPart, 3f) }
                }
            };
            var arrangement = Describe(3, new[] { ClosedPart(FirstClosedPart), ClosedPart(SecondClosedPart) },
                new[] { Stratum(new[] { Entry(FirstClosedPart) }) }, null, rules);

            for (var seed = 0UL; seed < 10UL; seed++) {
                var result = ArrangementComposer.Compose(arrangement, seed);

                Assert.AreEqual(SecondClosedPart, result.Cell(new CellAddress(2, 0, 0)).PartName);
                Assert.AreEqual(1, CountPlaced(result, SecondClosedPart, 0));
                Assert.AreEqual(7, CountPlaced(result, FirstClosedPart, 0));
                Assert.AreEqual(0, result.Contradictions.Count);
            }
        }

        [Test]
        public void AnAllowedRegionRuleKeepsThePartInside() {
            var region = new CellRegion { minimum = new CellAddress(0, 0, 0), maximum = new CellAddress(1, 4, 0) };
            var rules = new CellRule[] { new AllowedRegionRule { partName = SecondClosedPart, region = region } };
            var placedInside = 0;

            for (var seed = 0UL; seed < 20UL; seed++) {
                var result = ArrangementComposer.Compose(TwoPartPool(5, rules), seed);

                foreach (var cell in result.Cells.Where(cell => cell.PartName == SecondClosedPart)) {
                    Assert.IsTrue(region.Contains(cell.Address), cell.Address.ToString());
                    placedInside++;
                }

                Assert.AreEqual(0, result.Contradictions.Count);
            }

            Assert.Greater(placedInside, 0);
        }

        [Test]
        public void IncompatibleFixedNeighboursAreReported() {
            var rules = new CellRule[] {
                new FixedCellRule { cell = new CellAddress(0, 0, 0), partName = FirstClosedPart, rotationSteps = 0 },
                new FixedCellRule { cell = new CellAddress(2, 0, 0), partName = FirstClosedPart, rotationSteps = 0 },
                new FixedCellRule { cell = new CellAddress(1, 0, 0), partName = CorridorPartName, rotationSteps = -1 }
            };
            var arrangement = Describe(3, new[] { ClosedPart(FirstClosedPart), CorridorPart() },
                new[] { Stratum(new[] { Entry(FirstClosedPart) }) }, null, rules);

            var result = ArrangementComposer.Compose(arrangement, 3UL);

            Assert.AreEqual(CorridorPartName, result.Cell(new CellAddress(1, 0, 0)).PartName);
            Assert.AreEqual(1, result.Contradictions.Count);
            Assert.AreEqual(new CellAddress(1, 0, 0), result.Contradictions[0].Cell);
        }

        [Test]
        public void TheGridEdgeRequiresNoConnection() {
            var arrangement = Describe(5, new[] { ClosedPart(FirstClosedPart), CorridorPart(), DeadEndPart() },
                new[] {
                    Stratum(new[] { Entry(FirstClosedPart), Entry(CorridorPartName, 4f), Entry(DeadEndPartName) })
                });
            var corridorsOnTheEdge = 0;

            for (var seed = 0UL; seed < 20UL; seed++) {
                var result = ArrangementComposer.Compose(arrangement, seed);

                AssertEveryHorizontalFaceFits(arrangement, result);
                corridorsOnTheEdge += result.Cells.Count(cell => cell.PartName == CorridorPartName
                                                                 && (cell.Address.row == 0 || cell.Address.row == 4));
            }

            Assert.Greater(corridorsOnTheEdge, 0);
        }

        [Test]
        public void EverythingMatchesAnyFace() {
            var arrangement = Describe(3, new[] { ShaftDownPart(), OpenAbovePart() },
                new[] { Stratum(new[] { Entry(ShaftDownPartName) }), Stratum(new[] { Entry(OpenAbovePartName) }) });

            var result = ArrangementComposer.Compose(arrangement, 11UL);

            Assert.AreEqual(0, result.Contradictions.Count);
            Assert.AreEqual(0, result.OrdinaryCellsEmpty);
            Assert.AreEqual(16, result.OrdinaryCellsAttempted);
            Assert.AreEqual(8, CountPlaced(result, ShaftDownPartName, 0));
            Assert.AreEqual(8, CountPlaced(result, OpenAbovePartName, 1));
        }

        [Test]
        public void AVerticalFaceConstrainsTheCellBelow() {
            var arrangement = Describe(3,
                new[] { ShaftDownPart(), ShaftUpPart(), ClosedPart(FirstClosedPart) },
                new[] {
                    Stratum(new[] { Entry(ShaftDownPartName), Entry(FirstClosedPart) }),
                    Stratum(new[] { Entry(ShaftUpPartName), Entry(FirstClosedPart) })
                },
                new[] { Entry(FirstClosedPart) });
            var shafts = 0;

            for (var seed = 0UL; seed < 20UL; seed++) {
                var result = ArrangementComposer.Compose(arrangement, seed);

                for (var row = 0; row < 3; row++) {
                    for (var column = 0; column < 3; column++) {
                        var above = result.Cell(new CellAddress(column, row, 0));
                        var below = result.Cell(new CellAddress(column, row, 1));
                        var shaftAbove = above.PartName == ShaftDownPartName;
                        Assert.AreEqual(shaftAbove, below.PartName == ShaftUpPartName, above.Address.ToString());
                        shafts += shaftAbove ? 1 : 0;
                    }
                }

                Assert.AreEqual(FirstClosedPart, result.Cell(new CellAddress(1, 1, 1)).PartName);
                Assert.AreEqual(0, result.Contradictions.Count);
            }

            Assert.Greater(shafts, 0);
        }

        [Test]
        public void AnEmptiedDomainIsCountedAndLeftEmpty() {
            var arrangement = Describe(3, new[] { CorridorPart() },
                new[] { Stratum(new[] { Entry(CorridorPartName) }) });

            var result = ArrangementComposer.Compose(arrangement, 5UL);

            Assert.AreEqual(8, result.OrdinaryCellsAttempted);
            Assert.AreEqual(4, result.OrdinaryCellsEmpty);
            Assert.AreEqual(4, result.Contradictions.Count);

            foreach (var corner in new[] {
                         new CellAddress(0, 0, 0), new CellAddress(2, 0, 0), new CellAddress(0, 2, 0),
                         new CellAddress(2, 2, 0)
                     }) {
                Assert.AreEqual(CompositionCellState.Empty, result.Cell(corner).State, corner.ToString());
                Assert.IsTrue(result.Contradictions.Any(contradiction =>
                    contradiction.Cell == corner && contradiction.Reason == EmptiedDomainReason));
            }

            Assert.AreEqual(4, CountPlaced(result, CorridorPartName, 0));
            AssertEveryHorizontalFaceFits(arrangement, result);
        }

        [Test]
        public void EveryOneOfPartIsPlacedOnce() {
            var arrangement = Describe(5,
                new[] {
                    ClosedPart(FirstClosedPart), ClosedPart("one-of-first"), ClosedPart("one-of-second"),
                    ClosedPart("one-of-third")
                },
                new[] {
                    Stratum(new[] { Entry(FirstClosedPart) }, "one-of-first", "one-of-second"),
                    Stratum(new[] { Entry(FirstClosedPart) }, "one-of-third")
                });

            for (var seed = 0UL; seed < 20UL; seed++) {
                var result = ArrangementComposer.Compose(arrangement, seed);

                Assert.AreEqual(1, CountPlaced(result, "one-of-first", 0));
                Assert.AreEqual(1, CountPlaced(result, "one-of-second", 0));
                Assert.AreEqual(0, CountPlaced(result, "one-of-third", 0));
                Assert.AreEqual(1, CountPlaced(result, "one-of-third", 1));
                Assert.AreEqual(0, CountPlaced(result, "one-of-first", 1));
                Assert.IsFalse(result.Cells.Any(cell => cell.PartName != null && cell.PartName.StartsWith("one-of")
                                                        && cell.Address.column == 2 && cell.Address.row == 2));
                Assert.AreEqual(45, result.OrdinaryCellsAttempted);
                Assert.AreEqual(0, result.Contradictions.Count);
            }
        }

        [Test]
        public void AnUnknownPartNameIsAContradiction() {
            var rules = new CellRule[] {
                new FixedCellRule { cell = new CellAddress(0, 0, 0), partName = "missing-part", rotationSteps = 0 }
            };
            var arrangement = Describe(3, new[] { ClosedPart(FirstClosedPart) },
                new[] { Stratum(new[] { Entry(FirstClosedPart), Entry("missing-pool-part") }) }, null, rules);

            var result = ArrangementComposer.Compose(arrangement, 9UL);

            Assert.AreEqual(2, result.Contradictions.Count);
            Assert.IsTrue(result.Contradictions.Any(contradiction => contradiction.Reason.Contains("missing-part")));
            Assert.IsTrue(result.Contradictions.Any(contradiction =>
                contradiction.Reason.Contains("missing-pool-part")));
            Assert.AreEqual(FirstClosedPart, result.Cell(new CellAddress(0, 0, 0)).PartName);
            Assert.AreEqual(8, CountPlaced(result, FirstClosedPart, 0));
        }

        [Test]
        public void AnEvenGridSizeIsOneContradiction() {
            foreach (var gridSize in new[] { 4, 0, -3 }) {
                var result = ArrangementComposer.Compose(TwoPartPool(gridSize), 1UL);

                Assert.AreEqual(0, result.Cells.Count);
                Assert.AreEqual(1, result.Contradictions.Count);
                Assert.AreEqual(new CellAddress(0, 0, 0), result.Contradictions[0].Cell);
                Assert.IsFalse(string.IsNullOrEmpty(result.Contradictions[0].Reason));
            }
        }

        [Test]
        public void ANullArrangementThrows() {
            Assert.Throws<ArgumentNullException>(() => ArrangementComposer.Compose(null, 1UL));
        }

        [Test]
        public void ALiveSizedGridComposesWithinTwoSeconds() {
            var arrangement = LiveSized();
            ArrangementComposer.Compose(arrangement, 1UL);

            var stopwatch = Stopwatch.StartNew();
            var result = ArrangementComposer.Compose(arrangement, 2UL);
            stopwatch.Stop();

            Assert.AreEqual(3, result.StratumCount);
            Assert.AreEqual(147, result.Cells.Count);
            Assert.Less(stopwatch.ElapsedMilliseconds, 2000L);
            Assert.Pass("compose milliseconds " + stopwatch.ElapsedMilliseconds);
        }
    }
}
