namespace GunshotWound2.PedsFeature {
    using Configs;
    using Scellecs.Morpeh;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PedDamageModifierSystem : ISystem {
        private readonly SharedData sharedData;
        public EcsWorld World { get; set; }

        public PedDamageModifierSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) {
            UpdateDamageModifiers(MainConfig.DAMAGE_MODIFIER);
        }

        public void Dispose() {
            PedEffects.ResetMeleeDamageModifier();
            PedEffects.ResetWeaponDamageModifier();
        }

        private void UpdateDamageModifiers(float modifier) {
            if (sharedData.mainConfig.playerConfig.UseVanillaHealthSystem) {
                return;
            }

            PedEffects.SetMeleeDamageModifier(modifier);
            PedEffects.SetWeaponDamageModifier(modifier);
        }
    }
}