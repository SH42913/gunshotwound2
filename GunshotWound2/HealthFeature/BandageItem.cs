namespace GunshotWound2.HealthFeature {
    using GTA;
    using InventoryFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public static class BandageItem {
        public const string KEY = "Bandages";
        public static ItemTemplate template = new(key: KEY,
                                                  pluralKey: "Inventory.Bandages.Plural",
                                                  progressDescriptionKey: "BandagingProgress",
                                                  duration: 5f,
                                                  selfAnimation: ("move_m@_idles@shake_off", "shakeoff_1"),
                                                  otherAnimation: ("anim@amb@clubhouse@tutorial@bkr_tut_ig3@", "machinic_loop_mechandplayer"),
                                                  otherRagdollAnimation: ("anim@amb@clubhouse@tutorial@bkr_tut_ig3@", "machinic_loop_mechandplayer"),
                                                  StartAction,
                                                  ProgressAction,
                                                  FinishAction);

        private static bool StartAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
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
#if DEBUG
            sharedData.logger.WriteInfo($"Bandaging of {convertedTarget.name} started");
#endif

            if (!convertedTarget.isPlayer && !convertedTarget.isRagdoll) {
                convertedTarget.thisPed.Task.StandStill(template.duration.ConvertToMilliSec());
            }

            convertedTarget.thisPed.PlayAmbientSpeech("GENERIC_CURSE_HIGH", SpeechModifier.ForceShouted);

            string bleedingName = health.bleedingToBandage.GetComponent<Bleeding>().name;
            message = string.Format(sharedData.localeConfig.YouTryToBandage, bleedingName);
            return true;
        }

        private static bool ProgressAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
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

        private static bool FinishAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
            if (owner != target && !convertedTarget.isRagdoll) {
                convertedTarget.thisPed.PlayAmbientSpeech("GENERIC_THANKS", SpeechModifier.AllowRepeat);
            }

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
                   && ItemTemplate.IsAbleToInteract(convertedMedic.thisPed, convertedTarget.thisPed);
        }

        public static bool IsBandage(this ItemTemplate item) {
            return template.Equals(item);
        }
    }
}