namespace GunshotWound2.PlayerFeature {
    using Configs;
    using GTA;
    using Scellecs.Morpeh;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PlayerDamageModifierSystem : ISystem {
        private readonly SharedData sharedData;
        public EcsWorld World { get; set; }

        public PlayerDamageModifierSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) {
            UpdateDamageModifiers(MainConfig.DAMAGE_MODIFIER);
        }

        public void Dispose() {
            UpdateDamageModifiers(modifier: 1f);
        }

        private void UpdateDamageModifiers(float modifier) {
            if (sharedData.mainConfig.pedsConfig.UseVanillaHealthSystem) {
                return;
            }

            Player player = Game.Player;
            PlayerEffects.SetMeleeDamageModifier(player, modifier);
            PlayerEffects.SetWeaponDamageModifier(player, modifier);
        }
    }
}