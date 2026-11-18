// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Owners {
    public interface IPlaceLayerOwner<TLayer> where TLayer : PlaceLayer {
        void Stamp(TLayer layer, PlaceStampContext context);

        TLayer Capture(PlaceCaptureContext context);
    }
}
