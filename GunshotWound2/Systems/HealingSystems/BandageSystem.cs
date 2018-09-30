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
        private EcsFilterSingle<LocaleConfig> _localeConfig;
        
        private EcsFilter<ApplyBandageEvent> _requestEvents;
        private EcsFilter<SuccessfulBandageEvent> _successfulEvents;
        private EcsFilter<WoundedPedComponent, BandageInProgressComponent> _pedsWithBandageInProgress;
        private EcsFilter<BleedingComponent> _bleedings;
        
        public void Run()
        {
            for (int i = 0; i < _requestEvents.EntitiesCount; i++)
            {
                int pedEntity = _requestEvents.Components1[i].Entity;
                if(!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if(woundedPed?.MostDangerBleedingEntity == null) continue;
                if(woundedPed.InPermanentRagdoll) continue;

                var progress = _ecsWorld.EnsureComponent<BandageInProgressComponent>(pedEntity, out bool isNew);
                if (!isNew)
                {
                    SendMessage(_localeConfig.Data.AlreadyBandaging, pedEntity);
                    continue;
                }

                if (woundedPed.IsPlayer)
                {
                    if (Game.Player.Money < _config.Data.WoundConfig.BandageCost)
                    {
                        SendMessage(_localeConfig.Data.DontHaveMoneyForBandage, pedEntity);
                        continue;
                    }
                    Game.Player.Money -= _config.Data.WoundConfig.BandageCost;
                }

                float timeToBandage = _config.Data.WoundConfig.ApplyBandageTime;
                progress.EstimateTime = timeToBandage;
                SendMessage(string.Format(_localeConfig.Data.YouTryToBandage, timeToBandage), pedEntity);
            }
            _requestEvents.RemoveAllEntities();

            float frameTimeInSec = Game.LastFrameTime;
            for (int i = 0; i < _pedsWithBandageInProgress.EntitiesCount; i++)
            {
                WoundedPedComponent woundedPed = _pedsWithBandageInProgress.Components1[i];
                Ped thisPed = woundedPed.ThisPed;
                BandageInProgressComponent progress = _pedsWithBandageInProgress.Components2[i];
                int pedEntity = _pedsWithBandageInProgress.Entities[i];

                if (woundedPed.InPermanentRagdoll || thisPed.IsWalking || thisPed.IsRunning || 
                    thisPed.IsSprinting || thisPed.IsShooting || thisPed.IsRagdoll)
                {
                    SendMessage($"~r~{_localeConfig.Data.BandageFailed}", pedEntity);
                    _ecsWorld.RemoveComponent<BandageInProgressComponent>(pedEntity);
                    continue;
                }
                progress.EstimateTime -= frameTimeInSec;
                
                if(progress.EstimateTime > 0) continue;
                _ecsWorld.RemoveComponent<BandageInProgressComponent>(pedEntity);
                _ecsWorld.CreateEntityWith<SuccessfulBandageEvent>().Entity = pedEntity;
            }
            
            for (int i = 0; i < _successfulEvents.EntitiesCount; i++)
            {
                int pedEntity = _successfulEvents.Components1[i].Entity;
                if(!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if(woundedPed?.MostDangerBleedingEntity == null) continue;

                int bleedingEntity = woundedPed.MostDangerBleedingEntity.Value;
                if(!_ecsWorld.IsEntityExists(bleedingEntity)) continue;
                
                var bleeding = _ecsWorld.GetComponent<BleedingComponent>(woundedPed.MostDangerBleedingEntity.Value);
                if(bleeding == null) continue;
                
                bleeding.BleedSeverity = bleeding.BleedSeverity / 2;
                UpdateMostDangerWound(woundedPed, pedEntity);
                SendMessage(string.Format("~g~" + _localeConfig.Data.BandageSuccess, bleeding.Name), pedEntity);
            }
            _successfulEvents.RemoveAllEntities();
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