// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Editor.Tests {
    [PlaceLayerInspectorAction]
    public sealed class TestLayerInspectorActions : IPlaceLayerInspectorActions {
        public const string ActionLabel = "Test action";

        public Type LayerType => typeof(OwnedTestLayer);

        public IReadOnlyList<PlaceLayerInspectorAction> Actions(PlaceDefinition place, int layerIndex) {
            return new[] { new PlaceLayerInspectorAction(ActionLabel, (target, index) => { }) };
        }
    }
}
