using GTA.Native;
using GunshotWound2.Damage;
using GunshotWound2.GUI;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection
{
    [EcsInject]
    public class HitDetectSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<WoundedPedComponent> _peds;

        public void Run()
        {
            for (int i = 0; i < _peds.EntitiesCount; i++)
            {
                if (!Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, _peds.Components1[i].ThisPed))
                    continue;

                _ecsWorld.AddComponent<HaveDamageMarkComponent>(_peds.Entities[i]);
                SendMessage($"Ped {_peds.Entities[i]} got damage", NotifyLevels.DEBUG);
            }
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}