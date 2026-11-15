// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Places.Arrangement;

namespace Spellbound.Places {
    public static class PlacesKit {
        public static CompositionResult Compose(ArrangementDescription arrangement, ulong seed) {
            return ArrangementComposer.Compose(arrangement, seed);
        }
    }
}
