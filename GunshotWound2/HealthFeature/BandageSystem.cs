namespace GunshotWound2.HealthFeature {
    using System;
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class BandageSystem : ISystem {
        private const float MAX_BANDAGE_RANGE = 3f;
        private static int BANDAGE_START_POST;
        private static int BANDAGE_FINISH_POST;

        private readonly SharedData sharedData;
        private Filter requests;
        private Filter pedsWithHealth;
        private Stash<ConvertedPed> pedStash;
        private Stash<Health> healthStash;

        public World World { get; set; }

        public BandageSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            requests = World.Filter.With<BandageRequest>();
            pedsWithHealth = World.Filter.With<ConvertedPed>().With<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in requests) {
                ProcessRequest(entity, ref entity.GetComponent<BandageRequest>());
                entity.RemoveComponent<BandageRequest>();
            }

            World.Commit();

            foreach (Entity entity in pedsWithHealth) {
                ProcessBandaging(entity, deltaTime);
            }
        }

        void IDisposable.Dispose() { }

        private void ProcessRequest(Entity target, ref BandageRequest request) {
            if (request.medic.IsNullOrDisposed() || !request.medic.Has<ConvertedPed>()) {
                sharedData.logger.WriteError("Trying to bandage by invalid medic");
                return;
            }

            ref ConvertedPed convertedTarget = ref pedStash.Get(target);
#if DEBUG
            sharedData.logger.WriteInfo($"Processing {nameof(BandageRequest)} for {convertedTarget.name}");
#endif

            ref Health health = ref target.GetComponent<Health>(out bool hasHealth);
            if (!hasHealth || health.bleedingToBandage.IsNullOrDisposed()) {
#if DEBUG
                sharedData.logger.WriteInfo($"There's no bleeding to bandage at {convertedTarget.name}");
#endif
                return;
            }

            if (health.timeToBandage > 0f) {
#if DEBUG
                sharedData.logger.WriteInfo("Ped is already bandaging");
#endif
                MakeStartPost(sharedData.notifier, $"~y~{sharedData.localeConfig.AlreadyBandaging}");
                return;
            }

            ref ConvertedPed convertedMedic = ref pedStash.Get(request.medic);
            if (!CheckBandagingConditions(convertedTarget.thisPed, convertedMedic.thisPed)) {
#if DEBUG
                sharedData.logger.WriteInfo("Wrong conditions for bandaging");
#endif
                MakeStartPost(sharedData.notifier, $"~r~{sharedData.localeConfig.BandageFailed}");
                return;
            }

            WoundConfig woundConfig = sharedData.mainConfig.WoundConfig;
            if (convertedMedic.isPlayer) {
                GTA.Player player = GTA.Game.Player;
                if (player.Money > 0 && player.Money < woundConfig.BandageCost) {
                    MakeStartPost(sharedData.notifier, $"~r~{sharedData.localeConfig.DontHaveMoneyForBandage}");
                    return;
                }

                player.Money -= woundConfig.BandageCost;
            }

            health.timeToBandage = woundConfig.ApplyBandageTime;
            health.bandagingMedic = request.medic;
#if DEBUG
            sharedData.logger.WriteInfo($"Bandaging started at {convertedTarget.name}");
#endif

            string message = string.Format(sharedData.localeConfig.YouTryToBandage, health.timeToBandage.ToString("F1"));
            MakeStartPost(sharedData.notifier, message);
        }

        private void ProcessBandaging(Entity entity, float deltaTime) {
            ref Health health = ref healthStash.Get(entity);
            if (health.timeToBandage <= 0f || health.isDead) {
                return;
            }

            ref ConvertedPed convertedTarget = ref pedStash.Get(entity);
            ref ConvertedPed convertedMedic = ref pedStash.Get(health.bandagingMedic);
            if (!CheckBandagingConditions(convertedTarget.thisPed, convertedMedic.thisPed)) {
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
            health.bandagingMedic = null;

            string message = string.Format(sharedData.localeConfig.BandageSuccess, bleeding.name);
            MakeFinishPost(sharedData.notifier, $"~g~{message}");
            convertedTarget.thisPed.PlayAmbientSpeech("FIGHT_RUN");
        }

        private static bool CheckBandagingConditions(GTA.Ped target, GTA.Ped medic) {
            if (target == medic) {
                return CheckPedIsStandStill(target);
            }

            return CheckTargetIsAbleToBandage(target)
                   && CheckPedIsStandStill(medic)
                   && GTA.World.GetDistance(target.Position, medic.Position) <= MAX_BANDAGE_RANGE;
        }

        private static bool CheckPedIsStandStill(GTA.Ped thisPed) {
            return thisPed.IsIdle && thisPed.IsStopped;
        }

        private static bool CheckTargetIsAbleToBandage(GTA.Ped targetPed) {
            return targetPed.IsRagdoll || CheckPedIsStandStill(targetPed);
        }

        private static void MakeStartPost(Notifier notifier, string message) {
            BANDAGE_START_POST = notifier.ReplaceOne(message, blinking: true, BANDAGE_START_POST);
        }

        private static void MakeFinishPost(Notifier notifier, string message) {
            BANDAGE_FINISH_POST = notifier.ReplaceOne(message, blinking: true, BANDAGE_FINISH_POST);
        }
    }
}