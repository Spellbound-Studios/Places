// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public sealed class AllowedRegionRule : CellRule {
        public string partName;
        public CellRegion region;
    }
}
