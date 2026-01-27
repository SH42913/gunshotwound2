namespace GunshotWound2.HitDetection {
    using System;
    using Configs;
    using GTA.Native;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class MultiBulletHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public EcsWorld World { get; set; }

        public MultiBulletHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in damagedPeds) {
                ref PedHitData hitData = ref entity.GetComponent<PedHitData>();
                if (hitData.weaponType.Pellets < 2 || hitData.afterTakedown) {
                    continue;
                }

                float dealtDamage = hitData.armorDiff + hitData.healthDiff;
                float damagePerPellet = MainConfig.DAMAGE_MODIFIER * Function.Call<float>(Hash.GET_WEAPON_DAMAGE, hitData.weaponHash);
                float possibleHits = dealtDamage / damagePerPellet;
#if DEBUG
                sharedData.logger.WriteInfo($"Possible hit count {possibleHits}, totalDmg={dealtDamage} perPellet={damagePerPellet}");
#endif
                hitData.hits = MathHelpers.Clamp((int)possibleHits, 1, hitData.weaponType.Pellets);
            }
        }

        void IDisposable.Dispose() { }
    }
}