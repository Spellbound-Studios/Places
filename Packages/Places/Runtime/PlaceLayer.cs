// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Places {
    [Serializable]
    public abstract class PlaceLayer {
        [SerializeField] private string layerName;

        public string LayerName {
            get => layerName;
            set => layerName = value;
        }

        public abstract void Validate(ValidationReport report, string location);

        public virtual bool IsHarvestable => false;

        public virtual IEnumerable<PlacementPoint> PlacementPoints() => Array.Empty<PlacementPoint>();

        public virtual void CollectLayerReferences(List<PlaceLayerReference> references, string location) {
        }

        public virtual void RenameLayerReferences(string previousName, string newName) {
        }
    }
}
