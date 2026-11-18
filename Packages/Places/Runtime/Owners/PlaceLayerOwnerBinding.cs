// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Owners {
    internal abstract class PlaceLayerOwnerBinding {
        public abstract Type LayerType { get; }

        public abstract void Stamp(PlaceLayer layer, PlaceStampContext context);

        public abstract PlaceLayer Capture(PlaceCaptureContext context);
    }
}
