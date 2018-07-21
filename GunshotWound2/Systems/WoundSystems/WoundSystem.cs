using System;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class WoundSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<WoundComponent> _components;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(WoundSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                int pedEntity = component.PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    var damageDeviation = _config.Data.WoundConfig.DamageDeviation;
                    var bleedingDeviation = _config.Data.WoundConfig.BleedingDeviation;
                    
                    woundedPed.Health -= _config.Data.WoundConfig.DamageMultiplier * component.Damage +
                                         Random.NextFloat(-damageDeviation, damageDeviation);
                    woundedPed.ThisPed.Health = (int) woundedPed.Health;
                    
                    CreateBleeding(pedEntity, component.BleedSeverity +
                                              Random.NextFloat(-bleedingDeviation, bleedingDeviation), component.Name);
                    CreatePain(pedEntity, component.Pain);
                    CreateCritical(pedEntity, component.CriticalDamage);

                    if (component.ArterySevered)
                    {
                        CreateBleeding(pedEntity, 1f, "Severed artery");
                    }
                    
                    _ecsWorld.CreateEntityWith<DebugInfoRequestComponent>().PedEntity = pedEntity;
                    SendWoundInfo(component, woundedPed);
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }

        private void CreateCritical(int pedEntity, DamageTypes? damage)
        {
            if(damage == null) return;
            
            switch (damage)
            {
                case DamageTypes.LEGS_DAMAGED:
                    _ecsWorld.CreateEntityWith<LegsCriticalComponent>().PedEntity = pedEntity;
                    break;
                case DamageTypes.ARMS_DAMAGED:
                    _ecsWorld.CreateEntityWith<ArmsCriticalComponent>().PedEntity = pedEntity;
                    break;
                case DamageTypes.NERVES_DAMAGED:
                    _ecsWorld.CreateEntityWith<NervesCriticalComponent>().PedEntity = pedEntity;
                    break;
                case DamageTypes.GUTS_DAMAGED:
                    _ecsWorld.CreateEntityWith<GutsCritcalComponent>().PedEntity = pedEntity;
                    break;
                case DamageTypes.STOMACH_DAMAGED:
                    _ecsWorld.CreateEntityWith<StomachCriticalComponent>().PedEntity = pedEntity;
                    break;
                case DamageTypes.LUNGS_DAMAGED:
                    _ecsWorld.CreateEntityWith<LungsCriticalComponent>().PedEntity = pedEntity;
                    break;
                case DamageTypes.HEART_DAMAGED:
                    _ecsWorld.CreateEntityWith<HeartCriticalComponent>().PedEntity = pedEntity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateBleeding(int pedEntity, float bleedSeverity, string name)
        {
            var bleedingComponent = _ecsWorld.CreateEntityWith<BleedingComponent>();
            bleedingComponent.PedEntity = pedEntity;
            bleedingComponent.BleedSeverity = _config.Data.WoundConfig.BleedingMultiplier * bleedSeverity;
            bleedingComponent.Name = name;
        }

        private void CreatePain(int pedEntity, float painAmount)
        {
            var painComponent = _ecsWorld.CreateEntityWith<PainComponent>();
            painComponent.PedEntity = pedEntity;
            painComponent.PainAmount = _config.Data.WoundConfig.PainMultiplier * painAmount;
        }

        private void SendWoundInfo(WoundComponent component, WoundedPedComponent woundedPed)
        {
#if !DEBUG
            if(_config.Data.PlayerConfig.PlayerEntity != component.PedEntity) return;
#endif
            if(woundedPed.IsDead) return;
            
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();

            var message = $"{component.Name}";
            if (component.ArterySevered)
            {
                message += "\nArtery was severed!";
            }
            
            if (component.CriticalDamage != null || component.ArterySevered ||
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