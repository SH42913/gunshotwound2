namespace GunshotWound2.HealthFeature {
    using Configs;
    using Scellecs.Morpeh;

    public struct Bleeding : IComponent {
        public Entity target;
        public BodyPartConfig.BodyPart bodyPart;
        public string name;
        public string reason;
        public float severity;
        public bool isTrauma;

        public bool isProcessed;
        public int processedTime;
    }

    public static class BleedingExtensions {
        public static Entity CreateBleeding(this Entity target,
                                          in BodyPartConfig.BodyPart bodyPart,
                                          float severity,
                                          string name,
                                          string reason,
                                          bool isTrauma) {
            Entity newEntity = target.world.CreateEntity();
            ref Bleeding bleeding = ref newEntity.AddComponent<Bleeding>();
            bleeding.target = target;
            bleeding.bodyPart = bodyPart;
            bleeding.name = name;
            bleeding.reason = reason;
            bleeding.severity = severity;
            bleeding.isTrauma = isTrauma;
            return newEntity;
        }
    }
}