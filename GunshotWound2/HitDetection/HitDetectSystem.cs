using GTA.Native;
using GunshotWound2.Damage;
using GunshotWound2.GUI;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection
{
    [EcsInject]
    public sealed class HitDetectSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<WoundedPedComponent> _peds = null;
        private readonly InputArgument[] _inputArguments = new InputArgument[1];

        public void Run()
        {
            for (var i = 0; i < _peds.EntitiesCount; i++)
            {
                _inputArguments[0] = _peds.Components1[i].ThisPed;
                if (!Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, _inputArguments) &&
                    !Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_OBJECT, _inputArguments))
                    continue;

                _ecsWorld.AddComponent<HaveDamageMarkComponent>(_peds.Entities[i]);
                SendMessage($"Ped {_peds.Entities[i].ToString()} got damage", NotifyLevels.DEBUG);
            }
        }

        private void SendMessage(string message, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }
    }
}