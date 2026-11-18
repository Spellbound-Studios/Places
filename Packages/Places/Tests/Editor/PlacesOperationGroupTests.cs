// Copyright (c) Spellbound Studios. All rights reserved.

using System.Reflection;
using NUnit.Framework;
using Spellbound.Core.Operations;

namespace Spellbound.Places.Editor.Tests {
    public class PlacesOperationGroupTests {
        [Test]
        public void TheEditorAssemblyDeclaresTheGroupPlaces() {
            var group = typeof(PlaceDefinitionPreviewer).Assembly.GetCustomAttribute<OperationGroupAttribute>();

            Assert.IsNotNull(group);
            Assert.AreEqual("places", group.Name);
            Assert.IsTrue(group.Listed);
        }
    }
}
