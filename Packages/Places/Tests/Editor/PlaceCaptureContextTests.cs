// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Places.Owners;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceCaptureContextTests {
        [Test]
        public void AContextKeepsTheLayerNameReportAndFrame() {
            var report = new ValidationReport("place");

            var context = new PlaceCaptureContext("captured", report, new ReferenceFrameId(3));

            Assert.AreEqual("captured", context.LayerName);
            Assert.AreSame(report, context.Report);
            Assert.AreEqual(new ReferenceFrameId(3), context.Frame);
        }

        [Test]
        public void ANewContextCapturesInTheWorld() {
            var context = new PlaceCaptureContext("captured", new ValidationReport("place"));

            Assert.IsTrue(context.Frame.IsWorld);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void AnEmptyLayerNameThrows(string layerName) {
            Assert.Throws<ArgumentException>(() => new PlaceCaptureContext(layerName, new ValidationReport("place")));
        }

        [Test]
        public void ANullReportThrows() {
            Assert.Throws<ArgumentNullException>(() => new PlaceCaptureContext("captured", null));
        }
    }
}
