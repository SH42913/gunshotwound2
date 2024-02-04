namespace GunshotWound2.HealthFeature {
    using System;
    using Configs;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class BandageSystem : ISystem {
        private static int BANDAGE_START_POST;
        private static int BANDAGE_FINISH_POST;

        private readonly SharedData sharedData;
        private Filter pedsWithHealth;
        private Stash<ConvertedPed> pedStash;
        private Stash<Health> healthStash;

        public Scellecs.Morpeh.World World { get; set; }

        public BandageSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
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

        private void ProcessBandaging(Scellecs.Morpeh.Entity entity, float deltaTime) {
            ref Health health = ref healthStash.Get(entity);
            if (health.timeToBandage <= 0f) {
                return;
            }

            ref ConvertedPed convertedPed = ref pedStash.Get(entity);
            Ped thisPed = convertedPed.thisPed;
            if (!CheckPedIsStandStill(thisPed)) {
                MakeStartPost(sharedData.notifier, $"~r~{sharedData.localeConfig.BandageFailed}");
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
            MakeFinishPost(sharedData.notifier, $"~g~{message}");
            convertedPed.thisPed.PlayAmbientSpeech("FIGHT_RUN");
        }

        public static void TryToBandage(SharedData sharedData, Scellecs.Morpeh.Entity entity) {
            ref Health health = ref entity.GetComponent<Health>();
            if (health.bleedingToBandage.IsNullOrDisposed()) {
                return;
            }

            if (health.timeToBandage > 0f) {
                MakeStartPost(sharedData.notifier, $"~y~{sharedData.localeConfig.AlreadyBandaging}");
                return;
            }

            ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
            if (!CheckPedIsStandStill(convertedPed.thisPed)) {
                MakeStartPost(sharedData.notifier, $"~r~{sharedData.localeConfig.BandageFailed}");
                return;
            }

            WoundConfig woundConfig = sharedData.mainConfig.WoundConfig;
            if (convertedPed.isPlayer) {
                if (Game.Player.Money < woundConfig.BandageCost) {
                    MakeStartPost(sharedData.notifier, $"~r~{sharedData.localeConfig.DontHaveMoneyForBandage}");
                    return;
                }

                Game.Player.Money -= woundConfig.BandageCost;
            }

            health.timeToBandage = woundConfig.ApplyBandageTime;

            string message = string.Format(sharedData.localeConfig.YouTryToBandage, health.timeToBandage.ToString("F1"));
            MakeStartPost(sharedData.notifier, message);
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

        private static void MakeStartPost(Notifier notifier, string message) {
            BANDAGE_START_POST = notifier.ReplaceOne(message, blinking: true, BANDAGE_START_POST);
        }

        private static void MakeFinishPost(Notifier notifier, string message) {
            BANDAGE_FINISH_POST = notifier.ReplaceOne(message, blinking: true, BANDAGE_FINISH_POST);
        }
    }
}