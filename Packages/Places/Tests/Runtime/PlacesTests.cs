// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;

namespace Spellbound.Places.Tests {
    public class PlacesTests {
        [Test]
        public void FrontDoorTypeExists() {
            Assert.IsNotNull(typeof(PlacesKit));
        }
    }
}
