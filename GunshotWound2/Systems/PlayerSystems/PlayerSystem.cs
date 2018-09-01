using System;
using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.NpcEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.MarkComponents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class PlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<WoundedPedComponent, PlayerMarkComponent> _players;

        public void Initialize()
        {
            if(!_config.Data.PlayerConfig.WoundedPlayerEnabled) return;

            var ped = Game.Player.Character;

            PlayerMarkComponent playerComponent;
            WoundedPedComponent woundPed;
            var entity = _ecsWorld.CreateEntityWith(out playerComponent, out woundPed);
            woundPed.PainState = PainStates.DEADLY;
            
            woundPed.IsPlayer = true;
            woundPed.IsMale = ped.Gender == Gender.Male;
            woundPed.ThisPed = ped;
            
            woundPed.Armor = ped.Armor;
            woundPed.Health = _config.Data.PlayerConfig.MaximalHealth;
            woundPed.ThisPed.Health = (int) woundPed.Health;

            woundPed.PainMeter = 0;
            woundPed.MaximalPain = _config.Data.PlayerConfig.MaximalPain;
            woundPed.PainRecoverSpeed = _config.Data.PlayerConfig.PainRecoverSpeed;
            woundPed.StopBleedingAmount = _config.Data.PlayerConfig.BleedHealingSpeed;

            _config.Data.PlayerConfig.PlayerEntity = entity;
            FindDeadlyWound();

            _ecsWorld.CreateEntityWith<NoChangePainStateEvent>().PedEntity = entity;
        }
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(PlayerSystem);
#endif
            
            for (int i = 0; i < _players.EntitiesCount; i++)
            {
                var woundedPed = _players.Components1[i];
                woundedPed.ThisPed = Game.Player.Character;
                woundedPed.Armor = woundedPed.ThisPed.Armor;
                
                if (woundedPed.ThisPed.Health < _config.Data.PlayerConfig.MinimalHealth &&
                    !woundedPed.IsDead)
                {
                    woundedPed.Health = woundedPed.ThisPed.Health;
                    woundedPed.IsDead = true;
                    woundedPed.PainRecoverSpeed = 0;
                
                    var pain = _ecsWorld.CreateEntityWith<SetPedToRagdollEvent>();
                    pain.RagdollState = RagdollStates.PERMANENT;
                    pain.PedEntity = _players.Entities[i];

                    _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().PedEntity = _players.Entities[i];
                }
                else if(woundedPed.ThisPed.Health > _config.Data.PlayerConfig.MaximalHealth)
                {
                    _ecsWorld.CreateEntityWith<InstantHealEvent>().PedEntity = _players.Entities[i];
                    _ecsWorld.CreateEntityWith<ForceWorldPedUpdateEvent>();
                }
            }
        }

        private void FindDeadlyWound()
        {
            var totalHealth = _config.Data.PlayerConfig.MaximalHealth - _config.Data.PlayerConfig.MinimalHealth;
            var critical = (float) Math.Sqrt(totalHealth * _config.Data.PlayerConfig.BleedHealingSpeed);
            _config.Data.WoundConfig.EmergencyBleedingLevel = critical;
        }

        public void Destroy()
        {
            
        }
    }
}