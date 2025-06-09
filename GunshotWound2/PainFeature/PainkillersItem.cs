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
                                                  StartAction,
                                                  ProgressAction,
                                                  FinishAction);

        private const float MAX_USE_RANGE = 3f;

        private static bool StartAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message) {
            if (owner.IsNullOrDisposed() || !owner.Has<ConvertedPed>()) {
                sharedData.logger.WriteError("Trying to use painkillers by invalid medic");
                message = null;
                return false;
            }

            ref ConvertedPed convertedMedic = ref owner.GetComponent<ConvertedPed>();
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
#if DEBUG
            sharedData.logger.WriteInfo($"Start use painkillers to {convertedTarget.name} by {convertedMedic.name}");
#endif

            if (!CheckConditions(convertedTarget.thisPed, convertedMedic.thisPed)) {
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
            ref ConvertedPed convertedMedic = ref owner.GetComponent<ConvertedPed>();
            ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
            if (CheckConditions(convertedTarget.thisPed, convertedMedic.thisPed)) {
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

        private static bool CheckConditions(Ped target, Ped medic) {
            if (target == medic) {
                return target.IsStopped;
            }

            Vehicle vehicle = medic.CurrentVehicle;
            if (vehicle != null) {
                return vehicle == target.CurrentVehicle && medic.IsStopped && target.IsStopped;
            }

            return (target.IsRagdoll || target.IsStopped)
                   && medic.IsStopped
                   && GTA.World.GetDistance(target.Position, medic.Position) <= MAX_USE_RANGE;
        }
    }
}