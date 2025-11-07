namespace GunshotWound2.TraumaFeature {
    using GTA;
    using PedsFeature;
    using Utils;

    public sealed class LegsTraumaEffect : BaseTraumaEffect {
        private const int RAGDOLL_TIME_IN_MS = 1000;
        private const float RUN_RAGDOLL_CHANCE = 0.2f / 100f;

        public override string PlayerMessage => sharedData.localeConfig.PlayerLegsCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManLegsCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanLegsCritMessage;

        public LegsTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            RagdollType ragdollType = convertedPed.thisPed.IsRunning ? RagdollType.Balance : RagdollType.Relax;
            convertedPed.RequestRagdoll(RAGDOLL_TIME_IN_MS, ragdollType);
            convertedPed.hasBrokenLegs = true;

            convertedPed.moveRate = 0.8f;
            convertedPed.BlockSprint();

            convertedPed.thisPed.PlayAmbientSpeech("DEATH_HIGH_MEDIUM", SpeechModifier.InterruptShouted);
        }

        public override void Repeat(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsRagdoll) {
                return;
            }

            bool shouldRagdoll = ped.IsSprinting;
            if (!shouldRagdoll) {
                shouldRagdoll = ped.IsRunning && sharedData.random.IsTrueWithProbability(RUN_RAGDOLL_CHANCE);
            }

            if (!shouldRagdoll) {
                return;
            }

            convertedPed.RequestRagdoll(RAGDOLL_TIME_IN_MS, RagdollType.Balance);
            ShowRunningWarningMessage(convertedPed);
        }

        public override void Cancel(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasBrokenLegs = false;
            convertedPed.ResetMoveRate();
            convertedPed.UnBlockSprint();
        }
    }
}