// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceLayerInspectorActionTests {
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
        public void AnActionKeepsItsLabel() {
            var action = new PlaceLayerInspectorAction("Edit terrain", (place, layerIndex) => { });

            Assert.AreEqual("Edit terrain", action.Label);
        }

        [Test]
        public void InvokeReachesTheCallbackWithThePlaceAndTheLayerIndex() {
            PlaceDefinition invokedPlace = null;
            var invokedIndex = -1;
            var action = new PlaceLayerInspectorAction("Edit terrain", (place, layerIndex) => {
                invokedPlace = place;
                invokedIndex = layerIndex;
            });

            action.Invoke(_place, 2);

            Assert.AreSame(_place, invokedPlace);
            Assert.AreEqual(2, invokedIndex);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void AnEmptyLabelThrows(string label) {
            Assert.Throws<ArgumentException>(() => new PlaceLayerInspectorAction(label, (place, layerIndex) => { }));
        }

        [Test]
        public void ANullCallbackThrows() {
            Assert.Throws<ArgumentNullException>(() => new PlaceLayerInspectorAction("Edit terrain", null));
        }

        [Test]
        public void TheActionsContractNamesItsLayerTypeAndListsActionsForAPlaceLayer() {
            var contract = typeof(IPlaceLayerInspectorActions);

            var layerType = contract.GetProperty(nameof(IPlaceLayerInspectorActions.LayerType));
            var actions = contract.GetMethod(nameof(IPlaceLayerInspectorActions.Actions));

            Assert.AreEqual(typeof(Type), layerType.PropertyType);
            Assert.IsFalse(layerType.CanWrite);
            Assert.AreEqual(typeof(IReadOnlyList<PlaceLayerInspectorAction>), actions.ReturnType);
            CollectionAssert.AreEqual(new[] { typeof(PlaceDefinition), typeof(int) },
                Array.ConvertAll(actions.GetParameters(), parameter => parameter.ParameterType));
        }

        [Test]
        public void TheActionAttributeMarksOneConcreteClassWithoutInheritance() {
            var usage = typeof(PlaceLayerInspectorActionAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            Assert.AreEqual(AttributeTargets.Class, usage.ValidOn);
            Assert.IsFalse(usage.Inherited);
            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(typeof(PlaceLayerInspectorActionAttribute).IsSealed);
        }
    }
}
