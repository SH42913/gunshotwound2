// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.HealthFeature {
    using System;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class HealthChangeSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter peds;
        private Stash<Health> healthStash;

        public World World { get; set; }

        public HealthChangeSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Health>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (health.isDead) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (!convertedPed.thisPed.Exists() || convertedPed.thisPed.IsDead) {
                    health.isDead = true;
                    continue;
                }

                if (health.kill) {
                    MarkPedAsDead(entity, ref convertedPed, ref health);
                    continue;
                }

                float threshold = convertedPed.isPlayer && convertedPed.IsUsingPhone() ? 5f : 1f;
                if (Math.Abs(health.diff) < threshold) {
                    continue;
                }

                int currentDiff = health.diff > 0 ? (int)Math.Floor(health.diff) : (int)Math.Ceiling(health.diff);
                health.diff -= currentDiff;

                int oldHealth = convertedPed.thisPed.Health;
                int newHealth = oldHealth + currentDiff;

                uint weaponHash = health.lastDamageWeapon;
                if (weaponHash == 0) {
                    weaponHash = sharedData.mainConfig.weaponConfig.WEAPON_BLEEDING;
                }

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

        private void MarkPedAsDead(Entity entity, ref ConvertedPed convertedPed, ref Health health) {
#if DEBUG
            sharedData.logger.WriteInfo($"Ped {convertedPed.name} is marked as dead");
#endif
            health.isDead = true;
            convertedPed.thisPed.ApplyDamage(100000);
            CreateDeathReport(entity, ref convertedPed, ref health);
        }

        private void CreateDeathReport(Entity entity, ref ConvertedPed convertedPed, ref Health health) {
            if (health.isDead && convertedPed.isPlayer) {
                World.CreateEntity().SetComponent(new ShowPlayerDeathReportRequest {
                    report = sharedData.pedStateService.BuildString(entity),
                });
            }
        }

        void IDisposable.Dispose() { }
    }
}