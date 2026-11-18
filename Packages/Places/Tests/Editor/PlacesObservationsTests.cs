// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using Spellbound.Core;
using Spellbound.Core.Observations;

namespace Spellbound.Places.Editor.Tests {
    public class PlacesObservationsTests {
        private static readonly string[] OperationGroupKeys = { "places.operations", "places.operation_failures" };

        [Test]
        public void ThePackageDeclaresOneObservationUnderPlaces() {
            var declared = CoreKit.Observations.Declared()
                .Where(declaration => declaration.Key.Name.StartsWith("places.", StringComparison.Ordinal))
                .Where(declaration => !OperationGroupKeys.Contains(declaration.Key.Name))
                .ToList();

            Assert.AreEqual(1, declared.Count);
            Assert.AreEqual("places.compose.contradictions", declared[0].Key.Name);
            Assert.AreEqual(ObservationKind.Counter, declared[0].Kind);
            Assert.IsFalse(string.IsNullOrWhiteSpace(declared[0].Description));
            Assert.AreSame(PlacesObservations.CompositionContradictions.Declaration, declared[0]);
        }
    }
}
