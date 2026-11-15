// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class PartFacesTests {
        private const SocketKind UpFace = SocketKind.NoConnection | SocketKind.Connection;
        private const SocketKind DownFace = (SocketKind)(-1);

        private static PartFaces DistinctFaces() {
            return new PartFaces(SocketKind.NoConnection, SocketKind.Connection | SocketKind.SealedConnection, UpFace,
                DownFace, SocketKind.Connection, SocketKind.SealedConnection);
        }

        [Test]
        public void DirectionsAreNumberedClockwiseFromNorth() {
            Assert.AreEqual(0, (int)HorizontalDirection.North);
            Assert.AreEqual(1, (int)HorizontalDirection.East);
            Assert.AreEqual(2, (int)HorizontalDirection.South);
            Assert.AreEqual(3, (int)HorizontalDirection.West);
        }

        [Test]
        public void ConstructorTakesEastWestUpDownNorthSouthInThatOrder() {
            var faces = DistinctFaces();

            Assert.AreEqual(SocketKind.NoConnection, faces.east);
            Assert.AreEqual(SocketKind.Connection | SocketKind.SealedConnection, faces.west);
            Assert.AreEqual(UpFace, faces.up);
            Assert.AreEqual(DownFace, faces.down);
            Assert.AreEqual(SocketKind.Connection, faces.north);
            Assert.AreEqual(SocketKind.SealedConnection, faces.south);
        }

        [Test]
        public void HorizontalReadsTheFaceOfEachDirection() {
            var faces = DistinctFaces();

            Assert.AreEqual(faces.north, faces.Horizontal(HorizontalDirection.North));
            Assert.AreEqual(faces.east, faces.Horizontal(HorizontalDirection.East));
            Assert.AreEqual(faces.south, faces.Horizontal(HorizontalDirection.South));
            Assert.AreEqual(faces.west, faces.Horizontal(HorizontalDirection.West));
        }

        [Test]
        public void OneStepClockwisePresentsTheCapturedNorthFaceToWorldEast() {
            var faces = DistinctFaces();

            Assert.AreEqual(faces.north, faces.Rotated(1, HorizontalDirection.East));
            Assert.AreEqual(faces.east, faces.Rotated(1, HorizontalDirection.South));
            Assert.AreEqual(faces.south, faces.Rotated(1, HorizontalDirection.West));
            Assert.AreEqual(faces.west, faces.Rotated(1, HorizontalDirection.North));
        }

        [Test]
        public void RotatedReadsTheCapturedIndexForEveryRotationAndDirection() {
            var faces = DistinctFaces();

            for (var rotationSteps = -5; rotationSteps <= 9; rotationSteps++) {
                for (var world = 0; world < 4; world++) {
                    var captured = (HorizontalDirection)((world - rotationSteps % 4 + 4) % 4);
                    var presented = faces.Rotated(rotationSteps, (HorizontalDirection)world);
                    Assert.AreEqual(faces.Horizontal(captured), presented, rotationSteps + " " + world);
                }
            }
        }

        [Test]
        public void UpAndDownNeverRotate() {
            var faces = DistinctFaces();

            for (var rotationSteps = 0; rotationSteps < 4; rotationSteps++) {
                for (var world = 0; world < 4; world++) {
                    var presented = faces.Rotated(rotationSteps, (HorizontalDirection)world);
                    Assert.AreNotEqual(UpFace, presented);
                    Assert.AreNotEqual(DownFace, presented);
                }
            }

            Assert.AreEqual(UpFace, faces.up);
            Assert.AreEqual(DownFace, faces.down);
        }

        [Test]
        public void DefaultFacesNormaliseToNoConnection() {
            var faces = default(PartFaces);

            Assert.AreEqual(SocketKind.NoConnection, SocketCompatibility.Normalise(faces.north));
            Assert.AreEqual(SocketKind.NoConnection, SocketCompatibility.Normalise(faces.up));
        }
    }
}
