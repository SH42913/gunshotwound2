using System;
using GunshotWound2.Configs;
using GunshotWound2.Crits;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public sealed class WoundSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<ProcessWoundEvent> _components = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilterSingle<LocaleConfig> _locale = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(WoundSystem);
#endif

            for (var i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                var pedEntity = component.Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;

                var damageDeviation = component.Damage > 0
                    ? _config.Data.WoundConfig.DamageDeviation * component.Damage
                    : 0;

                if (!woundedPed.IsDead)
                {
                    woundedPed.Health -= _config.Data.WoundConfig.DamageMultiplier * component.Damage +
                                         GunshotWound2.Random.NextFloat(-damageDeviation, damageDeviation);
                    woundedPed.PedHealth = woundedPed.Health;
                }

                var bleedingDeviation = component.BleedSeverity > 0
                    ? _config.Data.WoundConfig.BleedingDeviation * component.BleedSeverity
                    : 0;

                var severity = component.BleedSeverity + GunshotWound2.Random.NextFloat(-bleedingDeviation, bleedingDeviation);
                CreateBleeding(woundedPed, pedEntity, severity, component.Name);
                CreatePain(pedEntity, component.Pain);
                CreateCritical(pedEntity, component.Crits);

                if (component.ArterySevered) CreateBleeding(woundedPed, pedEntity, 1f, _locale.Data.SeveredArtery);

#if DEBUG
                _ecsWorld.CreateEntityWith<ShowDebugInfoEvent>().Entity = pedEntity;
#endif
                SendWoundInfo(component, woundedPed);
            }

            _components.CleanFilter();
        }

        private void CreateCritical(int pedEntity, CritTypes? crit)
        {
            if (crit == null) return;

            switch (crit)
            {
                case CritTypes.LEGS_DAMAGED:
                    _ecsWorld.CreateEntityWith<LegsCriticalWoundEvent>().Entity = pedEntity;
                    break;
                case CritTypes.ARMS_DAMAGED:
                    _ecsWorld.CreateEntityWith<ArmsCriticalWoundEvent>().Entity = pedEntity;
                    break;
                case CritTypes.NERVES_DAMAGED:
                    _ecsWorld.CreateEntityWith<NervesCriticalWoundEvent>().Entity = pedEntity;
                    break;
                case CritTypes.GUTS_DAMAGED:
                    _ecsWorld.CreateEntityWith<GutsCriticalWoundEvent>().Entity = pedEntity;
                    break;
                case CritTypes.STOMACH_DAMAGED:
                    _ecsWorld.CreateEntityWith<StomachCriticalWoundEvent>().Entity = pedEntity;
                    break;
                case CritTypes.LUNGS_DAMAGED:
                    _ecsWorld.CreateEntityWith<LungsCriticalWoundEvent>().Entity = pedEntity;
                    break;
                case CritTypes.HEART_DAMAGED:
                    _ecsWorld.CreateEntityWith<HeartCriticalWoundEvent>().Entity = pedEntity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateBleeding(WoundedPedComponent woundedPed, int pedEntity, float bleedSeverity, string name)
        {
            var mult = _config.Data.WoundConfig.BleedingMultiplier;
            var entity = _ecsWorld.CreateEntityWith(out BleedingComponent bleedingComponent);
            bleedingComponent.Entity = pedEntity;
            bleedingComponent.BleedSeverity = mult * bleedSeverity;
            bleedingComponent.Name = name;
            bleedingComponent.CanBeHealed = bleedSeverity <= mult * BleedingComponent.MaxSeverityForHeal;
            woundedPed.BleedingCount++;
            if (!bleedingComponent.CanBeHealed) return;

            if (woundedPed.MostDangerBleedingEntity == null ||
                !_ecsWorld.IsEntityExists(woundedPed.MostDangerBleedingEntity.Value))
            {
                woundedPed.MostDangerBleedingEntity = entity;
                return;
            }

            var oldBleeding = _ecsWorld.GetComponent<BleedingComponent>(woundedPed.MostDangerBleedingEntity.Value);
            if (oldBleeding != null && oldBleeding.BleedSeverity >= bleedingComponent.BleedSeverity) return;

            woundedPed.MostDangerBleedingEntity = entity;
        }

        private void CreatePain(int pedEntity, float painAmount)
        {
            var painComponent = _ecsWorld.CreateEntityWith<IncreasePainEvent>();
            painComponent.Entity = pedEntity;
            painComponent.PainAmount = _config.Data.WoundConfig.PainMultiplier * painAmount;
        }

        private void SendWoundInfo(ProcessWoundEvent component, WoundedPedComponent woundedPed)
        {
#if !DEBUG
            if (_config.Data.PlayerConfig.PlayerEntity != component.Entity) return;
#endif
            if (woundedPed.IsDead) return;

            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();

            var message = $"{component.Name}";
            if (component.ArterySevered)
            {
                message += $"\n{_locale.Data.SeveredArteryMessage}";
            }

            if (component.Crits != null || component.ArterySevered ||
                component.BleedSeverity > _config.Data.WoundConfig.EmergencyBleedingLevel)
            {
                notification.Level = NotifyLevels.EMERGENCY;
            }
            else
            {
                notification.Level = NotifyLevels.ALERT;
            }

            notification.StringToShow = message;
        }
    }
}