using System;
using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using Weighted_Randomizer;

namespace GunshotWound2.Systems.DamageSystems
{
    public class ExplosionDamageSystem : BaseDamageSystem<ExplosionHitComponent>
    {
        public override void Initialize()
        {
            WeaponClass = "Explosive";

            HelmetSafeChance = 0;
            ArmorDamage = 100;
            
            DefaultAction = DefaultGrazeWound;
            
            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {HeadCase1, 1},
            };
            
            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {NeckCase1, 1},
            };
            
            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {UpperCase1, 1},
            };
            
            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LowerCase1, 1},
            };
            
            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {ArmCase1, 1},
            };
            
            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LegCase1, 1},
            };
        }

        private void HeadCase1(int obj)
        {
            CreateBlownWound("Head", obj);
        }

        private void NeckCase1(int obj)
        {
            CreateBlownWound("Neck", obj);
        }

        private void UpperCase1(int obj)
        {
            CreateBlownWound("Chest", obj);
        }

        private void LowerCase1(int obj)
        {
            CreateBlownWound("Lower body", obj);
        }

        private void ArmCase1(int obj)
        {
            CreateBlownWound("Arm", obj);
        }

        private void LegCase1(int obj)
        {
            CreateBlownWound("Leg", obj);
        }

        private void DefaultGrazeWound(int entity)
        {
            CreateWound("Body blown", entity, DamageMultiplier * 80f,
                2f, PainMultiplier * 80f);
        }

        private void CreateBlownWound(string position, int entity)
        {
            CreateWound($"{position} blown", entity, DamageMultiplier * 80f,
                BleeedingMultiplier * 2f, PainMultiplier * 80f);
        }
    }
}