// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spellbound.Places.Arrangement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceLayerInspectorActionRegistryTests {
        private PlaceDefinition _place;

        [SetUp]
        public void CreatePlace() {
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
        }

        [TearDown]
        public void DestroyPlace() {
            Object.DestroyImmediate(_place);
            PlaceLayerInspectorActionRegistry.Reset();
        }

        [Test]
        public void ActionsForReturnsTheProviderActionsForItsLayerType() {
            _place.AddLayer(new ArrangementLayer { LayerName = "arrangement" });
            _place.AddLayer(new OwnedTestLayer { LayerName = "owned" });
            var registry = new PlaceLayerInspectorActionRegistry(new[] { new TestLayerInspectorActions() });

            var actions = registry.ActionsFor(_place, 1);

            CollectionAssert.AreEqual(new[] { TestLayerInspectorActions.ActionLabel },
                actions.Select(action => action.Label));
            Assert.AreEqual(0, registry.ActionsFor(_place, 0).Count);
        }

        [Test]
        public void ActionsForIsEmptyForAnUnprovidedTypeAndANullSlot() {
            ((List<PlaceLayer>)_place.Layers).Add(null);
            _place.AddLayer(new HarvestableTestLayer { LayerName = "harvestable" });
            var registry = new PlaceLayerInspectorActionRegistry(new[] { new TestLayerInspectorActions() });

            Assert.AreEqual(0, registry.ActionsFor(_place, 0).Count);
            Assert.AreEqual(0, registry.ActionsFor(_place, 1).Count);
            Assert.Throws<ArgumentNullException>(() => registry.ActionsFor(null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.ActionsFor(_place, 2));
            Assert.Throws<ArgumentNullException>(() => new PlaceLayerInspectorActionRegistry(null));
        }

        [Test]
        public void TheDefaultRegistryFindsTheTestProviderThroughTypeCache() {
            _place.AddLayer(new OwnedTestLayer { LayerName = "owned" });
            PlaceLayerInspectorActionRegistry.Reset();

            var actions = PlaceLayerInspectorActionRegistry.Default.ActionsFor(_place, 0);

            CollectionAssert.AreEqual(new[] { TestLayerInspectorActions.ActionLabel },
                actions.Select(action => action.Label));
        }
    }
}
