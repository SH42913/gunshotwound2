namespace GunshotWound2.Peds {
    using GTA;
    using Scellecs.Morpeh;

    public struct ConvertedPed : IComponent {
        public Ped thisPed;
        public bool isPlayer;
#if DEBUG
        public Blip customBlip;
#endif
    }
}