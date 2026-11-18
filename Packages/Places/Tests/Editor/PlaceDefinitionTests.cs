// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Authoring;
using Spellbound.Core.Tooling;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceDefinitionTests {
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
        public void AddLayerReturnsTheLayerAndLayersListsItInOrder() {
            var first = new ArrangementLayer { LayerName = "first" };
            var second = new ArrangementLayer { LayerName = "second" };

            Assert.AreSame(first, _place.AddLayer(first));
            Assert.AreSame(second, _place.AddLayer(second));
            CollectionAssert.AreEqual(new PlaceLayer[] { first, second }, _place.Layers.ToList());
            Assert.AreEqual("second", _place.Layers[1].LayerName);
        }

        [Test]
        public void AddLayerRefusesANullLayer() {
            Assert.Throws<ArgumentNullException>(() => _place.AddLayer<ArrangementLayer>(null));
            Assert.AreEqual(0, _place.Layers.Count);
        }

        [Test]
        public void ValidateRefusesANullReport() {
            Assert.Throws<ArgumentNullException>(() => _place.Validate(null));
        }

        [Test]
        public void ValidateOfAnEmptyPlaceAddsNothing() {
            var report = new ValidationReport("place");

            _place.Validate(report);

            Assert.IsTrue(report.IsEmpty);
        }

        [Test]
        public void APlaceIsAnAuthoredDefinitionCreatedFromThePlacesMenu() {
            Assert.IsInstanceOf<IAuthoredDefinition>(_place);

            var menu = typeof(PlaceDefinition).GetCustomAttribute<CreateAssetMenuAttribute>();
            Assert.AreEqual("Place", menu.fileName);
            Assert.AreEqual("Spellbound/Places/Place", menu.menuName);
        }

        [Test]
        public void TheLayerListIsASerializeReferenceListWithoutATypePicker() {
            var field = typeof(PlaceDefinition).GetField("layers", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(field);
            Assert.IsNotNull(field.GetCustomAttribute<SerializeReference>());
            Assert.IsNull(field.GetCustomAttribute<SerializeReferenceDropdownAttribute>());
        }

        [Test]
        public void ANewPlaceHasNoEntranceNoMovementProfileAndANestingDepthLimitOfFour() {
            Assert.IsNotNull(_place.Entrance);
            Assert.IsFalse(_place.Entrance.IsSet);
            Assert.AreEqual("", _place.DefaultMovementProfileName);
            Assert.AreEqual(4, _place.NestingDepthLimit);
        }

        [Test]
        public void FindLayerFindsAPresentLayerByItsName() {
            var first = _place.AddLayer(new ArrangementLayer { LayerName = "first" });
            var second = _place.AddLayer(new ArrangementLayer { LayerName = "second" });

            Assert.AreSame(first, _place.FindLayer("first"));
            Assert.AreSame(second, _place.FindLayer("second"));
        }

        [Test]
        public void FindLayerOfAnUnknownNameOrOfNullIsNull() {
            _place.AddLayer(new ArrangementLayer { LayerName = "first" });

            Assert.IsNull(_place.FindLayer("First"));
            Assert.IsNull(_place.FindLayer("unknown"));
            Assert.IsNull(_place.FindLayer(null));
        }

        [Test]
        public void TheEntranceMovementProfileAndDepthLimitAreSerializedFields() {
            using var serialized = new SerializedObject(_place);

            Assert.IsNotNull(serialized.FindProperty("entrance"));
            Assert.IsNotNull(serialized.FindProperty("entrance.layerName"));
            Assert.AreEqual("", serialized.FindProperty("defaultMovementProfileName").stringValue);
            Assert.AreEqual(4, serialized.FindProperty("nestingDepthLimit").intValue);
        }
    }
}
