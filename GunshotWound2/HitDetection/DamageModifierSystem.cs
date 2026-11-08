namespace GunshotWound2.HitDetection {
    using Configs;
    using GTA;
    using GTA.Native;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class DamageModifierSystem : ISystem {
        private readonly SharedData sharedData;
        public EcsWorld World { get; set; }

        public DamageModifierSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            sharedData.pauseService.PauseStateChanged += OnPauseStateChanged;
        }

        public void OnUpdate(float deltaTime) {
            UpdateDamageModifiers(MainConfig.DAMAGE_MODIFIER);
        }

        public void Dispose() {
            sharedData.pauseService.PauseStateChanged -= OnPauseStateChanged;
            ResetModifiers();
        }

        private void UpdateDamageModifiers(float modifier) {
            bool pedsDisabled = sharedData.mainConfig.pedsConfig.UseVanillaHealthSystem;
            bool playerDisabled = sharedData.mainConfig.playerConfig.UseVanillaHealthSystem;
            if (pedsDisabled && playerDisabled) {
                return;
            }

            SetDamageModifierForAllWeapons(modifier);

            float backModifier = 1f / modifier;
            if (pedsDisabled) {
                SetPlayerDamageModifier(backModifier);
            }

            if (playerDisabled) {
                SetAiDamageModifier(backModifier);
            }
        }

        private void SetDamageModifierForAllWeapons(float modifier) {
            WeaponConfig.Weapon[] weapons = sharedData.mainConfig.weaponConfig.Weapons;
            for (int i = 0; i < weapons.Length; i++) {
                foreach (uint weaponHash in weapons[i].Hashes) {
                    if (GTAHelpers.IsHumanWeapon(weaponHash)) {
                        Function.Call(Hash.SET_WEAPON_DAMAGE_MODIFIER, weaponHash, modifier);
                    }
                }
            }
        }

        private static void SetPlayerDamageModifier(float modifier) {
            Player player = Game.Player;
            PlayerEffects.SetMeleeDamageModifier(player, modifier);
            PlayerEffects.SetWeaponDamageModifier(player, modifier);
        }

        private static void SetAiDamageModifier(float modifier) {
            PedEffects.SetAiMeleeDamageModifier(modifier);
            PedEffects.SetAiWeaponDamageModifier(modifier);
        }

        private static void ResetAiDamageModifier() {
            PedEffects.ResetAiMeleeDamageModifier();
            PedEffects.ResetAiWeaponDamageModifier();
        }

        private void ResetModifiers() {
            UpdateDamageModifiers(modifier: 1f);
            ResetAiDamageModifier();
        }

        private void OnPauseStateChanged(bool isPaused) {
            if (isPaused) {
                ResetModifiers();
            } else {
                OnUpdate(deltaTime: 0f);
            }
        }
    }
}