// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Places {
    public readonly struct PlacementPoint {
        public PlacementPoint(string layerName, string location, Vector3 position) {
            LayerName = layerName;
            Location = location;
            Position = position;
        }

        public string LayerName { get; }

        public string Location { get; }

        public Vector3 Position { get; }
    }
}
