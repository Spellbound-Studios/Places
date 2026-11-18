// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceLayerTests {
        private OwnedTestLayer _layer;

        [SetUp]
        public void CreateLayer() {
            _layer = new OwnedTestLayer { LayerName = "owned" };
        }

        [Test]
        public void ALayerIsNotHarvestableByDefault() {
            Assert.IsFalse(_layer.IsHarvestable);
        }

        [Test]
        public void ALayerHasNoPlacementPointsByDefault() {
            CollectionAssert.IsEmpty(_layer.PlacementPoints());
        }

        [Test]
        public void ALayerCollectsNoLayerReferencesByDefault() {
            var references = new List<PlaceLayerReference>();

            _layer.CollectLayerReferences(references, "layers[0]");

            Assert.AreEqual(0, references.Count);
        }

        [Test]
        public void RenamingLayerReferencesLeavesALayerWithoutReferencesAsItWas() {
            _layer.RenameLayerReferences("owned", "renamed");

            Assert.AreEqual("owned", _layer.LayerName);
        }

        [Test]
        public void APlacementPointKeepsItsLayerNameLocationAndPosition() {
            var point = new PlacementPoint("deposits", "deposits[7]", new Vector3(1f, 2f, 3f));

            Assert.AreEqual("deposits", point.LayerName);
            Assert.AreEqual("deposits[7]", point.Location);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), point.Position);
        }

        [Test]
        public void ALayerReferenceKeepsItsSourceLocationAndTargetLayerName() {
            var reference = new PlaceLayerReference("layers[2].arrangementLayerName", "rooms");

            Assert.AreEqual("layers[2].arrangementLayerName", reference.SourceLocation);
            Assert.AreEqual("rooms", reference.TargetLayerName);
        }
    }
}
