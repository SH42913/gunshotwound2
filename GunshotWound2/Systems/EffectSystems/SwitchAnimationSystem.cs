using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.EffectSystems
{
    [EcsInject]
    public class SwitchAnimationSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<ChangeWalkAnimationEvent> _events;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(SwitchAnimationSystem);
#endif
            
            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                int pedEntity = _events.Components1[i].PedEntity;
                if(!_ecsWorld.IsEntityExists(pedEntity)) continue;
                
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if(woundedPed == null) continue;

                var animationName = _events.Components1[i].AnimationName;
                if (string.IsNullOrEmpty(animationName) || !woundedPed.ThisPed.IsAlive) continue;
                
                Function.Call(Hash.REQUEST_ANIM_SET, animationName);
                if (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, animationName))
                {
                    Function.Call(Hash.REQUEST_ANIM_SET, animationName);
                }
                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, woundedPed.ThisPed, animationName, 1.0f);
            }
            _events.RemoveAllEntities();
        }
    }
}