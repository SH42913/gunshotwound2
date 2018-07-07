using System;
using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using GunshotWoundEcs.Components.UiComponents;
using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Configs;
using LeopotamGroup.Ecs;
using Weighted_Randomizer;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public abstract class BaseWeaponDamageSystem<T> : IEcsRunSystem where T : BaseWeaponDamageComponent, new()
    {
        protected string WeaponClass = "UNKNOWN";
        protected EcsWorld EcsWorld;
        protected EcsFilter<T> DamageComponents;
        protected EcsFilter<BodyDamageComponent> BodyComponents;
        
        protected EcsFilterSingle<MainConfig> MainConfig;
        protected EcsFilterSingle<PlayerConfig> PlayerConfig;

        protected Action<int> DefaultAction;
        protected IWeightedRandomizer<Action<int>> HeadActions;
        protected IWeightedRandomizer<Action<int>> NeckActions;
        protected IWeightedRandomizer<Action<int>> UpperBodyActions;
        protected IWeightedRandomizer<Action<int>> LowerBodyActions;
        protected IWeightedRandomizer<Action<int>> ArmActions;
        protected IWeightedRandomizer<Action<int>> LegActions;

        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(BaseWeaponDamageSystem<T>);
            for (int damageIndex = 0; damageIndex < DamageComponents.EntitiesCount; damageIndex++)
            {
                var damage = DamageComponents.Components1[damageIndex];
                var pedEntity = damage.PedEntity;

                BodyDamageComponent bodyDamageComponent = null;
                for (int bodyIndex = 0; bodyIndex < BodyComponents.EntitiesCount; bodyIndex++)
                {
                    var bodyDamage = BodyComponents.Components1[bodyIndex];
                    if(bodyDamage.PedEntity != pedEntity) continue;

                    bodyDamageComponent = bodyDamage;
                    EcsWorld.RemoveEntity(BodyComponents.Entities[bodyIndex]);
                    break;
                }
                
                EcsWorld.RemoveEntity(DamageComponents.Entities[damageIndex]);
                ProcessWound(bodyDamageComponent, pedEntity);
            }
        }

        protected virtual void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug($"{WeaponClass} processing");
            
            if (bodyDamage == null || bodyDamage.DamagedPart == BodyParts.NOTHING)
            {
                DefaultAction?.Invoke(pedEntity);
                return;
            }
            
            switch (bodyDamage.DamagedPart)
            {
                case BodyParts.HEAD:
                    HeadActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.NECK:
                    NeckActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.UPPER_BODY:
                    UpperBodyActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.LOWER_BODY:
                    LowerBodyActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.ARM:
                    ArmActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.LEG:
                    LegActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.NOTHING:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        

        protected void InstantDamage(int pedEntity, float amount)
        {
            var damageComponent = EcsWorld.CreateEntityWith<InstantDamageComponent>();
            damageComponent.PedEntity = pedEntity;
            damageComponent.DamageAmount = amount;
            
            CreatePain(pedEntity, 1.5f * amount);
        }

        protected void CreateBleeding(int pedEntity, float bleedSeverity, string name = "Wound")
        {
            var bleedingComponent = EcsWorld.CreateEntityWith<BleedingComponent>();
            bleedingComponent.PedEntity = pedEntity;
            bleedingComponent.BleedSeverity = bleedSeverity;
            bleedingComponent.Name = name;
        }

        protected void CreatePain(int pedEntity, float painAmount)
        {
            var bleedingComponent = EcsWorld.CreateEntityWith<PainComponent>();
            bleedingComponent.PedEntity = pedEntity;
            bleedingComponent.PainAmount = painAmount;
        }
        
        
        
        protected bool EntityIsPlayer(int entity)
        {
            return MainConfig.Data.Debug || PlayerConfig.Data.PlayerEntity == entity;
        }
        
        

        protected bool CanSendMessage(int entity)
        {
            return MainConfig.Data.Debug || EntityIsPlayer(entity);
        }

        protected void SendMessage(int entity, string message, NotifyLevels level = NotifyLevels.COMMON)
        {
            if(!CanSendMessage(entity)) return;
            
            var notification = EcsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }

        protected void SendDebug(string message)
        {
            var notification = EcsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
        }
    }
}