// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Core;
using Spellbound.Core.Authoring;
using Spellbound.Core.Packing;

namespace Spellbound.Places.Editor.Tests {
    public class PackableTestLayerTests {
        [Test]
        public void IsAPackablePlaceLayer() {
            var layer = new PackableTestLayer();

            Assert.IsInstanceOf<PlaceLayer>(layer);
            Assert.IsInstanceOf<IPacker>(layer);
        }

        [Test]
        public void ValueRoundTripsThroughPacking() {
            var packed = new PackableTestLayer { Value = 417 };
            var bytes = new byte[16];

            Assert.IsTrue(CoreKit.Packing.TryPack(packed, bytes, out var written));
            Assert.IsTrue(CoreKit.Packing.TryUnpack<PackableTestLayer>(new ReadOnlySpan<byte>(bytes, 0, written),
                out var unpacked));
            Assert.AreEqual(417, unpacked.Value);
        }

        [Test]
        public void UnpackReadsWhatPackWrote() {
            var bytes = new byte[16];
            var writer = new Span<byte>(bytes);
            new PackableTestLayer { Value = -9 }.Pack(ref writer);
            var reader = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - writer.Length);
            var unpacked = new PackableTestLayer();

            unpacked.Unpack(ref reader);

            Assert.AreEqual(-9, unpacked.Value);
            Assert.AreEqual(0, reader.Length);
        }

        [Test]
        public void ValidateReportsNothing() {
            var report = new ValidationReport("packable");

            new PackableTestLayer().Validate(report, "layers[0]");

            Assert.IsTrue(report.IsEmpty);
        }
    }
}
