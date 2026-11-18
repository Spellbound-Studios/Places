// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Places.Owners;
using Spellbound.Places.Tests.CarriedFixture;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlacesKitStampTests {
        private PlaceDefinition _place;

        [SetUp]
        public void CreatePlace() {
            FixtureLayerOwner.Clear();
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
        }

        [TearDown]
        public void DestroyPlace() {
            Object.DestroyImmediate(_place);
            FixtureLayerOwner.Clear();
            PlaceLayerOwnerRegistry.Reset();
        }

        [Test]
        public void LayersAreStampedInListOrder() {
            _place.AddLayer(new FixtureCarriedLayer { LayerName = "first" });
            _place.AddLayer(new FixtureCarriedLayer { LayerName = "second" });
            var report = new ValidationReport("place");

            var stamped = PlacesKit.Stamp(_place, new PlaceStampContext(report, owners: FixtureOwners()));

            Assert.IsTrue(stamped, report.ToText());
            CollectionAssert.AreEqual(new[] { "first", "second" }, FixtureLayerOwner.StampedLayerNames);
            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        [Test]
        public void TheContextReachesTheOwner() {
            _place.AddLayer(new FixtureCarriedLayer { LayerName = "deposits" });
            var report = new ValidationReport("place");

            var stamped = PlacesKit.Stamp(_place,
                new PlaceStampContext(report, new ReferenceFrameId(7), true, FixtureOwners()));

            Assert.IsTrue(stamped, report.ToText());
            Assert.AreEqual(new ReferenceFrameId(7), FixtureLayerOwner.StampedFrame);
            Assert.IsTrue(FixtureLayerOwner.StampedPreview);
        }

        [Test]
        public void CaptureRoundTripsTheLayer() {
            _place.AddLayer(new FixtureCarriedLayer { LayerName = "deposits", label = "ore", weight = 7 });
            var owners = FixtureOwners();
            var report = new ValidationReport("place");
            Assert.IsTrue(PlacesKit.Stamp(_place, new PlaceStampContext(report, owners: owners)), report.ToText());

            var captured = owners.Capture(typeof(FixtureCarriedLayer), new PlaceCaptureContext("ore-back", report));

            Assert.IsInstanceOf<FixtureCarriedLayer>(captured);
            var layer = (FixtureCarriedLayer)captured;
            Assert.AreEqual("ore-back", layer.LayerName);
            Assert.AreEqual("ore", layer.label);
            Assert.AreEqual(7, layer.weight);
        }

        [Test]
        public void ALayerWithNoOwnerIsRefusedAndNothingIsStamped() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(new FixtureCarriedLayer { LayerName = "deposits" });
            var report = new ValidationReport("place");

            var stamped = PlacesKit.Stamp(_place, new PlaceStampContext(report, owners: FixtureOwners()));

            Assert.IsFalse(stamped);
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            var finding = report.Findings[0];
            Assert.AreEqual(FindingSeverity.Error, finding.Severity);
            Assert.AreEqual(PlaceFindingCodes.LayerNotInstalled, finding.Code);
            Assert.AreEqual("places.layer-not-installed", finding.Code);
            Assert.AreEqual("layers[0]", finding.Location);
            StringAssert.Contains("of type ArrangementLayer has no installed owner; nothing is stamped",
                finding.Message);
            CollectionAssert.IsEmpty(FixtureLayerOwner.StampedLayerNames);
        }

        [Test]
        public void ACarriedLayerIsRefusedAsNotInstalled() {
            using var folder = new PlacesTestFolder();
            var carried = CarriedPlaces.CreateCarriedPlace(folder.Path("Carried.asset"), carriedFirst: true);
            var report = new ValidationReport("place");

            var stamped = PlacesKit.Stamp(carried, new PlaceStampContext(report, owners: FixtureOwners()));

            Assert.IsFalse(stamped);
            var refusals = report.Findings.Where(finding => finding.Code == PlaceFindingCodes.LayerNotInstalled)
                .ToArray();
            Assert.AreEqual(report.Findings.Count, refusals.Length, report.ToText());
            CollectionAssert.AreEqual(new[] { "layers[0]", "layers[1]" },
                refusals.Select(finding => finding.Location).ToArray(), report.ToText());
            Assert.AreEqual(FindingSeverity.Error, refusals[0].Severity);
            Assert.AreEqual(
                "holds no readable layer at runtime (its type is not installed or the slot is empty); "
                + "nothing is stamped",
                refusals[0].Message);
            Assert.IsNotNull(carried.Layers[1]);
            CollectionAssert.IsEmpty(FixtureLayerOwner.StampedLayerNames);
        }

        [Test]
        public void TheDefaultRegistryFindsTheFixtureOwnerByScan() {
            PlaceLayerOwnerRegistry.Reset();

            Assert.IsTrue(PlaceLayerOwnerRegistry.Default.Has(typeof(FixtureCarriedLayer)));
        }

        [Test]
        public void ANullPlaceOrContextThrows() {
            var context = new PlaceStampContext(new ValidationReport("place"), owners: FixtureOwners());

            Assert.Throws<ArgumentNullException>(() => PlacesKit.Stamp(null, context));
            Assert.Throws<ArgumentNullException>(() => PlacesKit.Stamp(_place, null));
        }

        private static PlaceLayerOwnerRegistry FixtureOwners() {
            return new PlaceLayerOwnerRegistry(new object[] { new FixtureLayerOwner() });
        }
    }
}
