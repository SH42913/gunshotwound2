using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PedSystems
{
    [EcsInject]
    public class InstantHealSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        
        private EcsFilter<InstantHealEvent> _events;
        private EcsFilter<BleedingComponent> _bleedingComponents;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(InstantHealSystem);
#endif
            
            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                int pedEntity = _events.Components1[i].PedEntity;
                if (!_ecsWorld.IsEntityExists(pedEntity))
                {
                    continue;
                }
                
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed != null)
                {
                    if (woundedPed.IsPlayer)
                    {
                        _ecsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.CLEAR;
                        Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, true);
                        Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
                        woundedPed.Health = _mainConfig.Data.PlayerConfig.MaximalHealth;
                        Game.Player.IgnoredByEveryone = false;
                    }
                    else
                    {
                        woundedPed.Health = Random.Next(50, _mainConfig.Data.NpcConfig.MaxStartHealth);
                        woundedPed.ThisPed.Accuracy = woundedPed.DefaultAccuracy;
                    }

                    woundedPed.IsDead = false;
                    woundedPed.Crits = 0;
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
                    woundedPed.Armor = woundedPed.ThisPed.Armor;
                    woundedPed.WoundCount = 0;

                    if (_ecsWorld.GetComponent<PainComponent>(pedEntity) != null)
                    {
                        _ecsWorld.RemoveComponent<PainComponent>(pedEntity);
                    }
                    
                    Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, woundedPed.ThisPed);
                    Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, 1f);

                    _ecsWorld.CreateEntityWith(out NoPainChangeStateEvent noPainEvent);
                    noPainEvent.PedEntity = pedEntity;
                    noPainEvent.ForceUpdate = true;
                }

                for (int bleedIndex = 0; bleedIndex < _bleedingComponents.EntitiesCount; bleedIndex++)
                {
                    if(_bleedingComponents.Components1[bleedIndex].PedEntity != pedEntity) continue;
                    
                    _ecsWorld.RemoveEntity(_bleedingComponents.Entities[bleedIndex]);
                }
            }
            
            _events.RemoveAllEntities();
        }
    }
}