namespace GunshotWound2.Configs {
    using System;
    using System.Xml.Linq;
    using Utils;

    public readonly struct DBPContainer {
        public readonly float damage;
        public readonly float bleed;
        public readonly float pain;

        public DBPContainer(float damage, float bleed, float pain) {
            this.damage = damage;
            this.bleed = bleed;
            this.pain = pain;
        }

        public DBPContainer(XElement node, bool isMult) {
            if (isMult) {
                damage = node.GetFloat("DamageMult", defaultValue: 1f);
                bleed = node.GetFloat("BleedMult", defaultValue: 1f);
                pain = node.GetFloat("PainMult", defaultValue: 1f);
            } else {
                damage = node.GetFloat("Damage");
                bleed = node.GetFloat("Bleed");
                pain = node.GetFloat("Pain");
            }
        }

        public DBPContainer Deviate(Random random, in DBPContainer deviations) {
            return new DBPContainer(damage: DeviateFloat(random, damage, deviations.damage),
                                    bleed: DeviateFloat(random, bleed, deviations.bleed),
                                    pain: DeviateFloat(random, pain, deviations.pain));
        }

        private static float DeviateFloat(Random random, float baseValue, float deviationPercent) {
            if (baseValue <= float.Epsilon) {
                return baseValue;
            } else {
                float maxDeviation = baseValue * deviationPercent;
                float deviation = random.NextFloat(-maxDeviation, maxDeviation);
                return baseValue + deviation;
            }
        }

        public static DBPContainer operator +(in DBPContainer first, in DBPContainer second) {
            float damage = first.damage + second.damage;
            float bleed = first.bleed + second.bleed;
            float pain = first.pain + second.pain;
            return new DBPContainer(damage, bleed, pain);
        }

        public static DBPContainer operator *(in DBPContainer first, in DBPContainer second) {
            float damage = first.damage * second.damage;
            float bleed = first.bleed * second.bleed;
            float pain = first.pain * second.pain;
            return new DBPContainer(damage, bleed, pain);
        }

        public static DBPContainer operator *(in DBPContainer first, float mult) {
            float damage = first.damage * mult;
            float bleed = first.bleed * mult;
            float pain = first.pain * mult;
            return new DBPContainer(damage, bleed, pain);
        }

        public override string ToString() {
            const string format = "F2";
            return $"D:{damage.ToString(format)} P:{pain.ToString(format)} B:{bleed.ToString(format)}";
        }
    }
}