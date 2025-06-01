namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PainGeneratingSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter generators;
        private Stash<PainGenerator> generatorStash;
        private Stash<ConvertedPed> pedStash;
        private Stash<Pain> painStash;
        private Stash<PainkillersEffect> painkillersStash;

        public EcsWorld World { get; set; }

        public PainGeneratingSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            generators = World.Filter.With<PainGenerator>();

            generatorStash = World.GetStash<PainGenerator>();
            pedStash = World.GetStash<ConvertedPed>();
            painStash = World.GetStash<Pain>();
            painkillersStash = World.GetStash<PainkillersEffect>();
        }

        public void OnUpdate(float deltaTime) {
            DBPContainer multipliers = sharedData.mainConfig.woundConfig.GlobalMultipliers;
            foreach (EcsEntity entity in generators) {
                ref PainGenerator generator = ref generatorStash.Get(entity);
                EcsEntity target = generator.target;
                if (painkillersStash.Has(target)) {
                    continue;
                }

                ref Pain targetPain = ref painStash.Get(target, out bool hasPain);
                if (!hasPain) {
                    sharedData.logger.WriteWarning($"{nameof(PainGenerator)} target has no {nameof(Pain)} component");
                    generatorStash.Remove(entity);
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(target, out bool hasPed);
                if (!hasPed) {
                    sharedData.logger.WriteWarning($"{nameof(PainGenerator)} target has no {nameof(ConvertedPed)}");
                    generatorStash.Remove(entity);
                    continue;
                }

                if (convertedPed.isRagdoll) {
                    continue;
                }

                var totalNewPain = 0f;
                if (generator.moveRate > 0f && convertedPed.thisPed.IsWalking) {
                    totalNewPain += generator.moveRate * multipliers.pain * deltaTime;
                }

                if (generator.runRate > 0f && (convertedPed.thisPed.IsRunning || convertedPed.thisPed.IsSprinting)) {
                    totalNewPain += generator.runRate * multipliers.pain * deltaTime;
                }

                if (totalNewPain > 0f) {
                    targetPain.diff += totalNewPain;
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}