// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Places.Owners;

namespace Spellbound.Places.Tests.CarriedFixture {
    [PlaceLayerOwner]
    public sealed class FixtureLayerOwner : IPlaceLayerOwner<FixtureCarriedLayer> {
        public static readonly List<string> StampedLayerNames = new();

        public FixtureLayerOwner() {
        }

        public static string StampedLabel { get; private set; }

        public static int StampedWeight { get; private set; }

        public static ReferenceFrameId StampedFrame { get; private set; }

        public static bool StampedPreview { get; private set; }

        public static void Clear() {
            StampedLayerNames.Clear();
            StampedLabel = null;
            StampedWeight = 0;
            StampedFrame = default;
            StampedPreview = false;
        }

        public void Stamp(FixtureCarriedLayer layer, PlaceStampContext context) {
            StampedLayerNames.Add(layer.LayerName);
            StampedLabel = layer.label;
            StampedWeight = layer.weight;
            StampedFrame = context.Frame;
            StampedPreview = context.Preview;
        }

        public FixtureCarriedLayer Capture(PlaceCaptureContext context) {
            return new FixtureCarriedLayer {
                LayerName = context.LayerName,
                label = StampedLabel,
                weight = StampedWeight
            };
        }
    }
}
