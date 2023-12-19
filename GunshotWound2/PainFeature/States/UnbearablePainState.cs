namespace GunshotWound2.PainFeature.States {
    using GTA;
    using PedsFeature;
    using PlayerFeature;
    using Utils;

    public sealed class UnbearablePainState : IPainState {
        private readonly int[] painInjuryMessages = { 787, };

        public float PainThreshold => 1f;
        public string Color => "~r~";

        public void ApplyPainIncreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.RequestPermanentRagdoll();
            convertedPed.nmMessages = painInjuryMessages;

            int deathAnimIndex = sharedData.random.Next(1, 3);
            //TODO SET_FACIAL_IDLE_ANIM_OVERRIDE
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"dead_{deathAnimIndex.ToString()}", convertedPed.isMale);

            string speech = sharedData.random.IsTrueWithProbability(0.5f) ? "DYING_HELP" : "DYING_MOAN";
            convertedPed.thisPed.PlayAmbientSpeech(speech, SpeechModifier.ShoutedClear); //TODO PLAY_PAIN

            if (!convertedPed.isPlayer) {
                // convertedPed.thisPed.Task.Wait(5000); TODO Repeat while have pain
                convertedPed.thisPed.Weapons.Drop();
                return;
            }

            Player player = Game.Player;
            player.CanControlCharacter = false;
            if (player.WantedLevel <= 3) {
                player.IgnoredByEveryone = true;
                if (sharedData.mainConfig.PlayerConfig.PoliceCanForgetYou) {
                    player.WantedLevel = 0;
                }
            }

            if (sharedData.mainConfig.PlayerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            if (!convertedPed.hasSpineDamage) {
                sharedData.notifier.emergency.AddMessage(sharedData.localeConfig.UnbearablePainMessage);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            if (convertedPed.isPlayer) {
                Player player = Game.Player;
                player.IgnoredByEveryone = false;
                player.CanControlCharacter = true;
                PlayerEffects.SetSprint(false);
            }
        }

        public bool TryGetMoveSets(SharedData sharedData, bool isPlayer, out string[] moveSets) {
            moveSets = default;
            return false;
        }
    }
}