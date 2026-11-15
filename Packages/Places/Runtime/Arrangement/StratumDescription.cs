// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public sealed class StratumDescription {
        public List<PoolEntry> pool = new();
        public List<string> oneOfParts = new();
    }
}
