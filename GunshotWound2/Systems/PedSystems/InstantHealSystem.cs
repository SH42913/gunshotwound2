using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.NpcEvents;
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
        
        private EcsFilter<InstantHealEvent> _components;
        private EcsFilter<BleedingComponent> _bleedingComponents;
        
        private static readonly Random _random = new Random();
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(InstantHealSystem);
#endif
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                int pedEntity = _components.Components1[i].PedEntity;
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

                        if (_mainConfig.Data.PlayerConfig.AdrenalineSlowMotion)
                        {
                            _ecsWorld.CreateEntityWith<AddPlayerAdrenalineEffectEvent>().RestoreState = true;
                        }
                    }
                    else
                    {
                        woundedPed.Health = _random.Next(50, _mainConfig.Data.NpcConfig.UpperStartHealth);
                        woundedPed.ThisPed.Accuracy = woundedPed.DefaultAccuracy;
                    }

                    woundedPed.IsDead = false;
                    woundedPed.Crits = 0;
                    woundedPed.PainMeter = 0;
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
                    woundedPed.Armor = woundedPed.ThisPed.Armor;
                    
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
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }
    }
}