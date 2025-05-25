// ReSharper disable InconsistentNaming

namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;
    using HitDetection;
    using Utils;

    public struct WoundData {
        public string Name;
        public float Damage;
        public float Pain;
        public float BleedSeverity;
        public bool InternalBleeding;
        public bool ArterySevered;
        public bool HasCrits;

        public WoundData(LocaleConfig localeConfig, Random random, WoundConfig.Wound wound, PedHitData hitData) {
            string woundName = localeConfig.GetTranslation(wound.LocKey);
            Name = hitData.bodyPart.IsValid ? $"{woundName} {localeConfig.GetTranslation(hitData.bodyPart.LocKey)}" : woundName;
            Damage = wound.Damage;
            Pain = wound.Pain;
            BleedSeverity = wound.Bleed;
            InternalBleeding = wound.IsInternal;
            ArterySevered = wound.ArteryChance > 0 && random.IsTrueWithProbability(wound.ArteryChance);
            HasCrits = wound.WithCrit && (wound.ForceCrit || random.IsTrueWithProbability(hitData.weaponType.CritChance));
        }

        public override string ToString() {
            const string format = "F";
            return $"{Name} D:{Damage.ToString(format)} P:{Pain.ToString(format)} B:{BleedSeverity.ToString(format)} "
                   + $"In:{InternalBleeding.ToString()} A:{ArterySevered.ToString()} C:{HasCrits.ToString()}";
        }
    }
}