// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;

namespace Spellbound.Places.Editor.Tests {
    public class LayerAssemblyPackageTests {
        [Test]
        public void AStudioTestAssemblyNamesItsPackage() {
            Assert.AreEqual("Places", LayerAssemblyPackage.Of("Spellbound.Places.Tests.CarriedFixture"));
        }

        [Test]
        public void AStudioAssemblyWithoutASecondDotIsItsOwnPackage() {
            Assert.AreEqual("Core", LayerAssemblyPackage.Of("Spellbound.Core"));
        }

        [Test]
        public void AnAssemblyOutsideTheStudioHasNoPackage() {
            Assert.IsNull(LayerAssemblyPackage.Of("Assembly-CSharp"));
        }

        [Test]
        public void AnEmptyNameOrThePrefixAloneHasNoPackage() {
            Assert.IsNull(LayerAssemblyPackage.Of(null));
            Assert.IsNull(LayerAssemblyPackage.Of(string.Empty));
            Assert.IsNull(LayerAssemblyPackage.Of("Spellbound."));
        }
    }
}
