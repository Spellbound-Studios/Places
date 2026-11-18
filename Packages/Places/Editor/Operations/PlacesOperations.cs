// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Globalization;
using Spellbound.Core.Operations;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor {
    public static class PlacesOperations {
        [Operation("compose",
            "Composes every arrangement layer of a place at a seed and prints each stratum's grid, the contradictions"
            + " count and one line per contradiction with its reason; the asset's own seed is left as it is.",
            OperationEffect.Reads)]
        public static OperationResult Compose(OperationContext context,
            [Argument("The place, as asset:<path> or asset:<name>.")] PlaceDefinition target,
            [Argument("The seed to compose with.")] ulong seed) {
            var lines = new List<string>();
            var seedText = seed.ToString(CultureInfo.InvariantCulture);

            for (var index = 0; index < target.Layers.Count; index++) {
                if (!(target.Layers[index] is ArrangementLayer layer)) {
                    continue;
                }

                var result = PlacesKit.Compose(layer.ToDescription(), seed);
                lines.Add(LayerLine(index, layer) + " seed " + seedText);
                lines.Add(result.ToText());
                PlacesObservations.CompositionContradictions.Add(context.Observations, result.Contradictions.Count);
            }

            return lines.Count == 0 ? NoArrangementLayer(target) : OperationResult.Success(string.Join("\n", lines));
        }

        [Operation("parts",
            "Lists the parts of every arrangement layer of a place with their six faces: east, west, up, down,"
            + " north and south.", OperationEffect.Reads)]
        public static OperationResult Parts(OperationContext context,
            [Argument("The place, as asset:<path> or asset:<name>.")] PlaceDefinition target,
            [Argument("How many parts to skip from the start of the list.")] int offset = 0,
            [Argument("How many parts to list at most; zero lists every one.")] int limit = 0) {
            if (offset < 0 || limit < 0) {
                return OperationResult.Invalid("--offset and --limit are zero or more");
            }

            var lines = new List<string>();
            var partIndex = 0;
            var listed = 0;

            for (var index = 0; index < target.Layers.Count; index++) {
                if (!(target.Layers[index] is ArrangementLayer layer)) {
                    continue;
                }

                lines.Add(LayerLine(index, layer) + ": " + layer.Parts.Count.ToString(CultureInfo.InvariantCulture)
                    + " parts");

                foreach (var part in layer.Parts) {
                    if (partIndex++ < offset || (limit > 0 && listed >= limit)) {
                        continue;
                    }

                    lines.Add(PartLine(part));
                    listed++;
                }
            }

            return lines.Count == 0 ? NoArrangementLayer(target) : OperationResult.Success(string.Join("\n", lines));
        }

        private static string LayerLine(int index, ArrangementLayer layer) {
            return "layers[" + index.ToString(CultureInfo.InvariantCulture) + "] " + layer.GetType().Name + " '"
                + layer.LayerName + "'";
        }

        private static string PartLine(PartDescription part) {
            var faces = part.faces;
            return part.partName + " east " + SocketKindText.Of(faces.east) + " west " + SocketKindText.Of(faces.west)
                + " up " + SocketKindText.Of(faces.up) + " down " + SocketKindText.Of(faces.down) + " north "
                + SocketKindText.Of(faces.north) + " south " + SocketKindText.Of(faces.south);
        }

        private static OperationResult NoArrangementLayer(PlaceDefinition target) {
            return OperationResult.Invalid(target.name + " holds no arrangement layer.");
        }
    }
}
