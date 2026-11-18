// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class NestedPlaceLayerTests {
        private PlaceDefinition _nested;

        [SetUp]
        public void CreateNestedPlace() {
            _nested = ScriptableObject.CreateInstance<PlaceDefinition>();
        }

        [TearDown]
        public void DestroyNestedPlace() {
            Object.DestroyImmediate(_nested);
        }

        [Test]
        public void ANewNestedLayerNamesNoPlaceAndNoArrangementLayer() {
            var layer = new NestedPlaceLayer();

            Assert.IsNull(layer.Place);
            Assert.AreEqual("", layer.ArrangementLayerName);
            Assert.AreEqual(default(CellAddress), layer.Cell);
        }

        [Test]
        public void ANestedLayerKeepsItsPlaceArrangementLayerNameAndCell() {
            var layer = new NestedPlaceLayer {
                LayerName = "nested",
                Place = _nested,
                ArrangementLayerName = "rooms",
                Cell = new CellAddress(2, 4, 0)
            };

            Assert.AreSame(_nested, layer.Place);
            Assert.AreEqual("rooms", layer.ArrangementLayerName);
            Assert.AreEqual(new CellAddress(2, 4, 0), layer.Cell);
            Assert.IsInstanceOf<PlaceLayer>(layer);
        }
    }
}
