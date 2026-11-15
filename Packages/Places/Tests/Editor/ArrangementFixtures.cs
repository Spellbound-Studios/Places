// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    internal static class ArrangementFixtures {
        public const string FirstClosedPart = "closed-first";
        public const string SecondClosedPart = "closed-second";
        public const string CorridorPartName = "corridor";
        public const string DeadEndPartName = "dead-end";
        public const string OpenAbovePartName = "open-above";
        public const string ShaftDownPartName = "shaft-down";
        public const string ShaftUpPartName = "shaft-up";

        public const SocketKind Everything = (SocketKind)(-1);

        public static PartFaces ClosedFaces() {
            return new PartFaces(SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection,
                SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection);
        }

        public static PartDescription Part(string partName, PartFaces faces) {
            return new PartDescription { partName = partName, faces = faces };
        }

        public static PartDescription ClosedPart(string partName) {
            return Part(partName, ClosedFaces());
        }

        public static PartDescription CorridorPart() {
            var faces = ClosedFaces();
            faces.east = SocketKind.Connection;
            faces.west = SocketKind.Connection;
            return Part(CorridorPartName, faces);
        }

        public static PartDescription DeadEndPart() {
            var faces = ClosedFaces();
            faces.west = SocketKind.Connection;
            return Part(DeadEndPartName, faces);
        }

        public static PartDescription OpenAbovePart() {
            var faces = ClosedFaces();
            faces.up = Everything;
            return Part(OpenAbovePartName, faces);
        }

        public static PartDescription ShaftDownPart() {
            var faces = ClosedFaces();
            faces.down = SocketKind.Connection;
            return Part(ShaftDownPartName, faces);
        }

        public static PartDescription ShaftUpPart() {
            var faces = ClosedFaces();
            faces.up = SocketKind.Connection;
            return Part(ShaftUpPartName, faces);
        }

        public static PoolEntry Entry(string partName, float weight = 1f) {
            return new PoolEntry { partName = partName, weight = weight };
        }

        public static StratumDescription Stratum(IEnumerable<PoolEntry> pool, params string[] oneOfParts) {
            return new StratumDescription {
                pool = new List<PoolEntry>(pool),
                oneOfParts = new List<string>(oneOfParts)
            };
        }

        public static ArrangementDescription Describe(int gridSize, PartDescription[] parts,
            StratumDescription[] strata, PoolEntry[] centrePool = null, CellRule[] rules = null) {
            return new ArrangementDescription(gridSize, parts, strata, centrePool, rules);
        }

        public static ArrangementDescription TwoPartPool(int gridSize, CellRule[] rules = null) {
            return Describe(gridSize,
                new[] { ClosedPart(FirstClosedPart), ClosedPart(SecondClosedPart) },
                new[] { Stratum(new[] { Entry(FirstClosedPart), Entry(SecondClosedPart) }) },
                null, rules);
        }

        public static ArrangementDescription LiveSized() {
            var kinds = new[] {
                SocketKind.NoConnection, SocketKind.Connection, SocketKind.NoConnection | SocketKind.Connection
            };
            var parts = new PartDescription[12];

            for (var index = 0; index < parts.Length; index++) {
                var faces = new PartFaces(kinds[index % 3], kinds[index / 3 % 3], kinds[2], kinds[(index + 1) % 3],
                    kinds[(index + 2) % 3], kinds[index / 4 % 3]);
                parts[index] = Part("part-" + index, faces);
            }

            var pool = new List<PoolEntry>();

            for (var index = 0; index < 8; index++) {
                pool.Add(Entry("part-" + index, index + 1));
            }

            var strata = new[] {
                Stratum(pool, "part-8"),
                Stratum(pool, "part-9", "part-10"),
                Stratum(pool, "part-11")
            };

            return Describe(7, parts, strata, new[] { Entry("part-0"), Entry("part-1", 2f) });
        }
    }
}
