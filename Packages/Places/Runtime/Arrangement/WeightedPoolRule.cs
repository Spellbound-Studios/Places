// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public sealed class WeightedPoolRule : CellRule {
        public CellAddress cell;
        public List<PoolEntry> pool = new();
    }
}
