namespace GunshotWound2.HealthFeature {
    using Configs;
    using Scellecs.Morpeh;

    public struct Bleeding : IComponent {
        public Entity target;
        public BodyPartConfig.BodyPart bodyPart;
        public string name;
        public float severity;
        public bool isInternal;
        public bool isProcessed;
    }

    public static class BleedingExtensions {
        public static void CreateBleeding(this Entity target, in BodyPartConfig.BodyPart bodyPart, float severity, string name, bool isInternal = false) {
            ref Bleeding bleeding = ref target.world.CreateEntity().AddComponent<Bleeding>();
            bleeding.target = target;
            bleeding.bodyPart = bodyPart;
            bleeding.severity = severity;
            bleeding.name = name;
            bleeding.isInternal = isInternal;
        }
    }
}