using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class InstantDamageSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<InstantDamageComponent> _components;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(InstantDamageSystem);
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                int pedEntity = component.PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    woundedPed.Health -= component.DamageAmount;
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
                    SendDebug($"{pedEntity} has {woundedPed.Health}HP and {woundedPed.DamagedParts}");
                    //_ecsWorld.CreateEntityWith<CheckPedComponent>().PedEntity = pedEntity;
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }

        private void SendDebug(string message)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
        }
    }
}