namespace GunshotWound2.PainFeature.States {
    using GTA;
    using HitDetection;
    using Peds;

    public sealed class UnbearablePainState : IPainState {
        public float PainThreshold => 1f;
        public string Color => "~r~";

        public void ApplyState(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            // PlayFacialAnim(woundedPed, "dead_1");
            // SendPedToRagdoll(pedEntity, RagdollStates.PERMANENT);
            //
            // if (woundedPed.IsPlayer && Config.Data.PlayerConfig.CanDropWeapon) {
            //     woundedPed.ThisPed.Weapons.Drop();
            // } else if (!woundedPed.IsPlayer) {
            //     woundedPed.ThisPed.Weapons.Drop();
            // }
            //
            // string speech = GunshotWound2.Random.IsTrueWithProbability(0.5f)
            //         ? "DYING_HELP"
            //         : "DYING_MOAN";
            //
            // woundedPed.ThisPed.PlayAmbientSpeech(speech, SpeechModifier.ShoutedClear);
            //
            // if (!woundedPed.IsPlayer) {
            //     return;
            // }
            //
            // Game.Player.IgnoredByEveryone = true;
            // EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
            //
            // if (Config.Data.PlayerConfig.PoliceCanForgetYou) {
            //     Game.Player.WantedLevel = 0;
            // }
            //
            // if (woundedPed.Crits.Has(CritTypes.NERVES_DAMAGED) || woundedPed.IsDead) {
            //     return;
            // }
            //
            // SendMessage(Locale.Data.UnbearablePainMessage, NotifyLevels.WARNING);
        }
    }
}