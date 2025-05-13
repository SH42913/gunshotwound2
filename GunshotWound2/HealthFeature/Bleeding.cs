namespace GunshotWound2.HealthFeature {
    using HitDetection;
    using Scellecs.Morpeh;

    public struct Bleeding : IComponent {
        public Entity target;
        public PedHitData.BodyParts bodyPart;
        public string name;
        public float severity;
        public bool isInternal;
        public bool isProcessed;
    }

    public static class BleedingExtensions {
        public static void CreateBleeding(this Entity target, PedHitData.BodyParts bodyPart, float severity, string name, bool isInternal = false) {
            ref Bleeding bleeding = ref target.world.CreateEntity().AddComponent<Bleeding>();
            bleeding.target = target;
            bleeding.bodyPart = bodyPart;
            bleeding.severity = severity;
            bleeding.name = name;
            bleeding.isInternal = isInternal;
        }
    }
}