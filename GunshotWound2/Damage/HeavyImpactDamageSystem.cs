using System;
using GunshotWound2.HitDetection;
using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public sealed class HeavyImpactDamageSystem : BaseImpactDamageSystem<HeavyImpactHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "HeavyImpact";

            HelmetSafeChance = 0.8f;
            ArmorDamage = 0;
            CanPenetrateArmor = false;

            CritChance = 0.7f;

            DefaultAction = DefaultGrazeWound;

            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {HeadCase1, 3},
                {HeadCase2, 3},
                {HeadCase3, 1},
                {HeadCase4, 1},
            };

            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {NeckCase1, 3},
                {NeckCase2, 2},
                {NeckCase3, 1},
            };

            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {UpperCase1, 3},
                {UpperCase2, 2},
                {UpperCase3, 1},
            };

            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LowerCase1, 3},
                {LowerCase2, 2},
                {LowerCase3, 1},
            };

            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {ArmCase1, 3},
                {ArmCase2, 2},
                {ArmCase3, 1},
            };

            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LegCase1, 3},
                {LegCase2, 2},
                {LegCase3, 1},
            };

            LoadMultsFromConfig();
        }


        private void Blackout(int entity)
        {
            CreateWound(Locale.Data.Blackout, entity, DamageMultiplier * 30f, -1, PainMultiplier * 70f);
        }

        private void HeavyBrainDamage(int entity)
        {
            CreateHeavyBrainDamage(Locale.Data.BleedingInHead, entity);
        }

        private void BrainInjury(int entity)
        {
            CreateHeavyBrainDamage(Locale.Data.TraumaticBrainInjury, entity);
        }

        private void BrokenNeck(int entity)
        {
            CreateWound(Locale.Data.BrokenNeck, entity, DamageMultiplier * 50f,
                -1, -1, 70f, CritTypes.NERVES_DAMAGED);
        }


        private void HeadCase1(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartHead, entity);
        }

        private void HeadCase2(int entity)
        {
            Blackout(entity);
        }

        private void HeadCase3(int entity)
        {
            BrainInjury(entity);
        }

        private void HeadCase4(int entity)
        {
            HeavyBrainDamage(entity);
        }


        private void NeckCase1(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartNeck, entity);
        }

        private void NeckCase2(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartNeck, entity);
        }

        private void NeckCase3(int entity)
        {
            BrokenNeck(entity);
        }


        private void UpperCase1(int entity)
        {
            AbrasionWoundOn(Locale.Data.BodyPartChest, entity);
        }

        private void UpperCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartChest, entity);
        }

        private void UpperCase3(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartChest, entity, CritTypes.LUNGS_DAMAGED, CritTypes.HEART_DAMAGED);
        }


        private void LowerCase1(int entity)
        {
            AbrasionWoundOn(Locale.Data.BodyPartLowerBody, entity);
        }

        private void LowerCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartLowerBody, entity);
        }

        private void LowerCase3(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartLowerBody, entity, CritTypes.STOMACH_DAMAGED, CritTypes.GUTS_DAMAGED);
        }


        private void ArmCase1(int entity)
        {
            AbrasionWoundOn(Locale.Data.BodyPartArm, entity);
        }

        private void ArmCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartArm, entity);
        }

        private void ArmCase3(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartArm, entity, CritTypes.ARMS_DAMAGED);
        }


        private void LegCase1(int entity)
        {
            AbrasionWoundOn(Locale.Data.BodyPartLeg, entity);
        }

        private void LegCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartLeg, entity);
        }

        private void LegCase3(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartLeg, entity, CritTypes.LEGS_DAMAGED);
        }
    }
}