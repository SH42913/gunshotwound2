namespace GunshotWound2.PainFeature {
    using GTA;
    using InventoryFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public static class PainkillersItem {
        public const string KEY = "Painkillers";
        public static ItemTemplate template = new(key: KEY,
                                                  pluralKey: "Inventory.Painkillers.Plural",
                                                  progressDescriptionKey: null,
                                                  duration: 2f,
                                                  selfAnimation: ("mp_player_intdrink", "loop_bottle"),
                                                  otherAnimation: ("mp_common", "givetake1_b"),
                                                  otherRagdollAnimation: ("amb@medic@standing@tendtodead@idle_a", "idle_c"),
                                                  StartAction,
                                                  ProgressAction,
                                                  FinishAction);

        private static bool StartAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
            if (owner.IsNullOrDisposed() || !owner.Has<ConvertedPed>()) {
                sharedData.logger.WriteError("Trying to use painkillers by invalid medic");
                message = null;
                return false;
            }

            ref ConvertedPed convertedOwner = ref owner.GetComponent<ConvertedPed>();
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
#if DEBUG
            sharedData.logger.WriteInfo($"Start use painkillers to {convertedTarget.name} by {convertedOwner.name}");
#endif

            if (!ItemTemplate.IsAbleToInteract(convertedOwner.thisPed, convertedTarget.thisPed)) {
#if DEBUG
                sharedData.logger.WriteInfo("Wrong conditions for painkillers");
#endif
                message = sharedData.localeConfig.StayStillWhileUsingItem;
                return false;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Painkillers applying to {convertedTarget.name} started");
#endif

            float timeToApply = template.duration;
            if (!convertedTarget.isPlayer && !convertedTarget.isRagdoll) {
                convertedTarget.thisPed.Task.StandStill(timeToApply.ConvertToMilliSec());
            }

            message = sharedData.localeConfig.YouTryToUsePainkillers;
            return true;
        }

        private static bool ProgressAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
            ref ConvertedPed convertedOwner = ref owner.GetComponent<ConvertedPed>();
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
            if (ItemTemplate.IsAbleToInteract(convertedOwner.thisPed, convertedTarget.thisPed)) {
                message = null;
                return true;
            } else {
                message = sharedData.localeConfig.StayStillWhileUsingItem;
                return false;
            }
        }

        private static bool FinishAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
            if (owner != target && !convertedTarget.isRagdoll) {
                convertedTarget.thisPed.PlayAmbientSpeech("GENERIC_THANKS", SpeechModifier.AllowRepeat);
            }

            target.SetComponent(new PainkillersEffect {
                rate = sharedData.mainConfig.inventoryConfig.PainkillersRate,
                remainingTime = sharedData.mainConfig.inventoryConfig.PainkillersDuration,
            });

            message = sharedData.localeConfig.PainkillersSuccess;
            return true;
        }
    }
}