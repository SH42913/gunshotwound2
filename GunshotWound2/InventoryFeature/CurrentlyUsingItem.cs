namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct CurrentlyUsingItem : IComponent {
        public ItemTemplate itemTemplate;
        public float remainingTime;
        public Entity target;
    }

    public static class CurrentlyUsingItemExtensions {
        public static float ProgressPercent(this in CurrentlyUsingItem currentlyUsing) {
            return 1f - (currentlyUsing.remainingTime / currentlyUsing.itemTemplate.duration);
        }
    }
}