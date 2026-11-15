// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    internal readonly struct RequiredFaces {
        public SocketKind North { get; }
        public SocketKind East { get; }
        public SocketKind South { get; }
        public SocketKind West { get; }
        public SocketKind Up { get; }
        public SocketKind Down { get; }

        public RequiredFaces(SocketKind north, SocketKind east, SocketKind south, SocketKind west, SocketKind up,
            SocketKind down) {
            North = north;
            East = east;
            South = south;
            West = west;
            Up = up;
            Down = down;
        }

        public SocketKind Horizontal(HorizontalDirection direction) {
            switch (direction) {
                case HorizontalDirection.North:
                    return North;
                case HorizontalDirection.East:
                    return East;
                case HorizontalDirection.South:
                    return South;
                case HorizontalDirection.West:
                    return West;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
