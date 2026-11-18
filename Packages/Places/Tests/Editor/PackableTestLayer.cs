// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;
using Spellbound.Core.Packing;
using UnityEngine;

namespace Spellbound.Places.Editor.Tests {
    [Serializable]
    public sealed class PackableTestLayer : PlaceLayer, IPacker {
        [SerializeField] private int value;

        public int Value {
            get => value;
            set => this.value = value;
        }

        public override void Validate(ValidationReport report, string location) {
        }

        public void Pack(ref Span<byte> buffer) {
            Packer.WriteInt(ref buffer, value);
        }

        public void Unpack(ref ReadOnlySpan<byte> buffer) {
            value = Packer.ReadInt(ref buffer);
        }
    }
}
