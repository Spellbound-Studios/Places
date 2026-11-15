// Copyright (c) Spellbound Studios. All rights reserved.

using System;
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
    }
}
