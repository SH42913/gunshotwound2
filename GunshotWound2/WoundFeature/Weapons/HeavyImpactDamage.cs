namespace GunshotWound2.WoundFeature {
    using System;

    public sealed class HeavyImpactDamage : BaseImpactDamage {
        protected override bool CanPenetrateArmor => false;
        protected override float HelmetSafeChance => 0.8f;

        public HeavyImpactDamage(SharedData sharedData) : base(sharedData, "HeavyImpact") { }

        protected override WoundData GetHeadWound() {
            randomizer.Clear();
            randomizer.Add(0, 3);
            randomizer.Add(1, 2);
            randomizer.Add(2, 1);
            randomizer.Add(3, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartHead, false);
                case 1:  return CreateWound(sharedData.localeConfig.Blackout, 30f, -1, 70f);
                case 2:  return CreateHeavyBrainDamage(sharedData.localeConfig.TraumaticBrainInjury);
                case 3:  return CreateHeavyBrainDamage(sharedData.localeConfig.BleedingInHead);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            randomizer.Clear();
            randomizer.Add(0, 2);
            randomizer.Add(1, 2);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return MediumBruiseWound(sharedData.localeConfig.BodyPartNeck);
                case 1:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartNeck, false);
                case 2:  return CreateWound(sharedData.localeConfig.BrokenNeck, 50f, -1, -1, 70f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            randomizer.Clear();
            randomizer.Add(0, 3);
            randomizer.Add(1, 2);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartChest);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartChest);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartChest, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            randomizer.Clear();
            randomizer.Add(0, 3);
            randomizer.Add(1, 2);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartLowerBody);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartLowerBody);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartLowerBody, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            randomizer.Clear();
            randomizer.Add(0, 1);
            randomizer.Add(1, 1);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartArm);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartArm);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartArm, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            randomizer.Clear();
            randomizer.Add(0, 1);
            randomizer.Add(1, 1);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartLeg);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartLeg);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartLeg, true);
                default: throw new InvalidOperationException();
            }
        }
    }
}