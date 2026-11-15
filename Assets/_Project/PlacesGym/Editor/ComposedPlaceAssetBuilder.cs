// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Spellbound.Places;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PlacesGym.Editor {
    public static class ComposedPlaceAssetBuilder {
        public const string AssetPath = "Assets/_Project/PlacesGym/ComposedPlace.asset";
        public const string ScenePath = "Assets/_Project/PlacesGym/PlacesGym.unity";
        public const string LayerName = "arrangement";
        public const int GridSize = 7;
        public const ulong FixedSeed = 1UL;

        public static string Build() {
            var folder = Path.Combine(Application.dataPath, "_Project", "PlacesConvergence");
            var parts = ReadParts(Path.Combine(folder, "Parts.txt"));
            var centrePool = new List<PoolEntry>();
            var strata = ReadLivePools(Path.Combine(folder, "LivePools.txt"), centrePool);

            var place = ScriptableObject.CreateInstance<PlaceDefinition>();
            var layer = place.AddLayer(new ArrangementLayer { LayerName = LayerName, Seed = FixedSeed });

            if (layer.GridSize != GridSize) {
                throw new InvalidOperationException("a new arrangement layer has grid size " + layer.GridSize
                    + "; the composed place needs " + GridSize);
            }

            ((List<PartDescription>)layer.Parts).AddRange(parts);
            ((List<StratumDescription>)layer.Strata).AddRange(strata);
            ((List<PoolEntry>)layer.CentrePool).AddRange(centrePool);

            var existing = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(AssetPath);

            if (existing != null) {
                EditorUtility.CopySerialized(place, existing);
                Object.DestroyImmediate(place);
                EditorUtility.SetDirty(existing);
            } else {
                AssetDatabase.CreateAsset(place, AssetPath);
            }

            AssetDatabase.SaveAssets();
            return AssetPath + ": " + parts.Count + " parts, " + strata.Count + " strata, " + centrePool.Count
                   + " centre entries, seed " + FixedSeed;
        }

        public static string BuildScene() {
            var place = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(AssetPath);

            if (place == null) {
                throw new InvalidOperationException(AssetPath + " does not exist; build the place first");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);

            var cameraObject = new GameObject("Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 500f;

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            var viewObject = new GameObject("Composed Place");
            var view = viewObject.AddComponent<ComposedPlaceView>();
            view.Place = place;
            view.LabelCamera = camera;

            ComposedPlaceCapture.PlaceCamera(camera, view.GridBounds(), ComposedPlaceCapture.SouthWest);

            if (!EditorSceneManager.SaveScene(scene, ScenePath)) {
                throw new InvalidOperationException("the scene did not save to " + ScenePath);
            }

            return ScenePath;
        }

        private static List<PartDescription> ReadParts(string path) {
            var parts = new List<PartDescription>();

            foreach (var line in File.ReadAllLines(path)) {
                var fields = Fields(line);

                if (fields.Length == 0) {
                    continue;
                }

                if (fields.Length != 7) {
                    throw new FormatException("a part line needs a name and six faces: " + line);
                }

                parts.Add(new PartDescription {
                    partName = fields[0],
                    faces = new PartFaces(Face(fields[1]), Face(fields[2]), Face(fields[3]), Face(fields[4]),
                        Face(fields[5]), Face(fields[6]))
                });
            }

            return parts;
        }

        private static List<StratumDescription> ReadLivePools(string path, List<PoolEntry> centrePool) {
            var strata = new List<StratumDescription>();
            StratumDescription current = null;

            foreach (var line in File.ReadAllLines(path)) {
                var fields = Fields(line);

                if (fields.Length == 0) {
                    continue;
                }

                switch (fields[0]) {
                    case "centre":
                        centrePool.Add(new PoolEntry { partName = fields[1], weight = Weight(fields[2]) });
                        break;
                    case "stratum":
                        current = new StratumDescription();
                        strata.Add(current);
                        break;
                    case "pool":
                        RequireStratum(current, line).pool.Add(new PoolEntry {
                            partName = fields[1],
                            weight = Weight(fields[2])
                        });
                        break;
                    case "oneof":
                        RequireStratum(current, line).oneOfParts.Add(fields[1]);
                        break;
                    default:
                        throw new FormatException("an unknown live pool line: " + line);
                }
            }

            return strata;
        }

        private static StratumDescription RequireStratum(StratumDescription stratum, string line) {
            if (stratum == null) {
                throw new FormatException("a pool line before the first stratum: " + line);
            }

            return stratum;
        }

        private static string[] Fields(string line) {
            return line.Split(new[] { ' ', '\t', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static SocketKind Face(string text) {
            return (SocketKind)int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static float Weight(string text) {
            return float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
