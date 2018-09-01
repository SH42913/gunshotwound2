using GunshotWound2.Components;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public abstract class BaseCriticalSystem<T> : IEcsRunSystem where T : ComponentWithPedEntity, new()
    {
        protected EcsWorld EcsWorld;
        protected EcsFilter<T> Events;
        
        protected EcsFilterSingle<MainConfig> Config;
        protected EcsFilterSingle<LocaleConfig> Locale;
        
        protected CritTypes CurrentCrit;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BaseCriticalSystem<T>);
#endif
            
            for (int i = 0; i < Events.EntitiesCount; i++)
            {
                int pedEntity = Events.Components1[i].PedEntity;
                var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null)
                {
                    EcsWorld.RemoveEntity(Events.Entities[i]);
                    continue;
                }

                if (woundedPed.Crits.HasFlag(CurrentCrit))
                {
                    EcsWorld.RemoveEntity(Events.Entities[i]);
                    continue;
                }

                woundedPed.Crits = woundedPed.Crits | CurrentCrit;
                if (woundedPed.IsPlayer)
                {
                    ActionForPlayer(woundedPed, pedEntity);
                }
                else
                {
                    ActionForNpc(woundedPed, pedEntity);
                }
                
                EcsWorld.RemoveEntity(Events.Entities[i]);
            }
        }

        protected abstract void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity);
        protected abstract void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity);

        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
#endif
            
            var notification = EcsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }

        protected void CreatePain(int entity, float amount)
        {
            var pain = EcsWorld.CreateEntityWith<AddPainEvent>();
            pain.PedEntity = entity;
            pain.PainAmount = amount;
        }

        protected void CreateInternalBleeding(int entity, float amount)
        {
            var bleed = EcsWorld.CreateEntityWith<BleedingComponent>();
            bleed.PedEntity = entity;
            bleed.BleedSeverity = amount;
            bleed.Name = Locale.Data.InternalBleeding;
        }

        protected void SendPedToRagdoll(int pedEntity, RagdollStates ragdollType)
        {
            SetPedToRagdollEvent ragdoll;
            EcsWorld.CreateEntityWith(out ragdoll);
            ragdoll.PedEntity = pedEntity;
            ragdoll.RagdollState = ragdollType;
        }
    }
}