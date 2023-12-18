﻿namespace GunshotWound2.WoundFeature {
    public abstract class BaseImpactDamage : BaseWeaponDamage {
        protected BaseImpactDamage(SharedData sharedData, string weaponClass) : base(sharedData, weaponClass) { }

        protected override WoundData DefaultWound() {
            return CreateWound(sharedData.localeConfig.LightBruise, 3f, -1f, 15f);
        }

        protected WoundData AbrasionWoundOn(string position) {
            return CreateWound($"{sharedData.localeConfig.AbrazionWoundOn} {position}", 3f, 0.15f, 15f);
        }

        protected WoundData LightBruiseWound(string position) {
            return CreateWound($"{sharedData.localeConfig.LightBruiseOn} {position}", 3f, -1f, 15f);
        }

        protected WoundData MediumBruiseWound(string position) {
            return CreateWound($"{sharedData.localeConfig.MediumBruiseOn} {position}", 8f, -1f, 30f, 0.001f);
        }

        protected WoundData HeavyBruiseWound(string position, bool hasCrits) {
            return CreateWound($"{sharedData.localeConfig.HeavyBruiseOn} {position}", 10f, -1f, 45f, 0.05f, hasCrits);
        }

        protected WoundData WindedFromImpact() {
            return CreateWound(sharedData.localeConfig.WindedFromImpact, 10f, -1, 40f);
        }
    }
}