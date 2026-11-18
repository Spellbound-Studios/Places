// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Editor {
    public interface IPlaceLayerInspectorActions {
        Type LayerType { get; }

        IReadOnlyList<PlaceLayerInspectorAction> Actions(PlaceDefinition place, int layerIndex);
    }
}
