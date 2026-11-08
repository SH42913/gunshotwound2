namespace GunshotWound2.HitDetection {
    using System;
    using GTA.Native;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class MultiBulletHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;
        private AspectFactory<PedWithHit> damagedPedAspect;

        public EcsWorld World { get; set; }

        public MultiBulletHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.Extend<PedWithHit>();
            damagedPedAspect = World.GetAspectFactory<PedWithHit>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in damagedPeds) {
                ref PedHitData hitData = ref damagedPedAspect.Get(entity).Hit;
                if (hitData.weaponType.Pellets < 2 || hitData.afterTakedown) {
                    continue;
                }

                float dealtDamage = hitData.armorDiff + hitData.healthDiff;
                float damagePerPellet = Function.Call<float>(Hash.GET_WEAPON_DAMAGE, hitData.weaponHash);
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