// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.HealthFeature {
    using System;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class HealthChangeSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter peds;
        private Stash<Health> healthStash;

        public EcsWorld World { get; set; }

        public HealthChangeSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Health>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (health.isDead) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (!convertedPed.thisPed.Exists() || convertedPed.thisPed.IsDead) {
                    health.isDead = true;
                    continue;
                }

                int oldHealth = convertedPed.thisPed.Health;
                if (health.kill || Configs.WoundConfig.ConvertHealthFromNative(oldHealth) < 0) {
                    MarkPedAsDead(entity, ref convertedPed, ref health);
                    continue;
                }

                float threshold = convertedPed.isPlayer && convertedPed.IsUsingPhone() ? 5f : 1f;
                if (Math.Abs(health.diff) < threshold) {
                    continue;
                }

                int currentDiff = health.diff > 0 ? (int)Math.Floor(health.diff) : (int)Math.Ceiling(health.diff);
                health.diff -= currentDiff;

                uint weaponHash = health.lastDamageWeapon;
                if (weaponHash == 0) {
                    weaponHash = sharedData.mainConfig.weaponConfig.WEAPON_BLEEDING;
                }

                int newHealth = oldHealth + currentDiff;
                PedEffects.SetHealth(convertedPed.thisPed, newHealth, convertedPed.lastAggressor, weaponHash);
                health.lastDamageWeapon = 0;

#if DEBUG && DEBUG_EVERY_FRAME
                var healthString = $"{oldHealth} --({weaponHash})> {newHealth}/{health}";
                sharedData.logger.WriteInfo($"Changed health: {currentDiff.ToString()} to {convertedPed.name}. {healthString}");
#endif

                if (Configs.WoundConfig.ConvertHealthFromNative(newHealth) < 0) {
                    MarkPedAsDead(entity, ref convertedPed, ref health);
                }
            }
        }

        private void MarkPedAsDead(EcsEntity entity, ref ConvertedPed convertedPed, ref Health health) {
#if DEBUG
            sharedData.logger.WriteInfo($"Ped {convertedPed.name} is marked as dead");
#endif
            health.isDead = true;
            convertedPed.thisPed.ApplyDamage(100000);
            CreateDeathReport(entity, ref convertedPed, ref health);
        }

        private void CreateDeathReport(EcsEntity entity, ref ConvertedPed convertedPed, ref Health health) {
            if (health.isDead && convertedPed.isPlayer) {
                World.CreateEntity().SetComponent(new ShowPlayerDeathReportRequest {
                    report = sharedData.pedStateService.BuildString(entity),
                });
            }
        }

        void IDisposable.Dispose() { }
    }
}