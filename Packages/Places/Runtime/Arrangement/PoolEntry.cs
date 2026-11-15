// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public sealed class PoolEntry {
        public string partName;
        [Min(0f)] public float weight = 1f;
    }
}
