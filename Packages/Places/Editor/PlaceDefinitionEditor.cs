// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Controls.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Spellbound.Places.Editor {
    [CustomEditor(typeof(PlaceDefinition))]
    public sealed class PlaceDefinitionEditor : UnityEditor.Editor {
        public const string RootName = "places-inspector";
        public const string LayerListName = "places-layer-list";
        public const string LayerActionsName = "places-layer-actions";
        public const string ReadOnlyState = "read-only";

        public const string ReadOnlySentence =
            "the type of a stored layer or rule is not installed, so the list is read-only until it is";

        private const string DuplicateSuffix = " Copy";

        private SectionCard _layersCard;
        private PropertyField _layerList;
        private VisualElement _layerActions;

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement { name = RootName };

            var placeCard = new SectionCard("Place");
            placeCard.Add(new PropertyField(serializedObject.FindProperty("defaultMovementProfileName")));
            placeCard.Add(new PropertyField(serializedObject.FindProperty("entrance")));
            placeCard.Add(new PropertyField(serializedObject.FindProperty("nestingDepthLimit")));
            root.Add(placeCard);

            _layersCard = new SectionCard("Layers");
            _layersCard.Header.Add(new Button(DuplicateTarget) { text = "Duplicate" });
            _layerList = new PropertyField(serializedObject.FindProperty("layers")) { name = LayerListName };
            _layersCard.Add(_layerList);
            _layerActions = new VisualElement { name = LayerActionsName };
            _layersCard.Add(_layerActions);
            root.Add(_layersCard);

            Refresh();
            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            return root;
        }

        private void Refresh() {
            var place = target as PlaceDefinition;

            if (place == null) {
                return;
            }

            var readOnly = SerializationUtility.HasManagedReferencesWithMissingTypes(place);
            _layersCard.Header.State = readOnly ? ReadOnlyState : null;
            _layersCard.Header.Sentence = readOnly ? ReadOnlySentence : null;
            _layerList.SetEnabled(!readOnly);
            _layerActions.SetEnabled(!readOnly);
            DrawLayerActions(place);
        }

        private void DrawLayerActions(PlaceDefinition place) {
            _layerActions.Clear();
            var registry = PlaceLayerInspectorActionRegistry.Default;

            for (var index = 0; index < place.Layers.Count; index++) {
                var layer = place.Layers[index];
                var location = "layers[" + index + "]";
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.Add(new Label(layer == null ? location + " (carried or empty)"
                    : location + " '" + layer.LayerName + "'"));

                var layerIndex = index;

                foreach (var action in registry.ActionsFor(place, index)) {
                    row.Add(new Button(() => action.Invoke(place, layerIndex)) { text = action.Label });
                }

                _layerActions.Add(row);
            }
        }

        private void DuplicateTarget() {
            var place = (PlaceDefinition)target;
            var report = PlaceDuplicator.Duplicate(place, place.name + DuplicateSuffix);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(report.CopyPath);
        }
    }
}
