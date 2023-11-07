namespace GunshotWound2.PainFeature.States {
    using HitDetection;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class NoPainState : IPainState {
        public float PainThreshold => 0f;
        public string Color => "~g~";

        public void ApplyState(Entity pedEntity, ref ConvertedPed convertedPed) {
            // SendPedToRagdoll(pedEntity, RagdollStates.WAKE_UP);
            // ResetMoveSet(pedEntity);
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
            // PlayFacialAnim(woundedPed, "mood_happy_1");
            //
            // if (!_config.Data.PlayerConfig.CameraIsShaking)
            // {
            //     Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 0);
            //     Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
            // }
        }
    }
}