using GTA;
using GTA.Native;
using GunshotWound2.Components.EffectComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.EffectSystems
{
    [EcsInject]
    public class RagdollSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<RagdollRequestComponent> _requests;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(RagdollSystem);

            for (int i = 0; i < _requests.EntitiesCount; i++)
            {
                var pedEntity = _requests.Components1[i].PedEntity;
                var woundedPed = _ecsWorld
                    .GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null)
                {
                    _ecsWorld.RemoveEntity(_requests.Entities[i]);
                    continue;
                }

                if (_requests.Components1[i].Enable)
                {
                    if(woundedPed.ThisPed.IsRagdoll) continue;
                    
                    SendDebug($"Set {pedEntity} to ragdoll");
                    Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, -1, -1, 0, 0, 0, 0);
                    woundedPed.GivesInToPain = true;

                    _ecsWorld.RemoveEntity(_requests.Entities[i]);
                }
                else
                {
                    if(woundedPed.GivesInToPain)
                    {
                        SendDebug($"Unset {pedEntity} from ragdoll");
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, woundedPed.ThisPed, 1, 1, 1, 0, 0, 0);
                        woundedPed.GivesInToPain = false;
                        if(woundedPed.ThisPed.IsPlayer) Game.Player.IgnoredByEveryone = false;
                    }

                    _ecsWorld.RemoveEntity(_requests.Entities[i]);
                }
            }
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}