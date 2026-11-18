// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Spellbound.Core.Controls.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceDefinitionEditorTests {
        private const string ReadOnlySentence =
            "the type of a stored layer or rule is not installed, so the list is read-only until it is";

        private readonly List<Object> _created = new();
        private PlacesTestFolder _folder;

        [TearDown]
        public void DestroyCreated() {
            foreach (var created in _created) {
                if (created != null) {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
            _folder?.Dispose();
            _folder = null;
            PlaceLayerInspectorActionRegistry.Reset();
        }

        [Test]
        public void TheLayerListIsDisabledOnACarriedAsset() {
            var root = Draw(CarriedPlace());

            Assert.AreEqual("places-inspector", root.name);
            Assert.IsFalse(root.Q("places-layer-list").enabledSelf);
            Assert.IsFalse(root.Q("places-layer-actions").enabledSelf);
            var header = LayersCard(root).Header;
            Assert.AreEqual("read-only", header.State);
            Assert.AreEqual(ReadOnlySentence, header.Sentence);
        }

        [Test]
        public void TheLayerListIsEnabledOnACleanAsset() {
            var place = CleanPlace();
            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(place));

            var root = Draw(place);

            Assert.IsTrue(root.Q("places-layer-list").enabledSelf);
            Assert.IsTrue(root.Q("places-layer-actions").enabledSelf);
            var header = LayersCard(root).Header;
            Assert.AreEqual(HeaderStates.Neutral, header.State);
            Assert.AreEqual(string.Empty, header.Sentence);
        }

        [Test]
        public void ADuplicateButtonSitsInTheLayersHeader() {
            var clean = LayersCard(Draw(CleanPlace())).Header.Actions.Query<Button>().ToList();
            var carried = LayersCard(Draw(CarriedPlace())).Header.Actions.Query<Button>().ToList();

            CollectionAssert.AreEqual(new[] { "Duplicate" }, clean.Select(button => button.text));
            CollectionAssert.AreEqual(new[] { "Duplicate" }, carried.Select(button => button.text));
            Assert.IsTrue(clean[0].enabledSelf);
            Assert.IsTrue(carried[0].enabledSelf);
        }

        [Test]
        public void AnOwnerActionIsDrawnForItsLayerAndDisabledWithTheList() {
            var clean = CleanPlace();
            clean.AddLayer(new OwnedTestLayer { LayerName = "owned" });
            var carried = CarriedPlace();
            carried.AddLayer(new OwnedTestLayer { LayerName = "owned" });
            Assert.IsTrue(SerializationUtility.HasManagedReferencesWithMissingTypes(carried));

            var cleanActions = Draw(clean).Q("places-layer-actions");
            var carriedActions = Draw(carried).Q("places-layer-actions");

            Assert.AreEqual(2, cleanActions.childCount);
            Assert.AreEqual(0, cleanActions[0].Query<Button>().ToList().Count);
            CollectionAssert.AreEqual(new[] { TestLayerInspectorActions.ActionLabel },
                cleanActions[1].Query<Button>().ToList().Select(button => button.text));
            Assert.AreEqual("layers[1] 'owned'", cleanActions[1].Q<Label>().text);
            Assert.IsTrue(cleanActions.enabledSelf);
            Assert.AreEqual(3, carriedActions.childCount);
            CollectionAssert.AreEqual(new[] { TestLayerInspectorActions.ActionLabel },
                carriedActions[2].Query<Button>().ToList().Select(button => button.text));
            Assert.IsFalse(carriedActions.enabledSelf);
        }

        private VisualElement Draw(PlaceDefinition place) {
            var editor = UnityEditor.Editor.CreateEditor(place);
            _created.Add(editor);
            Assert.IsInstanceOf<PlaceDefinitionEditor>(editor);
            var root = editor.CreateInspectorGUI();
            Assert.IsNotNull(root);
            return root;
        }

        private static SectionCard LayersCard(VisualElement root) {
            var card = root.Query<SectionCard>().Where(section => section.Title == "Layers").First();
            Assert.IsNotNull(card);
            return card;
        }

        private PlaceDefinition CleanPlace() {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            place.name = "Clean";
            place.AddLayer(ArrangementLayerValidationTests.CleanLayer());
            _created.Add(place);
            return place;
        }

        private PlaceDefinition CarriedPlace() {
            _folder ??= new PlacesTestFolder();
            return CarriedPlaces.CreateCarriedPlace(_folder.Path("Carried" + _created.Count + ".asset"), true);
        }
    }
}
