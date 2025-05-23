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
                case 0:  return HeavyBruiseWound(sharedData.localeConfig.BodyPartHead);
                case 1:  return CreateWound("Blackout");
                case 2:  return CreateWound("TraumaticBrainInjury");
                case 3:  return CreateWound("BleedingInHead");
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartNeck;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return MediumBruiseWound(bodyPart);
                case 1:  return HeavyBruiseWound(bodyPart);
                case 2:  return CreateWound("BrokenNeck");
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 3);
            Randomizer.Add(1, 2);
            Randomizer.Add(2, 2);
            Randomizer.Add(3, 1);

            string bodyPart = sharedData.localeConfig.BodyPartChest;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return MediumBruiseWound(bodyPart);
                case 3:  return ClosedFracture(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 3);
            Randomizer.Add(1, 2);
            Randomizer.Add(2, 2);

            string bodyPart = sharedData.localeConfig.BodyPartLowerBody;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);
            Randomizer.Add(3, 1);

            string bodyPart = sharedData.localeConfig.BodyPartArm;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                case 3:  return ClosedFracture(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);
            Randomizer.Add(3, 1);

            string bodyPart = sharedData.localeConfig.BodyPartLeg;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return AbrasionWoundOn(bodyPart);
                case 1:  return MediumBruiseWound(bodyPart);
                case 2:  return HeavyBruiseWound(bodyPart);
                case 3:  return ClosedFracture(bodyPart);
                default: throw new InvalidOperationException();
            }
        }
    }
}