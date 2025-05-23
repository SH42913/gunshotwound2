namespace GunshotWound2.WoundFeature {
    using System;

    public abstract class BaseGunDamage : BaseWeaponDamage {
        protected abstract int GrazeWoundWeight { get; }
        protected abstract int FleshWoundWeight { get; }
        protected abstract int PenetratingWoundWeight { get; }
        protected abstract int PerforatingWoundWeight { get; }
        protected abstract int AvulsiveWoundWeight { get; }

        protected BaseGunDamage(SharedData sharedData) : base(sharedData) { }

        protected override WoundData DefaultWound() {
            return CreateWound("GrazeDefault");
        }

        protected override WoundData GetHeadWound() {
            Randomizer.Clear();
            Randomizer.Add(0, GrazeWoundWeight);
            Randomizer.Add(1, FleshWoundWeight);
            Randomizer.Add(2, PenetratingWoundWeight);
            Randomizer.Add(3, PerforatingWoundWeight);
            Randomizer.Add(4, AvulsiveWoundWeight);

            string bodyPart = sharedData.localeConfig.BodyPartHead;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(bodyPart);
                case 1:  return CreateFleshWound(bodyPart);
                case 2:  return CreateWound("HeavyBrainGsw");
                case 3:  return CreateWound("BulletFlyThroughHead");
                case 4:  return CreateWound("BulletTornApartBrain");
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetNeckWound() {
            Randomizer.Clear();
            Randomizer.Add(0, GrazeWoundWeight);
            Randomizer.Add(1, FleshWoundWeight);
            Randomizer.Add(2, PenetratingWoundWeight);
            Randomizer.Add(3, PerforatingWoundWeight);
            Randomizer.Add(4, AvulsiveWoundWeight);

            string bodyPart = sharedData.localeConfig.BodyPartNeck;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(bodyPart);
                case 1:  return CreateFleshWound(bodyPart);
                case 2:  return CreatePenetratingWound(bodyPart);
                case 3:  return CreatePerforatingWound(bodyPart);
                case 4:  return CreateAvulsiveWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetUpperWound() {
            Randomizer.Clear();
            Randomizer.Add(0, GrazeWoundWeight);
            Randomizer.Add(1, FleshWoundWeight);
            Randomizer.Add(2, PenetratingWoundWeight);
            Randomizer.Add(3, PerforatingWoundWeight);
            Randomizer.Add(4, AvulsiveWoundWeight);

            string bodyPart = sharedData.localeConfig.BodyPartChest;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(bodyPart);
                case 1:  return CreateFleshWound(bodyPart);
                case 2:  return CreatePenetratingWound(bodyPart);
                case 3:  return CreatePerforatingWound(bodyPart);
                case 4:  return CreateAvulsiveWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLowerWound() {
            Randomizer.Clear();
            Randomizer.Add(0, GrazeWoundWeight);
            Randomizer.Add(1, FleshWoundWeight);
            Randomizer.Add(2, PenetratingWoundWeight);
            Randomizer.Add(3, PerforatingWoundWeight);
            Randomizer.Add(4, AvulsiveWoundWeight);

            string bodyPart = sharedData.localeConfig.BodyPartLowerBody;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(bodyPart);
                case 1:  return CreateFleshWound(bodyPart);
                case 2:  return CreatePenetratingWound(bodyPart);
                case 3:  return CreatePerforatingWound(bodyPart);
                case 4:  return CreateAvulsiveWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetArmWound() {
            Randomizer.Clear();
            Randomizer.Add(0, GrazeWoundWeight);
            Randomizer.Add(1, FleshWoundWeight);
            Randomizer.Add(2, PenetratingWoundWeight);
            Randomizer.Add(3, PerforatingWoundWeight);
            Randomizer.Add(4, AvulsiveWoundWeight);

            string bodyPart = sharedData.localeConfig.BodyPartArm;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(bodyPart);
                case 1:  return CreateFleshWound(bodyPart);
                case 2:  return CreatePenetratingWound(bodyPart);
                case 3:  return CreatePerforatingWound(bodyPart);
                case 4:  return CreateAvulsiveWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        protected override WoundData GetLegWound() {
            Randomizer.Clear();
            Randomizer.Add(0, GrazeWoundWeight);
            Randomizer.Add(1, FleshWoundWeight);
            Randomizer.Add(2, PenetratingWoundWeight);
            Randomizer.Add(3, PerforatingWoundWeight);
            Randomizer.Add(4, AvulsiveWoundWeight);

            string bodyPart = sharedData.localeConfig.BodyPartLeg;
            switch (Randomizer.NextWithReplacement()) {
                case 0:  return CreateGrazeWound(bodyPart);
                case 1:  return CreateFleshWound(bodyPart);
                case 2:  return CreatePenetratingWound(bodyPart);
                case 3:  return CreatePerforatingWound(bodyPart);
                case 4:  return CreateAvulsiveWound(bodyPart);
                default: throw new InvalidOperationException();
            }
        }

        private WoundData CreateGrazeWound(string bodyPart) {
            return CreateWound("GrazeGsw", bodyPart);
        }

        private WoundData CreateFleshWound(string bodyPart) {
            return CreateWound("FleshGsw", bodyPart);
        }

        private WoundData CreatePenetratingWound(string bodyPart) {
            return CreateWound("PenetratingGsw", bodyPart);
        }

        private WoundData CreatePerforatingWound(string bodyPart) {
            return CreateWound("PerforatingGsw", bodyPart);
        }

        private WoundData CreateAvulsiveWound(string bodyPart) {
            return CreateWound("AvulsiveGsw", bodyPart);
        }
    }
}