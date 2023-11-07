namespace GunshotWound2.PainFeature.States {
    using HitDetection;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class MildPainState : IPainState {
        public float PainThreshold => 0.01f;
        public string Color => "~s~";

        public void ApplyState(Entity pedEntity, ref ConvertedPed convertedPed) {
            // ChangeMoveSet(pedEntity,
            //               woundedPed.IsPlayer
            //                       ? Config.Data.PlayerConfig.MildPainSets
            //                       : Config.Data.NpcConfig.MildPainSets);
            //
            // if (!woundedPed.IsPlayer) {
            //     return;
            // }
            //
            // if (!woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED)) {
            //     EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.CLEAR;
            // }
            //
            // EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = false;
        }
    }
}