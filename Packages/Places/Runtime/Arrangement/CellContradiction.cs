// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Arrangement {
    public sealed class CellContradiction {
        public CellAddress Cell { get; }
        public string Reason { get; }

        public CellContradiction(CellAddress cell, string reason) {
            Cell = cell;
            Reason = reason;
        }
    }
}
