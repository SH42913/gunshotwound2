namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;

    public sealed class LightImpactDamage : BaseImpactDamage {
        protected override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.LightImpact;

        public LightImpactDamage(SharedData sharedData) : base(sharedData) { }

        protected override WoundData GetHeadWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(sharedData.localeConfig.BodyPartHead);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartHead);
                case 2:  return WindedFromImpact();
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 8);
            Randomizer.Add(1, 5);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartNeck;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 8);
            Randomizer.Add(1, 5);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartChest;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 8);
            Randomizer.Add(1, 5);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartLowerBody;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 8);
            Randomizer.Add(1, 5);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartArm;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 8);
            Randomizer.Add(1, 5);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartLeg;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return LightBruiseWound(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }
    }
}