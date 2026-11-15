// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    public static class ArrangementComposer {
        public static CompositionResult Compose(ArrangementDescription arrangement, ulong seed) {
            if (arrangement == null) {
                throw new ArgumentNullException(nameof(arrangement));
            }

            var gridSize = arrangement.GridSize;

            if (gridSize < 1 || gridSize % 2 == 0) {
                var contradiction = new CellContradiction(new CellAddress(0, 0, 0),
                    "the grid size " + gridSize + " is not an odd number of at least 1");
                return new CompositionResult(gridSize, 0, Array.Empty<CompositionCell>(), new[] { contradiction }, 0,
                    0);
            }

            return new ArrangementComposition(arrangement, seed).Compose();
        }
    }
}
