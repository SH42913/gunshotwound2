using System;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.WoundEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class WoundSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<ProcessWoundEvent> _components;
        
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilterSingle<LocaleConfig> _locale;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(WoundSystem);
#endif
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                ProcessWoundEvent component = _components.Components1[i];
                int pedEntity = component.Entity;
                if(!_ecsWorld.IsEntityExists(pedEntity)) continue;
                
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;
                
                float damageDeviation = component.Damage > 0 
                    ? _config.Data.WoundConfig.DamageDeviation * component.Damage 
                    : 0;
                float bleedingDeviation = component.BleedSeverity > 0 
                    ? _config.Data.WoundConfig.BleedingDeviation * component.BleedSeverity 
                    : 0;

                if (!woundedPed.IsDead)
                {
                    woundedPed.Health -= _config.Data.WoundConfig.DamageMultiplier * component.Damage +
                                         Random.NextFloat(-damageDeviation, damageDeviation);
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
                }
                    
                CreateBleeding(woundedPed, pedEntity, component.BleedSeverity +
                                          Random.NextFloat(-bleedingDeviation, bleedingDeviation), component.Name);
                woundedPed.BleedingCount++;
                
                CreatePain(pedEntity, component.Pain);
                CreateCritical(pedEntity, component.Crits);

                if (component.ArterySevered)
                {
                    CreateBleeding(woundedPed, pedEntity, 1f, _locale.Data.SeveredArtery);
                    woundedPed.BleedingCount++;
                }

#if DEBUG
                _ecsWorld.CreateEntityWith<ShowDebugInfoEvent>().Entity = pedEntity;
#endif
                SendWoundInfo(component, woundedPed);
            }
            _components.RemoveAllEntities();
        }

        private void CreateCritical(int pedEntity, CritTypes? crit)
        {
            if(crit == null) return;
            
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
                    _ecsWorld.CreateEntityWith<GutsCritcalWoundEvent>().Entity = pedEntity;
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
            float mult = _config.Data.WoundConfig.BleedingMultiplier;
            int entity = _ecsWorld.CreateEntityWith(out BleedingComponent bleedingComponent);
            bleedingComponent.Entity = pedEntity;
            bleedingComponent.BleedSeverity = mult * bleedSeverity;
            bleedingComponent.Name = name;
            bleedingComponent.CanBeHealed = bleedSeverity <= mult * BleedingComponent.MAX_SEVERITY_FOR_HEAL;
            if(!bleedingComponent.CanBeHealed) return;

            if (woundedPed.MostDangerBleedingEntity == null || !_ecsWorld.IsEntityExists(woundedPed.MostDangerBleedingEntity.Value))
            {
                woundedPed.MostDangerBleedingEntity = entity;
                return;
            }

            var oldBleeding = _ecsWorld.GetComponent<BleedingComponent>(woundedPed.MostDangerBleedingEntity.Value);
            if(oldBleeding != null && oldBleeding.BleedSeverity >= bleedingComponent.BleedSeverity) return;
            
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
            if(_config.Data.PlayerConfig.PlayerEntity != component.Entity) return;
#endif
            if(woundedPed.IsDead) return;
            
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