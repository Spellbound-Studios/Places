// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public sealed class FixedCellRule : CellRule {
        public CellAddress cell;
        public string partName;
        public int rotationSteps = -1;
    }
}
