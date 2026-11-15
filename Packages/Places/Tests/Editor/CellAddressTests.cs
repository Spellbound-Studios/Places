// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Places.Arrangement;

namespace Spellbound.Places.Editor.Tests {
    public class CellAddressTests {
        [Test]
        public void TheConstructorSetsColumnRowAndStratum() {
            var cell = new CellAddress(3, 4, 1);

            Assert.AreEqual(3, cell.column);
            Assert.AreEqual(4, cell.row);
            Assert.AreEqual(1, cell.stratum);
        }

        [Test]
        public void AddressesWithTheSameCoordinatesAreEqual() {
            var first = new CellAddress(3, 4, 1);
            var second = new CellAddress(3, 4, 1);
            var other = new CellAddress(3, 4, 2);

            Assert.IsTrue(first.Equals(second));
            Assert.IsTrue(first.Equals((object)second));
            Assert.IsTrue(first == second);
            Assert.IsFalse(first != second);
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
            Assert.IsFalse(first.Equals(other));
            Assert.IsTrue(first != other);
            Assert.IsFalse(first.Equals("3,4,1"));
        }

        [Test]
        public void ToStringIsColumnRowStratum() {
            Assert.AreEqual("3,4,1", new CellAddress(3, 4, 1).ToString());
        }

        [Test]
        public void ARegionContainsItsCellsInclusiveOnEveryAxis() {
            var region = new CellRegion { minimum = new CellAddress(1, 2, 0), maximum = new CellAddress(3, 4, 1) };

            Assert.IsTrue(region.Contains(new CellAddress(1, 2, 0)));
            Assert.IsTrue(region.Contains(new CellAddress(3, 4, 1)));
            Assert.IsTrue(region.Contains(new CellAddress(2, 3, 1)));
            Assert.IsFalse(region.Contains(new CellAddress(0, 3, 0)));
            Assert.IsFalse(region.Contains(new CellAddress(4, 3, 0)));
            Assert.IsFalse(region.Contains(new CellAddress(2, 1, 0)));
            Assert.IsFalse(region.Contains(new CellAddress(2, 5, 0)));
            Assert.IsFalse(region.Contains(new CellAddress(2, 3, 2)));
            Assert.IsFalse(region.Contains(new CellAddress(2, 3, -1)));
        }
    }
}
