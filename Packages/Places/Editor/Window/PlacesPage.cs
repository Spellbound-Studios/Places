// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Spellbound.Core.Controls.Editor;
using Spellbound.Core.Simulation;
using Spellbound.Core.Window.Editor;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Places.Editor {
    [WindowPage(PageTitle, 50)]
    public sealed class PlacesPage : WindowPage {
        public const string PageTitle = "Places";
        public const string PlaceListName = "places-place-list";
        public const string SeedName = "places-seed";
        public const string RerollName = "places-reroll";
        public const string GridName = "places-grid";
        public const string ContradictionsName = "places-contradictions";
        public const string LayerLineClassName = "places-layer-line";
        public const string GridLineClassName = "places-grid-line";
        public const string StratumLineClassName = "places-stratum-line";
        public const string ContradictionLineClassName = "places-contradiction-line";
        public const string RerollText = "Reroll";
        public const string NoArrangementLayerText = "no arrangement layer";
        public const string NoContradictionsText = "no contradictions";
        public const string FixedWidthFontPath = "Fonts/RobotoMono/RobotoMono-Regular.ttf";

        private const ulong RerollKey = 1;
        private const string ContradictionsLinePrefix = "contradictions: ";
        private const string StratumLinePrefix = "stratum ";
        private const string SeedPrefix = "seed ";

        private static Font _fixedWidthFont;

        private readonly List<PlaceDefinition> _places = new List<PlaceDefinition>();

        private ListView _placeList;
        private Label _seedLabel;
        private VisualElement _grid;
        private VisualElement _contradictions;

        public IReadOnlyList<PlaceDefinition> Places => _places;
        public PlaceDefinition Selected { get; private set; }
        public ulong Seed { get; private set; }

        public override VisualElement CreateContent() {
            var root = new VisualElement();
            root.style.flexGrow = 1;

            var placesCard = new SectionCard("Places");
            _placeList = new ListView {
                name = PlaceListName,
                itemsSource = _places,
                makeItem = () => new Label(),
                bindItem = BindPlace,
                selectionType = SelectionType.Single
            };
            _placeList.selectionChanged += OnPlaceSelectionChanged;
            placesCard.Add(_placeList);
            root.Add(placesCard);

            var compositionCard = new SectionCard("Composition");
            _seedLabel = new Label(SeedPrefix + Seed) { name = SeedName };
            compositionCard.Header.Add(_seedLabel);
            compositionCard.Header.Add(new Button(Reroll) { name = RerollName, text = RerollText });
            var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scroll.style.flexGrow = 1;
            _grid = new VisualElement { name = GridName };
            scroll.Add(_grid);
            _contradictions = new VisualElement { name = ContradictionsName };
            scroll.Add(_contradictions);
            compositionCard.Add(scroll);
            root.Add(compositionCard);

            Refresh();
            return root;
        }

        public void Refresh() {
            _places.Clear();

            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(PlaceDefinition))) {
                var place = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(AssetDatabase.GUIDToAssetPath(guid));

                if (place != null && !_places.Contains(place)) {
                    _places.Add(place);
                }
            }

            _places.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase));

            if (Selected != null && !_places.Contains(Selected)) {
                Selected = null;
            }

            HeaderSentence = _places.Count + " places in the project";
            _placeList?.RefreshItems();
            ShowSelectionInList();
            Draw();
        }

        public void Select(PlaceDefinition place) {
            Selected = place;
            var first = place == null ? null : ArrangementLayers(place).FirstOrDefault();
            Seed = first?.Seed ?? 0UL;
            ShowSelectionInList();
            Draw();
        }

        public void Reroll() {
            Seed = new SeededStream(Seed).Fork(RerollKey).Seed;
            Draw();
        }

        private void Draw() {
            if (_seedLabel != null) {
                _seedLabel.text = SeedPrefix + Seed;
            }

            FooterText = SeedPrefix + Seed;

            if (_grid == null) {
                return;
            }

            _grid.Clear();
            _contradictions.Clear();

            if (Selected == null) {
                return;
            }

            var layers = Selected.Layers;
            var drawnLayers = 0;
            var contradictionCount = 0;

            for (var index = 0; index < layers.Count; index++) {
                if (!(layers[index] is ArrangementLayer layer)) {
                    continue;
                }

                drawnLayers++;
                var layerLine = new Label("layers[" + index + "] " + layer.GetType().Name + " '" + layer.LayerName
                                          + "'");
                layerLine.AddToClassList(LayerLineClassName);
                _grid.Add(layerLine);
                contradictionCount += DrawComposition(layer);
            }

            if (drawnLayers == 0) {
                _grid.Add(new Label(NoArrangementLayerText));
            }

            if (contradictionCount == 0) {
                _contradictions.Add(new Label(NoContradictionsText));
            }
        }

        private int DrawComposition(ArrangementLayer layer) {
            var lines = PlacesKit.Compose(layer.ToDescription(), Seed).ToText().Split('\n');
            var index = 0;

            for (; index < lines.Length; index++) {
                var line = lines[index];
                var label = FixedWidth(new Label(line));
                label.AddToClassList(GridLineClassName);

                if (line.StartsWith(StratumLinePrefix, StringComparison.Ordinal)) {
                    label.AddToClassList(StratumLineClassName);
                }

                _grid.Add(label);

                if (line.StartsWith(ContradictionsLinePrefix, StringComparison.Ordinal)) {
                    index++;
                    break;
                }
            }

            var listed = 0;

            for (; index < lines.Length; index++) {
                var label = new Label(lines[index]);
                label.AddToClassList(ContradictionLineClassName);
                _contradictions.Add(label);
                listed++;
            }

            return listed;
        }

        private void BindPlace(VisualElement element, int index) {
            ((Label)element).text = index >= 0 && index < _places.Count && _places[index] != null
                ? _places[index].name
                : string.Empty;
        }

        private void OnPlaceSelectionChanged(IEnumerable<object> selection) {
            var place = selection.OfType<PlaceDefinition>().FirstOrDefault();

            if (place != null && place != Selected) {
                Select(place);
            }
        }

        private void ShowSelectionInList() {
            if (_placeList == null) {
                return;
            }

            var index = Selected == null ? -1 : _places.IndexOf(Selected);

            if (index < 0) {
                _placeList.SetSelectionWithoutNotify(Array.Empty<int>());
            } else {
                _placeList.SetSelectionWithoutNotify(new[] { index });
            }
        }

        private static IEnumerable<ArrangementLayer> ArrangementLayers(PlaceDefinition place) {
            return place.Layers.OfType<ArrangementLayer>();
        }

        private static Label FixedWidth(Label label) {
            _fixedWidthFont ??= EditorGUIUtility.Load(FixedWidthFontPath) as Font;

            if (_fixedWidthFont != null) {
                label.style.unityFontDefinition = FontDefinition.FromFont(_fixedWidthFont);
            }

            label.style.whiteSpace = WhiteSpace.NoWrap;
            return label;
        }
    }
}
