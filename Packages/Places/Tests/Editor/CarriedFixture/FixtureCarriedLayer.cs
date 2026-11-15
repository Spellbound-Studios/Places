// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;

namespace Spellbound.Places.Tests.CarriedFixture {
    [Serializable]
    public sealed class FixtureCarriedLayer : PlaceLayer {
        public string label;
        public int weight;

        public override void Validate(ValidationReport report, string location) {
        }
    }
}
