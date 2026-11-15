// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.IO;
using Spellbound.Places;
using Spellbound.Places.Arrangement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlacesGym.Editor {
    public static class ComposedPlaceCapture {
        public const string SouthWest = "southwest";
        public const string NorthEast = "northeast";
        public const int Width = 1600;
        public const int Height = 1000;
        public const float Elevation = 45f;
        public const float FrameMargin = 1.05f;

        public static long Capture(ulong seed, string angle, string outputPath) {
            if (string.IsNullOrEmpty(outputPath)) {
                throw new ArgumentException("the capture needs an output path", nameof(outputPath));
            }

            EditorSceneManager.OpenScene(ComposedPlaceAssetBuilder.ScenePath, OpenSceneMode.Single);
            var view = Object.FindFirstObjectByType<ComposedPlaceView>();

            if (view == null) {
                throw new InvalidOperationException(ComposedPlaceAssetBuilder.ScenePath
                                                    + " has no composed place view");
            }

            var camera = view.LabelCamera != null ? view.LabelCamera : Object.FindFirstObjectByType<Camera>();

            if (camera == null) {
                throw new InvalidOperationException(ComposedPlaceAssetBuilder.ScenePath + " has no camera");
            }

            PlaceCamera(camera, view.GridBounds(), angle);
            view.LabelCamera = camera;
            view.SeedOverride = seed;
            view.Build();

            var renderTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) {
                antiAliasing = 4
            };
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;

            try {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                image.Apply();
                var bytes = image.EncodeToPNG();
                var directory = Path.GetDirectoryName(outputPath);

                if (!string.IsNullOrEmpty(directory)) {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(outputPath, bytes);
                return bytes.LongLength;
            } finally {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
                Object.DestroyImmediate(image);
            }
        }

        public static string SetPlaceSeed(ulong seed) {
            var place = AssetDatabase.LoadAssetAtPath<PlaceDefinition>(ComposedPlaceAssetBuilder.AssetPath);

            if (place == null) {
                throw new InvalidOperationException(ComposedPlaceAssetBuilder.AssetPath + " does not exist");
            }

            var serialized = new SerializedObject(place);
            var layers = serialized.FindProperty("layers");

            for (var index = 0; index < layers.arraySize; index++) {
                var layer = layers.GetArrayElementAtIndex(index);

                if (layer.managedReferenceValue is ArrangementLayer) {
                    layer.FindPropertyRelative("seed").ulongValue = seed;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.SaveAssets();
                    return ComposedPlaceAssetBuilder.AssetPath + " seed " + seed;
                }
            }

            throw new InvalidOperationException(ComposedPlaceAssetBuilder.AssetPath + " has no arrangement layer");
        }

        public static void PlaceCamera(Camera camera, Bounds bounds, string angle) {
            var yaw = YawOf(angle);
            var rotation = Quaternion.Euler(Elevation, yaw, 0f);
            var radius = bounds.extents.magnitude;
            var halfAngle = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var distance = radius / Mathf.Sin(halfAngle) * FrameMargin;
            camera.transform.rotation = rotation;
            camera.transform.position = bounds.center - rotation * Vector3.forward * distance;
        }

        private static float YawOf(string angle) {
            switch (angle) {
                case SouthWest:
                    return 45f;
                case NorthEast:
                    return 225f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(angle), angle,
                        "the angle is " + SouthWest + " or " + NorthEast);
            }
        }
    }
}
