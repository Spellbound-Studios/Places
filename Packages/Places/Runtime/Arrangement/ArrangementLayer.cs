// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public class ArrangementLayer : PlaceLayer {
        [SerializeField] private int gridSize = 7;
        [SerializeField] private List<PartDescription> parts = new();
        [SerializeField] private List<StratumDescription> strata = new();
        [SerializeField] private List<PoolEntry> centrePool = new();
        [SerializeReference] private List<CellRule> rules = new();
        [SerializeField] private ulong seed;

        public int GridSize => gridSize;
        public IReadOnlyList<PartDescription> Parts => parts;
        public IReadOnlyList<StratumDescription> Strata => strata;
        public IReadOnlyList<PoolEntry> CentrePool => centrePool;
        public IReadOnlyList<CellRule> Rules => rules;

        public ulong Seed {
            get => seed;
            set => seed = value;
        }

        public ArrangementDescription ToDescription() {
            return new ArrangementDescription(gridSize, parts, strata, centrePool, rules);
        }

        public bool ContainsCell(CellAddress cell) {
            return cell.column >= 0 && cell.column < gridSize && cell.row >= 0 && cell.row < gridSize
                && cell.stratum >= 0 && cell.stratum < strata.Count;
        }

        public override void Validate(ValidationReport report, string location) {
            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            if (gridSize < 1 || gridSize % 2 == 0) {
                report.Error(PlaceFindingCodes.GridSizeEven,
                    location + ".gridSize is " + gridSize + "; the grid size must be odd and at least 1",
                    location + ".gridSize");
            }

            var partNames = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < parts.Count; index++) {
                var part = parts[index];
                var partLocation = location + ".parts[" + index + "]";

                if (part == null || string.IsNullOrWhiteSpace(part.partName)) {
                    report.Error(PlaceFindingCodes.PartNameEmpty, partLocation + " has no part name", partLocation);
                } else if (!partNames.Add(part.partName)) {
                    report.Error(PlaceFindingCodes.PartNameDuplicate,
                        partLocation + " repeats the part name '" + part.partName + "'", partLocation);
                }
            }

            for (var stratumIndex = 0; stratumIndex < strata.Count; stratumIndex++) {
                var stratum = strata[stratumIndex];

                if (stratum == null) {
                    continue;
                }

                var stratumLocation = location + ".strata[" + stratumIndex + "]";
                CheckPool(stratum.pool, stratumLocation + ".pool");

                if (stratum.oneOfParts == null) {
                    continue;
                }

                for (var index = 0; index < stratum.oneOfParts.Count; index++) {
                    CheckRulePart(stratum.oneOfParts[index], stratumLocation + ".oneOfParts[" + index + "]");
                }
            }

            CheckPool(centrePool, location + ".centrePool");

            for (var ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++) {
                var ruleLocation = location + ".rules[" + ruleIndex + "]";

                switch (rules[ruleIndex]) {
                    case FixedCellRule fixedCell:
                        CheckRulePart(fixedCell.partName, ruleLocation + ".partName");
                        CheckCell(fixedCell.cell, ruleLocation + ".cell");
                        break;
                    case WeightedPoolRule weightedPool:
                        CheckCell(weightedPool.cell, ruleLocation + ".cell");
                        CheckPool(weightedPool.pool, ruleLocation + ".pool");
                        break;
                    case AllowedRegionRule allowedRegion:
                        CheckRulePart(allowedRegion.partName, ruleLocation + ".partName");
                        CheckCell(allowedRegion.region.minimum, ruleLocation + ".region.minimum");
                        CheckCell(allowedRegion.region.maximum, ruleLocation + ".region.maximum");
                        break;
                }
            }

            void CheckPool(List<PoolEntry> pool, string poolLocation) {
                if (pool == null) {
                    return;
                }

                for (var index = 0; index < pool.Count; index++) {
                    var entry = pool[index];

                    if (entry == null) {
                        continue;
                    }

                    var entryLocation = poolLocation + "[" + index + "]";

                    if (!partNames.Contains(entry.partName ?? string.Empty)) {
                        report.Error(PlaceFindingCodes.PoolPartUnknown,
                            entryLocation + " names the part '" + entry.partName + "', which this layer does not have",
                            entryLocation);
                    }

                    if (entry.weight < 0f) {
                        report.Error(PlaceFindingCodes.WeightNegative,
                            entryLocation + ".weight is "
                            + entry.weight.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + "; a weight is zero or more", entryLocation + ".weight");
                    }
                }
            }

            void CheckRulePart(string partName, string partLocation) {
                if (!partNames.Contains(partName ?? string.Empty)) {
                    report.Error(PlaceFindingCodes.RulePartUnknown,
                        partLocation + " names the part '" + partName + "', which this layer does not have",
                        partLocation);
                }
            }

            void CheckCell(CellAddress cell, string cellLocation) {
                if (ContainsCell(cell)) {
                    return;
                }

                report.Error(PlaceFindingCodes.RuleCellOutsideGrid,
                    cellLocation + " is " + cell + ", outside the " + gridSize + " by " + gridSize + " grid of "
                    + strata.Count + " strata", cellLocation);
            }
        }
    }
}
