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
            var layer = ArrangementLayerValidationTests.CleanLayer();
            layer.AddCentrePoolEntry(new PoolEntry { partName = "corridor", weight = 1f });
            layer.AddRule(new FixedCellRule { partName = "corridor", cell = new CellAddress(1, 1, 0) });

            var arrangement = layer.ToDescription();

            Assert.AreEqual(layer.GridSize, arrangement.GridSize);
            CollectionAssert.AreEqual(layer.Parts, arrangement.Parts);
            CollectionAssert.AreEqual(layer.Strata, arrangement.Strata);
            CollectionAssert.AreEqual(layer.CentrePool, arrangement.CentrePool);
            CollectionAssert.AreEqual(layer.Rules, arrangement.Rules);
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
        public void TheRuleListIsASerializeReferenceListWithATypePicker() {
            var field = typeof(ArrangementLayer).GetField("rules", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetCustomAttribute<SerializeReference>());
            Assert.IsNotNull(field.GetCustomAttribute<SerializeReferenceDropdownAttribute>());
        }

        [Test]
        public void TheAddMembersReturnWhatTheyAdd() {
            var layer = new ArrangementLayer();
            var part = new PartDescription { partName = "corridor" };
            var stratum = new StratumDescription();
            var entry = new PoolEntry { partName = "corridor", weight = 2f };
            var rule = new FixedCellRule { partName = "corridor" };

            Assert.AreSame(part, layer.AddPart(part));
            Assert.AreSame(stratum, layer.AddStratum(stratum));
            Assert.AreSame(entry, layer.AddCentrePoolEntry(entry));
            Assert.AreSame(rule, layer.AddRule(rule));
            CollectionAssert.AreEqual(new[] { part }, layer.Parts);
            CollectionAssert.AreEqual(new[] { stratum }, layer.Strata);
            CollectionAssert.AreEqual(new[] { entry }, layer.CentrePool);
            CollectionAssert.AreEqual(new CellRule[] { rule }, layer.Rules);
        }

        [Test]
        public void AddMembersRefuseNull() {
            var layer = new ArrangementLayer();

            Assert.Throws<ArgumentNullException>(() => layer.AddPart(null));
            Assert.Throws<ArgumentNullException>(() => layer.AddStratum(null));
            Assert.Throws<ArgumentNullException>(() => layer.AddCentrePoolEntry(null));
            Assert.Throws<ArgumentNullException>(() => layer.AddRule(null));
            Assert.AreEqual(0, layer.Parts.Count + layer.Strata.Count + layer.CentrePool.Count + layer.Rules.Count);
        }

        [Test]
        public void ToDescriptionCopiesTheListsSoLaterEditsDoNotReachIt() {
            var layer = new ArrangementLayer();
            layer.AddPart(new PartDescription { partName = "corridor" });
            layer.AddStratum(new StratumDescription());
            layer.AddCentrePoolEntry(new PoolEntry { partName = "corridor", weight = 1f });
            layer.AddRule(new FixedCellRule { partName = "corridor" });

            var arrangement = layer.ToDescription();
            layer.AddPart(new PartDescription { partName = "end" });
            layer.AddStratum(new StratumDescription());
            layer.AddCentrePoolEntry(new PoolEntry { partName = "end", weight = 1f });
            layer.AddRule(new FixedCellRule { partName = "end" });

            Assert.AreEqual(1, arrangement.Parts.Count);
            Assert.AreEqual(1, arrangement.Strata.Count);
            Assert.AreEqual(1, arrangement.CentrePool.Count);
            Assert.AreEqual(1, arrangement.Rules.Count);
            Assert.AreEqual(2, layer.Parts.Count);
        }
    }
}
