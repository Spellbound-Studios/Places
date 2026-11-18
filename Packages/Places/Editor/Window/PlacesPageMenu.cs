// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Window.Editor;
using UnityEditor;

namespace Spellbound.Places.Editor {
    internal static class PlacesPageMenu {
        public const string MenuPath = "Spellbound/Places/Places";

        [MenuItem(MenuPath)]
        public static void OpenPlacesPage() {
            SpellboundWindow.Open(PlacesPage.PageTitle);
        }
    }
}
