using System;
using GTA;
using GunshotWound2.Components.Events.GuiEvents;
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

            var entity = _ecsWorld.CreateEntityWith(out PlayerMarkComponent _, out WoundedPedComponent woundedPed);
            woundedPed.PainState = PainStates.DEADLY;
            
            woundedPed.IsPlayer = true;
            woundedPed.IsMale = ped.Gender == Gender.Male;
            woundedPed.ThisPed = ped;
            
            woundedPed.Armor = ped.Armor;
            woundedPed.Health = _config.Data.PlayerConfig.MaximalHealth;
            woundedPed.ThisPed.MaxHealth = (int) woundedPed.Health + 101;
            woundedPed.ThisPed.Health = (int) woundedPed.Health;

            woundedPed.MaximalPain = _config.Data.PlayerConfig.MaximalPain;
            woundedPed.PainRecoverSpeed = _config.Data.PlayerConfig.PainRecoverSpeed;
            woundedPed.StopBleedingAmount = _config.Data.PlayerConfig.BleedHealingSpeed;

            woundedPed.BleedingCount = 0;
            woundedPed.MostDangerBleedingEntity = null;

            _config.Data.PlayerConfig.PlayerEntity = entity;
            FindDeadlyWound();

            _ecsWorld.CreateEntityWith<NoPainChangeStateEvent>().Entity = entity;
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
                    woundedPed.InPermanentRagdoll = true;
                    Game.Player.WantedLevel = -1;
                
                    var pain = _ecsWorld.CreateEntityWith<SetPedToRagdollEvent>();
                    pain.RagdollState = RagdollStates.PERMANENT;
                    pain.Entity = _players.Entities[i];

                    _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = _players.Entities[i];
                }
                else if(woundedPed.ThisPed.Health > _config.Data.PlayerConfig.MaximalHealth)
                {
                    _ecsWorld.CreateEntityWith<InstantHealEvent>().Entity = _players.Entities[i];
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