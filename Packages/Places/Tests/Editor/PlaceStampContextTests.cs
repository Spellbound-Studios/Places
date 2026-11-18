// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Places.Owners;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceStampContextTests {
        [TearDown]
        public void ResetDefault() {
            PlaceLayerOwnerRegistry.Reset();
        }

        [Test]
        public void ANewContextStampsInTheWorldWithoutPreviewThroughTheDefaultOwners() {
            var report = new ValidationReport("place");

            var context = new PlaceStampContext(report);

            Assert.AreSame(report, context.Report);
            Assert.IsTrue(context.Frame.IsWorld);
            Assert.IsFalse(context.Preview);
            Assert.AreSame(PlaceLayerOwnerRegistry.Default, context.Owners);
        }

        [Test]
        public void AContextKeepsTheGivenFramePreviewAndOwners() {
            var owners = new PlaceLayerOwnerRegistry(new object[] { new RecordingLayerOwner() });

            var context = new PlaceStampContext(new ValidationReport("place"), new ReferenceFrameId(7), true, owners);

            Assert.AreEqual(new ReferenceFrameId(7), context.Frame);
            Assert.IsTrue(context.Preview);
            Assert.AreSame(owners, context.Owners);
        }

        [Test]
        public void ANullReportThrows() {
            Assert.Throws<ArgumentNullException>(() => new PlaceStampContext(null));
        }
    }
}
