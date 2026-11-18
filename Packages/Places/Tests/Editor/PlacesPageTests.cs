// Copyright (c) Spellbound Studios. All rights reserved.

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Simulation;
using Spellbound.Core.Window.Editor;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Places.Editor.Tests {
    public class PlacesPageTests {
        private const string ContradictionsLinePrefix = "contradictions: ";

        private PlacesTestFolder _folder;

        [SetUp]
        public void CreateFolder() {
            _folder = new PlacesTestFolder();
        }

        [TearDown]
        public void DeleteFolder() {
            _folder.Dispose();
        }

        [Test]
        public void ThePageIsFoundByTitle() {
            WindowPages.Reset();

            var description = WindowPages.Default.Find("Places");

            Assert.IsNotNull(description);
            Assert.AreEqual(typeof(PlacesPage), description.PageType);
        }

        [Test]
        public void TheGridMatchesThePreviewerForTheLayerSeed() {
            var place = CreatePlace("Previewed", ArrangementLayerValidationTests.CleanLayer());
            var layer = (ArrangementLayer)place.Layers[0];
            var page = new PlacesPage();
            var root = page.CreateContent();

            page.Refresh();
            page.Select(place);

            Assert.AreEqual(layer.Seed, page.Seed);
            CollectionAssert.AreEqual(PreviewGridLines(place), GridLines(root), string.Join("\n", GridLines(root)));
            CollectionAssert.AreEqual(new[] { "layers[0] ArrangementLayer 'arrangement'" }, LayerLines(root));
            Assert.AreEqual("stratum 0", StratumLines(root).Single());
            Assert.AreSame(place, page.Selected);
            CollectionAssert.Contains(page.Places, place);
        }

        [Test]
        public void RerollMovesTheSeedThroughFork() {
            var place = CreatePlace("Rolled", ArrangementLayerValidationTests.CleanLayer());
            var layerSeed = ((ArrangementLayer)place.Layers[0]).Seed;
            var page = new PlacesPage();
            var root = page.CreateContent();
            page.Refresh();
            page.Select(place);

            page.Reroll();

            var rerolled = new SeededStream(layerSeed).Fork(1).Seed;
            var copyLayer = ArrangementLayerValidationTests.CleanLayer();
            copyLayer.Seed = rerolled;
            var copy = CreatePlace("RolledCopy", copyLayer);
            Assert.AreEqual(rerolled, page.Seed);
            Assert.AreEqual("seed " + rerolled, root.Q<Label>("places-seed").text);
            Assert.AreEqual("seed " + rerolled, page.FooterText);
            CollectionAssert.AreEqual(PreviewGridLines(copy), GridLines(root), string.Join("\n", GridLines(root)));
        }

        [Test]
        public void RerollLeavesTheAssetAlone() {
            var place = CreatePlace("Untouched", ArrangementLayerValidationTests.CleanLayer());
            var layer = (ArrangementLayer)place.Layers[0];
            var startingSeed = layer.Seed;
            var page = new PlacesPage();
            page.CreateContent();
            page.Refresh();
            page.Select(place);

            page.Reroll();
            page.Reroll();

            Assert.AreEqual(startingSeed, layer.Seed);
            Assert.AreEqual(startingSeed, ((ArrangementLayer)place.Layers[0]).Seed);
            Assert.IsFalse(EditorUtility.IsDirty(place));
            Assert.AreNotEqual(startingSeed, page.Seed);
        }

        [Test]
        public void TheContradictionsAreListedWithTheirReasons() {
            var layer = new ArrangementLayer { LayerName = "corridors", Seed = 5UL };
            ArrangementLayerValidationTests.SetGridSize(layer, 3);
            layer.AddPart(new PartDescription {
                partName = "corridor",
                faces = new PartFaces(SocketKind.Connection, SocketKind.Connection, SocketKind.NoConnection,
                    SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection)
            });
            var stratum = new StratumDescription();
            stratum.pool.Add(new PoolEntry { partName = "corridor", weight = 1f });
            layer.AddStratum(stratum);
            var place = CreatePlace("Contradicted", layer);
            var text = PlacesKit.Compose(layer.ToDescription(), 5UL).ToText().Split('\n');
            var expected = text.SkipWhile(line => !line.StartsWith(ContradictionsLinePrefix)).Skip(1).ToArray();
            Assert.Greater(expected.Length, 0);
            var page = new PlacesPage();
            var root = page.CreateContent();

            page.Refresh();
            page.Select(place);

            CollectionAssert.AreEqual(expected, ContradictionLines(root), string.Join("\n", ContradictionLines(root)));
        }

        [Test]
        public void APlaceWithoutAnArrangementLayerSaysSo() {
            var place = CreatePlace("Unarranged", new HarvestableTestLayer { LayerName = "ore" });
            var page = new PlacesPage();
            var root = page.CreateContent();

            page.Refresh();
            page.Select(place);

            Assert.AreEqual(0UL, page.Seed);
            CollectionAssert.AreEqual(new[] { "no arrangement layer" },
                root.Q("places-grid").Query<Label>().ToList().Select(label => label.text).ToArray());
            CollectionAssert.AreEqual(new[] { "no contradictions" },
                root.Q("places-contradictions").Query<Label>().ToList().Select(label => label.text).ToArray());
        }

        [Test]
        public void TheMenuPathIsUnderSpellboundPlaces() {
            var menuItems = typeof(PlacesPageMenu)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SelectMany(method => method.GetCustomAttributes<MenuItem>())
                .Select(attribute => attribute.menuItem)
                .ToArray();

            CollectionAssert.AreEqual(new[] { "Spellbound/Places/Places" }, menuItems);
        }

        private PlaceDefinition CreatePlace(string placeName, params PlaceLayer[] layers) {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();

            foreach (var layer in layers) {
                place.AddLayer(layer);
            }

            AssetDatabase.CreateAsset(place, _folder.Path(placeName + ".asset"));
            AssetDatabase.SaveAssetIfDirty(place);
            return place;
        }

        private static string[] PreviewGridLines(PlaceDefinition place) {
            return new PlaceDefinitionPreviewer().Preview(place).Lines.Skip(1).ToArray();
        }

        private static string[] GridLines(VisualElement root) {
            return Texts(root.Q("places-grid"), "places-grid-line");
        }

        private static string[] LayerLines(VisualElement root) {
            return Texts(root.Q("places-grid"), "places-layer-line");
        }

        private static string[] StratumLines(VisualElement root) {
            return Texts(root.Q("places-grid"), "places-stratum-line");
        }

        private static string[] ContradictionLines(VisualElement root) {
            return Texts(root.Q("places-contradictions"), "places-contradiction-line");
        }

        private static string[] Texts(VisualElement container, string className) {
            return container.Query<Label>(className: className).ToList().Select(label => label.text).ToArray();
        }
    }
}
