namespace GunshotWound2.WoundFeature {
    using Configs;
    using HitDetection;
    using Utils;

    public abstract class BaseWeaponDamage {
        protected readonly SharedData sharedData;

        protected Weighted_Randomizer.IWeightedRandomizer<int> Randomizer => sharedData.weightRandom;
        public abstract WeaponConfig.Stats Stats { get; }

        protected BaseWeaponDamage(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public WoundData? ProcessHit(ref PedHitData hit) {
            switch (hit.bodyPart) {
                case PedHitData.BodyParts.Head:    return GetHeadWound();
                case PedHitData.BodyParts.Neck:    return GetNeckWound();
                case PedHitData.BodyParts.Chest:   return GetUpperWound();
                case PedHitData.BodyParts.Abdomen: return GetLowerWound();
                case PedHitData.BodyParts.Arm:     return GetArmWound();
                case PedHitData.BodyParts.Leg:     return GetLegWound();
                default:                           return null;
            }
        }

        public abstract WoundData DefaultWound();
        protected abstract WoundData GetHeadWound();
        protected abstract WoundData GetNeckWound();
        protected abstract WoundData GetUpperWound();
        protected abstract WoundData GetLowerWound();
        protected abstract WoundData GetArmWound();
        protected abstract WoundData GetLegWound();

        protected WoundData CreateWound(string key, string bodyPart = null) {
            if (!sharedData.mainConfig.woundConfig.Wounds.TryGetValue(key, out WoundConfig.Wound template)) {
                sharedData.logger.WriteError($"Can't find wound {key}, will use default one");
                return DefaultWound();
            }

            string name = sharedData.localeConfig.GetTranslation(template.LocKey);
            if (!string.IsNullOrEmpty(bodyPart)) {
                name = $"{name} {bodyPart}";
            }

            return new WoundData {
                Name = name,
                Damage = Stats.DamageMult * template.Damage,
                Pain = Stats.PainMult * template.Pain,
                BleedSeverity = Stats.BleedMult * template.Bleed,
                InternalBleeding = template.IsInternal,
                ArterySevered = template.ArteryChance > 0 && sharedData.random.IsTrueWithProbability(template.ArteryChance),
                HasCrits = template.WithCrit && (template.ForceCrit || sharedData.random.IsTrueWithProbability(Stats.CritChance)),
            };
        }
    }
}