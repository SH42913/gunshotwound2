namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;

    public sealed class CuttingDamage : BaseWeaponDamage {
        public CuttingDamage(SharedData sharedData) : base(sharedData) { }

        protected override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.Cutting;

        protected override WoundData DefaultWound() {
            return CreateWound(sharedData.localeConfig.GrazeWound, 10f, 0.1f, 15f);
        }

        protected override WoundData GetHeadWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 1);
            Randomizer.Add(1, 1);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartHead);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartHead, 0.05f);
                case 2:  return CreateHeavyBrainDamage(sharedData.localeConfig.HeavyBrainDamage);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartNeck);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartNeck, 0.5f);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartNeck, 0.7f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartChest);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartChest, 0.3f);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartChest, 0.5f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartLowerBody);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartLowerBody, 0.1f, true);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartLowerBody, 0.3f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartArm);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartArm, 0.1f, true);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartArm, 0.3f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            Randomizer.Clear();
            Randomizer.Add(0, 5);
            Randomizer.Add(1, 3);
            Randomizer.Add(2, 1);

            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartLeg);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartLeg, 0.3f, true);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartLeg, 0.5f, true);
                default: throw new InvalidOperationException();
            }
        }

        private WoundData CreateIncisionWound(string position) {
            return CreateWound($"{sharedData.localeConfig.IncisionWoundOn} {position}", 10f, 0.3f, 20f);
        }

        private WoundData CreateLacerationWound(string position, float arteryChance, bool hasCrits = false) {
            return CreateWound($"{sharedData.localeConfig.LacerationWoundOn} {position}", 20f, 0.8f, 30f, arteryChance, hasCrits);
        }

        private WoundData CreateStabWound(string position, float arteryChance, bool hasCrits = false) {
            return CreateWound($"{sharedData.localeConfig.StabWoundOn} {position}", 30f, 0.6f, 40f, arteryChance, hasCrits);
        }
    }
}