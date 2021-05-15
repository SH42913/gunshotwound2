using GTA.Native;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public sealed class FlashSystem : IEcsRunSystem
    {
        private readonly EcsFilter<AddFlashEvent> _events = null;

        public void Run()
        {
            if (_events.EntitiesCount <= 0) return;

            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
            _events.CleanFilter();
        }
    }
}