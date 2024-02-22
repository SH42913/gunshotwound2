namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class AveragePainState : IPainState {
        private const string POST_FX = "FocusIn";

        public float PainThreshold => 0.3f;
        public string Color => "~y~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(true);
                CameraEffects.StartPostFx(POST_FX, 5000);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(false);
                CameraEffects.StopPostFx(POST_FX);

                if (!convertedPed.hasBrokenLegs) {
                    PlayerEffects.SetSprint(true);
                }
            }
        }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = mainConfig.GetPainMoveSetsFor(convertedPed).average;
            return true;
        }
    }
}