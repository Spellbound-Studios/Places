// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    [Serializable]
    public sealed class PointsTestLayer : PlaceLayer {
        public override void Validate(ValidationReport report, string location) {
        }

        public override IEnumerable<PlacementPoint> PlacementPoints() {
            yield return new PlacementPoint(LayerName, "deposits[0]", new Vector3(1f, 0f, 0f));
            yield return new PlacementPoint(LayerName, "deposits[1]", new Vector3(2f, 0f, 0f));
        }
    }
}
