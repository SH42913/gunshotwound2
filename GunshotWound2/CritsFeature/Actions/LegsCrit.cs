namespace GunshotWound2.CritsFeature {
    using GTA;
    using PedsFeature;
    using Utils;

    public sealed class LegsCrit : BaseCrit {
        private const int RAGDOLL_TIME_IN_MS = 2000;
        private const float RUN_PAIN_MULT = 1f;
        private const float RUN_RAGDOLL_CHANCE = 0.1f / 100f;

        protected override string CritName => sharedData.localeConfig.LegsCrit;
        protected override string PlayerMessage => sharedData.localeConfig.PlayerLegsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLegsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLegsCritMessage;

        public LegsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 20f);

            RagdollType ragdollType = convertedPed.thisPed.IsRunning ? RagdollType.Balance : RagdollType.Relax;
            convertedPed.RequestRagdoll(RAGDOLL_TIME_IN_MS, ragdollType);
            convertedPed.hasBrokenLegs = true;

            convertedPed.moveRate = sharedData.mainConfig.woundConfig.MoveRateOnLegsCrit;
            convertedPed.BlockSprint();

            convertedPed.thisPed.PlayAmbientSpeech("DEATH_HIGH_MEDIUM", SpeechModifier.InterruptShouted);
        }

        public override void EveryFrame(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.thisPed.IsSprinting) {
                convertedPed.RequestRagdoll(RAGDOLL_TIME_IN_MS, RagdollType.Balance);
                ShowRunningWarningMessage(convertedPed);
                return;
            }

            if (!convertedPed.thisPed.IsRunning) {
                return;
            }

            CreatePain(pedEntity, RUN_PAIN_MULT * sharedData.deltaTime);
            bool painRagdoll = sharedData.random.IsTrueWithProbability(RUN_RAGDOLL_CHANCE);
            if (painRagdoll) {
                convertedPed.RequestRagdoll(RAGDOLL_TIME_IN_MS, RagdollType.Balance);
                ShowRunningWarningMessage(convertedPed);
            }
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasBrokenLegs = false;
            convertedPed.ResetMoveRate();
            convertedPed.UnBlockSprint();
            convertedPed.thisPed.SetConfigFlag(PedConfigFlagToggles.IsInjured, false);
        }
    }
}