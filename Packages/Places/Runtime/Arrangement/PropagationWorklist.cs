// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;

namespace Spellbound.Places.Arrangement {
    internal sealed class PropagationWorklist {
        private readonly Queue<int> _cells = new();
        private readonly bool[] _queued;

        public PropagationWorklist(int cellCount) {
            _queued = new bool[cellCount];
        }

        public void Enqueue(int cellIndex) {
            if (cellIndex < 0 || _queued[cellIndex]) {
                return;
            }

            _queued[cellIndex] = true;
            _cells.Enqueue(cellIndex);
        }

        public bool TryDequeue(out int cellIndex) {
            if (_cells.Count == 0) {
                cellIndex = -1;
                return false;
            }

            cellIndex = _cells.Dequeue();
            _queued[cellIndex] = false;
            return true;
        }
    }
}
