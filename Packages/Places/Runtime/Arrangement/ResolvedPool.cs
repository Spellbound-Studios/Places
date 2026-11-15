// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Arrangement {
    internal sealed class ResolvedPool {
        private readonly int[] _parts;
        private readonly float[] _weights;

        public ResolvedPool(int[] parts, float[] weights) {
            _parts = parts;
            _weights = weights;
        }

        public int Count => _parts.Length;
        public IReadOnlyList<int> Parts => _parts;
        public IReadOnlyList<float> Weights => _weights;

        public CellDomain DomainAt(CellAddress cell, Func<int, CellAddress, bool> isPermitted) {
            var permittedCount = 0;

            for (var entry = 0; entry < _parts.Length; entry++) {
                if (isPermitted(_parts[entry], cell)) {
                    permittedCount++;
                }
            }

            if (permittedCount == _parts.Length) {
                return new CellDomain(_parts, _weights);
            }

            var parts = new int[permittedCount];
            var weights = new float[permittedCount];
            var next = 0;

            for (var entry = 0; entry < _parts.Length; entry++) {
                if (isPermitted(_parts[entry], cell)) {
                    parts[next] = _parts[entry];
                    weights[next] = _weights[entry];
                    next++;
                }
            }

            return new CellDomain(parts, weights);
        }
    }
}
