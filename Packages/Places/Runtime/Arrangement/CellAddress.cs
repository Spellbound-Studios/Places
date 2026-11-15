// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public struct CellAddress : IEquatable<CellAddress> {
        public int column;
        public int row;
        public int stratum;

        public CellAddress(int column, int row, int stratum) {
            this.column = column;
            this.row = row;
            this.stratum = stratum;
        }

        public bool Equals(CellAddress other) {
            return column == other.column && row == other.row && stratum == other.stratum;
        }

        public override bool Equals(object other) {
            return other is CellAddress address && Equals(address);
        }

        public override int GetHashCode() {
            return HashCode.Combine(column, row, stratum);
        }

        public override string ToString() {
            return column + "," + row + "," + stratum;
        }

        public static bool operator ==(CellAddress first, CellAddress second) {
            return first.Equals(second);
        }

        public static bool operator !=(CellAddress first, CellAddress second) {
            return !first.Equals(second);
        }
    }
}
