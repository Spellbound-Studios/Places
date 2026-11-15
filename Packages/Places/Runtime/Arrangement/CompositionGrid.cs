// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Arrangement {
    internal sealed class CompositionGrid {
        public const SocketKind AnyFace = (SocketKind)(-1);
        public const int NoPart = -1;

        private readonly PartFaces[] _faces;
        private readonly int[] _placedParts;
        private readonly int[] _placedRotations;
        private readonly bool[] _voidCentres;
        private readonly bool[] _verticallyClosed;
        private readonly CellDomain[] _domains;

        public CompositionGrid(int size, int stratumCount, PartFaces[] faces) {
            Size = size;
            StratumCount = stratumCount;
            CellCount = size * size * stratumCount;
            _faces = faces;
            _placedParts = new int[CellCount];
            _placedRotations = new int[CellCount];
            _voidCentres = new bool[CellCount];
            _verticallyClosed = new bool[CellCount];
            _domains = new CellDomain[CellCount];

            for (var index = 0; index < CellCount; index++) {
                _placedParts[index] = NoPart;
            }
        }

        public int Size { get; }
        public int StratumCount { get; }
        public int CellCount { get; }
        public int Centre => Size / 2;

        public bool Contains(CellAddress cell) {
            return cell.column >= 0 && cell.column < Size && cell.row >= 0 && cell.row < Size && cell.stratum >= 0
                   && cell.stratum < StratumCount;
        }

        public int IndexOf(CellAddress cell) {
            return (cell.stratum * Size + cell.row) * Size + cell.column;
        }

        public CellAddress AddressOf(int index) {
            var column = index % Size;
            var row = index / Size % Size;
            var stratum = index / (Size * Size);
            return new CellAddress(column, row, stratum);
        }

        public bool IsCentreColumn(int index) {
            var cell = AddressOf(index);
            return cell.column == Centre && cell.row == Centre;
        }

        public bool IsFree(int index) {
            return _placedParts[index] == NoPart && !_voidCentres[index];
        }

        public bool IsVoidCentre(int index) {
            return _voidCentres[index];
        }

        public int PlacedPart(int index) {
            return _placedParts[index];
        }

        public int PlacedRotation(int index) {
            return _placedRotations[index];
        }

        public CellDomain Domain(int index) {
            return _domains[index];
        }

        public void SetDomain(int index, CellDomain domain) {
            _domains[index] = domain;
        }

        public void MarkVoidCentre(int index) {
            _voidCentres[index] = true;
        }

        public void Place(int index, int part, int rotationSteps, bool verticallyClosed) {
            _placedParts[index] = part;
            _placedRotations[index] = rotationSteps;
            _verticallyClosed[index] = verticallyClosed;
            _domains[index] = null;
        }

        public int NeighbourIndex(int index, HorizontalDirection direction) {
            var cell = AddressOf(index);

            switch (direction) {
                case HorizontalDirection.North:
                    return cell.row + 1 < Size ? index + Size : -1;
                case HorizontalDirection.East:
                    return cell.column + 1 < Size ? index + 1 : -1;
                case HorizontalDirection.South:
                    return cell.row > 0 ? index - Size : -1;
                default:
                    return cell.column > 0 ? index - 1 : -1;
            }
        }

        public int AboveIndex(int index) {
            return index >= Size * Size ? index - Size * Size : -1;
        }

        public int BelowIndex(int index) {
            var below = index + Size * Size;
            return below < CellCount ? below : -1;
        }

        public RequiredFaces RequiredAt(int index) {
            return new RequiredFaces(
                HorizontalFacePresented(NeighbourIndex(index, HorizontalDirection.North), HorizontalDirection.South),
                HorizontalFacePresented(NeighbourIndex(index, HorizontalDirection.East), HorizontalDirection.West),
                HorizontalFacePresented(NeighbourIndex(index, HorizontalDirection.South), HorizontalDirection.North),
                HorizontalFacePresented(NeighbourIndex(index, HorizontalDirection.West), HorizontalDirection.East),
                VerticalFacePresented(AboveIndex(index), true),
                VerticalFacePresented(BelowIndex(index), false));
        }

        public int ValidRotationMask(int part, in RequiredFaces required) {
            var faces = _faces[part];

            if (!SocketCompatibility.AreCompatible(faces.up, required.Up)
                || !SocketCompatibility.AreCompatible(faces.down, required.Down)) {
                return 0;
            }

            var mask = 0;

            for (var rotation = 0; rotation < 4; rotation++) {
                if (Fits(faces, rotation, HorizontalDirection.North, required)
                    && Fits(faces, rotation, HorizontalDirection.East, required)
                    && Fits(faces, rotation, HorizontalDirection.South, required)
                    && Fits(faces, rotation, HorizontalDirection.West, required)) {
                    mask |= 1 << rotation;
                }
            }

            return mask;
        }

        private static bool Fits(PartFaces faces, int rotationSteps, HorizontalDirection direction,
            in RequiredFaces required) {
            return SocketCompatibility.AreCompatible(faces.Rotated(rotationSteps, direction),
                required.Horizontal(direction));
        }

        public void EnqueueOpenNeighbours(int index, PropagationWorklist worklist) {
            EnqueueIfOpen(NeighbourIndex(index, HorizontalDirection.North), worklist);
            EnqueueIfOpen(NeighbourIndex(index, HorizontalDirection.East), worklist);
            EnqueueIfOpen(NeighbourIndex(index, HorizontalDirection.South), worklist);
            EnqueueIfOpen(NeighbourIndex(index, HorizontalDirection.West), worklist);
            EnqueueIfOpen(AboveIndex(index), worklist);
            EnqueueIfOpen(BelowIndex(index), worklist);
        }

        public CompositionCell CellAt(int index, string[] partNames) {
            var address = AddressOf(index);

            if (_voidCentres[index]) {
                return new CompositionCell(address, CompositionCellState.VoidCentre, null, 0);
            }

            var part = _placedParts[index];

            return part == NoPart
                ? new CompositionCell(address, CompositionCellState.Empty, null, 0)
                : new CompositionCell(address, CompositionCellState.Placed, partNames[part], _placedRotations[index]);
        }

        private void EnqueueIfOpen(int index, PropagationWorklist worklist) {
            if (index >= 0 && _domains[index] != null) {
                worklist.Enqueue(index);
            }
        }

        private SocketKind HorizontalFacePresented(int index, HorizontalDirection towardCell) {
            if (index < 0 || _voidCentres[index]) {
                return SocketKind.NoConnection;
            }

            var part = _placedParts[index];

            if (part != NoPart) {
                return _faces[part].Rotated(_placedRotations[index], towardCell);
            }

            var domain = _domains[index];

            if (domain == null) {
                return AnyFace;
            }

            var faces = domain.HorizontalFaces(_faces, towardCell);
            return faces == 0 ? AnyFace : faces;
        }

        private SocketKind VerticalFacePresented(int index, bool downFace) {
            if (index < 0) {
                return AnyFace;
            }

            if (_voidCentres[index]) {
                return SocketKind.NoConnection;
            }

            var part = _placedParts[index];

            if (part != NoPart) {
                if (_verticallyClosed[index]) {
                    return SocketKind.NoConnection;
                }

                return downFace ? _faces[part].down : _faces[part].up;
            }

            var domain = _domains[index];

            if (domain == null) {
                return AnyFace;
            }

            var faces = domain.VerticalFaces(_faces, downFace);
            return faces == 0 ? AnyFace : faces;
        }
    }
}
