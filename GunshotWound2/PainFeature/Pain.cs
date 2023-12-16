namespace GunshotWound2.PainFeature {
    using System;
    using Scellecs.Morpeh;
    using States;

    [Serializable]
    public struct Pain : IComponent {
        public float amount;
        public float diff;
        public float recoveryRate;
        public float max;
        public IPainState currentState;
    }

    public static class PainExtensions {
        public static bool HasPain(this in Pain pain) {
            return pain.amount > 0f;
        }

        public static float Percent(this in Pain pain) {
            return pain.amount / pain.max;
        }
    }
}