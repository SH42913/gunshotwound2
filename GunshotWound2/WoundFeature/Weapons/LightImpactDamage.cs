namespace GunshotWound2.WoundFeature {
    using System;

    public sealed class LightImpactDamage : BaseImpactDamage {
        protected override bool CanPenetrateArmor => false;
        protected override float HelmetSafeChance => 0.9f;

        public LightImpactDamage(SharedData sharedData) : base(sharedData, "LightImpact") { }

        protected override WoundData GetHeadWound() {
            randomizer.Clear();
            randomizer.Add(0, 1);
            randomizer.Add(1, 1);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartHead);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartHead);
                case 2:  return WindedFromImpact();
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            randomizer.Clear();
            randomizer.Add(0, 8);
            randomizer.Add(1, 5);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartNeck);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartNeck);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartNeck, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            randomizer.Clear();
            randomizer.Add(0, 8);
            randomizer.Add(1, 5);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartChest);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartChest);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartChest, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            randomizer.Clear();
            randomizer.Add(0, 8);
            randomizer.Add(1, 5);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartLowerBody);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartLowerBody);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartLowerBody, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            randomizer.Clear();
            randomizer.Add(0, 8);
            randomizer.Add(1, 5);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartArm);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartArm);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartArm, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            randomizer.Clear();
            randomizer.Add(0, 8);
            randomizer.Add(1, 5);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartLeg);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartLeg);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartLeg, true);
                default: throw new InvalidOperationException();
            }
        }
    }
}