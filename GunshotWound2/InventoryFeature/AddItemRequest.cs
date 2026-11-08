namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct AddItemRequest : IRequestData {
        public EcsEntity target;
        public (string Key, int Count)[] items;
    }
}