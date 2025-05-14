namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;

    public sealed class HeavyImpactDamage : BaseImpactDamage {
        protected override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.HeavyImpact;

        public HeavyImpactDamage(SharedData sharedData) : base(sharedData) { }

        protected override WoundData GetHeadWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 2);
            Randomizer.Add(1, 2);
            Randomizer.Add(2, 1);
            Randomizer.Add(3, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartHead, false);
                case 1:  return CreateWound(sharedData.localeConfig.Blackout, 30f, -1, 70f);
                case 2:  return CreateHeavyBrainDamage(sharedData.localeConfig.TraumaticBrainInjury);
                case 3:  return CreateHeavyBrainDamage(sharedData.localeConfig.BleedingInHead);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return MediumBruiseWound(sharedData.localeConfig.BodyPartNeck);
                case 1:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartNeck, false);
                case 2:  return CreateWound(sharedData.localeConfig.BrokenNeck, 50f, -1, -1, 70f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 3);
            Randomizer.Add(1, 2);
            Randomizer.Add(2, 2);
            Randomizer.Add(3, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartChest);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartChest);
                case 2:  return MediumBruiseWound(sharedData.localeConfig.BodyPartChest);
                case 3:  return ClosedFracture(sharedData.localeConfig.BodyPartChest);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 3);
            Randomizer.Add(1, 2);
            Randomizer.Add(2, 2);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartLowerBody);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartLowerBody);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartLowerBody, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);
            Randomizer.Add(3, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartArm);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartArm);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartArm, true);
                case 3:  return ClosedFracture(sharedData.localeConfig.BodyPartArm);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);
            Randomizer.Add(3, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(sharedData.localeConfig.BodyPartLeg);
                case 1:  return MediumBruiseWound(sharedData.localeConfig.BodyPartLeg);
                case 2:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartLeg, true);
                case 3:  return ClosedFracture(sharedData.localeConfig.BodyPartLeg);
                default: throw new InvalidOperationException();
            }
        }
    }
}