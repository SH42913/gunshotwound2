namespace GunshotWound2.Peds {
    using GTA;
    using Scellecs.Morpeh;

    public struct ConvertedPed : IComponent {
        public string name;
        public Ped thisPed;
        public bool isPlayer;
        public int lastFrameHealth;
#if DEBUG
        public Blip customBlip;
#endif
    }
}