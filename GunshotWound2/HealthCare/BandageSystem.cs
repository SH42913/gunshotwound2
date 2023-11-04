namespace GunshotWound2.HealthCare {
    using System;
    using Configs;
    using GTA;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class BandageSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter pedsWithHealth;
        private Stash<ConvertedPed> pedStash;
        private Stash<Health> healthStash;

        public Scellecs.Morpeh.World World { get; set; }

        public BandageSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.BandageKey, TryToBandagePlayer);
            pedsWithHealth = World.Filter.With<ConvertedPed>().With<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in pedsWithHealth) {
                ProcessBandaging(entity, deltaTime);
            }
        }

        void IDisposable.Dispose() { }

        private void TryToBandagePlayer() {
            if (!sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                return;
            }

            ref ConvertedPed convertedPed = ref pedStash.Get(playerEntity);
            ref Health health = ref healthStash.Get(playerEntity);
            if (health.bleedingToBandage.IsNullOrDisposed()) {
                return;
            }

            LocaleConfig localeConfig = sharedData.localeConfig;
            if (health.timeToBandage > 0f) {
                sharedData.notifier.warning.AddMessage(localeConfig.AlreadyBandaging);
                return;
            }

            if (!CheckPedIsStandStill(convertedPed.thisPed)) {
                sharedData.notifier.emergency.AddMessage(sharedData.localeConfig.BandageFailed);
                return;
            }

            WoundConfig woundConfig = sharedData.mainConfig.WoundConfig;
            if (Game.Player.Money < woundConfig.BandageCost) {
                sharedData.notifier.emergency.AddMessage(localeConfig.DontHaveMoneyForBandage);
                return;
            }

            Game.Player.Money -= woundConfig.BandageCost;
            health.timeToBandage = woundConfig.ApplyBandageTime;
            sharedData.notifier.info.AddMessage(string.Format(localeConfig.YouTryToBandage, health.timeToBandage.ToString("F1")));
        }

        private void ProcessBandaging(Scellecs.Morpeh.Entity entity, float deltaTime) {
            ref Health health = ref healthStash.Get(entity);
            if (health.timeToBandage <= 0f) {
                return;
            }

            ref ConvertedPed convertedPed = ref pedStash.Get(entity);
            Ped thisPed = convertedPed.thisPed;
            if (!CheckPedIsStandStill(thisPed)) {
                sharedData.notifier.emergency.AddMessage(sharedData.localeConfig.BandageFailed);
                health.timeToBandage = -1f;
                return;
            }

            health.timeToBandage -= deltaTime;
            if (health.timeToBandage > 0 || health.bleedingToBandage.IsNullOrDisposed()) {
                return;
            }

            ref Bleeding bleeding = ref health.bleedingToBandage.GetComponent<Bleeding>();
            bleeding.severity *= 0.5f;
            health.bleedingToBandage = null;

            string message = string.Format(sharedData.localeConfig.BandageSuccess, bleeding.name);
            sharedData.notifier.info.AddMessage($"~g~{message}");
        }

        private static bool CheckPedIsStandStill(Ped thisPed) {
            return thisPed.IsIdle && thisPed.IsStopped;

            // return thisPed.IsRagdoll
            //        || thisPed.IsWalking
            //        || thisPed.IsRunning
            //        || thisPed.IsSprinting
            //        || thisPed.IsJumping
            //        || thisPed.IsSwimming
            //        || thisPed.IsShooting
            //        || thisPed.IsReloading
            //        || thisPed.IsAiming
            //        || thisPed.IsClimbing
            //        || thisPed.IsCuffed
            //        || thisPed.IsDiving
            //        || thisPed.IsFalling;
        }
    }
}