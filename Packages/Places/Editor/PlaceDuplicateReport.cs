// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Places.Editor {
    public sealed class PlaceDuplicateReport {
        public PlaceDuplicateReport(string copyPath, IReadOnlyList<LayerRename> renames,
            IReadOnlyList<string> carriedLayers) {
            if (string.IsNullOrEmpty(copyPath)) {
                throw new ArgumentException("A duplicate report needs the copy's path", nameof(copyPath));
            }

            CopyPath = copyPath;
            Renames = renames ?? Array.Empty<LayerRename>();
            CarriedLayers = carriedLayers ?? Array.Empty<string>();
        }

        public string CopyPath { get; }

        public IReadOnlyList<LayerRename> Renames { get; }

        public IReadOnlyList<string> CarriedLayers { get; }
    }
}
