namespace GunshotWound2.PainFeature.States {
    using HitDetection;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class IntensePainState : IPainState {
        public float PainThreshold => 0.6f;
        public string Color => "~o~";

        public void ApplyState(Entity pedEntity, ref ConvertedPed convertedPed) {
            // ChangeMoveSet(pedEntity,
            //               woundedPed.IsPlayer
            //                       ? Config.Data.PlayerConfig.IntensePainSets
            //                       : Config.Data.NpcConfig.IntensePainSets);
            //
            // PlayFacialAnim(woundedPed, "mood_injured_1");
            //
            // if (woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED)) {
            //     return;
            // }
            //
            // var pain = EcsWorld.GetComponent<PainComponent>(pedEntity);
            // float backPercent = 1f - pain.CurrentPain / woundedPed.MaximalPain;
            // if (woundedPed.IsPlayer) {
            //     EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.PERMANENT;
            //     EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
            // } else {
            //     woundedPed.ThisPed.Accuracy = (int)(backPercent * woundedPed.DefaultAccuracy);
            // }
        }
    }
}