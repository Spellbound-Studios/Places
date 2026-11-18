// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;

namespace Spellbound.Places.Editor.Tests {
    [Serializable]
    public sealed class HarvestableTestLayer : PlaceLayer {
        public override void Validate(ValidationReport report, string location) {
        }

        public override bool IsHarvestable => true;
    }
}
