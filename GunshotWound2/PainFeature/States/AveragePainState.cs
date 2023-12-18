﻿namespace GunshotWound2.PainFeature.States {
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class AveragePainState : IPainState {
        public float PainThreshold => 0.3f;
        public string Color => "~y~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(true);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(false);

                if (!convertedPed.hasBrokenLegs) {
                    PlayerEffects.SetSprint(true);
                }
            }
        }

        public bool TryGetMoveSets(SharedData sharedData, bool isPlayer, out string[] moveSets) {
            moveSets = isPlayer ? sharedData.mainConfig.PlayerConfig.AvgPainSets : sharedData.mainConfig.NpcConfig.AvgPainSets;
            return true;
        }
    }
}