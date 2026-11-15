// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using Spellbound.Core.Authoring.Editor;

[assembly: InternalsVisibleTo("Spellbound.Places.Editor.Tests")]

namespace Spellbound.Places.Editor {
    internal static class LayerAssemblyPackage {
        internal static string Of(string assemblyName) {
            var prefix = PackageLabel.StudioPrefix;

            if (string.IsNullOrEmpty(assemblyName)
                || !assemblyName.StartsWith(prefix, StringComparison.Ordinal)
                || assemblyName.Length == prefix.Length) {
                return null;
            }

            var remainder = assemblyName.Substring(prefix.Length);
            var dot = remainder.IndexOf('.');
            return dot < 0 ? remainder : remainder.Substring(0, dot);
        }
    }
}
