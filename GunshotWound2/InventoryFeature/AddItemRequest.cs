namespace GunshotWound2.InventoryFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;

    [Serializable]
    public struct AddItemRequest : IComponent {
        public InventoryConfig.Loadout loadout;
    }
}