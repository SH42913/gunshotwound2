namespace GunshotWound2.InventoryFeature {
    using System;

    public readonly struct ItemTemplate : IEquatable<ItemTemplate> {
        public readonly string internalName;
        public readonly string progressDescriptionKey;
        public readonly float duration;
        public readonly InventoryFeature.ItemAction startAction;
        public readonly InventoryFeature.ItemAction progressAction;
        public readonly InventoryFeature.ItemAction finishAction;

        public bool IsValid => !string.IsNullOrEmpty(internalName);

        public ItemTemplate(string internalName,
                            string progressDescriptionKey,
                            float duration,
                            InventoryFeature.ItemAction startAction,
                            InventoryFeature.ItemAction progressAction,
                            InventoryFeature.ItemAction finishAction) {
            this.internalName = internalName;
            this.progressDescriptionKey = progressDescriptionKey;
            this.duration = duration;
            this.startAction = startAction;
            this.progressAction = progressAction;
            this.finishAction = finishAction;
        }

        public bool Equals(ItemTemplate other) {
            return internalName == other.internalName;
        }

        public override bool Equals(object obj) {
            return obj is ItemTemplate other && Equals(other);
        }

        public override int GetHashCode() {
            return (internalName != null ? internalName.GetHashCode() : 0);
        }
    }
}