// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;
using Spellbound.Core.ReferenceFrames;

namespace Spellbound.Places.Owners {
    public sealed class PlaceStampContext {
        private readonly PlaceLayerOwnerRegistry _owners;

        public PlaceStampContext(ValidationReport report, ReferenceFrameId frame = default, bool preview = false,
            PlaceLayerOwnerRegistry owners = null) {
            Report = report ?? throw new ArgumentNullException(nameof(report));
            Frame = frame;
            Preview = preview;
            _owners = owners;
        }

        public ValidationReport Report { get; }

        public ReferenceFrameId Frame { get; }

        public bool Preview { get; }

        public PlaceLayerOwnerRegistry Owners => _owners ?? PlaceLayerOwnerRegistry.Default;
    }
}
