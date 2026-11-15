// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Core.Authoring.Editor;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceDefinitionPreviewerTests {
        private PlaceDefinition _place;

        [SetUp]
        public void CreatePlace() {
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
            _place.name = "Clean";
        }

        [TearDown]
        public void DestroyPlace() {
            Object.DestroyImmediate(_place);
        }

        [Test]
        public void ThePreviewListsLayersAndTheGrid() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());

            var preview = new PlaceDefinitionPreviewer().Preview(_place);

            Assert.AreEqual("Clean: 1 layers", preview.Summary);
            Assert.AreEqual("layers[0] ArrangementLayer 'arrangement'", preview.Lines[0]);
            Assert.AreEqual("stratum 0", preview.Lines[1]);
            Assert.AreEqual("end@3 end@1 end@3", preview.Lines[2]);
            Assert.AreEqual("end@3 # end@1", preview.Lines[3]);
            Assert.AreEqual("end@0 end@1 end@3", preview.Lines[4]);
            Assert.AreEqual("contradictions: 0", preview.Lines[5]);
            Assert.AreEqual(6, preview.Lines.Count, string.Join("\n", preview.Lines));
        }

        [Test]
        public void ACarriedOrEmptySlotIsListedWithoutAType() {
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            ((List<PlaceLayer>)_place.Layers).Add(null);

            var preview = new PlaceDefinitionPreviewer().Preview(_place);

            Assert.AreEqual("Clean: 2 layers", preview.Summary);
            Assert.AreEqual("layers[1] (carried or empty)", preview.Lines[preview.Lines.Count - 1]);
        }

        [Test]
        public void ThePreviewUsesTheLayerSeedAndLeavesTheLayerAlone() {
            var layer = _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            layer.Seed = 9UL;

            var first = new PlaceDefinitionPreviewer().Preview(_place);
            var second = new PlaceDefinitionPreviewer().Preview(_place);

            Assert.AreEqual(9UL, layer.Seed);
            CollectionAssert.AreEqual(first.Lines, second.Lines);
            Assert.AreEqual("end@2 end@3 end@1", first.Lines[2]);
        }

        [Test]
        public void TheRegistryFindsThePreviewerForAPlace() {
            Assert.IsInstanceOf<PlaceDefinitionPreviewer>(
                DefinitionPreviewerRegistry.Default.For(typeof(PlaceDefinition)));
        }
    }
}
