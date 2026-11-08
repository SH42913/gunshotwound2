namespace GunshotWound2.HealthFeature {
    using Configs;
    using Scellecs.Morpeh;

    public struct Bleeding : IComponent {
        public EcsEntity target;
        public BodyPartConfig.BodyPart bodyPart;
        public string name;
        public string reason;
        public float severity;
        public bool isTrauma;
        public bool bluntDamageReason;

        public bool isProcessed;
        public int processedTime;
    }

    public static class BleedingExtensions {
        public static EcsEntity CreateCommonBleeding(this EcsEntity target,
                                                     in BodyPartConfig.BodyPart bodyPart,
                                                     float severity,
                                                     string name,
                                                     string reason,
                                                     bool bluntDamageReason) {
            EcsEntity newEntity = target.world.CreateEntity();
            newEntity.SetComponent(new Bleeding {
                target = target,
                bodyPart = bodyPart,
                name = name,
                reason = reason,
                severity = severity,
                bluntDamageReason = bluntDamageReason,
            });

            return newEntity;
        }

        public static void CreateTraumaBleeding(this EcsEntity target,
                                                in BodyPartConfig.BodyPart bodyPart,
                                                float severity,
                                                string name,
                                                string reason,
                                                EcsEntity traumaEntity = null) {
            traumaEntity ??= target.world.CreateEntity();
            traumaEntity.SetComponent(new Bleeding {
                target = target,
                bodyPart = bodyPart,
                name = name,
                reason = reason,
                severity = severity,
                isTrauma = true,
            });
        }
    }
}