// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;

namespace Spellbound.Places.Arrangement {
    public static class SocketKindText {
        private static readonly SocketKind[] FlagOrder = {
            SocketKind.NoConnection, SocketKind.Connection, SocketKind.SealedConnection
        };

        public static string Of(SocketKind kind) {
            if ((int)kind == -1) {
                return "Any";
            }

            if ((int)kind == 0) {
                return "NoConnection";
            }

            var names = new List<string>(FlagOrder.Length);

            foreach (var flag in FlagOrder) {
                if ((kind & flag) != 0) {
                    names.Add(flag.ToString());
                }
            }

            return string.Join("|", names);
        }
    }
}
