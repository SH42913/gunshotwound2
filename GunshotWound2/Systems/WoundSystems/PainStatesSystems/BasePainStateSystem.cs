using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystems
{
    [EcsInject]
    public abstract class BasePainStateSystem<TStateEvent> : IEcsRunSystem
        where TStateEvent : BaseChangePainStateEvent, new ()
    {   
        protected EcsWorld EcsWorld;
        protected EcsFilter<TStateEvent> Events;
        
        protected EcsFilterSingle<MainConfig> Config;
        protected EcsFilterSingle<LocaleConfig> Locale;
        
        protected PainStates CurrentState;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BasePainStateSystem<TStateEvent>);
#endif
            
            for (int i = 0; i < Events.EntitiesCount; i++)
            {
                var changeEvent = Events.Components1[i];
                var pedEntity = changeEvent.PedEntity;
                var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null && !woundedPed.IsDead && 
                    (woundedPed.PainState != CurrentState || changeEvent.ForceUpdate))
                {
                    ExecuteState(woundedPed, pedEntity);
                    woundedPed.PainState = CurrentState;
                }
                
                EcsWorld.RemoveEntity(Events.Entities[i]);
            }
        }

        protected virtual void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            SendMessage($"{pedEntity} switch to {CurrentState} pain state", NotifyLevels.DEBUG);
        }

        protected void SendPedToRagdoll(int pedEntity, RagdollStates ragdollType)
        {
            SetPedToRagdollEvent ragdoll;
            EcsWorld.CreateEntityWith(out ragdoll);
            ragdoll.PedEntity = pedEntity;
            ragdoll.RagdollState = ragdollType;
        }

        protected void ChangeWalkingAnimation(int pedEntity, string animationName)
        {
            EcsWorld.CreateEntityWith(out ChangeWalkAnimationEvent anim);
            anim.PedEntity = pedEntity;
            anim.AnimationName = animationName;
        }
        
        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
#endif
            
            var notification = EcsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}