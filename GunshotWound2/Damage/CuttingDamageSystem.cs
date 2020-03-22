using System;
using GunshotWound2.HitDetection;
using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public sealed class CuttingDamageSystem : BaseDamageSystem<CuttingHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "Cutting";

            HelmetSafeChance = 0.5f;
            ArmorDamage = 1;
            CanPenetrateArmor = false;

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
            CreateWound(Locale.Data.GrazeWound, entity, DamageMultiplier * 10f,
                0.1f, PainMultiplier * 15f);
        }

        private void CreateIncisionWound(string position, int entity)
        {
            CreateWound($"{Locale.Data.IncisionWoundOn} {position}", entity, DamageMultiplier * 10f,
                BleeedingMultiplier * 0.3f, PainMultiplier * 20f);
        }

        private void CreateLacerationWound(string position, int entity,
            float arteryChance, params CritTypes?[] crits)
        {
            CreateWound($"{Locale.Data.LacerationWoundOn} {position}", entity, DamageMultiplier * 20f,
                BleeedingMultiplier * 0.8f, PainMultiplier * 30f, arteryChance, crits);
        }

        private void CreateStabWound(string position, int entity,
            float arteryChance, params CritTypes?[] crits)
        {
            CreateWound($"{Locale.Data.StabWoundOn} {position}", entity, DamageMultiplier * 30f,
                BleeedingMultiplier * 0.6f, PainMultiplier * 40f, arteryChance, crits);
        }


        private void HeadCase1(int entity)
        {
            CreateIncisionWound(Locale.Data.BodyPartHead, entity);
        }

        private void HeadCase2(int entity)
        {
            CreateLacerationWound(Locale.Data.BodyPartHead, entity, 0.05f);
        }

        private void HeadCase3(int entity)
        {
            CreateHeavyBrainDamage(Locale.Data.HeavyBrainDamage, entity);
        }


        private void NeckCase1(int entity)
        {
            CreateIncisionWound(Locale.Data.BodyPartNeck, entity);
        }

        private void NeckCase2(int entity)
        {
            CreateLacerationWound(Locale.Data.BodyPartNeck, entity, 0.5f);
        }

        private void NeckCase3(int entity)
        {
            CreateStabWound(Locale.Data.BodyPartNeck, entity, 0.7f, CritTypes.NERVES_DAMAGED);
        }


        private void UpperCase1(int entity)
        {
            CreateIncisionWound(Locale.Data.BodyPartChest, entity);
        }

        private void UpperCase2(int entity)
        {
            CreateLacerationWound(Locale.Data.BodyPartChest, entity, 0.3f);
        }

        private void UpperCase3(int entity)
        {
            CreateStabWound(Locale.Data.BodyPartChest, entity, 0.5f, CritTypes.HEART_DAMAGED, CritTypes.LEGS_DAMAGED);
        }


        private void LowerCase1(int entity)
        {
            CreateIncisionWound(Locale.Data.BodyPartLowerBody, entity);
        }

        private void LowerCase2(int entity)
        {
            CreateLacerationWound(Locale.Data.BodyPartLowerBody, entity, 0.1f, CritTypes.STOMACH_DAMAGED,
                CritTypes.GUTS_DAMAGED);
        }

        private void LowerCase3(int entity)
        {
            CreateStabWound(Locale.Data.BodyPartLowerBody, entity, 0.3f, CritTypes.GUTS_DAMAGED,
                CritTypes.STOMACH_DAMAGED);
        }


        private void ArmCase1(int entity)
        {
            CreateIncisionWound(Locale.Data.BodyPartArm, entity);
        }

        private void ArmCase2(int entity)
        {
            CreateLacerationWound(Locale.Data.BodyPartArm, entity, 0.1f, CritTypes.ARMS_DAMAGED);
        }

        private void ArmCase3(int entity)
        {
            CreateStabWound(Locale.Data.BodyPartArm, entity, 0.3f, CritTypes.ARMS_DAMAGED);
        }


        private void LegCase1(int entity)
        {
            CreateIncisionWound(Locale.Data.BodyPartLeg, entity);
        }

        private void LegCase2(int entity)
        {
            CreateLacerationWound(Locale.Data.BodyPartLeg, entity, 0.3f, CritTypes.LEGS_DAMAGED);
        }

        private void LegCase3(int entity)
        {
            CreateStabWound(Locale.Data.BodyPartLeg, entity, 0.5f, CritTypes.LEGS_DAMAGED);
        }
    }
}