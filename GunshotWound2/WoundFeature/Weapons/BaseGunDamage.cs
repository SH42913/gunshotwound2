namespace GunshotWound2.WoundFeature {
    using System;

    public abstract class BaseGunDamage : BaseWeaponDamage {
        protected abstract int GrazeWoundWeight { get; }
        protected abstract int FleshWoundWeight { get; }
        protected abstract int PenetratingWoundWeight { get; }
        protected abstract int PerforatingWoundWeight { get; }
        protected abstract int AvulsiveWoundWeight { get; }

        protected BaseGunDamage(SharedData sharedData, string weaponClass) : base(sharedData, weaponClass) { }

        protected override WoundData DefaultWound() {
            return CreateWound(sharedData.localeConfig.GrazeWound, 15f, 0.05f, 15f);
        }

        protected override WoundData GetHeadWound() {
            randomizer.Clear();
            randomizer.Add(0, GrazeWoundWeight);
            randomizer.Add(1, FleshWoundWeight);
            randomizer.Add(2, PenetratingWoundWeight);
            randomizer.Add(3, PerforatingWoundWeight);
            randomizer.Add(4, AvulsiveWoundWeight);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(sharedData.localeConfig.BodyPartHead);
                case 1:  return CreateFleshWound(sharedData.localeConfig.BodyPartHead, 0.25f);
                case 2:  return CreateHeavyBrainDamage(sharedData.localeConfig.HeavyBrainDamage);
                case 3:  return CreateHeavyBrainDamage(sharedData.localeConfig.BulletFlyThroughHead);
                case 4:  return CreateHeavyBrainDamage(sharedData.localeConfig.BulletTornApartBrain);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            randomizer.Clear();
            randomizer.Add(0, GrazeWoundWeight);
            randomizer.Add(1, FleshWoundWeight);
            randomizer.Add(2, PenetratingWoundWeight);
            randomizer.Add(3, PerforatingWoundWeight);
            randomizer.Add(4, AvulsiveWoundWeight);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(sharedData.localeConfig.BodyPartNeck);
                case 1:  return CreateFleshWound(sharedData.localeConfig.BodyPartNeck, 0.05f);
                case 2:  return CreatePenetratingWound(sharedData.localeConfig.BodyPartNeck, 0.2f, false);
                case 3:  return CreatePerforatingWound(sharedData.localeConfig.BodyPartNeck, 0.1f, false);
                case 4:  return CreateAvulsiveWound(sharedData.localeConfig.BodyPartNeck, 0.3f, false);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            randomizer.Clear();
            randomizer.Add(0, GrazeWoundWeight);
            randomizer.Add(1, FleshWoundWeight);
            randomizer.Add(2, PenetratingWoundWeight);
            randomizer.Add(3, PerforatingWoundWeight);
            randomizer.Add(4, AvulsiveWoundWeight);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(sharedData.localeConfig.BodyPartChest);
                case 1:  return CreateFleshWound(sharedData.localeConfig.BodyPartChest, 0.1f);
                case 2:  return CreatePenetratingWound(sharedData.localeConfig.BodyPartChest, 0.2f, true);
                case 3:  return CreatePerforatingWound(sharedData.localeConfig.BodyPartChest, 0.2f, true);
                case 4:  return CreateAvulsiveWound(sharedData.localeConfig.BodyPartChest, 0.3f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            randomizer.Clear();
            randomizer.Add(0, GrazeWoundWeight);
            randomizer.Add(1, FleshWoundWeight);
            randomizer.Add(2, PenetratingWoundWeight);
            randomizer.Add(3, PerforatingWoundWeight);
            randomizer.Add(4, AvulsiveWoundWeight);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(sharedData.localeConfig.BodyPartLowerBody);
                case 1:  return CreateFleshWound(sharedData.localeConfig.BodyPartLowerBody, 0.05f);
                case 2:  return CreatePenetratingWound(sharedData.localeConfig.BodyPartLowerBody, 0.1f, true);
                case 3:  return CreatePerforatingWound(sharedData.localeConfig.BodyPartLowerBody, 0.2f, true);
                case 4:  return CreateAvulsiveWound(sharedData.localeConfig.BodyPartLowerBody, 0.2f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            randomizer.Clear();
            randomizer.Add(0, GrazeWoundWeight);
            randomizer.Add(1, FleshWoundWeight);
            randomizer.Add(2, PenetratingWoundWeight);
            randomizer.Add(3, PerforatingWoundWeight);
            randomizer.Add(4, AvulsiveWoundWeight);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(sharedData.localeConfig.BodyPartArm);
                case 1:  return CreateFleshWound(sharedData.localeConfig.BodyPartArm, 0.01f);
                case 2:  return CreatePenetratingWound(sharedData.localeConfig.BodyPartArm, 0.05f, true);
                case 3:  return CreatePerforatingWound(sharedData.localeConfig.BodyPartArm, 0.05f, true);
                case 4:  return CreateAvulsiveWound(sharedData.localeConfig.BodyPartArm, 0.1f, true);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            randomizer.Clear();
            randomizer.Add(0, GrazeWoundWeight);
            randomizer.Add(1, FleshWoundWeight);
            randomizer.Add(2, PenetratingWoundWeight);
            randomizer.Add(3, PerforatingWoundWeight);
            randomizer.Add(4, AvulsiveWoundWeight);

            switch (randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(sharedData.localeConfig.BodyPartLeg);
                case 1:  return CreateFleshWound(sharedData.localeConfig.BodyPartLeg, 0.05f);
                case 2:  return CreatePenetratingWound(sharedData.localeConfig.BodyPartLeg, 0.1f, true);
                case 3:  return CreatePerforatingWound(sharedData.localeConfig.BodyPartLeg, 0.1f, true);
                case 4:  return CreateAvulsiveWound(sharedData.localeConfig.BodyPartLeg, 0.2f, true);
                default: throw new InvalidOperationException();
            }
        }

        private WoundData CreateGrazeWound(string position) {
            return CreateWound($"{sharedData.localeConfig.GrazeGswOn} {position}", 15f, 0.05f, 15f);
        }

        private WoundData CreateFleshWound(string position, float arteryChance) {
            return CreateWound($"{sharedData.localeConfig.FleshGswOn} {position}", 20f, 0.1f, 30f, arteryChance);
        }

        private WoundData CreatePenetratingWound(string position, float arteryChance, bool hasCrits) {
            return CreateWound($"{sharedData.localeConfig.PenetratingGswOn} {position}", 25f, 0.2f, 40f, arteryChance, hasCrits);
        }

        private WoundData CreatePerforatingWound(string position, float arteryChance, bool hasCrits) {
            return CreateWound($"{sharedData.localeConfig.PerforatingGswOn} {position}", 20f, 0.25f, 40f, arteryChance, hasCrits);
        }

        private WoundData CreateAvulsiveWound(string position, float arteryChance, bool hasCrits) {
            return CreateWound($"{sharedData.localeConfig.AvulsiveGswOn} {position}", 30f, 0.30f, 50f, arteryChance, hasCrits);
        }
    }
}