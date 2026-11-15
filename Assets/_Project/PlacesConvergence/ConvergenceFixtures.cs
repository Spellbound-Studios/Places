// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace PlacesConvergence.Tests {
    public sealed class ConvergenceFixtures {
        public const int GridSize = 7;
        public const int StratumCount = 3;

        private readonly List<PartDescription> _parts = new();
        private readonly List<StratumDescription> _liveStrata = new();
        private readonly List<PoolEntry> _centrePool = new();

        public IReadOnlyList<PartDescription> Parts => _parts;
        public IReadOnlyList<StratumDescription> LiveStrata => _liveStrata;
        public IReadOnlyList<PoolEntry> CentrePool => _centrePool;

        public ConvergenceFixtures(string partsPath, string livePoolsPath) {
            ReadParts(partsPath);
            ReadLivePools(livePoolsPath);
        }

        public static ConvergenceFixtures FromProject() {
            var folder = Path.Combine(Application.dataPath, "_Project", "PlacesConvergence");
            return new ConvergenceFixtures(Path.Combine(folder, "Parts.txt"), Path.Combine(folder, "LivePools.txt"));
        }

        public ArrangementDescription LiveArrangement(IReadOnlyList<CellRule> rules) {
            return new ArrangementDescription(GridSize, _parts, _liveStrata, _centrePool, rules);
        }

        public ArrangementDescription CorpusArrangement(IReadOnlyList<CellRule> rules) {
            return new ArrangementDescription(GridSize, _parts, CorpusStrata(), _centrePool, rules);
        }

        public IReadOnlyList<StratumDescription> CorpusStrata() {
            var strata = new List<StratumDescription>();

            for (var stratum = 0; stratum < StratumCount; stratum++) {
                var description = new StratumDescription();

                foreach (var part in _parts) {
                    description.pool.Add(new PoolEntry { partName = part.partName, weight = 1f });
                }

                strata.Add(description);
            }

            return strata;
        }

        private void ReadParts(string path) {
            foreach (var line in File.ReadAllLines(path)) {
                var fields = Fields(line);

                if (fields.Length == 0) {
                    continue;
                }

                if (fields.Length != 7) {
                    throw new FormatException("a part line needs a name and six faces: " + line);
                }

                _parts.Add(new PartDescription {
                    partName = fields[0],
                    faces = new PartFaces(Face(fields[1]), Face(fields[2]), Face(fields[3]), Face(fields[4]),
                        Face(fields[5]), Face(fields[6]))
                });
            }
        }

        private void ReadLivePools(string path) {
            StratumDescription current = null;

            foreach (var line in File.ReadAllLines(path)) {
                var fields = Fields(line);

                if (fields.Length == 0) {
                    continue;
                }

                switch (fields[0]) {
                    case "centre":
                        _centrePool.Add(new PoolEntry { partName = fields[1], weight = Weight(fields[2]) });
                        break;
                    case "stratum":
                        current = new StratumDescription();
                        _liveStrata.Add(current);
                        break;
                    case "pool":
                        RequireStratum(current, line).pool.Add(new PoolEntry {
                            partName = fields[1],
                            weight = Weight(fields[2])
                        });
                        break;
                    case "oneof":
                        RequireStratum(current, line).oneOfParts.Add(fields[1]);
                        break;
                    default:
                        throw new FormatException("an unknown live pool line: " + line);
                }
            }
        }

        private static StratumDescription RequireStratum(StratumDescription stratum, string line) {
            if (stratum == null) {
                throw new FormatException("a pool line before the first stratum: " + line);
            }

            return stratum;
        }

        private static string[] Fields(string line) {
            return line.Split(new[] { ' ', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static SocketKind Face(string text) {
            return (SocketKind)int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static float Weight(string text) {
            return float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
