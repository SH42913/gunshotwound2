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
}