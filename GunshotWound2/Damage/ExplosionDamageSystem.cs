using System;
using GunshotWound2.HitDetection.WeaponHitSystems;
using Weighted_Randomizer;

namespace GunshotWound2.Damage
{
    public sealed class ExplosionDamageSystem : BaseDamageSystem<ExplosionHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "Explosive";

            HelmetSafeChance = 0;
            ArmorDamage = 200;
            CanPenetrateArmor = true;

            CritChance = 1f;

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
            CreateBlownWound(Locale.Data.HeadBlown, obj);
        }

        private void NeckCase1(int obj)
        {
            CreateBlownWound(Locale.Data.NeckBlown, obj);
        }

        private void UpperCase1(int obj)
        {
            CreateBlownWound(Locale.Data.ChestBlown, obj);
        }

        private void LowerCase1(int obj)
        {
            CreateBlownWound(Locale.Data.LowerBodyBlown, obj);
        }

        private void ArmCase1(int obj)
        {
            CreateBlownWound(Locale.Data.ArmBlown, obj);
        }

        private void LegCase1(int obj)
        {
            CreateBlownWound(Locale.Data.LegBlown, obj);
        }

        private void DefaultGrazeWound(int entity)
        {
            CreateWound(Locale.Data.BodyBlown, entity, DamageMultiplier * 80f,
                2f, PainMultiplier * 80f);
        }

        private void CreateBlownWound(string position, int entity)
        {
            CreateWound(position, entity, DamageMultiplier * 80f,
                BleeedingMultiplier * 2f, PainMultiplier * 80f);
        }
    }
}