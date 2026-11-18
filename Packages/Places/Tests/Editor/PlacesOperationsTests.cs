// Copyright (c) Spellbound Studios. All rights reserved.

using System.Linq;
using NUnit.Framework;
using Spellbound.Core;
using Spellbound.Core.Observations;
using Spellbound.Core.Operations;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class PlacesOperationsTests {
        private const string ContradictionsLinePrefix = "contradictions: ";
        private const string SucceededPrefix = "succeeded: ";

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
        public void ComposePrintsTheGridAndTheContradictionsForTheSeed() {
            var place = CreatePlace("Composed", ArrangementLayerValidationTests.CleanLayer());
            var layer = (ArrangementLayer)place.Layers[0];

            var lines = SucceededLines("places compose --target asset:" + PathOf(place) + " --seed 7");

            Assert.AreEqual("layers[0] ArrangementLayer 'arrangement' seed 7", lines[0]);
            Assert.AreEqual(PlacesKit.Compose(layer.ToDescription(), 7UL).ToText(),
                string.Join("\n", lines.Skip(1)));
        }

        [Test]
        public void ComposeAtTheLayerSeedMatchesThePreviewerGrid() {
            var place = CreatePlace("Previewed", ArrangementLayerValidationTests.CleanLayer());
            var layer = (ArrangementLayer)place.Layers[0];

            var lines = SucceededLines("places compose --target asset:" + PathOf(place) + " --seed " + layer.Seed);
            var preview = new PlaceDefinitionPreviewer().Preview(place);

            Assert.AreEqual(preview.Lines[0] + " seed " + layer.Seed, lines[0]);
            CollectionAssert.AreEqual(preview.Lines.Skip(1).ToArray(),
                lines.Skip(1).Take(preview.Lines.Count - 1).ToArray(), string.Join("\n", lines));
            StringAssert.StartsWith(ContradictionsLinePrefix, preview.Lines[preview.Lines.Count - 1]);
        }

        [Test]
        public void ComposeLeavesTheAssetSeedAlone() {
            var layer = ArrangementLayerValidationTests.CleanLayer();
            layer.Seed = 9UL;
            var place = CreatePlace("Seeded", layer);

            SucceededLines("places compose --target asset:" + PathOf(place) + " --seed 7");

            Assert.AreEqual(9UL, layer.Seed);
            Assert.AreEqual(9UL, ((ArrangementLayer)place.Layers[0]).Seed);
        }

        [Test]
        public void ComposeRecordsTheContradictionsCount() {
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
            var expected = PlacesKit.Compose(layer.ToDescription(), 5UL).Contradictions.Count;
            Assert.Greater(expected, 0);

            using (var observations = CoreKit.Observations.CreateContext("places-compose-observation-tests")) {
                var run = CoreKit.Operations.Run("places compose --target asset:" + PathOf(place) + " --seed 5",
                    OperationCaller.Development(), observations);

                Assert.IsTrue(run.IsDone);
                Assert.IsTrue(run.Result.Succeeded, run.Result.Text);
                Assert.IsTrue(observations.TryRead(ObservationKey.Parse("places.compose.contradictions"),
                    out var reading));
                Assert.AreEqual(expected, reading.Total);
                StringAssert.Contains(ContradictionsLinePrefix + expected, run.Result.Text);
            }
        }

        [Test]
        public void ComposeWithoutAnArrangementLayerIsInvalid() {
            var place = CreatePlace("Unarranged", new HarvestableTestLayer { LayerName = "ore" });

            var run = Run("places compose --target asset:" + PathOf(place) + " --seed 7");

            Assert.AreEqual(OperationStatus.Invalid, run.Status, run.Text);
            StringAssert.Contains("Unarranged", run.Text);
            StringAssert.Contains("holds no arrangement layer", run.Text);
        }

        [Test]
        public void PartsListsEveryPartWithSixFaces() {
            var layer = ArrangementLayerValidationTests.CleanLayer();
            layer.AddPart(new PartDescription {
                partName = "open-above",
                faces = new PartFaces(SocketKind.NoConnection, SocketKind.Connection, (SocketKind)(-1),
                    SocketKind.NoConnection | SocketKind.Connection, SocketKind.SealedConnection,
                    SocketKind.NoConnection)
            });
            var place = CreatePlace("Listed", new HarvestableTestLayer { LayerName = "ore" }, layer);

            var lines = SucceededLines("places parts --target asset:" + PathOf(place));

            CollectionAssert.AreEqual(new[] {
                "layers[1] ArrangementLayer 'arrangement': 3 parts",
                "corridor east Connection west Connection up NoConnection down NoConnection north NoConnection"
                + " south NoConnection",
                "end east NoConnection west NoConnection up NoConnection down NoConnection north NoConnection"
                + " south NoConnection",
                "open-above east NoConnection west Connection up Any down NoConnection|Connection"
                + " north SealedConnection south NoConnection"
            }, lines, string.Join("\n", lines));

            foreach (var line in lines.Skip(1)) {
                var words = line.Split(' ');

                foreach (var face in new[] { "east", "west", "up", "down", "north", "south" }) {
                    Assert.AreEqual(1, words.Count(word => word == face), line);
                }
            }
        }

        [Test]
        public void PartsListsAPageOfTwoFromAnOffset() {
            var layer = ArrangementLayerValidationTests.CleanLayer();
            layer.AddPart(new PartDescription {
                partName = "open-above",
                faces = new PartFaces(SocketKind.NoConnection, SocketKind.Connection, SocketKind.Connection,
                    SocketKind.NoConnection, SocketKind.NoConnection, SocketKind.NoConnection)
            });
            layer.AddPart(new PartDescription {
                partName = "open-below",
                faces = new PartFaces(SocketKind.NoConnection, SocketKind.Connection, SocketKind.NoConnection,
                    SocketKind.Connection, SocketKind.NoConnection, SocketKind.NoConnection)
            });
            var place = CreatePlace("Paged", layer);

            var lines = SucceededLines("places parts --target asset:" + PathOf(place) + " --offset 1 --limit 2");

            CollectionAssert.AreEqual(new[] {
                "layers[0] ArrangementLayer 'arrangement': 4 parts",
                "end east NoConnection west NoConnection up NoConnection down NoConnection north NoConnection"
                + " south NoConnection",
                "open-above east NoConnection west Connection up Connection down NoConnection north NoConnection"
                + " south NoConnection"
            }, lines, string.Join("\n", lines));
        }

        [Test]
        public void PartsWithoutAnArrangementLayerIsInvalid() {
            var place = CreatePlace("Partless", new HarvestableTestLayer { LayerName = "ore" });

            var run = Run("places parts --target asset:" + PathOf(place));

            Assert.AreEqual(OperationStatus.Invalid, run.Status, run.Text);
            StringAssert.Contains("Partless", run.Text);
            StringAssert.Contains("holds no arrangement layer", run.Text);
        }

        [Test]
        public void HelpListsBothOperationsUnderPlaces() {
            var run = Run("core help --group places");

            Assert.IsTrue(run.Succeeded, run.Text);
            StringAssert.Contains("compose", run.Text);
            StringAssert.Contains("parts", run.Text);
        }

        private PlaceDefinition CreatePlace(string placeName, params PlaceLayer[] layers) {
            var place = ScriptableObject.CreateInstance<PlaceDefinition>();

            foreach (var layer in layers) {
                place.AddLayer(layer);
            }

            AssetDatabase.CreateAsset(place, _folder.Path(placeName + ".asset"));
            return place;
        }

        private static string PathOf(PlaceDefinition place) {
            return AssetDatabase.GetAssetPath(place);
        }

        private static OperationResult Run(string line) {
            var run = CoreKit.Operations.Run(line, OperationCaller.Development(), ObservationContext.Discard);
            Assert.IsTrue(run.IsDone);
            return run.Result;
        }

        private static string[] SucceededLines(string line) {
            var result = Run(line);
            Assert.IsTrue(result.Succeeded, result.Text);
            StringAssert.StartsWith(SucceededPrefix, result.Text);
            return result.Text.Substring(SucceededPrefix.Length).Split('\n');
        }
    }
}
