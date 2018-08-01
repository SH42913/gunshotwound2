using System;
using GTA;
using GunshotWound2.Components;
using GunshotWound2.Components.PlayerComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class PlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<WoundedPedComponent, PlayerComponent> _playerComponents;

        public void Initialize()
        {
            if(!_config.Data.PlayerConfig.WoundedPlayerEnabled) return;

            var ped = Game.Player.Character;

            PlayerComponent playerComponent;
            WoundedPedComponent woundPed;
            var entity = _ecsWorld.CreateEntityWith(out playerComponent, out woundPed);
            woundPed.PainState = PainStates.DEADLY;
            
            woundPed.IsPlayer = true;
            woundPed.ThisPed = ped;
            
            woundPed.Armor = ped.Armor;
            woundPed.Health = _config.Data.PlayerConfig.MaximalHealth;
            woundPed.ThisPed.Health = (int) woundPed.Health;
            
            woundPed.HeShe = ped.Gender == Gender.Male
                ? "He"
                : "She";
            woundPed.HisHer = ped.Gender == Gender.Male
                ? "His"
                : "Her";

            woundPed.PainMeter = 0;
            woundPed.MaximalPain = _config.Data.PlayerConfig.MaximalPain;
            woundPed.PainRecoverSpeed = _config.Data.PlayerConfig.PainRecoverSpeed;
            woundPed.StopBleedingAmount = _config.Data.PlayerConfig.BleedHealingSpeed;

            _config.Data.PlayerConfig.PlayerEntity = entity;
            FindDeadlyWound();

            _ecsWorld.CreateEntityWith<NoPainStateComponent>().PedEntity = entity;
        }
        
        public void Run()
        {
            for (int i = 0; i < _playerComponents.EntitiesCount; i++)
            {
                var woundedPed = _playerComponents.Components1[i];
                woundedPed.ThisPed = Game.Player.Character;
                woundedPed.Armor = woundedPed.ThisPed.Armor;
                
                if (woundedPed.ThisPed.Health < _config.Data.PlayerConfig.MinimalHealth &&
                    !woundedPed.IsDead)
                {
                    woundedPed.Health = woundedPed.ThisPed.Health;
                    woundedPed.IsDead = true;
                    woundedPed.PainRecoverSpeed = 0;
                
                    var pain = _ecsWorld.CreateEntityWith<PainComponent>();
                    pain.PainAmount = int.MaxValue;
                    pain.PedEntity = _playerComponents.Entities[i];

                    _ecsWorld.CreateEntityWith<CheckPedComponent>().PedEntity = _playerComponents.Entities[i];
                }
                else if(woundedPed.ThisPed.Health > _config.Data.PlayerConfig.MaximalHealth)
                {
                    _ecsWorld.CreateEntityWith<InstantHealComponent>().PedEntity = _playerComponents.Entities[i];
                }
            }
        }

        public void Destroy()
        {}

        private void FindDeadlyWound()
        {
            var totalHealth = _config.Data.PlayerConfig.MaximalHealth - _config.Data.PlayerConfig.MinimalHealth;
            var critical = (float) Math.Sqrt(totalHealth * _config.Data.PlayerConfig.BleedHealingSpeed);
            _config.Data.WoundConfig.EmergencyBleedingLevel = critical;
        }
    }
}