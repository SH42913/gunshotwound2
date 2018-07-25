using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public abstract class BasePainStateSystem<TState> : IEcsRunSystem
        where TState : BasePainStateComponent, new ()
    {   
        protected EcsWorld EcsWorld;
        protected EcsFilter<TState> States;
        protected EcsFilterSingle<MainConfig> Config;
        protected PainStates CurrentState;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(BasePainStateSystem<TState>);
            
            for (int i = 0; i < States.EntitiesCount; i++)
            {
                var pedEntity = States.Components1[i].PedEntity;
                var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    ExecuteState(woundedPed, pedEntity);
                    woundedPed.PainState = CurrentState;
                }
                
                EcsWorld.RemoveEntity(States.Entities[i]);
            }
        }

        protected virtual void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            SendMessage($"{pedEntity} switch to {CurrentState} pain state", NotifyLevels.DEBUG);
        }
        
        protected void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
            var notification = EcsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}