namespace GunshotWound2.HitDetection {
    using Configs;
    using GTA;
    using GTA.Native;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class DamageModifierSystem : ISystem {
        private const float BACK_MODIFIER = 1f / MainConfig.DAMAGE_MODIFIER;

        private readonly SharedData sharedData;
        public EcsWorld World { get; set; }

        public DamageModifierSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) {
            UpdateDamageModifiers(MainConfig.DAMAGE_MODIFIER);
        }

        public void Dispose() {
            UpdateDamageModifiers(modifier: 1f);
            PedEffects.ResetMeleeDamageModifier();
            PedEffects.ResetWeaponDamageModifier();
        }

        private void UpdateDamageModifiers(float modifier) {
            bool pedsEnabled = !sharedData.mainConfig.pedsConfig.UseVanillaHealthSystem;
            if (pedsEnabled) {
                Player player = Game.Player;
                PlayerEffects.SetMeleeDamageModifier(player, modifier);
                PlayerEffects.SetWeaponDamageModifier(player, modifier);
            }

            bool playerEnabled = !sharedData.mainConfig.playerConfig.UseVanillaHealthSystem;
            if (playerEnabled) {
                PedEffects.SetMeleeDamageModifier(modifier);
                PedEffects.SetWeaponDamageModifier(modifier);
            }

            if (pedsEnabled || playerEnabled) {
                CompensateDamageForIgnoreSet();
            }
        }

        private void CompensateDamageForIgnoreSet() {
            foreach (uint hash in sharedData.mainConfig.weaponConfig.IgnoreSet) {
                if (GTAHelpers.IsHumanWeapon(hash)) {
                    Function.Call(Hash.SET_WEAPON_DAMAGE_MODIFIER, hash, BACK_MODIFIER);
                }
            }
        }
    }
}