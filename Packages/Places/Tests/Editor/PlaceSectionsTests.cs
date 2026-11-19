// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.IO;
using NUnit.Framework;
using Spellbound.Core;
using Spellbound.Core.Authoring;
using Spellbound.Core.Hashing;
using Spellbound.Core.Packing;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    public class PlaceSectionsTests {
        private const string AbsentLayerType = "Spellbound.Absent.MissingLayer, Spellbound.Absent";

        private PlaceDefinition _place;
        private PlaceDefinition _into;

        [SetUp]
        public void CreatePlaces() {
            _place = ScriptableObject.CreateInstance<PlaceDefinition>();
            _place.name = "sunken-hall";
            _into = ScriptableObject.CreateInstance<PlaceDefinition>();
        }

        [TearDown]
        public void DestroyPlaces() {
            UnityEngine.Object.DestroyImmediate(_place);
            UnityEngine.Object.DestroyImmediate(_into);
        }

        [Test]
        public void APlaceOfTwoPackableLayersRoundTripsInOrder() {
            _place.AddLayer(new PackableTestLayer { LayerName = "floor", Value = 417 });
            _place.AddLayer(new PackableTestLayer { LayerName = "ceiling", Value = -9 });
            var report = new ValidationReport("pack");

            Assert.IsTrue(PlaceSections.TryPack(_place, report, out var section), report.ToText());
            Assert.IsTrue(report.IsEmpty, report.ToText());
            Assert.AreEqual("sunken-hall", section.PlaceName);

            var bytes = PackSection(section);
            var unpackReport = new ValidationReport("unpack");

            Assert.IsTrue(PlaceSections.TryUnpack(bytes, _into, unpackReport), unpackReport.ToText());
            Assert.IsTrue(unpackReport.IsEmpty, unpackReport.ToText());
            Assert.AreEqual("sunken-hall", _into.name);
            Assert.AreEqual(2, _into.Layers.Count);
            var floor = (PackableTestLayer)_into.Layers[0];
            var ceiling = (PackableTestLayer)_into.Layers[1];
            Assert.AreEqual("floor", floor.LayerName);
            Assert.AreEqual(417, floor.Value);
            Assert.AreEqual("ceiling", ceiling.LayerName);
            Assert.AreEqual(-9, ceiling.Value);
            Assert.AreNotSame(_place.Layers[0], floor);
        }

        [Test]
        public void AnEmptyPlaceRoundTrips() {
            Assert.IsTrue(PlaceSections.TryPack(_place, new ValidationReport("pack"), out var section));

            Assert.IsTrue(PlaceSections.TryUnpack(PackSection(section), _into, new ValidationReport("unpack")));
            Assert.AreEqual("sunken-hall", _into.name);
            Assert.AreEqual(0, _into.Layers.Count);
        }

        [Test]
        public void TheSectionUnpacksThroughCorePacking() {
            _place.AddLayer(new PackableTestLayer { LayerName = "floor", Value = 3 });
            Assert.IsTrue(PlaceSections.TryPack(_place, new ValidationReport("pack"), out var section));

            Assert.IsTrue(CoreKit.Packing.TryUnpack<PlaceSection>(PackSection(section), out var unpacked));
            Assert.AreEqual("sunken-hall", unpacked.PlaceName);
        }

        [Test]
        public void AnEmptySlotRefusesThePackNamingTheSlot() {
            _place.AddLayer(new PackableTestLayer { LayerName = "floor" });
            _place.AddLayer(new PackableTestLayer { LayerName = "lost" });
            _place.AddLayer(new PackableTestLayer { LayerName = "ceiling" });
            var serialized = new SerializedObject(_place);
            serialized.FindProperty("layers").GetArrayElementAtIndex(1).managedReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsNull(_place.Layers[1]);
            var report = new ValidationReport("pack");

            Assert.IsFalse(PlaceSections.TryPack(_place, report, out var section));
            Assert.IsNull(section);
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            Assert.AreEqual(PlaceFindingCodes.LayerNotInstalled, report.Findings[0].Code);
            Assert.AreEqual(FindingSeverity.Error, report.Findings[0].Severity);
            Assert.AreEqual("layers[1]", report.Findings[0].Location);
        }

        [Test]
        public void ALayerThatIsNotPackableRefusesThePackNamingTheLayer() {
            _place.AddLayer(new PackableTestLayer { LayerName = "floor" });
            _place.AddLayer(new OwnedTestLayer { LayerName = "owned" });
            var report = new ValidationReport("pack");

            Assert.IsFalse(PlaceSections.TryPack(_place, report, out var section));
            Assert.IsNull(section);
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            Assert.AreEqual(PlaceFindingCodes.LayerNotPackable, report.Findings[0].Code);
            Assert.AreEqual("layers[1]", report.Findings[0].Location);
            StringAssert.Contains("'owned'", report.Findings[0].Message);
            StringAssert.Contains(nameof(OwnedTestLayer), report.Findings[0].Message);
        }

        [Test]
        public void AHandBuiltStreamOfTheCurrentLayoutUnpacks() {
            var bytes = HandBuiltStream(typeof(PackableTestLayer).AssemblyQualifiedName,
                Fingerprint(typeof(PackableTestLayer)), "floor", 25);
            var report = new ValidationReport("unpack");

            Assert.IsTrue(PlaceSections.TryUnpack(bytes, _into, report), report.ToText());
            Assert.AreEqual(1, _into.Layers.Count);
            Assert.AreEqual("floor", _into.Layers[0].LayerName);
            Assert.AreEqual(25, ((PackableTestLayer)_into.Layers[0]).Value);
        }

        [Test]
        public void ADifferentFingerprintRefusesTheUnpackNamingTheLayer() {
            var bytes = HandBuiltStream(typeof(PackableTestLayer).AssemblyQualifiedName,
                Fingerprint(typeof(PackableTestLayer)) + 1u, "floor", 25);
            var report = new ValidationReport("unpack");

            Assert.IsFalse(PlaceSections.TryUnpack(bytes, _into, report));
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            Assert.AreEqual(PlaceFindingCodes.LayerLayoutOutdated, report.Findings[0].Code);
            Assert.AreEqual("layers[0]", report.Findings[0].Location);
            StringAssert.Contains("'floor'", report.Findings[0].Message);
            Assert.AreEqual(0, _into.Layers.Count);
            Assert.AreEqual(string.Empty, _into.name);
        }

        [Test]
        public void AStoredTypeTheProjectLacksRefusesTheUnpackNamingTheType() {
            var bytes = HandBuiltStream(AbsentLayerType, 7u, "floor", 25);
            var report = new ValidationReport("unpack");

            Assert.IsFalse(PlaceSections.TryUnpack(bytes, _into, report));
            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            Assert.AreEqual(PlaceFindingCodes.LayerNotInstalled, report.Findings[0].Code);
            Assert.AreEqual("layers[0]", report.Findings[0].Location);
            StringAssert.Contains(AbsentLayerType, report.Findings[0].Message);
            Assert.AreEqual(0, _into.Layers.Count);
        }

        [Test]
        public void UnpackAloneThrowsOnADifferentFingerprint() {
            var bytes = HandBuiltStream(typeof(PackableTestLayer).AssemblyQualifiedName,
                Fingerprint(typeof(PackableTestLayer)) + 1u, "floor", 25);

            Assert.Throws<InvalidDataException>(() => UnpackSection(bytes));
        }

        [Test]
        public void UnpackAloneThrowsOnAStoredTypeTheProjectLacks() {
            var bytes = HandBuiltStream(AbsentLayerType, 7u, "floor", 25);

            Assert.Throws<InvalidDataException>(() => UnpackSection(bytes));
        }

        [Test]
        public void ArgumentsAreRequired() {
            var report = new ValidationReport("arguments");

            Assert.Throws<ArgumentNullException>(() => PlaceSections.TryPack(null, report, out _));
            Assert.Throws<ArgumentNullException>(() => PlaceSections.TryPack(_place, null, out _));
            Assert.Throws<ArgumentNullException>(() => PlaceSections.TryUnpack(Array.Empty<byte>(), null, report));
            Assert.Throws<ArgumentNullException>(() => PlaceSections.TryUnpack(Array.Empty<byte>(), _into, null));
        }

        private static byte[] PackSection(PlaceSection section) {
            var bytes = new byte[4096];
            Assert.IsTrue(CoreKit.Packing.TryPack(section, bytes, out var written));
            return new ReadOnlySpan<byte>(bytes, 0, written).ToArray();
        }

        private static void UnpackSection(byte[] bytes) {
            var reader = new ReadOnlySpan<byte>(bytes);
            new PlaceSection().Unpack(ref reader);
        }

        private static uint Fingerprint(Type layerType) {
            var description = CoreKit.Packing.Describe(layerType);
            var firstLineEnd = description.IndexOf('\n');
            return StableHash.Fnv1A32(firstLineEnd < 0 ? string.Empty : description.Substring(firstLineEnd + 1));
        }

        private static byte[] HandBuiltStream(string layerType, uint fingerprint, string layerName, int value) {
            var bytes = new byte[4096];
            var writer = new Span<byte>(bytes);
            Packer.WriteString(ref writer, "sunken-hall");
            Packer.WriteInt(ref writer, 1);
            Packer.WriteString(ref writer, layerType);
            Packer.WriteUInt(ref writer, fingerprint);
            Packer.WriteString(ref writer, layerName);
            Packer.WriteInt(ref writer, value);
            return new ReadOnlySpan<byte>(bytes, 0, bytes.Length - writer.Length).ToArray();
        }
    }
}
