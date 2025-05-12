namespace GunshotWound2.HealthFeature {
    using InventoryFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public static class BandageItem {
        public static ItemTemplate template = new(internalName: "Bandages",
                                                  pluralKey: "XBandages",
                                                  progressDescriptionKey: "BandagingProgress",
                                                  duration: 3f,
                                                  StartAction,
                                                  ProgressAction,
                                                  FinishAction);

        private const float MAX_BANDAGE_RANGE = 3f;

        private static bool StartAction(SharedData sharedData, Entity owner, Entity target, out string message) {
            if (owner.IsNullOrDisposed() || !owner.Has<ConvertedPed>()) {
                sharedData.logger.WriteError("Trying to bandage by invalid medic");
                message = null;
                return false;
            }

            ref ConvertedPed convertedMedic = ref owner.GetComponent<ConvertedPed>();
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
#if DEBUG
            sharedData.logger.WriteInfo($"Start bandaging of {convertedTarget.name} by {convertedMedic.name}");
#endif

            ref Health health = ref target.GetComponent<Health>(out bool hasHealth);
            if (!hasHealth || health.bleedingToBandage.IsNullOrDisposed()) {
#if DEBUG
                sharedData.logger.WriteInfo($"There's no bleeding to bandage at {convertedTarget.name}");
#endif
                message = null;
                return false;
            }

            if (!CheckBandagingConditions(convertedTarget, convertedMedic, health)) {
#if DEBUG
                sharedData.logger.WriteInfo("Wrong conditions for bandaging");
#endif
                message = sharedData.localeConfig.BandageFailed;
                return false;
            }

            convertedTarget.thisPed.PlayAmbientSpeech("FIGHT_RUN"); // TODO: Decide on start or on finish
#if DEBUG
            sharedData.logger.WriteInfo($"Bandaging of {convertedTarget.name} started");
#endif

            float timeToBandage = template.duration;
            if (!convertedTarget.isRagdoll) {
                convertedTarget.thisPed.Task.StandStill(timeToBandage.ConvertToMilliSec());
            }

            message = string.Format(sharedData.localeConfig.YouTryToBandage, timeToBandage.ToString("F1"));
            return true;
        }

        private static bool ProgressAction(SharedData sharedData, Entity owner, Entity target, out string message) {
            ref Health health = ref target.GetComponent<Health>();
            ref ConvertedPed convertedMedic = ref owner.GetComponent<ConvertedPed>();
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
            if (CheckBandagingConditions(convertedTarget, convertedMedic, health)) {
                message = null;
                return true;
            } else {
                message = sharedData.localeConfig.BandageFailed;
                return false;
            }
        }

        private static bool FinishAction(SharedData sharedData, Entity owner, Entity target, out string message) {
            ref Health health = ref target.GetComponent<Health>();
            ref Bleeding bleeding = ref health.bleedingToBandage.GetComponent<Bleeding>();
            bleeding.severity *= 0.5f;

            health.bleedingToBandage = null;
            message = string.Format(sharedData.localeConfig.BandageSuccess, bleeding.name);
            return true;
        }

        private static bool CheckBandagingConditions(in ConvertedPed convertedTarget,
                                                     in ConvertedPed convertedMedic,
                                                     in Health health) {
            return !health.isDead
                   && !health.bleedingToBandage.IsNullOrDisposed()
                   && CheckBandagingConditions(convertedTarget.thisPed, convertedMedic.thisPed);
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

        public static bool IsBandage(this ItemTemplate item) {
            return template.Equals(item);
        }
    }
}