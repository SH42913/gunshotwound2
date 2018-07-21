using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems
{
    [EcsInject]
    public class HealSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        
        private EcsFilter<HealComponent> _components;
        private EcsFilter<BleedingComponent> _bleedingComponents;
        
        private static Random _random = new Random();
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(HealSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    if (woundedPed.IsPlayer)
                    {
                        Function.Call(Hash._SET_CAM_EFFECT, 0);
                        Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, true);
                        Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
                        woundedPed.Health = _mainConfig.Data.PlayerConfig.MaximalHealth;
                        _ecsWorld.CreateEntityWith<AdrenalineComponent>().Revert = true;
                    }
                    else
                    {
                        woundedPed.Health = _random.Next(50, _mainConfig.Data.NpcConfig.MaximalHealth);
                        woundedPed.ThisPed.Accuracy = woundedPed.DefaultAccuracy;
                    }

                    woundedPed.IsDead = false;
                    woundedPed.DamagedParts = 0;
                    woundedPed.PainMeter = 0.01f;
                    Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, woundedPed.ThisPed);
                    Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, 1f);
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
                    woundedPed.Armor = woundedPed.ThisPed.Armor;
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