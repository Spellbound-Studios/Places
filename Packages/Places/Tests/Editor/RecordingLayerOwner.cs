// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Places.Owners;

namespace Spellbound.Places.Editor.Tests {
    public sealed class RecordingLayerOwner : IPlaceLayerOwner<OwnedTestLayer> {
        private readonly List<OwnedTestLayer> _stamped = new();

        public IReadOnlyList<OwnedTestLayer> Stamped => _stamped;

        public PlaceStampContext LastStampContext { get; private set; }

        public PlaceCaptureContext LastCaptureContext { get; private set; }

        public void Stamp(OwnedTestLayer layer, PlaceStampContext context) {
            _stamped.Add(layer);
            LastStampContext = context;
        }

        public OwnedTestLayer Capture(PlaceCaptureContext context) {
            LastCaptureContext = context;
            return new OwnedTestLayer { LayerName = context.LayerName };
        }
    }
}
