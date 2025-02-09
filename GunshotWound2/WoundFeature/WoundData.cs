// ReSharper disable InconsistentNaming
namespace GunshotWound2.WoundFeature {
    public struct WoundData {
        public string Name;
        public float Damage;
        public float Pain;
        public float BleedSeverity;
        public bool InternalBleeding;
        public bool ArterySevered;
        public bool HasCrits;

        public override string ToString() {
            const string format = "F";
            return $"{Name} D:{Damage.ToString(format)} P:{Pain.ToString(format)} B:{BleedSeverity.ToString(format)} "
                   + $"In:{InternalBleeding.ToString()} A:{ArterySevered.ToString()} C:{HasCrits.ToString()}";
        }
    }
}