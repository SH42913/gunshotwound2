namespace GunshotWound2.WoundFeature {
    using System;
    using GTA;
    using GTA.Math;
    using Scellecs.Morpeh;

    [Serializable]
    public struct WoundData : IComponent {
        public PedBone damagedBone;
        public float totalPain;
        public float totalBleed;

        public bool hasHitData;
        public Vector3 localHitPos;
        public Vector3 localHitNormal;
    }
}