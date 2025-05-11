namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct CurrentlyUsingItem : IComponent {
        public ItemTemplate itemTemplate;
        public float remainingTime;
        public Entity target;
    }
}