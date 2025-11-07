namespace GunshotWound2.TraumaFeature {
    using Configs;
    using GTA;
    using HealthFeature;
    using PedsFeature;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public sealed class AbdomenTraumaEffect : BaseTraumaEffect {
        private const float NEW_BLEEDING_CHANCE = 0.25f / 100f;
        private const float BLEEDING_SEVERITY = 0.5f;

        private static readonly int[] NM_MESSAGES = { 1119, }; // shotInGuts

        public override string PlayerMessage => sharedData.localeConfig.PlayerStomachCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManStomachCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanStomachCritMessage;

        public AbdomenTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(EcsEntity entity, ref ConvertedPed convertedPed) {
            convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS", SpeechModifier.InterruptShouted);

            convertedPed.RequestRagdoll(4000);
            // convertedPed.nmMessages = NM_MESSAGES; TODO restore as nmHelper
        }

        public override void Repeat(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(EcsEntity entity, ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsRagdoll) {
                return;
            }

            if (!ped.IsRunning && !ped.IsSprinting) {
                return;
            }

            bool openNewBleeding = sharedData.random.IsTrueWithProbability(NEW_BLEEDING_CHANCE);
            if (openNewBleeding) {
                BodyPartConfig.BodyPart bodyPart = sharedData.mainConfig.bodyPartConfig.GetBodyPartByBone(Bone.SkelSpine1);
                string name = sharedData.localeConfig.InternalBleeding;
                string reason = sharedData.localeConfig.TraumaType;
                entity.CreateBleeding(bodyPart, 0.5f * BLEEDING_SEVERITY, name, reason, isTrauma: true, causedByPenetration: false);
                ped.PlayAmbientSpeech("PAIN_RAPIDS", SpeechModifier.InterruptShouted);
                ShowRunningWarningMessage(convertedPed);
            }
        }

        public override void Cancel(EcsEntity entity, ref ConvertedPed convertedPed) { }
    }
}