using System;
using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.EffectSystems
{
    [EcsInject]
    public class RagdollSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<SetPedToRagdollEvent> _events;
        private EcsFilterSingle<MainConfig> _config;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(RagdollSystem);
#endif
            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                int pedEntity = _events.Components1[i].PedEntity;
                if (!_ecsWorld.IsEntityExists(pedEntity))
                {
                    _ecsWorld.RemoveEntity(_events.Entities[i]);
                    continue;
                }
                
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null || woundedPed.IsDead || woundedPed.ThisPed.IsDead)
                {
                    _ecsWorld.RemoveEntity(_events.Entities[i]);
                    continue;
                }

                switch (_events.Components1[i].RagdollState)
                {
                    case RagdollStates.PERMANENT:
                        if(woundedPed.ThisPed.IsRagdoll) continue;
                    
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, -1, -1, 0, 0, 0, 0);
                        woundedPed.InPermanentRagdoll = true;
                        _ecsWorld.RemoveEntity(_events.Entities[i]);
                        continue;
                    case RagdollStates.WAKE_UP:
                        if(woundedPed.InPermanentRagdoll && !woundedPed.Crits.HasFlag(CritTypes.NERVES_DAMAGED))
                        {
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 1, 1, 1, 0, 0, 0);
                            woundedPed.InPermanentRagdoll = false;
                        }
                        _ecsWorld.RemoveEntity(_events.Entities[i]);
                        continue;
                    case RagdollStates.SHORT:
                        if (woundedPed.InPermanentRagdoll)
                        {
                            _ecsWorld.RemoveEntity(_events.Entities[i]);
                        }
                        if(woundedPed.ThisPed.IsRagdoll) continue;
                        
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 2000, 2000, 0, 0, 0, 0);
                        _ecsWorld.RemoveEntity(_events.Entities[i]);
                        continue;
                    case RagdollStates.LONG:
                        if (woundedPed.InPermanentRagdoll)
                        {
                            _ecsWorld.RemoveEntity(_events.Entities[i]);
                        }
                        if(woundedPed.ThisPed.IsRagdoll) continue;
                        
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 4000, 4000, 0, 0, 0, 0);
                        _ecsWorld.RemoveEntity(_events.Entities[i]);
                        continue;
                    case RagdollStates.LEG_DAMAGE:
                        if (woundedPed.InPermanentRagdoll)
                        {
                            _ecsWorld.RemoveEntity(_events.Entities[i]);
                        }
                        if(woundedPed.ThisPed.IsRagdoll) continue;
                        
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 3000, 3000, 4, 0, 0, 0);
                        _ecsWorld.RemoveEntity(_events.Entities[i]);
                        continue;
                    case RagdollStates.HEART_DAMAGE:
                        if (woundedPed.InPermanentRagdoll)
                        {
                            _ecsWorld.RemoveEntity(_events.Entities[i]);
                        }
                        if(woundedPed.ThisPed.IsRagdoll) continue;
                        
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 6000, 6000, 1, 0, 0, 0);
                        _ecsWorld.RemoveEntity(_events.Entities[i]);
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}