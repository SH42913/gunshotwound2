namespace GunshotWound2.HealthFeature {
    using System;
    using Scellecs.Morpeh;
    using Utils;

    [Serializable]
    public struct BloodPoolFx : IComponent, IDisposable {
        public int bloodPoolIndex;

        public float timeToNextUpdate;
        public float timeToStopGrow;
        public int effectHandle;

        public void Dispose() {
            if (effectHandle != 0) {
                GTAHelpers.RemoveParticleEffect(effectHandle);
            }
        }
    }
}