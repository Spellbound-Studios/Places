// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Simulation;

namespace Spellbound.Places.Arrangement {
    internal static class WeightedPick {
        internal static int Pick(SeededStream stream, IReadOnlyList<float> weights) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }

            if (weights == null) {
                throw new ArgumentNullException(nameof(weights));
            }

            if (weights.Count == 0) {
                throw new ArgumentException("A pick needs at least one weight.", nameof(weights));
            }

            var total = 0.0;

            for (var index = 0; index < weights.Count; index++) {
                total += Math.Max(0f, weights[index]);
            }

            if (total <= 0.0) {
                return stream.NextInteger(0, weights.Count);
            }

            var roll = stream.NextDouble() * total;
            var running = 0.0;
            var lastWeighted = 0;

            for (var index = 0; index < weights.Count; index++) {
                var weight = Math.Max(0f, weights[index]);

                if (weight <= 0f) {
                    continue;
                }

                lastWeighted = index;
                running += weight;

                if (roll < running) {
                    return index;
                }
            }

            return lastWeighted;
        }
    }
}
