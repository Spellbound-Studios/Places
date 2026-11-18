// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Places.Editor {
    public readonly struct LayerRename {
        public LayerRename(string previousName, string newName) {
            PreviousName = previousName;
            NewName = newName;
        }

        public string PreviousName { get; }

        public string NewName { get; }
    }
}
