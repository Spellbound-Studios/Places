// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places {
    [Serializable]
    public sealed class NestedPlaceLayer : PlaceLayer {
        [SerializeField] private PlaceDefinition place;
        [SerializeField] private string arrangementLayerName = "";
        [SerializeField] private CellAddress cell;

        public PlaceDefinition Place {
            get => place;
            set => place = value;
        }

        public string ArrangementLayerName {
            get => arrangementLayerName;
            set => arrangementLayerName = value;
        }

        public CellAddress Cell {
            get => cell;
            set => cell = value;
        }

        public override void Validate(ValidationReport report, string location) {
            if (report == null) {
                throw new ArgumentNullException(nameof(report));
            }

            if (place == null) {
                var placeLocation = location + ".place";
                report.Error(PlaceFindingCodes.NestingPlaceEmpty, placeLocation + " names no place to nest",
                    placeLocation);
            }
        }
    }
}
