using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public abstract class BaseCriticalSystem<T> : IEcsRunSystem where T : BaseCriticalComponent, new()
    {
        protected EcsWorld EcsWorld;
        protected EcsFilter<T> Components;
        protected EcsFilterSingle<WoundConfig> WoundConfig;
        protected EcsFilterSingle<NpcConfig> NpcConfig;
        protected DamageTypes Damage;
        
        public void Run()
        {
            for (int i = 0; i < Components.EntitiesCount; i++)
            {
                int pedEntity = Components.Components1[i].PedEntity;
                var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null)
                {
                    EcsWorld.RemoveEntity(Components.Entities[i]);
                    continue;
                }

                woundedPed.DamagedParts = woundedPed.DamagedParts | Damage;
                if (woundedPed.IsPlayer)
                {
                    ActionForPlayer(woundedPed, pedEntity);
                }
                else
                {
                    ActionForNpc(woundedPed, pedEntity);
                }
                
                EcsWorld.RemoveEntity(Components.Entities[i]);
            }
        }

        protected abstract void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity);
        protected abstract void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity);

        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
            var notification = EcsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}