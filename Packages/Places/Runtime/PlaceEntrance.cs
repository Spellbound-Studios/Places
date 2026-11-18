// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Places.Arrangement;
using UnityEngine;

namespace Spellbound.Places {
    [Serializable]
    public sealed class PlaceEntrance {
        [SerializeField] private string layerName = "";
        [SerializeField] private CellAddress cell;
        [SerializeField] private HorizontalDirection facing;

        public string LayerName {
            get => layerName;
            set => layerName = value;
        }

        public CellAddress Cell {
            get => cell;
            set => cell = value;
        }

        public HorizontalDirection Facing {
            get => facing;
            set => facing = value;
        }

        public bool IsSet => !string.IsNullOrEmpty(layerName);
    }
}
