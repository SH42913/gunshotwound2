namespace GunshotWound2.HealthFeature {
    using Scellecs.Morpeh;

    public struct Bleeding : IComponent {
        public Entity target;
        public string name;
        public float severity;
        public bool canBeBandaged;
        public bool isInternal;
        public bool isProcessed;
    }

    public static class BleedingExtensions {
        public static void CreateBleeding(this Entity target, float severity, string name, bool isInternal = false) {
            ref Bleeding bleeding = ref target.world.CreateEntity().AddComponent<Bleeding>();
            bleeding.target = target;
            bleeding.severity = severity;
            bleeding.name = name;
            bleeding.isInternal = isInternal;
        }
    }
}