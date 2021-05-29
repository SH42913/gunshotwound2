using GTA;
using GunshotWound2.Configs;
using GunshotWound2.Damage;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Healing
{
    [EcsInject]
    public sealed class BandageSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<LocaleConfig> _localeConfig = null;
        private readonly EcsFilter<ApplyBandageEvent> _requestEvents = null;
        private readonly EcsFilter<SuccessfulBandageEvent> _successfulEvents = null;
        private readonly EcsFilter<WoundedPedComponent, BandageInProgressComponent> _pedsWithBandageInProgress = null;
        private readonly EcsFilter<BleedingComponent> _bleedings = null;

        public void Run()
        {
            for (var i = 0; i < _requestEvents.EntitiesCount; i++)
            {
                var pedEntity = _requestEvents.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed?.MostDangerBleedingEntity == null) continue;
                if (woundedPed.InPermanentRagdoll) continue;

                var progress = _ecsWorld.EnsureComponent<BandageInProgressComponent>(pedEntity, out var isNew);
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

                var timeToBandage = _config.Data.WoundConfig.ApplyBandageTime;
                progress.EstimateTime = timeToBandage;
                SendMessage(string.Format(_localeConfig.Data.YouTryToBandage, timeToBandage.ToString("F1")), pedEntity);
            }

            _requestEvents.CleanFilter();

            var frameTimeInSec = Game.LastFrameTime;
            for (var i = 0; i < _pedsWithBandageInProgress.EntitiesCount; i++)
            {
                var woundedPed = _pedsWithBandageInProgress.Components1[i];
                var thisPed = woundedPed.ThisPed;
                var progress = _pedsWithBandageInProgress.Components2[i];
                var pedEntity = _pedsWithBandageInProgress.Entities[i];

                if (woundedPed.InPermanentRagdoll || thisPed.IsWalking || thisPed.IsRunning ||
                    thisPed.IsSprinting || thisPed.IsShooting || thisPed.IsRagdoll ||
                    thisPed.IsJumping || thisPed.IsReloading || thisPed.IsSwimming)
                {
                    SendMessage($"~r~{_localeConfig.Data.BandageFailed}", pedEntity);
                    _ecsWorld.RemoveComponent<BandageInProgressComponent>(pedEntity);
                    continue;
                }

                progress.EstimateTime -= frameTimeInSec;

                if (progress.EstimateTime > 0) continue;
                _ecsWorld.RemoveComponent<BandageInProgressComponent>(pedEntity);
                _ecsWorld.CreateEntityWith<SuccessfulBandageEvent>().Entity = pedEntity;
            }

            for (var i = 0; i < _successfulEvents.EntitiesCount; i++)
            {
                var pedEntity = _successfulEvents.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed?.MostDangerBleedingEntity == null) continue;

                var bleedingEntity = woundedPed.MostDangerBleedingEntity.Value;
                if (!_ecsWorld.IsEntityExists(bleedingEntity)) continue;

                var bleeding = _ecsWorld.GetComponent<BleedingComponent>(woundedPed.MostDangerBleedingEntity.Value);
                if (bleeding == null) continue;

                bleeding.BleedSeverity = bleeding.BleedSeverity / 2;
                UpdateMostDangerWound(woundedPed, pedEntity);
                SendMessage(string.Format("~g~" + _localeConfig.Data.BandageSuccess, bleeding.Name), pedEntity);
            }

            _successfulEvents.CleanFilter();
        }

        private void SendMessage(string message, int pedEntity, NotifyLevels level = NotifyLevels.COMMON)
        {
#if !DEBUG
            if (level == NotifyLevels.DEBUG) return;
            if (_config.Data.PlayerConfig.PlayerEntity != pedEntity) return;
#endif

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }

        private void UpdateMostDangerWound(WoundedPedComponent woundedPed, int pedEntity)
        {
            if (woundedPed.ThisPed.IsDead) return;

            float maxBleeding = 0;
            int? mostDangerEntity = null;

            for (var i = 0; i < _bleedings.EntitiesCount; i++)
            {
                var bleeding = _bleedings.Components1[i];
                if (!bleeding.CanBeHealed) continue;
                if (bleeding.Entity != pedEntity) continue;
                if (bleeding.BleedSeverity <= maxBleeding) continue;

                maxBleeding = bleeding.BleedSeverity;
                mostDangerEntity = _bleedings.Entities[i];
            }

            woundedPed.MostDangerBleedingEntity = mostDangerEntity;
        }
    }
}