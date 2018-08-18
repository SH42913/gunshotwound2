using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.EffectSystems
{
    [EcsInject]
    public class SwitchAnimationSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<ChangeWalkAnimationEvent> _components;
        
        public void Run()
        {
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var woundedPed = _ecsWorld
                    .GetComponent<WoundedPedComponent>(_components.Components1[i].PedEntity);

                var animationName = _components.Components1[i].AnimationName;
                if (woundedPed != null && !string.IsNullOrEmpty(animationName) && woundedPed.ThisPed.IsAlive)
                {
                    SendMessage($"New animation is {animationName}");
                    Function.Call(Hash.REQUEST_ANIM_SET, animationName);
            
                    if (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, animationName))
                    {
                        Function.Call(Hash.REQUEST_ANIM_SET, animationName);
                    }
                    Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, woundedPed.ThisPed, animationName, 1.0f);
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }

        private void SendMessage(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}