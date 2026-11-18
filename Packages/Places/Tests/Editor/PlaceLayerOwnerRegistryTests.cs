// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Places.Arrangement;
using Spellbound.Places.Owners;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceLayerOwnerRegistryTests {
        private RecordingLayerOwner _owner;
        private PlaceLayerOwnerRegistry _registry;

        [SetUp]
        public void CreateRegistry() {
            _owner = new RecordingLayerOwner();
            _registry = new PlaceLayerOwnerRegistry(new object[] { _owner });
        }

        [TearDown]
        public void ResetDefault() {
            PlaceLayerOwnerRegistry.Reset();
        }

        [Test]
        public void ARegistryOverOneOwnerListsItsLayerTypeAndHasItByExactType() {
            CollectionAssert.AreEqual(new[] { typeof(OwnedTestLayer) }, _registry.LayerTypes.ToList());
            Assert.IsTrue(_registry.Has(typeof(OwnedTestLayer)));
            Assert.IsFalse(_registry.Has(typeof(PlaceLayer)));
            Assert.IsFalse(_registry.Has(typeof(ArrangementLayer)));
            Assert.IsFalse(_registry.Has(null));
        }

        [Test]
        public void TryStampReachesTheOwnerWithTheLayerAndTheContext() {
            var layer = new OwnedTestLayer { LayerName = "owned" };
            var context = new PlaceStampContext(new ValidationReport("place"), owners: _registry);

            Assert.IsTrue(_registry.TryStamp(layer, context));
            CollectionAssert.AreEqual(new[] { layer }, _owner.Stamped.ToList());
            Assert.AreSame(context, _owner.LastStampContext);
        }

        [Test]
        public void TryStampOfALayerWithoutAnOwnerIsFalseAndStampsNothing() {
            var context = new PlaceStampContext(new ValidationReport("place"), owners: _registry);

            Assert.IsFalse(_registry.TryStamp(new ArrangementLayer { LayerName = "arrangement" }, context));
            Assert.AreEqual(0, _owner.Stamped.Count);
            Assert.IsNull(_owner.LastStampContext);
        }

        [Test]
        public void CaptureReturnsTheOwnersLayerNamedFromTheContext() {
            var context = new PlaceCaptureContext("captured", new ValidationReport("place"));

            var captured = _registry.Capture(typeof(OwnedTestLayer), context);

            Assert.IsInstanceOf<OwnedTestLayer>(captured);
            Assert.AreEqual("captured", captured.LayerName);
            Assert.AreSame(context, _owner.LastCaptureContext);
        }

        [Test]
        public void CaptureOfALayerTypeWithoutAnOwnerIsNull() {
            var context = new PlaceCaptureContext("captured", new ValidationReport("place"));

            Assert.IsNull(_registry.Capture(typeof(ArrangementLayer), context));
            Assert.IsNull(_owner.LastCaptureContext);
        }

        [Test]
        public void AnOwnerWithoutTheOwnerInterfaceThrowsNamingItsType() {
            var exception = Assert.Throws<ArgumentException>(() => new PlaceLayerOwnerRegistry(new[] { new object() }));

            StringAssert.Contains("System.Object", exception.Message);
        }

        [Test]
        public void TwoOwnersOfOneLayerTypeThrowNamingTheLayerType() {
            var owners = new object[] { new RecordingLayerOwner(), new RecordingLayerOwner() };

            var exception = Assert.Throws<ArgumentException>(() => new PlaceLayerOwnerRegistry(owners));

            StringAssert.Contains(typeof(OwnedTestLayer).FullName, exception.Message);
        }

        [Test]
        public void ANullOwnerListThrows() {
            Assert.Throws<ArgumentNullException>(() => new PlaceLayerOwnerRegistry(null));
        }

        [Test]
        public void DefaultIsOneInstanceUntilReset() {
            var first = PlaceLayerOwnerRegistry.Default;

            Assert.IsNotNull(first);
            Assert.AreSame(first, PlaceLayerOwnerRegistry.Default);

            PlaceLayerOwnerRegistry.Reset();

            Assert.AreNotSame(first, PlaceLayerOwnerRegistry.Default);
        }

        [Test]
        public void TheOwnerAttributeMarksOneConcreteClassWithoutInheritance() {
            var usage = typeof(PlaceLayerOwnerAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            Assert.AreEqual(AttributeTargets.Class, usage.ValidOn);
            Assert.IsFalse(usage.Inherited);
            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(typeof(PlaceLayerOwnerAttribute).IsSealed);
        }
    }
}
