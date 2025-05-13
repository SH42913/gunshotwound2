namespace GunshotWound2.PainFeature {
    using System;
    using Scellecs.Morpeh;
    using States;

    [Serializable]
    public struct Pain : IComponent {
        public float amount;
        public float diff;
        public float delayedDiff;
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

        public static float DelayedPercent(this in Pain pain) {
            return pain.delayedDiff / pain.max;
        }

        public static bool TooMuchPain(this in Pain pain) {
            return pain.Percent() > UnbearablePainState.PAIN_THRESHOLD;
        }

        public static float TimeToRecover(this in Pain pain) {
            return pain.TooMuchPain() ? (pain.amount - pain.max) / pain.recoveryRate : 0f;
        }
    }
}