namespace GunshotWound2.PainFeature.States {
    using GTA;
    using PedsFeature;
    using PlayerFeature;
    using Utils;

    public sealed class UnbearablePainState : IPainState {
        public float PainThreshold => 1f;
        public string Color => "~r~";

        public void ApplyPainIncreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.RequestPermanentRagdoll();

            int deathAnimIndex = sharedData.random.Next(1, 3);
            //TODO SET_FACIAL_IDLE_ANIM_OVERRIDE
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"dead_{deathAnimIndex.ToString()}", convertedPed.isMale);

            string speech = sharedData.random.IsTrueWithProbability(0.5f) ? "DYING_HELP" : "DYING_MOAN";
            convertedPed.thisPed.PlayAmbientSpeech(speech, SpeechModifier.ShoutedClear); //TODO PLAY_PAIN

            if (!convertedPed.isPlayer) {
                convertedPed.thisPed.Weapons.Drop();
                return;
            }

            Player player = Game.Player;
            if (player.WantedLevel <= 3) {
                player.IgnoredByEveryone = true;
                if (sharedData.mainConfig.PlayerConfig.PoliceCanForgetYou) {
                    player.WantedLevel = 0;
                }
            }

            if (sharedData.mainConfig.PlayerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            // if (!woundedPed.Crits.Has(CritTypes.NERVES_DAMAGED) && !woundedPed.IsDead) {
            sharedData.notifier.emergency.AddMessage(sharedData.localeConfig.UnbearablePainMessage);

            // }
        }

        public void ApplyPainDecreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            if (convertedPed.isPlayer) {
                Game.Player.IgnoredByEveryone = false;
                PlayerEffects.SetSprint(false);
            }
        }

        public bool TryGetMoveSets(SharedData sharedData, bool isPlayer, out string[] moveSets) {
            moveSets = default;
            return false;
        }
    }
}