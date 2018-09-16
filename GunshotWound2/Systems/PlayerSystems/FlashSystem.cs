using GTA.Native;
using GunshotWound2.Components.Events.PlayerEvents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class FlashSystem : IEcsRunSystem
    {
        private EcsFilter<AddFlashEvent> _events;
        
        public void Run()
        {
            if(_events.EntitiesCount <= 0) return;
            
            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
            _events.RemoveAllEntities();
        }
    }
}