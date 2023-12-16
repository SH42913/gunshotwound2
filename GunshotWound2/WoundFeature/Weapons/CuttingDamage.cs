namespace GunshotWound2.WoundFeature {
    using System;

    public sealed class CuttingDamage : BaseWeaponDamage {
        protected override float HelmetSafeChance => 0.5f;
        protected override bool CanPenetrateArmor => false;

        public CuttingDamage(SharedData sharedData) : base(sharedData, "Cutting") { }

        protected override WoundData DefaultWound() {
            return CreateWound(sharedData.localeConfig.GrazeWound, 10f, 0.1f, 15f);
        }

        protected override WoundData GetHeadWound() {
            randomizer.Clear();
            randomizer.Add(0, 1);
            randomizer.Add(1, 1);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartHead);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartHead, 0.05f);
                case 2:  return CreateHeavyBrainDamage(sharedData.localeConfig.HeavyBrainDamage);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            randomizer.Clear();
            randomizer.Add(0, 5);
            randomizer.Add(1, 3);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartNeck);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartNeck, 0.5f);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartNeck, 0.7f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            randomizer.Clear();
            randomizer.Add(0, 5);
            randomizer.Add(1, 3);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartChest);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartChest, 0.3f);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartChest, 0.5f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            randomizer.Clear();
            randomizer.Add(0, 5);
            randomizer.Add(1, 3);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartLowerBody);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartLowerBody, 0.1f, true);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartLowerBody, 0.3f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            randomizer.Clear();
            randomizer.Add(0, 5);
            randomizer.Add(1, 3);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateIncisionWound(sharedData.localeConfig.BodyPartArm);
                case 1:  return CreateLacerationWound(sharedData.localeConfig.BodyPartArm, 0.1f, true);
                case 2:  return CreateStabWound(sharedData.localeConfig.BodyPartArm, 0.3f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            randomizer.Clear();
            randomizer.Add(0, 5);
            randomizer.Add(1, 3);
            randomizer.Add(2, 1);

            switch (randomizer.NextWithReplacement()) {
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