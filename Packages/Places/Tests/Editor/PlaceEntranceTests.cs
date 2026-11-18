// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceEntranceTests {
        [Test]
        public void ANewEntranceIsNotSetWithAnEmptyLayerNameFacingNorth() {
            var entrance = new PlaceEntrance();

            Assert.IsFalse(entrance.IsSet);
            Assert.AreEqual("", entrance.LayerName);
            Assert.AreEqual(default(CellAddress), entrance.Cell);
            Assert.AreEqual(HorizontalDirection.North, entrance.Facing);
        }

        [Test]
        public void AnEntranceWithALayerNameIsSetAndKeepsItsCellAndFacing() {
            var entrance = new PlaceEntrance {
                LayerName = "rooms",
                Cell = new CellAddress(3, 0, 1),
                Facing = HorizontalDirection.West
            };

            Assert.IsTrue(entrance.IsSet);
            Assert.AreEqual("rooms", entrance.LayerName);
            Assert.AreEqual(new CellAddress(3, 0, 1), entrance.Cell);
            Assert.AreEqual(HorizontalDirection.West, entrance.Facing);
        }
    }
}
