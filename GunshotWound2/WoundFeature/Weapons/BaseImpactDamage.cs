namespace GunshotWound2.WoundFeature {
    public abstract class BaseImpactDamage : BaseWeaponDamage {
        protected BaseImpactDamage(SharedData sharedData) : base(sharedData) { }

        public override WoundData DefaultWound() {
            return CreateWound("LightBruiseDefault");
        }

        protected WoundData AbrasionWoundOn(string bodyPart) {
            return CreateWound("AbrasionWound", bodyPart);
        }

        protected WoundData LightBruiseWound(string bodyPart) {
            return CreateWound("LightBruise", bodyPart);
        }

        protected WoundData MediumBruiseWound(string bodyPart) {
            return CreateWound("MediumBruise", bodyPart);
        }

        protected WoundData HeavyBruiseWound(string bodyPart) {
            return CreateWound("HeavyBruise", bodyPart);
        }

        protected WoundData WindedFromImpact() {
            return CreateWound("WindedFromImpact");
        }

        protected WoundData ClosedFracture(string bodyPart) {
            return CreateWound("ClosedFracture", bodyPart);
        }
    }
}