// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Packing;
using Spellbound.Core.Sections;

namespace Spellbound.Places {
    [Section("places.place")]
    public sealed class PlaceSection : IPacker {
        private string _placeName;

        public PlaceSection() { }

        public PlaceSection(string placeName) {
            _placeName = placeName;
        }

        public string PlaceName => _placeName ?? string.Empty;

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteString(ref buffer, PlaceName);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            _placeName = Packer.ReadString(ref buffer);
        }
    }
}
