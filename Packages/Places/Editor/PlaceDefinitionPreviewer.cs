// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring.Editor;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor {
    public sealed class PlaceDefinitionPreviewer : DefinitionPreviewer<PlaceDefinition> {
        private const string ContradictionsLine = "contradictions: ";

        protected override DefinitionPreview Preview(PlaceDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            var layers = definition.Layers;
            var lines = new List<string>();

            for (var index = 0; index < layers.Count; index++) {
                var location = "layers[" + index + "]";
                var layer = layers[index];

                if (layer == null) {
                    lines.Add(location + " (carried or empty)");
                    continue;
                }

                lines.Add(location + " " + layer.GetType().Name + " '" + layer.LayerName + "'");

                if (layer is ArrangementLayer arrangement) {
                    AddGrid(arrangement, lines);
                }
            }

            return new DefinitionPreview(definition.name + ": " + layers.Count + " layers", lines: lines);
        }

        private static void AddGrid(ArrangementLayer arrangement, List<string> lines) {
            var composed = PlacesKit.Compose(arrangement.ToDescription(), arrangement.Seed);

            foreach (var line in composed.ToText().Split('\n')) {
                lines.Add(line);

                if (line.StartsWith(ContradictionsLine, StringComparison.Ordinal)) {
                    return;
                }
            }
        }
    }
}
