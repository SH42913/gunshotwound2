namespace GunshotWound2.HitDetection {
    using System;
    using Configs;
    using GTA.Native;
    using PedsFeature;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class MultiBulletHitSystem : ISystem {
        private const float DAMAGE_MODIFIER = 0.1f;
        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public EcsWorld World { get; set; }

        public MultiBulletHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
            SetDamageModifierForAllShotguns();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in damagedPeds) {
                ref PedHitData hitData = ref entity.GetComponent<PedHitData>();
                if (hitData.weaponType.Pellets < 2) {
                    continue;
                }

                int dealtDamage = hitData.armorDiff + hitData.healthDiff;
                float damagePerPellet = Function.Call<float>(Hash.GET_WEAPON_DAMAGE, hitData.weaponHash);
                int possibleHits = (int)(dealtDamage / damagePerPellet);
#if DEBUG
                sharedData.logger.WriteInfo($"Possible hit count {possibleHits}, dealt={dealtDamage} perPellet={damagePerPellet}");
#endif
                hitData.hits = Clamp(possibleHits, 1, hitData.weaponType.Pellets);
                Function.Call(Hash.SET_WEAPON_DAMAGE_MODIFIER, hitData.weaponHash, DAMAGE_MODIFIER);
            }
        }

        void IDisposable.Dispose() { }

        private void SetDamageModifierForAllShotguns() {
            foreach (WeaponConfig.Weapon weapon in sharedData.mainConfig.weaponConfig.Weapons) {
                if (weapon.Pellets < 2) {
                    continue;
                }

                foreach (uint hash in weapon.Hashes) {
                    Function.Call(Hash.SET_WEAPON_DAMAGE_MODIFIER, hash, DAMAGE_MODIFIER);
                }
            }
        }

        private static int Clamp(int value, int min, int max) {
            if (value >= max) {
                return max;
            } else if (value <= min) {
                return min;
            } else {
                return value;
            }
        }
    }
}