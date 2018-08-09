using System;
using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using GunshotWound2.Components.WoundComponents;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class CuttingDamageSystem : BaseDamageSystem<CuttingHitComponent>
    {
        public override void Initialize()
        {
            WeaponClass = "Cutting";

            HelmetSafeChance = 0.5f;
            ArmorDamage = 5;
            
            DefaultAction = DefaultGrazeWound;
            
            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {HeadCase1, 1},
                {HeadCase2, 1},
                {HeadCase3, 1},
            };
            
            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {NeckCase1, 5},
                {NeckCase2, 3},
                {NeckCase3, 1},
            };
            
            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {UpperCase1, 5},
                {UpperCase2, 3},
                {UpperCase3, 1},
            };
            
            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LowerCase1, 5},
                {LowerCase2, 3},
                {LowerCase3, 1},
            };
            
            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {ArmCase1, 5},
                {ArmCase2, 3},
                {ArmCase3, 1},
            };
            
            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LegCase1, 5},
                {LegCase2, 3},
                {LegCase3, 1},
            };
            
            LoadMultsFromConfig();
        }
        
        private void DefaultGrazeWound(int entity)
        {
            CreateWound("Graze wound", entity, DamageMultiplier * 10f,
                0.1f, PainMultiplier * 15f);
        }

        private void CreateIncisionWound(string position, int entity)
        {
            CreateWound($"Incision wound on {position}", entity, DamageMultiplier * 10f,
                BleeedingMultiplier * 0.3f, PainMultiplier * 20f);
        }

        private void CreateLacerationWound(string position, int entity, 
            float arteryChance, params DamageTypes?[] crits)
        {
            CreateWound($"Laceration wound on {position}", entity, DamageMultiplier * 20f,
                BleeedingMultiplier * 0.8f, PainMultiplier * 30f, arteryChance, crits);
        }

        private void CreateStabWound(string position, int entity, 
            float arteryChance, params DamageTypes?[] crits)
        {
            CreateWound($"Stab wound on {position}", entity, DamageMultiplier * 30f,
                BleeedingMultiplier * 0.8f, PainMultiplier * 40f, arteryChance, crits);
        }
        
        
        
        private void HeadCase1(int entity)
        {
            CreateIncisionWound("head", entity);
        }

        private void HeadCase2(int entity)
        {
            CreateLacerationWound("head", entity, 0f);
        }

        private void HeadCase3(int entity)
        {
            CreateWound("Heavy brain damage", entity, DamageMultiplier * 60f,
                BleeedingMultiplier * 2f, PainMultiplier * 70f);
        }

        

        private void NeckCase1(int entity)
        {
            CreateIncisionWound("neck", entity);
        }

        private void NeckCase2(int entity)
        {
            CreateLacerationWound("neck", entity, 0.7f);
        }

        private void NeckCase3(int entity)
        {
            CreateStabWound("neck", entity, 0.7f, DamageTypes.NERVES_DAMAGED);
        }



        private void UpperCase1(int entity)
        {
            CreateIncisionWound("upper body", entity);
        }

        private void UpperCase2(int entity)
        {
            CreateLacerationWound("upper body", entity, 0.5f);
        }

        private void UpperCase3(int entity)
        {
            CreateStabWound("upper body", entity, 0.5f, DamageTypes.HEART_DAMAGED, DamageTypes.LEGS_DAMAGED);
        }

        

        private void LowerCase1(int entity)
        {
            CreateIncisionWound("lower body", entity);
        }

        private void LowerCase2(int entity)
        {
            CreateLacerationWound("lower body", entity, 0.3f, DamageTypes.STOMACH_DAMAGED, DamageTypes.GUTS_DAMAGED);
        }

        private void LowerCase3(int entity)
        {
            CreateStabWound("lower body", entity, 0.3f, DamageTypes.GUTS_DAMAGED, DamageTypes.STOMACH_DAMAGED);
        }



        private void ArmCase1(int entity)
        {
            CreateIncisionWound("arm", entity);
        }

        private void ArmCase2(int entity)
        {
            CreateLacerationWound("arm", entity, 0.3f, DamageTypes.ARMS_DAMAGED);
        }

        private void ArmCase3(int entity)
        {
            CreateStabWound("arm", entity, 0.3f, DamageTypes.ARMS_DAMAGED);
        }



        private void LegCase1(int entity)
        {
            CreateIncisionWound("leg", entity);
        }

        private void LegCase2(int entity)
        {
            CreateLacerationWound("leg", entity, 0.5f, DamageTypes.LEGS_DAMAGED);
        }

        private void LegCase3(int entity)
        {
            CreateStabWound("leg", entity, 0.5f, DamageTypes.LEGS_DAMAGED);
        }
    }
}