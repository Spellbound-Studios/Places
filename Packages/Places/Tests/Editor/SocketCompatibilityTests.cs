// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class SocketCompatibilityTests {
        private const SocketKind Everything = (SocketKind)(-1);

        [Test]
        public void ZeroReadsAsNoConnection() {
            Assert.AreEqual(SocketKind.NoConnection, SocketCompatibility.Normalise((SocketKind)0));
            Assert.AreEqual(SocketKind.Connection, SocketCompatibility.Normalise(SocketKind.Connection));
        }

        [Test]
        public void TwoFacesSharingABitAreCompatible() {
            Assert.IsTrue(SocketCompatibility.AreCompatible(SocketKind.Connection,
                SocketKind.NoConnection | SocketKind.Connection));
            Assert.IsTrue(SocketCompatibility.AreCompatible(SocketKind.SealedConnection, SocketKind.SealedConnection));
        }

        [Test]
        public void NoConnectionAgainstConnectionIsNotCompatible() {
            Assert.IsFalse(SocketCompatibility.AreCompatible(SocketKind.NoConnection, SocketKind.Connection));
            Assert.IsFalse(SocketCompatibility.AreCompatible(SocketKind.Connection, SocketKind.SealedConnection));
        }

        [Test]
        public void EverythingIsCompatibleWithEveryNonZeroFace() {
            for (var value = 1; value <= 7; value++) {
                Assert.IsTrue(SocketCompatibility.AreCompatible(Everything, (SocketKind)value), value.ToString());
                Assert.IsTrue(SocketCompatibility.AreCompatible((SocketKind)value, Everything), value.ToString());
            }
        }

        [Test]
        public void EverythingAgainstZeroIsCompatibleBecauseZeroIsNoConnection() {
            Assert.IsTrue(SocketCompatibility.AreCompatible(Everything, (SocketKind)0));
            Assert.IsTrue(SocketCompatibility.AreCompatible((SocketKind)0, SocketKind.NoConnection));
            Assert.IsFalse(SocketCompatibility.AreCompatible((SocketKind)0, SocketKind.Connection));
        }

        [Test]
        public void EverythingStaysEverythingWhenNormalised() {
            Assert.AreEqual(Everything, SocketCompatibility.Normalise(Everything));
        }
    }
}
