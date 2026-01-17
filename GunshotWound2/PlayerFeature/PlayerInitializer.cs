namespace GunshotWound2.PlayerFeature {
    using GTA;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PlayerInitializer : IInitializer {
        private readonly SharedData sharedData;
        public EcsWorld World { get; set; }

        public PlayerInitializer(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            PlayerEffects.SetMeleeDamageModifier(Game.Player, 0.15f);
        }

        public void Dispose() {
            PlayerEffects.SetMeleeDamageModifier(Game.Player, 1f);
        }
    }
}