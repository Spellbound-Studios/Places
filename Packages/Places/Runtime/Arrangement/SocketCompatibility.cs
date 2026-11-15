// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Arrangement {
    public static class SocketCompatibility {
        public static bool AreCompatible(SocketKind first, SocketKind second) {
            return (Normalise(first) & Normalise(second)) != 0;
        }

        public static SocketKind Normalise(SocketKind socket) {
            return socket == 0 ? SocketKind.NoConnection : socket;
        }
    }
}
