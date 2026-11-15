// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Places.Arrangement {
    [Serializable]
    public struct PartFaces {
        public SocketKind east;
        public SocketKind west;
        public SocketKind up;
        public SocketKind down;
        public SocketKind north;
        public SocketKind south;

        public PartFaces(SocketKind east, SocketKind west, SocketKind up, SocketKind down, SocketKind north,
            SocketKind south) {
            this.east = east;
            this.west = west;
            this.up = up;
            this.down = down;
            this.north = north;
            this.south = south;
        }

        public SocketKind Horizontal(HorizontalDirection direction) {
            switch (direction) {
                case HorizontalDirection.North:
                    return north;
                case HorizontalDirection.East:
                    return east;
                case HorizontalDirection.South:
                    return south;
                case HorizontalDirection.West:
                    return west;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public SocketKind Rotated(int rotationSteps, HorizontalDirection worldDirection) {
            var captured = ((int)worldDirection - rotationSteps % 4 + 4) % 4;
            return Horizontal((HorizontalDirection)captured);
        }
    }
}
