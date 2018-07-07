using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems
{
    [EcsInject]
    public class HealSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<HealComponent> _components;
        private EcsFilter<BleedingComponent> _bleedingComponents;
        private EcsFilterSingle<PlayerConfig> _playerConfig;
        private EcsFilterSingle<NpcConfig> _npcConfig;
        private static Random _random = new Random();
        
        public void Run()
        {
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
                        woundedPed.Health = _playerConfig.Data.MaximalHealth;
                    }
                    else
                    {
                        woundedPed.Health = _random.Next(50, (int) _npcConfig.Data.MaximalHealth);
                        woundedPed.ThisPed.Accuracy = woundedPed.DefaultAccuracy;
                    }
                    
                    woundedPed.DamagedParts = 0;
                    woundedPed.PainMeter = 0;
                    Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, woundedPed.ThisPed);
                    Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, 1f);
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
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