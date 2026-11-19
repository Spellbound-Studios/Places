// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Spellbound.Core.Authoring;
using Spellbound.Core.Packing;
using Spellbound.Core.Sections;

namespace Spellbound.Places {
    [Section("places.place")]
    public sealed class PlaceSection : IPacker {
        private const string SectionName = "places.place";

        private readonly List<PlaceLayer> _layers = new();
        private string _placeName;

        public PlaceSection() { }

        public PlaceSection(string placeName) {
            _placeName = placeName;
        }

        internal PlaceSection(string placeName, IEnumerable<PlaceLayer> layers) : this(placeName) {
            _layers.AddRange(layers);
        }

        public string PlaceName => _placeName ?? string.Empty;

        internal IReadOnlyList<PlaceLayer> Layers => _layers;

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteString(ref buffer, PlaceName);
            Packer.WriteInt(ref buffer, _layers.Count);

            foreach (var layer in _layers) {
                var layerType = layer.GetType();
                Packer.WriteString(ref buffer, layerType.AssemblyQualifiedName);
                Packer.WriteUInt(ref buffer, PlaceLayerLayouts.Fingerprint(layerType));
                Packer.WriteString(ref buffer, layer.LayerName ?? string.Empty);
                ((IPacker)layer).Pack(ref buffer);
            }
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            var report = new ValidationReport(SectionName);

            if (!TryRead(ref buffer, report)) {
                throw new InvalidDataException(report.Findings[0].Message);
            }
        }

        internal bool TryRead(ref ReadOnlySpan<byte> buffer, ValidationReport report) {
            var placeName = Packer.ReadString(ref buffer);
            var layerCount = Packer.ReadInt(ref buffer);

            if (layerCount < 0) {
                throw new InvalidDataException(SectionName + " stores a layer count of " + layerCount);
            }

            var layers = new List<PlaceLayer>(layerCount);

            for (var index = 0; index < layerCount; index++) {
                var location = "layers[" + index + "]";
                var storedTypeName = Packer.ReadString(ref buffer);
                var storedFingerprint = Packer.ReadUInt(ref buffer);
                var layerName = Packer.ReadString(ref buffer);
                var layerType = PlaceLayerLayouts.FindLayerType(storedTypeName);

                if (layerType == null) {
                    report.Error(PlaceFindingCodes.LayerNotInstalled,
                        location + " '" + layerName + "' stores the layer type '" + storedTypeName
                        + "', which is not installed in this project", location);
                    return false;
                }

                if (!PlaceLayerLayouts.IsPackable(layerType)) {
                    report.Error(PlaceFindingCodes.LayerNotPackable,
                        location + " '" + layerName + "' is a " + layerType.FullName
                        + ", which does not pack itself: a packable layer implements IPacker and has a public"
                        + " parameterless constructor", location);
                    return false;
                }

                if (storedFingerprint != PlaceLayerLayouts.Fingerprint(layerType)) {
                    report.Error(PlaceFindingCodes.LayerLayoutOutdated,
                        location + " '" + layerName + "' was packed with another layout of " + layerType.FullName
                        + " than the one installed, so it cannot be read", location);
                    return false;
                }

                var layer = (PlaceLayer)Activator.CreateInstance(layerType);
                layer.LayerName = layerName;
                ((IPacker)layer).Unpack(ref buffer);
                layers.Add(layer);
            }

            _placeName = placeName;
            _layers.Clear();
            _layers.AddRange(layers);
            return true;
        }
    }
}
