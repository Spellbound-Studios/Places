// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spellbound.Places.Arrangement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceReferenceTableTests {
        private PlaceDefinition _place;

        [SetUp]
        public void CreatePlace() {
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(new NestedPlaceLayer {
                LayerName = "cellar",
                ArrangementLayerName = "arrangement",
                Cell = new CellAddress(1, 1, 0)
            });
            _place.Entrance.LayerName = "arrangement";
        }

        [TearDown]
        public void DestroyPlace() {
            Object.DestroyImmediate(_place);
        }

        [Test]
        public void OfListsTheNestedLayerReferenceAndTheEntrance() {
            var table = PlaceReferenceTable.Of(_place);

            CollectionAssert.AreEqual(new[] { "arrangement", "cellar" }, table.LayerNames);
            CollectionAssert.AreEqual(new[] { "layers[1].arrangementLayerName", "entrance" },
                table.LayerReferences.Select(reference => reference.SourceLocation));
            CollectionAssert.AreEqual(new[] { "arrangement", "arrangement" },
                table.LayerReferences.Select(reference => reference.TargetLayerName));
        }

        [Test]
        public void OfSkipsEmptySlotsAndAnUnsetEntrance() {
            ((List<PlaceLayer>)_place.Layers).Insert(0, null);
            _place.Entrance.LayerName = "";

            var table = PlaceReferenceTable.Of(_place);

            CollectionAssert.AreEqual(new[] { "arrangement", "cellar" }, table.LayerNames);
            CollectionAssert.AreEqual(new[] { "layers[2].arrangementLayerName" },
                table.LayerReferences.Select(reference => reference.SourceLocation));
        }

        [Test]
        public void OfANullPlaceThrows() {
            Assert.Throws<ArgumentNullException>(() => PlaceReferenceTable.Of(null));
        }

        [Test]
        public void ReferencesToFindsEveryReferenceToOneLayer() {
            var upper = ArrangementLayerValidationTests.CleanLayer();
            upper.LayerName = "upper";
            _place.AddLayer(upper);
            _place.AddLayer(new NestedPlaceLayer { LayerName = "attic", ArrangementLayerName = "upper" });

            var table = PlaceReferenceTable.Of(_place);

            CollectionAssert.AreEqual(new[] { "layers[1].arrangementLayerName", "entrance" },
                table.ReferencesTo("arrangement").Select(reference => reference.SourceLocation));
            CollectionAssert.AreEqual(new[] { "layers[3].arrangementLayerName" },
                table.ReferencesTo("upper").Select(reference => reference.SourceLocation));
            CollectionAssert.IsEmpty(table.ReferencesTo("cellar"));
        }

        [Test]
        public void RenameLayerRenamesTheLayerTheNestedReferenceAndTheEntrance() {
            PlaceReferenceTable.RenameLayer(_place, "arrangement", "hall");

            var nested = (NestedPlaceLayer)_place.Layers[1];
            Assert.AreEqual("hall", _place.Layers[0].LayerName);
            Assert.AreEqual("cellar", nested.LayerName);
            Assert.AreEqual("hall", nested.ArrangementLayerName);
            Assert.AreEqual("hall", _place.Entrance.LayerName);
            CollectionAssert.IsEmpty(PlaceReferenceTable.Of(_place).ReferencesTo("arrangement"));
            Assert.AreEqual(2, PlaceReferenceTable.Of(_place).ReferencesTo("hall").Count);
        }

        [Test]
        public void RenameLayerLeavesPartNamesAlone() {
            var arrangement = (ArrangementLayer)_place.Layers[0];
            ((List<PartDescription>)arrangement.Parts).Add(new PartDescription { partName = "arrangement" });
            ((List<CellRule>)arrangement.Rules).Add(new FixedCellRule {
                cell = new CellAddress(0, 0, 0),
                partName = "arrangement"
            });

            PlaceReferenceTable.RenameLayer(_place, "arrangement", "hall");

            CollectionAssert.AreEqual(new[] { "corridor", "end", "arrangement" },
                arrangement.Parts.Select(part => part.partName));
            CollectionAssert.AreEqual(new[] { "corridor", "end" },
                arrangement.Strata[0].pool.Select(entry => entry.partName));
            Assert.AreEqual("arrangement", ((FixedCellRule)arrangement.Rules[0]).partName);
        }

        [Test]
        public void RenameLayerOfAnUnknownNameThrows() {
            Assert.Throws<ArgumentException>(() => PlaceReferenceTable.RenameLayer(_place, "missing", "hall"));
            Assert.AreEqual("arrangement", _place.Layers[0].LayerName);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void RenameLayerToAnEmptyNameThrows(string newName) {
            Assert.Throws(Is.InstanceOf<ArgumentException>(),
                () => PlaceReferenceTable.RenameLayer(_place, "arrangement", newName));
            Assert.AreEqual("arrangement", _place.Layers[0].LayerName);
        }
    }
}
