// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core;
using Spellbound.Core.Observations;

namespace Spellbound.Places {
    public static class PlacesObservations {
        public static readonly Counter CompositionContradictions =
            CoreKit.Observations.Counter("places.compose.contradictions",
                "A composition left cells empty because no part fitted them; the amount added is the count of"
                + " contradictions of one composition.");
    }
}
