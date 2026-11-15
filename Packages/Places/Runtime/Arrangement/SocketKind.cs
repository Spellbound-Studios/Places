// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    [Flags]
    public enum SocketKind {
        NoConnection = 1,
        Connection = 2,
        SealedConnection = 4
    }
}
