namespace GunshotWound2.PlayerFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct PlayerSpeedHistory : IComponent {
        public float[] array;
        public int lastIndex;
        public float max;
    }
}