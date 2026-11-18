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

            var lines = new List<string>();
            AddLayers(definition, new List<PlaceDefinition>(), definition.NestingDepthLimit, lines);

            return new DefinitionPreview(definition.name + ": " + definition.Layers.Count + " layers", lines: lines);
        }

        private static void AddLayers(PlaceDefinition place, List<PlaceDefinition> path, int depthLimit,
            List<string> lines) {
            var indent = new string(' ', path.Count * 2);

            if (path.Contains(place)) {
                lines.Add(indent + "(cycle: " + place.name + ")");
                return;
            }

            if (path.Count > 0 && path.Count > depthLimit) {
                lines.Add(indent + "(depth limit " + depthLimit + " reached)");
                return;
            }

            path.Add(place);
            var layers = place.Layers;

            for (var index = 0; index < layers.Count; index++) {
                var location = indent + "layers[" + index + "]";
                var layer = layers[index];

                if (layer == null) {
                    lines.Add(location + " (carried or empty)");
                    continue;
                }

                var line = location + " " + layer.GetType().Name + " '" + layer.LayerName + "'";

                if (layer is NestedPlaceLayer nested) {
                    lines.Add(line + " nests " + (nested.Place == null ? "nothing" : nested.Place.name));

                    if (nested.Place != null) {
                        AddLayers(nested.Place, path, depthLimit, lines);
                    }

                    continue;
                }

                lines.Add(line);

                if (path.Count == 1 && layer is ArrangementLayer arrangement) {
                    AddGrid(arrangement, lines);
                }
            }

            path.RemoveAt(path.Count - 1);
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
