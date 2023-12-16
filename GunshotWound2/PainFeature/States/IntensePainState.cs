namespace GunshotWound2.PainFeature.States {
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class IntensePainState : IPainState {
        public float PainThreshold => 0.6f;
        public string Color => "~o~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            PedEffects.PlayFacialAnim(convertedPed.thisPed, "mood_injured_1", convertedPed.isMale);

            // if (woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED)) {
            //     return;
            // }
            
            if (convertedPed.isPlayer) {
                CameraEffects.ShakeCameraPermanent();
            } else {
                ref Pain pain = ref pedEntity.GetComponent<Pain>();
                float backPercent = 1f - pain.Percent();
                convertedPed.thisPed.Accuracy = (int)(backPercent * convertedPed.defaultAccuracy);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                CameraEffects.ClearCameraShake();
                PlayerEffects.SetSprint(false);
            } else {
                convertedPed.thisPed.Accuracy = convertedPed.defaultAccuracy;
            }
        }

        public bool TryGetMoveSets(SharedData sharedData, bool isPlayer, out string[] moveSets) {
            moveSets = isPlayer ? sharedData.mainConfig.PlayerConfig.IntensePainSets : sharedData.mainConfig.NpcConfig.IntensePainSets;
            return true;
        }
    }
}