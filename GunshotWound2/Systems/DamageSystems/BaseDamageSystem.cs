using System;
using GunshotWound2.Components.HitComponents.BodyDamageComponents;
using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public abstract class BaseDamageSystem<T> : IEcsInitSystem, IEcsRunSystem 
        where T : BaseWeaponHitComponent, new()
    {
        protected EcsWorld EcsWorld;
        protected EcsFilterSingle<MainConfig> Config;
        
        protected EcsFilter<T> HitComponents;
        protected EcsFilter<BodyHitComponent> BodyHitComponents;
        
        protected string WeaponClass = "UNKNOWN";
        protected float DamageMultiplier = 1f;
        protected float BleeedingMultiplier = 1f;
        protected float PainMultiplier = 1f;

        protected Action<int> DefaultAction;
        protected IWeightedRandomizer<Action<int>> HeadActions;
        protected IWeightedRandomizer<Action<int>> NeckActions;
        protected IWeightedRandomizer<Action<int>> UpperBodyActions;
        protected IWeightedRandomizer<Action<int>> LowerBodyActions;
        protected IWeightedRandomizer<Action<int>> ArmActions;
        protected IWeightedRandomizer<Action<int>> LegActions;

        protected bool CanPenetrateArmor = false;
        protected int ArmorDamage = 0;
        protected float HelmetSafeChance = 0;
        
        private static readonly Random Random = new Random();

        public abstract void Initialize();

        public void Run()
        {
            GunshotWound2.LastSystem = nameof(BaseDamageSystem<T>);
            
            for (int damageIndex = 0; damageIndex < HitComponents.EntitiesCount; damageIndex++)
            {
                var damage = HitComponents.Components1[damageIndex];
                var pedEntity = damage.PedEntity;

                BodyHitComponent bodyHitComponent = null;
                for (int bodyIndex = 0; bodyIndex < BodyHitComponents.EntitiesCount; bodyIndex++)
                {
                    var bodyDamage = BodyHitComponents.Components1[bodyIndex];
                    if(bodyDamage.PedEntity != pedEntity) continue;

                    bodyHitComponent = bodyDamage;
                    EcsWorld.RemoveEntity(BodyHitComponents.Entities[bodyIndex]);
                    break;
                }
                
                EcsWorld.RemoveEntity(HitComponents.Entities[damageIndex]);
                ProcessWound(bodyHitComponent, pedEntity);
            }
        }

        protected virtual void ProcessWound(BodyHitComponent bodyHit, int pedEntity)
        {
            SendDebug($"{WeaponClass} processing");
            
            if (bodyHit == null || bodyHit.DamagedPart == BodyParts.NOTHING)
            {
                DefaultAction?.Invoke(pedEntity);
                return;
            }

            var woundedPed = EcsWorld.GetComponent<WoundedPedComponent>(pedEntity);
            if(woundedPed == null) return;
            
            switch (bodyHit.DamagedPart)
            {
                case BodyParts.HEAD:
                    if (woundedPed.ThisPed.IsWearingHelmet && Random.IsTrueWithProbability(HelmetSafeChance))
                    {
                        SendMessage("Helmet saved your head", pedEntity, NotifyLevels.WARNING);
                        return;
                    }
                    HeadActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.NECK:
                    NeckActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.UPPER_BODY:
                    if (!CheckArmorPenetration(woundedPed, pedEntity))
                    {
                        SendMessage("Armor saved your upper body", pedEntity, NotifyLevels.WARNING);
                        CreatePain(pedEntity, ArmorDamage/5f);
                        return;
                    }
                    UpperBodyActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.LOWER_BODY:
                    if (!CheckArmorPenetration(woundedPed, pedEntity))
                    {
                        SendMessage("Armor saved your upper body", pedEntity, NotifyLevels.WARNING);
                        CreatePain(pedEntity, ArmorDamage/5f);
                        return;
                    }
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
        
        protected void CreateWound(
            string name, 
            int pedEntity,
            float damage, 
            float bleeding, 
            float pain, 
            float arteryDamageChance = -1,
            params DamageTypes?[] possibleCrits)
        {
            var wound = EcsWorld.CreateEntityWith<WoundComponent>();
            wound.Name = name;
            wound.PedEntity = pedEntity;
            wound.Damage = damage;
            wound.Pain = pain;
            wound.BleedSeverity = bleeding;

            if (possibleCrits.Length == 0)
            {
                wound.CriticalDamage = null;
                return;
            }
            
            int critIndex = Random.Next(-1, possibleCrits.Length);
            var crit = critIndex == -1
                ? null
                : possibleCrits[critIndex];
            wound.CriticalDamage = crit;
            
            if(arteryDamageChance <= 0) return;
            wound.ArterySevered = Random.IsTrueWithProbability(arteryDamageChance);
        }

        private bool CheckArmorPenetration(WoundedPedComponent woundedPed, int pedEntity)
        {
            if (woundedPed.Armor == 0) return true;

            woundedPed.Armor -= ArmorDamage;
            if (woundedPed.Armor < 0)
            {
                return true;
            }

            if (!CanPenetrateArmor) return false;
            
            float armorPercent = woundedPed.Armor / 100f;
            bool penetration = Random.IsTrueWithProbability(1 - (0.6f + 0.4f * armorPercent));
            if (penetration)
            {
                SendMessage("Your armor was penetrated", pedEntity, NotifyLevels.WARNING);
            }

            return penetration;
        }

        protected void LoadMultsFromConfig()
        {
            var woundConfig = Config.Data.WoundConfig;
            if(woundConfig.DamageSystemConfigs == null) return;
            if(!woundConfig.DamageSystemConfigs.ContainsKey(WeaponClass)) return;
            
            var multsArray = woundConfig.DamageSystemConfigs[WeaponClass];
            if(multsArray.Length < 3) return;
            
            var damage = multsArray[0];
            if (damage.HasValue) DamageMultiplier = damage.Value;

            var bleeding = multsArray[1];
            if (bleeding.HasValue) BleeedingMultiplier = bleeding.Value;

            var pain = multsArray[2];
            if (pain.HasValue) PainMultiplier = pain.Value;
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = EcsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
        
        private void SendMessage(string message, int pedEntity, NotifyLevels level = NotifyLevels.COMMON)
        {  
#if !DEBUG
            if(Config.Data.PlayerConfig.PlayerEntity != pedEntity) return;
#endif
            
            var notification = EcsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = level;
            notification.StringToShow = message;
        }

        private void CreatePain(int pedEntity, float amount)
        {
            var pain = EcsWorld.CreateEntityWith<PainComponent>();
            pain.PedEntity = pedEntity;
            pain.PainAmount = amount;
        }

        public void Destroy()
        {
            
        }
    }
}