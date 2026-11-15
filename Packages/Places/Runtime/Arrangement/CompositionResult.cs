// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Spellbound.Places.Arrangement {
    public sealed class CompositionResult {
        private readonly CompositionCell[] _grid;

        public int GridSize { get; }
        public int StratumCount { get; }
        public IReadOnlyList<CompositionCell> Cells { get; }
        public IReadOnlyList<CellContradiction> Contradictions { get; }
        public int OrdinaryCellsAttempted { get; }
        public int OrdinaryCellsEmpty { get; }

        public CompositionResult(int gridSize, int stratumCount, IReadOnlyList<CompositionCell> cells,
            IReadOnlyList<CellContradiction> contradictions, int ordinaryCellsAttempted, int ordinaryCellsEmpty) {
            GridSize = Math.Max(0, gridSize);
            StratumCount = Math.Max(0, stratumCount);
            Cells = cells ?? Array.Empty<CompositionCell>();
            Contradictions = contradictions ?? Array.Empty<CellContradiction>();
            OrdinaryCellsAttempted = ordinaryCellsAttempted;
            OrdinaryCellsEmpty = ordinaryCellsEmpty;

            _grid = new CompositionCell[GridSize * GridSize * StratumCount];
            for (var stratum = 0; stratum < StratumCount; stratum++) {
                for (var row = 0; row < GridSize; row++) {
                    for (var column = 0; column < GridSize; column++) {
                        var address = new CellAddress(column, row, stratum);
                        _grid[IndexOf(address)] = new CompositionCell(address, CompositionCellState.Empty, null, 0);
                    }
                }
            }

            foreach (var cell in Cells) {
                if (IsInside(cell.Address)) {
                    _grid[IndexOf(cell.Address)] = cell;
                }
            }
        }

        public CompositionCell Cell(CellAddress address) {
            if (!IsInside(address)) {
                throw new ArgumentOutOfRangeException(nameof(address), address, null);
            }

            return _grid[IndexOf(address)];
        }

        public string ToText() {
            var lines = new List<string>();
            var row = new StringBuilder();

            for (var stratum = 0; stratum < StratumCount; stratum++) {
                lines.Add("stratum " + stratum);

                for (var rowIndex = GridSize - 1; rowIndex >= 0; rowIndex--) {
                    row.Clear();

                    for (var column = 0; column < GridSize; column++) {
                        if (column > 0) {
                            row.Append(' ');
                        }

                        row.Append(CellText(Cell(new CellAddress(column, rowIndex, stratum))));
                    }

                    lines.Add(row.ToString());
                }
            }

            lines.Add("contradictions: " + Contradictions.Count);

            foreach (var contradiction in Contradictions) {
                lines.Add(contradiction.Cell + ": " + contradiction.Reason);
            }

            return string.Join("\n", lines);
        }

        private static string CellText(CompositionCell cell) {
            switch (cell.State) {
                case CompositionCellState.Placed:
                    return cell.PartName + "@" + cell.RotationSteps;
                case CompositionCellState.VoidCentre:
                    return "#";
                default:
                    return ".";
            }
        }

        private bool IsInside(CellAddress address) {
            return address.column >= 0 && address.column < GridSize && address.row >= 0 && address.row < GridSize
                   && address.stratum >= 0 && address.stratum < StratumCount;
        }

        private int IndexOf(CellAddress address) {
            return (address.stratum * GridSize + address.row) * GridSize + address.column;
        }
    }
}
