// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Owners {
    internal sealed class TypedPlaceLayerOwnerBinding<TLayer> : PlaceLayerOwnerBinding where TLayer : PlaceLayer {
        private readonly IPlaceLayerOwner<TLayer> _owner;

        public TypedPlaceLayerOwnerBinding(IPlaceLayerOwner<TLayer> owner) {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public override Type LayerType => typeof(TLayer);

        public override void Stamp(PlaceLayer layer, PlaceStampContext context) {
            _owner.Stamp((TLayer)layer, context);
        }

        public override PlaceLayer Capture(PlaceCaptureContext context) {
            return _owner.Capture(context);
        }
    }
}
