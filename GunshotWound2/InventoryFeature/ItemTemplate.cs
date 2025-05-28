namespace GunshotWound2.InventoryFeature {
    using System;
    using Configs;

    public readonly struct ItemTemplate : IEquatable<ItemTemplate> {
        public readonly string key;
        public readonly string pluralKey;
        public readonly string progressDescriptionKey;
        public readonly float duration;
        public readonly InventoryFeature.ItemAction startAction;
        public readonly InventoryFeature.ItemAction progressAction;
        public readonly InventoryFeature.ItemAction finishAction;

        public bool IsValid => !string.IsNullOrEmpty(key);

        public ItemTemplate(string key,
                            string pluralKey,
                            string progressDescriptionKey,
                            float duration,
                            InventoryFeature.ItemAction startAction,
                            InventoryFeature.ItemAction progressAction,
                            InventoryFeature.ItemAction finishAction) {
            this.key = key;
            this.pluralKey = pluralKey;
            this.progressDescriptionKey = progressDescriptionKey;
            this.duration = duration;
            this.startAction = startAction;
            this.progressAction = progressAction;
            this.finishAction = finishAction;
        }

        public bool Equals(ItemTemplate other) {
            return key == other.key;
        }

        public override bool Equals(object obj) {
            return obj is ItemTemplate other && Equals(other);
        }

        public override int GetHashCode() {
            return (key != null ? key.GetHashCode() : 0);
        }
    }

    public static class ItemExtensions {
        public static string GetPluralTranslation(this in ItemTemplate item, LocaleConfig localeConfig, int count) {
            string translation = localeConfig.GetTranslation(item.pluralKey);
            return string.Format(translation, count.ToString());
        }
    }
}