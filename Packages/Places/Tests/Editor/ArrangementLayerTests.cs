// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Tooling;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class ArrangementLayerTests {
        [Test]
        public void ANewLayerHasASevenCellGridAndEmptyLists() {
            var layer = new ArrangementLayer();

            Assert.AreEqual(7, layer.GridSize);
            Assert.AreEqual(0, layer.Parts.Count);
            Assert.AreEqual(0, layer.Strata.Count);
            Assert.AreEqual(0, layer.CentrePool.Count);
            Assert.AreEqual(0, layer.Rules.Count);
            Assert.AreEqual(0UL, layer.Seed);
        }

        [Test]
        public void SeedAndLayerNameCanBeSet() {
            var layer = new ArrangementLayer { Seed = 42UL, LayerName = "rooms" };

            Assert.AreEqual(42UL, layer.Seed);
            Assert.AreEqual("rooms", layer.LayerName);
            Assert.IsInstanceOf<PlaceLayer>(layer);
        }

        [Test]
        public void ToDescriptionCarriesTheGridAndTheLists() {
            var layer = new ArrangementLayer();

            var arrangement = layer.ToDescription();

            Assert.AreEqual(layer.GridSize, arrangement.GridSize);
            Assert.AreSame(layer.Parts, arrangement.Parts);
            Assert.AreSame(layer.Strata, arrangement.Strata);
            Assert.AreSame(layer.CentrePool, arrangement.CentrePool);
            Assert.AreSame(layer.Rules, arrangement.Rules);
        }

        [Test]
        public void ContainsCellIsTrueInsideTheGridAndStrata() {
            var layer = ArrangementLayerValidationTests.CleanLayer();

            Assert.IsTrue(layer.ContainsCell(new CellAddress(0, 0, 0)));
            Assert.IsTrue(layer.ContainsCell(new CellAddress(2, 2, 0)));
            Assert.IsTrue(layer.ContainsCell(new CellAddress(1, 2, 0)));
            Assert.IsFalse(layer.ContainsCell(new CellAddress(3, 0, 0)));
            Assert.IsFalse(layer.ContainsCell(new CellAddress(-1, 0, 0)));
            Assert.IsFalse(layer.ContainsCell(new CellAddress(0, 3, 0)));
            Assert.IsFalse(layer.ContainsCell(new CellAddress(0, -1, 0)));
            Assert.IsFalse(layer.ContainsCell(new CellAddress(0, 0, 1)));
            Assert.IsFalse(layer.ContainsCell(new CellAddress(0, 0, -1)));
            Assert.IsFalse(new ArrangementLayer().ContainsCell(new CellAddress(0, 0, 0)));
        }

        [Test]
        public void ValidateRefusesANullReport() {
            Assert.Throws<ArgumentNullException>(() => new ArrangementLayer().Validate(null, "layers[0]"));
        }

        [Test]
        public void TheRuleListIsASerializeReferenceListWithoutATypePicker() {
            var field = typeof(ArrangementLayer).GetField("rules", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetCustomAttribute<SerializeReference>());
            Assert.IsNull(field.GetCustomAttribute<SerializeReferenceDropdownAttribute>());
        }
    }
}
