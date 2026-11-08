// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.HealthFeature {
    using System;
    using Configs;
    using GTA;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class HealthChangeSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter peds;
        private AspectFactory<PedWithHealth> pedWithHealthAspect;

        public EcsWorld World { get; set; }

        public HealthChangeSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.Extend<PedWithHealth>();
            pedWithHealthAspect = World.GetAspectFactory<PedWithHealth>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                PedWithHealth aspect = pedWithHealthAspect.Get(entity);
                ref Health health = ref aspect.Health;
                if (health.isDead) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref aspect.Ped;
                Ped thisPed = convertedPed.thisPed;
                if (!thisPed.Exists()) {
                    health.isDead = true;
                    CreateDeathReport(entity, ref convertedPed, ref health);
                    continue;
                }

                if (thisPed.IsDead) {
                    MarkPedAsDead(entity, ref convertedPed, ref health);
                    continue;
                }

                int oldHealth = thisPed.Health;
                if (health.kill || WoundConfig.ConvertHealthFromNative(oldHealth) < 0) {
                    MarkPedAsDead(entity, ref convertedPed, ref health);
                    continue;
                }

                float threshold = convertedPed.isPlayer && convertedPed.IsUsingPhone() ? 5f : 1f;
                if (Math.Abs(health.diff) < threshold) {
                    continue;
                }

                int currentDiff = health.diff > 0 ? (int)Math.Floor(health.diff) : (int)Math.Ceiling(health.diff);
                health.diff -= currentDiff;

                int newHealth = oldHealth + currentDiff;
#if DEBUG && DEBUG_EVERY_FRAME
                uint weaponHash = health.lastDamageWeapon;
#endif
                SetHealth(ref convertedPed, ref health, newHealth);

#if DEBUG && DEBUG_EVERY_FRAME
                var healthString = $"{oldHealth} --({weaponHash})> {newHealth}/{health}";
                sharedData.logger.WriteInfo($"Changed health: {currentDiff.ToString()} to {convertedPed.name}. {healthString}");
#endif

                if (WoundConfig.ConvertHealthFromNative(newHealth) < 0) {
                    MarkPedAsDead(entity, ref convertedPed, ref health);
                }
            }
        }

        private void MarkPedAsDead(EcsEntity entity, ref ConvertedPed convertedPed, ref Health health) {
#if DEBUG
            sharedData.logger.WriteInfo($"Ped {convertedPed.name} is marked as dead");
#endif
            health.isDead = true;
            SetHealth(ref convertedPed, ref health, newHealth: 0);
            CreateDeathReport(entity, ref convertedPed, ref health);
        }

        private void CreateDeathReport(EcsEntity entity, ref ConvertedPed convertedPed, ref Health health) {
            if (health.isDead && convertedPed.isPlayer) {
                World.CreateEntity().SetComponent(new ShowPlayerDeathReportRequest {
                    report = sharedData.pedStateService.BuildString(entity),
                });
            }
        }

        private void SetHealth(ref ConvertedPed convertedPed, ref Health health, int newHealth) {
            uint weaponHash = health.lastDamageWeapon;
            if (weaponHash == 0) {
                weaponHash = sharedData.mainConfig.weaponConfig.WEAPON_BLEEDING;
            }

            newHealth = MathHelpers.Clamp(newHealth, min: 0, max: 1000);
            convertedPed.SetHealth(newHealth, convertedPed.lastAggressor, weaponHash);
            health.lastDamageWeapon = 0;
        }

        void IDisposable.Dispose() { }
    }
}