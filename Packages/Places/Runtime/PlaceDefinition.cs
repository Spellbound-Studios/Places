// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Places {
    [CreateAssetMenu(fileName = "Place", menuName = "Spellbound/Places/Place")]
    public class PlaceDefinition : ScriptableObject, IAuthoredDefinition {
        [SerializeReference] private List<PlaceLayer> layers = new();

        public IReadOnlyList<PlaceLayer> Layers => layers;

        public TLayer AddLayer<TLayer>(TLayer layer) where TLayer : PlaceLayer {
            if (layer == null) {
                throw new ArgumentNullException(nameof(layer));
            }

            layers.Add(layer);
            return layer;
        }

        public void Validate(ValidationReport report) {
            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }
        }
    }
}
