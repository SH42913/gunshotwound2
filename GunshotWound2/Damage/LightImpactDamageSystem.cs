using System;
using GunshotWound2.HitDetection;
using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public sealed class LightImpactDamageSystem : BaseImpactDamageSystem<LightImpactHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "LightImpact";

            HelmetSafeChance = 0.9f;
            ArmorDamage = 0;
            CanPenetrateArmor = false;

            CritChance = 0.3f;

            DefaultAction = DefaultGrazeWound;

            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {HeadCase1, 1},
                {HeadCase2, 1},
                {HeadCase3, 1},
            };

            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {NeckCase1, 8},
                {NeckCase2, 5},
                {NeckCase3, 1},
            };

            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {UpperCase1, 8},
                {UpperCase2, 5},
                {UpperCase3, 1},
            };

            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LowerCase1, 8},
                {LowerCase2, 5},
                {LowerCase3, 1},
            };

            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {ArmCase1, 8},
                {ArmCase2, 5},
                {ArmCase3, 1},
            };

            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LegCase1, 8},
                {LegCase2, 5},
                {LegCase3, 1},
            };

            LoadMultsFromConfig();
        }

        private void HeadCase1(int entity)
        {
            LightBruiseWound(Locale.Data.BodyPartHead, entity);
        }

        private void HeadCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartHead, entity);
        }

        private void HeadCase3(int entity)
        {
            WindedFromImpact(entity);
        }


        private void NeckCase1(int entity)
        {
            LightBruiseWound(Locale.Data.BodyPartNeck, entity);
        }

        private void NeckCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartNeck, entity);
        }

        private void NeckCase3(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartNeck, entity, CritTypes.NERVES_DAMAGED);
        }


        private void UpperCase1(int entity)
        {
            LightBruiseWound(Locale.Data.BodyPartChest, entity);
        }

        private void UpperCase2(int entity)
        {
            MediumBruiseWound(Locale.Data.BodyPartChest, entity);
        }

        private void UpperCase3(int entity)
        {
            HeavyBruiseWound(Locale.Data.BodyPartChest, entity, CritTypes.HEART_DAMAGED, CritTypes.LUNGS_DAMAGED);
        }


        private void LowerCase1(int entity)
        {
            LightBruiseWound(Locale.Data.BodyPartLowerBody, entity);
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
            LightBruiseWound(Locale.Data.BodyPartArm, entity);
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
            LightBruiseWound(Locale.Data.BodyPartLeg, entity);
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