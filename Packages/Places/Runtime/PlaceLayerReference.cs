// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places {
    public readonly struct PlaceLayerReference {
        public PlaceLayerReference(string sourceLocation, string targetLayerName) {
            SourceLocation = sourceLocation;
            TargetLayerName = targetLayerName;
        }

        public string SourceLocation { get; }

        public string TargetLayerName { get; }
    }
}
