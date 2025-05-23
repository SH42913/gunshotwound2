namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;

    public sealed class CuttingDamage : BaseWeaponDamage {
        public CuttingDamage(SharedData sharedData) : base(sharedData) { }

        public override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.Cutting;

        public override WoundData DefaultWound() {
            return CreateWound("GrazeDefault");
        }

        protected override WoundData GetHeadWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartHead;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateWound("IncisionWound", bodyPart);
                case 1:  return CreateWound("LacerationWound", bodyPart);
                case 2:  return CreateWound("HeavyBrainCut");
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartNeck;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateWound("IncisionWound", bodyPart);
                case 1:  return CreateWound("LacerationWound", bodyPart);
                case 2:  return CreateWound("StabWound", bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartChest;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateWound("IncisionWound", bodyPart);
                case 1:  return CreateWound("LacerationWound", bodyPart);
                case 2:  return CreateWound("StabWound", bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartLowerBody;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateWound("IncisionWound", bodyPart);
                case 1:  return CreateWound("LacerationWound", bodyPart);
                case 2:  return CreateWound("StabWound", bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartArm;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateWound("IncisionWound", bodyPart);
                case 1:  return CreateWound("LacerationWound", bodyPart);
                case 2:  return CreateWound("StabWound", bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            string bodyPart = sharedData.localeConfig.BodyPartLeg;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateWound("IncisionWound", bodyPart);
                case 1:  return CreateWound("LacerationWound", bodyPart);
                case 2:  return CreateWound("StabWound", bodyPart);
                default: throw new InvalidOperationException();
            }
        }
    }
}