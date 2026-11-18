// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Core;
using Spellbound.Core.Authoring;
using Spellbound.Core.Sections;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceSectionTests {
        [Test]
        public void SectionNameIsAFindingCode() {
            var section = typeof(PlaceSection).GetCustomAttribute<SectionAttribute>();

            Assert.IsNotNull(section);
            Assert.AreEqual("places.place", section.Name);
            Assert.IsTrue(ValidationFinding.IsCode(section.Name));
        }

        [Test]
        public void NamedSectionRoundTripsItsPlaceName() {
            var packed = new PlaceSection("sunken-hall");
            var bytes = new byte[256];

            Assert.IsTrue(CoreKit.Packing.TryPack(packed, bytes, out var written));
            Assert.IsTrue(CoreKit.Packing.TryUnpack<PlaceSection>(new ReadOnlySpan<byte>(bytes, 0, written),
                out var unpacked));
            Assert.AreEqual("sunken-hall", unpacked.PlaceName);
        }

        [Test]
        public void UnpackReadsWhatPackWrote() {
            var bytes = new byte[256];
            var writer = new Span<byte>(bytes);
            new PlaceSection("sunken-hall").Pack(ref writer);
            var reader = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - writer.Length);
            var unpacked = new PlaceSection();

            unpacked.Unpack(ref reader);

            Assert.AreEqual("sunken-hall", unpacked.PlaceName);
            Assert.AreEqual(0, reader.Length);
        }

        [Test]
        public void ParameterlessSectionRoundTripsAnEmptyPlaceName() {
            var packed = new PlaceSection();
            var bytes = new byte[256];

            Assert.AreEqual(string.Empty, packed.PlaceName);
            Assert.IsTrue(CoreKit.Packing.TryPack(packed, bytes, out var written));
            Assert.IsTrue(CoreKit.Packing.TryUnpack<PlaceSection>(new ReadOnlySpan<byte>(bytes, 0, written),
                out var unpacked));
            Assert.AreEqual(string.Empty, unpacked.PlaceName);
        }
    }
}
