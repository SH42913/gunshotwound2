namespace GunshotWound2.HealthFeature {
    using Scellecs.Morpeh;

    public struct Bleeding : IComponent {
        public Entity target;
        public string name;
        public float severity;
        public bool canBeBandaged;
        public bool isProcessed;
    }
}