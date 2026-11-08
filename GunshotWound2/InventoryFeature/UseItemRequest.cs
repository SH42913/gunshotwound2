namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct UseItemRequest : IRequestData {
        public EcsEntity owner;
        public ItemTemplate item;
        public EcsEntity target;
    }
}