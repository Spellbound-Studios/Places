// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public struct CellRegion {
        public CellAddress minimum;
        public CellAddress maximum;

        public bool Contains(CellAddress cell) {
            return cell.column >= minimum.column && cell.column <= maximum.column
                   && cell.row >= minimum.row && cell.row <= maximum.row
                   && cell.stratum >= minimum.stratum && cell.stratum <= maximum.stratum;
        }
    }
}
