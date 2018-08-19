using System;
using GunshotWound2.Components.Events.BodyHitEvents;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.WeaponHitEvents;
using GunshotWound2.Components.Events.WoundEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Systems.DamageProcessingSystems
{
    [EcsInject]
    public abstract class BaseDamageSystem<T> : IEcsInitSystem, IEcsRunSystem 
        where T : BaseWeaponHitEvent, new()
    {
        protected EcsWorld EcsWorld;
        protected EcsFilterSingle<MainConfig> Config;
        
        protected EcsFilter<T> HitComponents;
        protected EcsFilter<BodyPartWasHitEvent> BodyHitComponents;
        
        protected string WeaponClass = "UNKNOWN";
        protected float DamageMultiplier = 1f;
        protected float BleeedingMultiplier = 1f;
        protected float PainMultiplier = 1f;
        protected float CritChance = 0.5f;

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
#if DEBUG
            GunshotWound2.LastSystem = nameof(BaseDamageSystem<T>);
#endif
            
            for (int damageIndex = 0; damageIndex < HitComponents.EntitiesCount; damageIndex++)
            {
                var damage = HitComponents.Components1[damageIndex];
                var pedEntity = damage.PedEntity;

                BodyPartWasHitEvent bodyHitComponent = null;
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

        protected virtual void ProcessWound(BodyPartWasHitEvent bodyHit, int pedEntity)
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
                        SendMessage("Armor saved your chest", pedEntity, NotifyLevels.WARNING);
                        CreatePain(pedEntity, ArmorDamage/5f);
                        return;
                    }
                    UpperBodyActions?.NextWithReplacement()(pedEntity);
                    break;
                case BodyParts.LOWER_BODY:
                    if (!CheckArmorPenetration(woundedPed, pedEntity))
                    {
                        SendMessage("Armor saved your lower body", pedEntity, NotifyLevels.WARNING);
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
            var wound = EcsWorld.CreateEntityWith<ProcessWoundEvent>();
            wound.Name = name;
            wound.PedEntity = pedEntity;
            wound.Damage = damage;
            wound.Pain = pain;
            wound.BleedSeverity = bleeding;

            wound.ArterySevered = arteryDamageChance > 0 && Random.IsTrueWithProbability(arteryDamageChance);

            if (possibleCrits.Length <= 0)
            {
                wound.CriticalDamage = null;
                return;
            }
            
            if(!Random.IsTrueWithProbability(CritChance)) return;
            int critIndex = Random.Next(0, possibleCrits.Length);
            var crit = possibleCrits[critIndex];
            wound.CriticalDamage = crit;
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
            if(multsArray.Length < 4) return;
            
            var damage = multsArray[0];
            if (damage.HasValue) DamageMultiplier = damage.Value;

            var bleeding = multsArray[1];
            if (bleeding.HasValue) BleeedingMultiplier = bleeding.Value;

            var pain = multsArray[2];
            if (pain.HasValue) PainMultiplier = pain.Value;

            var critChance = multsArray[3];
            if (critChance.HasValue) CritChance = critChance.Value;
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = EcsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
        
        private void SendMessage(string message, int pedEntity, NotifyLevels level = NotifyLevels.COMMON)
        {  
#if !DEBUG
            if(Config.Data.PlayerConfig.PlayerEntity != pedEntity) return;
#endif
            
            var notification = EcsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = level;
            notification.StringToShow = message;
        }

        private void CreatePain(int pedEntity, float amount)
        {
            var pain = EcsWorld.CreateEntityWith<AddPainEvent>();
            pain.PedEntity = pedEntity;
            pain.PainAmount = amount;
        }

        public void Destroy()
        {
            
        }
    }
}