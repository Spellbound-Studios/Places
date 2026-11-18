// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;
using Spellbound.Core.ReferenceFrames;

namespace Spellbound.Places.Owners {
    public sealed class PlaceCaptureContext {
        public PlaceCaptureContext(string layerName, ValidationReport report, ReferenceFrameId frame = default) {
            if (string.IsNullOrWhiteSpace(layerName)) {
                throw new ArgumentException("A captured layer needs a layer name", nameof(layerName));
            }

            LayerName = layerName;
            Report = report ?? throw new ArgumentNullException(nameof(report));
            Frame = frame;
        }

        public string LayerName { get; }

        public ValidationReport Report { get; }

        public ReferenceFrameId Frame { get; }
    }
}
