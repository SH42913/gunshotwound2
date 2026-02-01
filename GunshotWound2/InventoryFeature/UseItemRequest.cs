namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct UseItemRequest : IComponent {
        public ItemTemplate item;
        public Entity target;
    }
}