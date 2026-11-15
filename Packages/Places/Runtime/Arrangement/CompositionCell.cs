// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Arrangement {
    public readonly struct CompositionCell {
        public CellAddress Address { get; }
        public CompositionCellState State { get; }
        public string PartName { get; }
        public int RotationSteps { get; }

        public CompositionCell(CellAddress address, CompositionCellState state, string partName, int rotationSteps) {
            Address = address;
            State = state;
            PartName = partName;
            RotationSteps = rotationSteps;
        }
    }
}
