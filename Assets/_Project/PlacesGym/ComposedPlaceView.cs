// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Logging;
using Spellbound.Places;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace PlacesGym {
    public class ComposedPlaceView : MonoBehaviour {
        private const float PlacedFootprint = 0.8f;
        private const float PlacedHeight = 0.3f;
        private const float MarkerFootprint = 0.7f;
        private const float VoidFootprint = 0.4f;
        private const float OpeningWidth = 0.4f;
        private const float FaceOffset = 0.01f;
        private const float LabelLift = 0.25f;
        private const int LabelFontSize = 48;
        private const float LabelCharacterSize = 0.045f;
        private const float StratumShadeStep = 0.18f;

        private static readonly HorizontalDirection[] HorizontalDirections = {
            HorizontalDirection.North, HorizontalDirection.East, HorizontalDirection.South, HorizontalDirection.West
        };

        [SerializeField] private PlaceDefinition place;
        [SerializeField] private ulong seedOverride;
        [SerializeField] private float cellSize = 2f;
        [SerializeField] private float stratumHeight = 3f;
        [SerializeField] private Material placedMaterial;
        [SerializeField] private Material emptyMaterial;
        [SerializeField] private Material openingMaterial;
        [SerializeField] private Camera labelCamera;

        private Material _createdPlacedMaterial;
        private Material _createdEmptyMaterial;
        private Material _createdOpeningMaterial;
        private Material _createdVoidMaterial;
        private Material _createdLabelMaterial;

        public PlaceDefinition Place {
            get => place;
            set => place = value;
        }

        public ulong SeedOverride {
            get => seedOverride;
            set => seedOverride = value;
        }

        public Camera LabelCamera {
            get => labelCamera;
            set => labelCamera = value;
        }

        public CompositionResult LastResult { get; private set; }

        public Bounds GridBounds() {
            var layer = ArrangementLayerOf(place);
            var gridSize = layer == null ? 1 : layer.GridSize;
            var stratumCount = layer == null ? 1 : Mathf.Max(1, layer.Strata.Count);
            var width = gridSize * cellSize;
            var height = (stratumCount - 1) * stratumHeight + stratumHeight * PlacedHeight;
            var centre = transform.position + new Vector3(0f, height * 0.5f, 0f);
            return new Bounds(centre, new Vector3(width, height, width));
        }

        [ContextMenu("Build")]
        public void Build() {
            ClearChildren();
            LastResult = null;
            var layer = ArrangementLayerOf(place);

            if (layer == null) {
                Log.Error(name + " has no place with an arrangement layer to compose", this);
                return;
            }

            var seed = seedOverride != 0 ? seedOverride : layer.Seed;
            var description = layer.ToDescription();
            var result = PlacesKit.Compose(description, seed);
            LastResult = result;

            for (var stratum = 0; stratum < result.StratumCount; stratum++) {
                var stratumRoot = new GameObject("stratum " + stratum).transform;
                stratumRoot.SetParent(transform, false);
                stratumRoot.localPosition = new Vector3(0f, (result.StratumCount - 1 - stratum) * stratumHeight, 0f);

                for (var row = 0; row < result.GridSize; row++) {
                    for (var column = 0; column < result.GridSize; column++) {
                        var cell = result.Cell(new CellAddress(column, row, stratum));
                        var centre = new Vector3((column - (result.GridSize - 1) * 0.5f) * cellSize, 0f,
                            (row - (result.GridSize - 1) * 0.5f) * cellSize);
                        BuildCell(stratumRoot, cell, centre, description);
                    }
                }
            }
        }

        private void BuildCell(Transform parent, CompositionCell cell, Vector3 centre,
            ArrangementDescription description) {
            var address = cell.Address;
            var cellName = address.column + " " + address.row;

            switch (cell.State) {
                case CompositionCellState.Placed:
                    BuildPlacedCell(parent, cell, centre, description, cellName);
                    break;
                case CompositionCellState.VoidCentre:
                    MakeFlatMarker(parent, "void " + cellName, centre, cellSize * VoidFootprint, VoidMaterial());
                    break;
                default:
                    MakeFlatMarker(parent, "empty " + cellName, centre, cellSize * MarkerFootprint,
                        Resolve(emptyMaterial, ref _createdEmptyMaterial, new Color(0.32f, 0.38f, 0.44f)));
                    break;
            }
        }

        private void BuildPlacedCell(Transform parent, CompositionCell cell, Vector3 centre,
            ArrangementDescription description, string cellName) {
            var footprint = cellSize * PlacedFootprint;
            var height = stratumHeight * PlacedHeight;
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = cell.PartName + " " + cellName;
            RemoveCollider(block);
            block.transform.SetParent(parent, false);
            block.transform.localPosition = centre + new Vector3(0f, height * 0.5f, 0f);
            block.transform.localScale = new Vector3(footprint, height, footprint);
            var blockRenderer = block.GetComponent<Renderer>();
            var material = Resolve(placedMaterial, ref _createdPlacedMaterial, new Color(0.78f, 0.7f, 0.56f));
            blockRenderer.sharedMaterial = material;
            ShadeByStratum(blockRenderer, material, cell.Address.stratum);

            if (description.TryFindPart(cell.PartName, out var part)) {
                foreach (var direction in HorizontalDirections) {
                    var socket = part.faces.Rotated(cell.RotationSteps, direction);

                    if ((socket & SocketKind.Connection) != 0) {
                        MakeOpening(parent, cell.PartName + " opening " + direction, centre, direction, footprint,
                            height);
                    }
                }
            }

            MakeLabel(parent, cell.PartName, centre + new Vector3(0f, height + LabelLift, 0f));
        }

        private void MakeOpening(Transform parent, string openingName, Vector3 centre, HorizontalDirection direction,
            float footprint, float height) {
            var outward = Outward(direction);
            var opening = GameObject.CreatePrimitive(PrimitiveType.Quad);
            opening.name = openingName;
            RemoveCollider(opening);
            opening.transform.SetParent(parent, false);
            opening.transform.localPosition =
                centre + outward * (footprint * 0.5f + FaceOffset) + new Vector3(0f, height * 0.5f, 0f);
            opening.transform.localRotation = Quaternion.LookRotation(-outward);
            opening.transform.localScale = new Vector3(footprint * OpeningWidth, height * 0.7f, 1f);
            opening.GetComponent<Renderer>().sharedMaterial =
                Resolve(openingMaterial, ref _createdOpeningMaterial, new Color(1f, 0.82f, 0.18f));
        }

        private void MakeFlatMarker(Transform parent, string markerName, Vector3 centre, float footprint,
            Material material) {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = markerName;
            RemoveCollider(marker);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = centre + new Vector3(0f, FaceOffset, 0f);
            marker.transform.localRotation = Quaternion.LookRotation(Vector3.down);
            marker.transform.localScale = new Vector3(footprint, footprint, 1f);
            marker.GetComponent<Renderer>().sharedMaterial = material;
        }

        private void MakeLabel(Transform parent, string text, Vector3 position) {
            var label = new GameObject("label " + text);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = position;

            if (labelCamera != null) {
                label.transform.rotation = labelCamera.transform.rotation;
            }

            var textMesh = label.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textMesh.font = font;
            textMesh.text = text;
            textMesh.fontSize = LabelFontSize;
            textMesh.characterSize = LabelCharacterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0.08f, 0.06f, 0.05f);
            label.GetComponent<MeshRenderer>().sharedMaterial = LabelMaterial(font);
        }

        private static void ShadeByStratum(Renderer target, Material material, int stratum) {
            var colorProperty = material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            var shade = Mathf.Max(0.3f, 1f - stratum * StratumShadeStep);
            var color = material.GetColor(colorProperty) * shade;
            color.a = 1f;
            var block = new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            block.SetColor(colorProperty, color);
            target.SetPropertyBlock(block);
        }

        private Material LabelMaterial(Font font) {
            if (_createdLabelMaterial == null) {
                var shader = Shader.Find("Sprites/Default");

                if (shader == null) {
                    return font.material;
                }

                _createdLabelMaterial = new Material(shader) {
                    mainTexture = font.material.mainTexture,
                    hideFlags = HideFlags.DontSave
                };
            }

            return _createdLabelMaterial;
        }

        private Material VoidMaterial() {
            return Resolve(null, ref _createdVoidMaterial, new Color(0.06f, 0.06f, 0.07f));
        }

        private void ClearChildren() {
            for (var index = transform.childCount - 1; index >= 0; index--) {
                var child = transform.GetChild(index).gameObject;

                if (Application.isPlaying) {
                    Destroy(child);
                } else {
                    DestroyImmediate(child);
                }
            }
        }

        private static void RemoveCollider(GameObject primitive) {
            var collider = primitive.GetComponent<Collider>();

            if (collider == null) {
                return;
            }

            if (Application.isPlaying) {
                Destroy(collider);
            } else {
                DestroyImmediate(collider);
            }
        }

        private static Vector3 Outward(HorizontalDirection direction) {
            switch (direction) {
                case HorizontalDirection.North:
                    return Vector3.forward;
                case HorizontalDirection.East:
                    return Vector3.right;
                case HorizontalDirection.South:
                    return Vector3.back;
                default:
                    return Vector3.left;
            }
        }

        private static Material Resolve(Material assigned, ref Material created, Color color) {
            if (assigned != null) {
                return assigned;
            }

            if (created == null) {
                var shader = Shader.Find("Universal Render Pipeline/Lit");

                if (shader == null) {
                    shader = Shader.Find("Standard");
                }

                created = new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
            }

            return created;
        }

        private static ArrangementLayer ArrangementLayerOf(PlaceDefinition definition) {
            if (definition == null) {
                return null;
            }

            foreach (var layer in definition.Layers) {
                if (layer is ArrangementLayer arrangement) {
                    return arrangement;
                }
            }

            return null;
        }
    }
}
