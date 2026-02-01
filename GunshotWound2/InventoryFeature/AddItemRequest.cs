namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct AddItemRequest : IComponent {
        public (string Key, int Count)[] items;
    }
}