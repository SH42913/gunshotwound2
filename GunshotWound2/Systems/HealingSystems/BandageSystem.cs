using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.HealingEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HealingSystems
{
    [EcsInject]
    public class BandageSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<ApplyBandageEvent> _events;
        private EcsFilter<BleedingComponent> _bleedings;
        
        public void Run()
        {
            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                int pedEntity = _events.Components1[i].Entity;
                if(!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if(woundedPed?.MostDangerBleedingEntity == null) continue;
                if(woundedPed.InPermanentRagdoll) continue;

                int bleedingEntity = woundedPed.MostDangerBleedingEntity.Value;
                if(!_ecsWorld.IsEntityExists(bleedingEntity)) continue;
                
                var bleeding = _ecsWorld.GetComponent<BleedingComponent>(woundedPed.MostDangerBleedingEntity.Value);
                if(bleeding == null) continue;

                if (woundedPed.IsPlayer && Game.Player.Money > _config.Data.WoundConfig.BandageCost)
                {
                    Game.Player.Money -= _config.Data.WoundConfig.BandageCost;
                }
                
                bleeding.BleedSeverity = bleeding.BleedSeverity / 2;
                UpdateMostDangerWound(woundedPed, pedEntity);
                SendMessage($"~g~You applied bandage to {bleeding.Name}", pedEntity);
            }
            
            _events.RemoveAllEntities();
        }
        
        private void SendMessage(string message, int pedEntity, NotifyLevels level = NotifyLevels.COMMON)
        {  
#if !DEBUG
            if(level == NotifyLevels.DEBUG) return;
            if(_config.Data.PlayerConfig.PlayerEntity != pedEntity) return;
#endif
            
            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
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