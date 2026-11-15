// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Spellbound.Core.Simulation;
using Spellbound.Places;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace PlacesConvergence.Tests {
    public sealed class ConvergenceMeasurement {
        public const int SeedCount = 50;
        public const string ReportFileName = "places-convergence.txt";

        public const string Header =
            "case fixed meanEmptyRate worstEmptyRate meanContradictions cleanShare medianMilliseconds";

        public static readonly IReadOnlyList<int> FixedCounts = new[] { 0, 5, 10, 15, 20, 25, 30 };

        private readonly ConvergenceFixtures _fixtures;

        public int CompletedRuns { get; private set; }

        public ConvergenceMeasurement(ConvergenceFixtures fixtures) {
            _fixtures = fixtures ?? throw new ArgumentNullException(nameof(fixtures));
        }

        public static string ReportPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs",
            ReportFileName));

        public IReadOnlyList<string> Run() {
            CompletedRuns = 0;
            var lines = new List<string> { Header };
            var corpusStrata = _fixtures.CorpusStrata();

            foreach (var fixedCount in FixedCounts) {
                lines.Add(MeasureCase("live", fixedCount, _fixtures.LiveStrata, _fixtures.LiveArrangement));
            }

            foreach (var fixedCount in FixedCounts) {
                lines.Add(MeasureCase("corpus", fixedCount, corpusStrata, _fixtures.CorpusArrangement));
            }

            return lines;
        }

        public static string WriteReport(IReadOnlyList<string> lines) {
            var path = ReportPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, string.Join("\n", lines) + "\n");
            return path;
        }

        private string MeasureCase(string caseName, int fixedCount, IReadOnlyList<StratumDescription> strata,
            Func<IReadOnlyList<CellRule>, ArrangementDescription> arrangementOf) {
            var emptyRateSum = 0.0;
            var worstEmptyRate = 0.0;
            var contradictionSum = 0;
            var cleanRuns = 0;
            var milliseconds = new List<double>();

            for (var seed = 1; seed <= SeedCount; seed++) {
                var pins = Pins(strata, fixedCount, seed);
                var arrangement = arrangementOf(pins);
                var stopwatch = Stopwatch.StartNew();
                var result = PlacesKit.Compose(arrangement, (ulong)seed);
                stopwatch.Stop();

                if (result == null) {
                    continue;
                }

                CompletedRuns++;
                milliseconds.Add(stopwatch.Elapsed.TotalMilliseconds);

                var emptyRate = result.OrdinaryCellsAttempted > 0
                    ? (double)result.OrdinaryCellsEmpty / result.OrdinaryCellsAttempted
                    : 0.0;
                emptyRateSum += emptyRate;
                worstEmptyRate = Math.Max(worstEmptyRate, emptyRate);
                contradictionSum += result.Contradictions.Count;

                if (result.Contradictions.Count == 0) {
                    cleanRuns++;
                }
            }

            return string.Join(" ", caseName, fixedCount.ToString(CultureInfo.InvariantCulture),
                Format(emptyRateSum / SeedCount, "F4"), Format(worstEmptyRate, "F4"),
                Format((double)contradictionSum / SeedCount, "F4"), Format((double)cleanRuns / SeedCount, "F4"),
                Format(Median(milliseconds), "F1"));
        }

        private static List<CellRule> Pins(IReadOnlyList<StratumDescription> strata, int fixedCount, int seed) {
            var stream = new SeededStream((ulong)seed).Fork((ulong)(1000 + fixedCount));
            var gridSize = ConvergenceFixtures.GridSize;
            var centre = gridSize / 2;
            var cellsPerStratum = gridSize * gridSize;
            var taken = new HashSet<int>();
            var pins = new List<CellRule>();

            while (pins.Count < fixedCount) {
                var index = stream.NextInteger(0, cellsPerStratum * strata.Count);
                var stratum = index / cellsPerStratum;
                var column = index % cellsPerStratum % gridSize;
                var row = index % cellsPerStratum / gridSize;

                if ((column == centre && row == centre) || !taken.Add(index)) {
                    continue;
                }

                var pool = strata[stratum].pool;
                var entry = pool[stream.NextInteger(0, pool.Count)];
                pins.Add(new FixedCellRule {
                    cell = new CellAddress(column, row, stratum),
                    partName = entry.partName,
                    rotationSteps = -1
                });
            }

            return pins;
        }

        private static double Median(List<double> values) {
            if (values.Count == 0) {
                return 0.0;
            }

            values.Sort();
            var middle = values.Count / 2;
            return values.Count % 2 == 1 ? values[middle] : (values[middle - 1] + values[middle]) / 2.0;
        }

        private static string Format(double value, string format) {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
