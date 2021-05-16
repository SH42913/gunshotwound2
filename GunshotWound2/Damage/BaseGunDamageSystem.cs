using System;
using GunshotWound2.HitDetection;
using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public abstract class BaseGunDamageSystem<T> : BaseDamageSystem<T>
        where T : BaseWeaponHitEvent, new()
    {
        protected int GrazeWoundWeight;
        protected int FleshWoundWeight;
        protected int PenetratingWoundWeight;
        protected int PerforatingWoundWeight;
        protected int AvulsiveWoundWeight;

        protected void FillWithDefaultGunActions()
        {
            DefaultAction = DefaultGrazeWound;

            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {HeadCase1, GrazeWoundWeight},
                {HeadCase2, FleshWoundWeight},
                {HeadCase3, PenetratingWoundWeight},
                {HeadCase4, PerforatingWoundWeight},
                {HeadCase5, AvulsiveWoundWeight},
            };

            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {NeckCase1, GrazeWoundWeight},
                {NeckCase2, FleshWoundWeight},
                {NeckCase3, PenetratingWoundWeight},
                {NeckCase4, PerforatingWoundWeight},
                {NeckCase5, AvulsiveWoundWeight},
            };

            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {UpperBodyCase1, GrazeWoundWeight},
                {UpperBodyCase2, FleshWoundWeight},
                {UpperBodyCase3, PenetratingWoundWeight},
                {UpperBodyCase4, PerforatingWoundWeight},
                {UpperBodyCase5, AvulsiveWoundWeight},
            };

            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LowerBodyCase1, GrazeWoundWeight},
                {LowerBodyCase2, FleshWoundWeight},
                {LowerBodyCase3, PenetratingWoundWeight},
                {LowerBodyCase4, PerforatingWoundWeight},
                {LowerBodyCase5, AvulsiveWoundWeight},
            };

            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {ArmCase1, GrazeWoundWeight},
                {ArmCase2, FleshWoundWeight},
                {ArmCase3, PenetratingWoundWeight},
                {ArmCase4, PerforatingWoundWeight},
                {ArmCase5, AvulsiveWoundWeight},
            };

            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LegCase1, GrazeWoundWeight},
                {LegCase2, FleshWoundWeight},
                {LegCase3, PenetratingWoundWeight},
                {LegCase4, PerforatingWoundWeight},
                {LegCase5, AvulsiveWoundWeight},
            };
        }

        private void DefaultGrazeWound(int entity)
        {
            CreateWound(Locale.Data.GrazeWound, entity,
                DamageMultiplier * 15f, BleeedingMultiplier * 0.05f, PainMultiplier * 15f);
        }

        private void CreateGrazeWound(string position, int entity)
        {
            CreateWound($"{Locale.Data.GrazeGswOn} {position}", entity,
                DamageMultiplier * 15f, BleeedingMultiplier * 0.05f, PainMultiplier * 15f);
        }

        private void CreateFleshWound(string position, int entity, float arteryDamageChance)
        {
            CreateWound($"{Locale.Data.FleshGswOn} {position}", entity,
                DamageMultiplier * 20f, BleeedingMultiplier * 0.1f, PainMultiplier * 30f, arteryDamageChance);
        }

        private void CreatePenetratingWound(string position, int entity,
            float arteryDamageChance, params CritTypes?[] possibleCrits)
        {
            CreateWound($"{Locale.Data.PenetratingGswOn} {position}", entity,
                DamageMultiplier * 25f, BleeedingMultiplier * 0.2f, PainMultiplier * 40f,
                arteryDamageChance, possibleCrits);
        }

        private void CreatePerforatingWound(string position, int entity,
            float arteryDamageChance, params CritTypes?[] possibleCrits)
        {
            CreateWound($"{Locale.Data.PerforatingGswOn} {position}", entity,
                DamageMultiplier * 20f, BleeedingMultiplier * 0.25f, PainMultiplier * 40f,
                arteryDamageChance, possibleCrits);
        }

        private void CreateAvulsiveWound(string position, int entity,
            float arteryDamageChance, params CritTypes?[] possibleCrits)
        {
            CreateWound($"{Locale.Data.AvulsiveGswOn} {position}", entity,
                DamageMultiplier * 30f, BleeedingMultiplier * 0.30f, PainMultiplier * 50f,
                arteryDamageChance, possibleCrits);
        }

        #region HeadCases

        private void HeadCase1(int pedEntity)
        {
            CreateGrazeWound(Locale.Data.BodyPartHead, pedEntity);
        }

        private void HeadCase2(int pedEntity)
        {
            CreateHeavyBrainDamage(Locale.Data.BulletFlyThroughYourHead, pedEntity);
        }

        private void HeadCase3(int pedEntity)
        {
            CreateHeavyBrainDamage(Locale.Data.HeavyBrainDamage, pedEntity);
        }

        private void HeadCase4(int pedEntity)
        {
            CreateHeavyBrainDamage(Locale.Data.BulletFlyThroughYourHead, pedEntity);
        }

        private void HeadCase5(int pedEntity)
        {
            CreateHeavyBrainDamage(Locale.Data.BulletTornApartYourBrain, pedEntity);
        }

        #endregion

        #region NeckCases

        private void NeckCase1(int entity)
        {
            CreateGrazeWound(Locale.Data.BodyPartNeck, entity);
        }

        private void NeckCase2(int entity)
        {
            CreateFleshWound(Locale.Data.BodyPartNeck, entity, 0.05f);
        }

        private void NeckCase3(int entity)
        {
            CreatePenetratingWound(Locale.Data.BodyPartNeck, entity, 0.2f);
        }

        private void NeckCase4(int entity)
        {
            CreatePerforatingWound(Locale.Data.BodyPartNeck, entity, 0.1f);
        }

        private void NeckCase5(int entity)
        {
            CreateAvulsiveWound(Locale.Data.BodyPartNeck, entity, 0.3f);
        }

        #endregion

        #region UpperBodyCases

        private void UpperBodyCase1(int entity)
        {
            CreateGrazeWound(Locale.Data.BodyPartChest, entity);
        }

        private void UpperBodyCase2(int entity)
        {
            CreateFleshWound(Locale.Data.BodyPartChest, entity, 0.1f);
        }

        private void UpperBodyCase3(int entity)
        {
            CreatePenetratingWound(Locale.Data.BodyPartChest, entity, 0.2f, CritTypes.NERVES_DAMAGED,
                CritTypes.LUNGS_DAMAGED, CritTypes.HEART_DAMAGED);
        }

        private void UpperBodyCase4(int entity)
        {
            CreatePerforatingWound(Locale.Data.BodyPartChest, entity, 0.2f, CritTypes.NERVES_DAMAGED,
                CritTypes.LUNGS_DAMAGED, CritTypes.HEART_DAMAGED);
        }

        private void UpperBodyCase5(int entity)
        {
            CreateAvulsiveWound(Locale.Data.BodyPartChest, entity, 0.3f, CritTypes.NERVES_DAMAGED,
                CritTypes.LUNGS_DAMAGED, CritTypes.HEART_DAMAGED);
        }

        #endregion

        #region LowerBodyCases

        private void LowerBodyCase1(int entity)
        {
            CreateGrazeWound(Locale.Data.BodyPartLowerBody, entity);
        }

        private void LowerBodyCase2(int entity)
        {
            CreateFleshWound(Locale.Data.BodyPartLowerBody, entity, 0.05f);
        }

        private void LowerBodyCase3(int entity)
        {
            CreatePenetratingWound(Locale.Data.BodyPartLowerBody, entity, 0.1f, CritTypes.NERVES_DAMAGED,
                CritTypes.STOMACH_DAMAGED, CritTypes.GUTS_DAMAGED);
        }

        private void LowerBodyCase4(int entity)
        {
            CreatePerforatingWound(Locale.Data.BodyPartLowerBody, entity, 0.2f, CritTypes.NERVES_DAMAGED,
                CritTypes.STOMACH_DAMAGED, CritTypes.GUTS_DAMAGED);
        }

        private void LowerBodyCase5(int entity)
        {
            CreateAvulsiveWound(Locale.Data.BodyPartLowerBody, entity, 0.2f, CritTypes.NERVES_DAMAGED,
                CritTypes.STOMACH_DAMAGED, CritTypes.GUTS_DAMAGED);
        }

        #endregion

        #region ArmCases

        private void ArmCase1(int entity)
        {
            CreateGrazeWound(Locale.Data.BodyPartArm, entity);
        }

        private void ArmCase2(int entity)
        {
            CreateFleshWound(Locale.Data.BodyPartArm, entity, 0.01f);
        }

        private void ArmCase3(int entity)
        {
            CreatePenetratingWound(Locale.Data.BodyPartArm, entity, 0.05f, CritTypes.ARMS_DAMAGED);
        }

        private void ArmCase4(int entity)
        {
            CreatePerforatingWound(Locale.Data.BodyPartArm, entity, 0.05f, CritTypes.ARMS_DAMAGED);
        }

        private void ArmCase5(int entity)
        {
            CreateAvulsiveWound(Locale.Data.BodyPartArm, entity, 0.1f, CritTypes.ARMS_DAMAGED);
        }

        #endregion

        #region LegCases

        private void LegCase1(int entity)
        {
            CreateGrazeWound(Locale.Data.BodyPartLeg, entity);
        }

        private void LegCase2(int entity)
        {
            CreateFleshWound(Locale.Data.BodyPartLeg, entity, 0.05f);
        }

        private void LegCase3(int entity)
        {
            CreatePenetratingWound(Locale.Data.BodyPartLeg, entity, 0.1f, CritTypes.LEGS_DAMAGED);
        }

        private void LegCase4(int entity)
        {
            CreatePerforatingWound(Locale.Data.BodyPartLeg, entity, 0.1f, CritTypes.LEGS_DAMAGED);
        }

        private void LegCase5(int entity)
        {
            CreateAvulsiveWound(Locale.Data.BodyPartLeg, entity, 0.2f, CritTypes.LEGS_DAMAGED);
        }

        #endregion
    }
}