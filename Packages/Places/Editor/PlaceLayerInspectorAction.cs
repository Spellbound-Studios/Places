// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Editor {
    public sealed class PlaceLayerInspectorAction {
        private readonly Action<PlaceDefinition, int> _invoke;

        public PlaceLayerInspectorAction(string label, Action<PlaceDefinition, int> invoke) {
            if (string.IsNullOrWhiteSpace(label)) {
                throw new ArgumentException("An inspector action needs a label", nameof(label));
            }

            Label = label;
            _invoke = invoke ?? throw new ArgumentNullException(nameof(invoke));
        }

        public string Label { get; }

        public void Invoke(PlaceDefinition place, int layerIndex) {
            _invoke(place, layerIndex);
        }
    }
}
