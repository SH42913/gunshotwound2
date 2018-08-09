using System;
using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using GunshotWound2.Components.WoundComponents;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class LightImpactDamageSystem : BaseImpactDamageSystem<LightImpactHitComponent>
    {
        public override void Initialize()
        {
            WeaponClass = "Light Impact";

            HelmetSafeChance = 0.9f;
            ArmorDamage = 0;
            
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

        private void HeadCase1(int entity)
        {
            LightBruiseWound("head", entity);
        }

        private void HeadCase2(int entity)
        {
            MediumBruiseWound("head", entity);
        }

        private void HeadCase3(int entity)
        {
            WindedFromImpact(entity);
        }

        

        private void NeckCase1(int entity)
        {
            LightBruiseWound("neck", entity);
        }

        private void NeckCase2(int entity)
        {
            MediumBruiseWound("neck", entity);
        }

        private void NeckCase3(int entity)
        {
            HeavyBruiseWound("neck", entity, DamageTypes.NERVES_DAMAGED);
        }



        private void UpperCase1(int entity)
        {
            LightBruiseWound("upper body", entity);
        }

        private void UpperCase2(int entity)
        {
            MediumBruiseWound("upper body", entity);
        }

        private void UpperCase3(int entity)
        {
            HeavyBruiseWound("upper body", entity, DamageTypes.HEART_DAMAGED, DamageTypes.LUNGS_DAMAGED);
        }

        

        private void LowerCase1(int entity)
        {
            LightBruiseWound("lower body", entity);
        }

        private void LowerCase2(int entity)
        {
            MediumBruiseWound("lower body", entity);
        }

        private void LowerCase3(int entity)
        {
            HeavyBruiseWound("lower body", entity, DamageTypes.STOMACH_DAMAGED, DamageTypes.GUTS_DAMAGED);
        }



        private void ArmCase1(int entity)
        {
            LightBruiseWound("arm", entity);
        }

        private void ArmCase2(int entity)
        {
            MediumBruiseWound("arm", entity);
        }

        private void ArmCase3(int entity)
        {
            HeavyBruiseWound("arm", entity, DamageTypes.ARMS_DAMAGED);
        }



        private void LegCase1(int entity)
        {
            LightBruiseWound("leg", entity);
        }

        private void LegCase2(int entity)
        {
            MediumBruiseWound("leg", entity);
        }

        private void LegCase3(int entity)
        {
            HeavyBruiseWound("leg", entity, DamageTypes.LEGS_DAMAGED);
        }
    }
}