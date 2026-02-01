namespace GunshotWound2.HealthFeature {
    using System;
    using GTA;
    using Scellecs.Morpeh;

    [Serializable]
    public struct BleedingFx : IComponent, IDisposable {
        public ParticleEffect particles;

        public void Dispose() {
            particles?.Delete();
        }
    }
}