namespace GunshotWound2.PainFeature.States {
    using Configs;
    using GTA;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class UnbearablePainState : IPainState {
        private const string BLACKOUT_FX = "DeathFailOut";
        private static readonly int[] NM_MESSAGES = { 787, };

        public float PainThreshold => 1f;
        public string Color => "~r~";

        public void ApplyPainIncreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.Percent() - PainThreshold < 0.1f) {
                pain.diff += 0.2f * pain.max;
            }

            convertedPed.ResetRagdoll();
            convertedPed.RequestPermanentRagdoll();
            convertedPed.nmMessages = NM_MESSAGES;
            convertedPed.isRestrictToDrive = true;

            int deathAnimIndex = sharedData.random.Next(1, 3);
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"die_{deathAnimIndex.ToString()}", convertedPed.isMale);

            string speech = sharedData.random.IsTrueWithProbability(0.5f) ? "DYING_HELP" : "DYING_MOAN";
            convertedPed.thisPed.PlayAmbientSpeech(speech, SpeechModifier.ShoutedClear);

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

            if (!convertedPed.hasSpineDamage) {
                sharedData.notifier.critical.QueueMessage(sharedData.localeConfig.UnbearablePainMessage);
            }

            CameraEffects.StartPostFx(BLACKOUT_FX, 5000);
        }

        public void ApplyPainDecreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.isRestrictToDrive = false;

            if (convertedPed.isPlayer) {
                Player player = Game.Player;
                player.IgnoredByEveryone = false;
                CameraEffects.StopPostFx(BLACKOUT_FX);
            }
        }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = default;
            return false;
        }
    }
}