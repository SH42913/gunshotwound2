using System;
using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
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
        private EcsFilter<SetPedToRagdollEvent> _requests;
        private EcsFilterSingle<MainConfig> _config;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(RagdollSystem);
#endif

            for (int i = 0; i < _requests.EntitiesCount; i++)
            {
                int pedEntity = _requests.Components1[i].PedEntity;
                var woundedPed = _ecsWorld
                    .GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null)
                {
                    _ecsWorld.RemoveEntity(_requests.Entities[i]);
                    continue;
                }

                switch (_requests.Components1[i].RagdollState)
                {
                    case RagdollStates.PERMANENT:
                        if(woundedPed.ThisPed.IsRagdoll) continue;
                    
                        SendDebug($"Set {pedEntity} to permanent ragdoll");
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, -1, -1, 0, 0, 0, 0);
                        woundedPed.GivesInToPain = true;

                        _ecsWorld.RemoveEntity(_requests.Entities[i]);
                        break;
                    case RagdollStates.WAKE_UP:
                        if(woundedPed.GivesInToPain)
                        {
                            SendDebug($"WakeUp {pedEntity} from ragdoll");
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 1, 1, 1, 0, 0, 0);
                            woundedPed.GivesInToPain = false;
                            if(woundedPed.ThisPed.IsPlayer) Game.Player.IgnoredByEveryone = false;
                        }

                        _ecsWorld.RemoveEntity(_requests.Entities[i]);
                        break;
                    case RagdollStates.SHORT:
                        if (!woundedPed.ThisPed.IsRagdoll)
                        {
                            SendDebug($"Set {pedEntity} to short ragdoll");
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 2000, 2000, 0, 0, 0, 0);
                        }

                        _ecsWorld.RemoveEntity(_requests.Entities[i]);
                        break;
                    case RagdollStates.LONG:
                        if (!woundedPed.ThisPed.IsRagdoll)
                        {
                            SendDebug($"Set {pedEntity} to long ragdoll");
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 4000, 4000, 0, 0, 0, 0);
                        }

                        _ecsWorld.RemoveEntity(_requests.Entities[i]);
                        break;
                    case RagdollStates.LEG_DAMAGE:
                        if (!woundedPed.ThisPed.IsRagdoll)
                        {
                            SendDebug($"Set {pedEntity} to leg-damage ragdoll");
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 3000, 3000, 4, 0, 0, 0);
                        }

                        _ecsWorld.RemoveEntity(_requests.Entities[i]);
                        break;
                    case RagdollStates.HEART_DAMAGE:
                        if (!woundedPed.ThisPed.IsRagdoll)
                        {
                            SendDebug($"Set {pedEntity} to heart-damage ragdoll");
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 6000, 6000, 1, 0, 0, 0);
                        }

                        _ecsWorld.RemoveEntity(_requests.Entities[i]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}