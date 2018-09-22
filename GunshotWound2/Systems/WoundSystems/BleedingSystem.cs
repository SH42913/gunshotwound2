using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class BleedingSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<BleedingComponent> _bleedings;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BleedingSystem);
#endif
            
            ProcessBleedings();
        }

        private void ProcessBleedings()
        {
            var frameTimeInSeconds = Game.LastFrameTime;
            
            for (int i = 0; i < _bleedings.EntitiesCount; i++)
            {
                BleedingComponent component = _bleedings.Components1[i];
                int pedEntity = _bleedings.Components1[i].Entity;
                int bleedingEntity = _bleedings.Entities[i];
                
                if (!_ecsWorld.IsEntityExists(pedEntity))
                {
                    RemoveBleeding(null, pedEntity, bleedingEntity);
                    continue;
                }
                
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null)
                {
                    RemoveBleeding(null, pedEntity, bleedingEntity);
                    continue;
                }
                
                if (component.BleedSeverity <= 0f)
                {
                    RemoveBleeding(woundedPed, pedEntity, bleedingEntity);
                    continue;
                }
                
                woundedPed.Health -= component.BleedSeverity * frameTimeInSeconds;
                component.BleedSeverity -= woundedPed.StopBleedingAmount * frameTimeInSeconds;
                woundedPed.ThisPed.Health = (int) woundedPed.Health;

                if (!woundedPed.ThisPed.IsDead) continue;
                RemoveBleeding(woundedPed, pedEntity, bleedingEntity);
            }
        }

        private void RemoveBleeding(WoundedPedComponent woundedPed, int pedEntity, int bleedingEntity)
        {
            _ecsWorld.RemoveEntity(bleedingEntity);
            if(woundedPed == null) return;
            
            woundedPed.BleedingCount--;
            UpdateMostDangerWound(woundedPed, pedEntity);
        }

        private void UpdateMostDangerWound(WoundedPedComponent woundedPed, int pedEntity)
        {
            if(woundedPed.ThisPed.IsDead) return;

            float maxBleeding = 0;
            int? mostDangerEntity = null;
            
            for (int i = 0; i < _bleedings.EntitiesCount; i++)
            {
                BleedingComponent bleeding = _bleedings.Components1[i];
                if(!bleeding.CanBeHealed) continue;
                if(bleeding.Entity != pedEntity) continue;
                if(bleeding.BleedSeverity <= maxBleeding) continue;

                maxBleeding = bleeding.BleedSeverity;
                mostDangerEntity = _bleedings.Entities[i];
            }

            woundedPed.MostDangerBleedingEntity = mostDangerEntity;
        }
    }
}