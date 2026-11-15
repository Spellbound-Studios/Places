// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Arrangement {
    internal sealed class CellDomain {
        public const int EveryRotation = 0b1111;

        private readonly int[] _parts;
        private readonly float[] _weights;
        private readonly int[] _rotationMasks;

        public CellDomain(int[] parts, float[] weights) {
            _parts = parts;
            _weights = weights;
            _rotationMasks = new int[parts.Length];

            for (var entry = 0; entry < _rotationMasks.Length; entry++) {
                _rotationMasks[entry] = EveryRotation;
            }

            Entropy = -1;
        }

        public int Count => _parts.Length;
        public int Entropy { get; private set; }

        public int Part(int entry) {
            return _parts[entry];
        }

        public float Weight(int entry) {
            return _weights[entry];
        }

        public int RotationMask(int entry) {
            return _rotationMasks[entry];
        }

        public bool Refilter(CompositionGrid grid, in RequiredFaces required) {
            var changed = false;
            var entropy = 0;

            for (var entry = 0; entry < _parts.Length; entry++) {
                var mask = grid.ValidRotationMask(_parts[entry], required);

                if (mask != _rotationMasks[entry]) {
                    _rotationMasks[entry] = mask;
                    changed = true;
                }

                entropy += RotationCount(mask);
            }

            if (entropy != Entropy) {
                Entropy = entropy;
                changed = true;
            }

            return changed;
        }

        public SocketKind HorizontalFaces(PartFaces[] faces, HorizontalDirection direction) {
            var union = (SocketKind)0;

            for (var entry = 0; entry < _parts.Length; entry++) {
                var mask = _rotationMasks[entry];

                for (var rotation = 0; rotation < 4; rotation++) {
                    if ((mask & (1 << rotation)) != 0) {
                        union |= SocketCompatibility.Normalise(faces[_parts[entry]].Rotated(rotation, direction));
                    }
                }
            }

            return union;
        }

        public SocketKind VerticalFaces(PartFaces[] faces, bool downFace) {
            var union = (SocketKind)0;

            for (var entry = 0; entry < _parts.Length; entry++) {
                if (_rotationMasks[entry] != 0) {
                    var part = faces[_parts[entry]];
                    union |= SocketCompatibility.Normalise(downFace ? part.down : part.up);
                }
            }

            return union;
        }

        public static int RotationCount(int mask) {
            var count = 0;

            for (var rotation = 0; rotation < 4; rotation++) {
                if ((mask & (1 << rotation)) != 0) {
                    count++;
                }
            }

            return count;
        }
    }
}
