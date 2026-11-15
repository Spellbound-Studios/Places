// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Arrangement {
    public sealed class ArrangementDescription {
        public int GridSize { get; }
        public IReadOnlyList<PartDescription> Parts { get; }
        public IReadOnlyList<StratumDescription> Strata { get; }
        public IReadOnlyList<PoolEntry> CentrePool { get; }
        public IReadOnlyList<CellRule> Rules { get; }

        public ArrangementDescription(int gridSize, IReadOnlyList<PartDescription> parts,
            IReadOnlyList<StratumDescription> strata, IReadOnlyList<PoolEntry> centrePool,
            IReadOnlyList<CellRule> rules) {
            GridSize = gridSize;
            Parts = parts ?? Array.Empty<PartDescription>();
            Strata = strata ?? Array.Empty<StratumDescription>();
            CentrePool = centrePool ?? Array.Empty<PoolEntry>();
            Rules = rules ?? Array.Empty<CellRule>();
        }

        public bool TryFindPart(string partName, out PartDescription part) {
            if (partName != null) {
                foreach (var candidate in Parts) {
                    if (candidate != null && string.Equals(candidate.partName, partName, StringComparison.Ordinal)) {
                        part = candidate;
                        return true;
                    }
                }
            }

            part = null;
            return false;
        }
    }
}
