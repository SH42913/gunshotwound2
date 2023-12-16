namespace GunshotWound2.PainFeature.States {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class MildPainState : IPainState {
        public float PainThreshold => 0.01f;
        public string Color => "~s~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) { }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            PedEffects.PlayFacialAnim(convertedPed.thisPed, "mood_happy_1", convertedPed.isMale);
        }

        public bool TryGetMoveSets(SharedData sharedData, bool isPlayer, out string[] moveSets) {
            moveSets = isPlayer ? sharedData.mainConfig.PlayerConfig.MildPainSets : sharedData.mainConfig.NpcConfig.MildPainSets;
            return true;
        }
    }
}