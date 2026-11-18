// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class SocketKindTextTests {
        [TestCase((SocketKind)(-1), "Any")]
        [TestCase((SocketKind)0, "NoConnection")]
        [TestCase(SocketKind.Connection, "Connection")]
        [TestCase(SocketKind.NoConnection | SocketKind.Connection, "NoConnection|Connection")]
        public void TheFaceReadsAsWritten(SocketKind kind, string expected) {
            Assert.AreEqual(expected, SocketKindText.Of(kind));
        }
    }
}
