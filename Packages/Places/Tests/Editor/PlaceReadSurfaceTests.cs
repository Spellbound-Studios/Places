// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceReadSurfaceTests {
        private PlaceDefinition _place;

        [SetUp]
        public void CreatePlace() {
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
        }

        [TearDown]
        public void DestroyPlace() {
            Object.DestroyImmediate(_place);
        }

        [Test]
        public void HarvestableLayersListsOnlyLayersThatAnswerTrue() {
            var first = _place.AddLayer(new HarvestableTestLayer { LayerName = "first" });
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            ((List<PlaceLayer>)_place.Layers).Add(null);
            _place.AddLayer(new PointsTestLayer { LayerName = "points" });
            var second = _place.AddLayer(new HarvestableTestLayer { LayerName = "second" });

            var harvestable = _place.HarvestableLayers;

            Assert.AreEqual(2, harvestable.Count);
            Assert.AreSame(first, harvestable[0]);
            Assert.AreSame(second, harvestable[1]);
        }

        [Test]
        public void RulesFlattensEveryArrangementLayerInOrderAndSkipsNullRules() {
            var firstLayer = _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(new HarvestableTestLayer { LayerName = "deposits" });
            ((List<PlaceLayer>)_place.Layers).Add(null);
            var secondLayer = _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            secondLayer.LayerName = "second";
            var fixedCell = new FixedCellRule { partName = "corridor" };
            var weightedPool = new WeightedPoolRule();
            var allowedRegion = new AllowedRegionRule { partName = "end" };
            ((List<CellRule>)firstLayer.Rules).Add(fixedCell);
            ((List<CellRule>)firstLayer.Rules).Add(null);
            ((List<CellRule>)firstLayer.Rules).Add(weightedPool);
            ((List<CellRule>)secondLayer.Rules).Add(null);
            ((List<CellRule>)secondLayer.Rules).Add(allowedRegion);

            var rules = _place.Rules;

            Assert.AreEqual(3, rules.Count);
            Assert.AreSame(fixedCell, rules[0]);
            Assert.AreSame(weightedPool, rules[1]);
            Assert.AreSame(allowedRegion, rules[2]);
        }

        [Test]
        public void PlacementPointsGathersEveryPresentLayerInOrder() {
            _place.AddLayer(new PointsTestLayer { LayerName = "alpha" });
            ((List<PlaceLayer>)_place.Layers).Add(null);
            _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _place.AddLayer(new PointsTestLayer { LayerName = "beta" });

            var points = _place.PlacementPoints;

            Assert.AreEqual(4, points.Count);
            CollectionAssert.AreEqual(new[] { "alpha", "alpha", "beta", "beta" },
                points.Select(point => point.LayerName).ToArray());
            CollectionAssert.AreEqual(new[] { "deposits[0]", "deposits[1]", "deposits[0]", "deposits[1]" },
                points.Select(point => point.Location).ToArray());
            Assert.AreEqual(new Vector3(1f, 0f, 0f), points[0].Position);
            Assert.AreEqual(new Vector3(2f, 0f, 0f), points[3].Position);
        }

        [Test]
        public void DefaultMovementProfileNameIsEmptyByDefaultAndIsStoredAsWritten() {
            Assert.AreEqual("", _place.DefaultMovementProfileName);

            var serializedPlace = new SerializedObject(_place);
            serializedPlace.FindProperty("defaultMovementProfileName").stringValue = " Swimming Profile ";
            serializedPlace.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(" Swimming Profile ", _place.DefaultMovementProfileName);
        }

        [Test]
        public void EachReadMemberIsANewListPerCall() {
            var layer = _place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            ((List<CellRule>)layer.Rules).Add(new FixedCellRule { partName = "corridor" });
            _place.AddLayer(new HarvestableTestLayer { LayerName = "deposits" });
            _place.AddLayer(new PointsTestLayer { LayerName = "points" });

            Assert.AreNotSame(_place.HarvestableLayers, _place.HarvestableLayers);
            Assert.AreNotSame(_place.Rules, _place.Rules);
            Assert.AreNotSame(_place.Rules, layer.Rules);
            Assert.AreNotSame(_place.PlacementPoints, _place.PlacementPoints);
            CollectionAssert.AreEqual(_place.HarvestableLayers, _place.HarvestableLayers);
            CollectionAssert.AreEqual(_place.Rules, _place.Rules);
            CollectionAssert.AreEqual(_place.PlacementPoints, _place.PlacementPoints);
        }
    }
}
