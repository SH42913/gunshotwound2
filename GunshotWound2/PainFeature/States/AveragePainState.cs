namespace GunshotWound2.PainFeature.States {
    using GTA;
    using Peds;

    public sealed class AveragePainState : IPainState {
        public float PainThreshold => 0.3f;
        public string Color => "~y~";

        public void ApplyState(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            // SendPedToRagdoll(pedEntity, RagdollStates.WAKE_UP);
            // ChangeMoveSet(pedEntity,
            //               woundedPed.IsPlayer
            //                       ? Config.Data.PlayerConfig.AvgPainSets
            //                       : Config.Data.NpcConfig.AvgPainSets);
            //
            // if (!woundedPed.IsPlayer) {
            //     return;
            // }
            //
            // Game.Player.IgnoredByEveryone = false;
            // EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
        }
    }
}