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

        public override void Validate(ValidationReport report, string location) {
            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }
        }
    }
}
