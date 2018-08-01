using System;
using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using GunshotWound2.Components.WoundComponents;
using Leopotam.Ecs;
using Weighted_Randomizer;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class BaseGunDamageSystem<T> : BaseDamageSystem<T> 
        where T : BaseWeaponHitComponent, new()
    {
        protected int GrazeWoundWeight;
        protected int FleshWoundWeight;
        protected int PenetratingWoundWeight;
        protected int PerforeatinWoundWeight;
        protected int AvulsiveWoundWeight;
        
        protected void FillWithDefaultGunActions()
        {
            DefaultAction = DefaultGrazeWound;
            
            HeadActions = new StaticWeightedRandomizer<Action<int>>
            {
                {HeadCase1, GrazeWoundWeight},
                {HeadCase2, FleshWoundWeight},
                {HeadCase3, PenetratingWoundWeight},
                {HeadCase4, PerforeatinWoundWeight},
                {HeadCase5, AvulsiveWoundWeight},
            };
            
            NeckActions = new StaticWeightedRandomizer<Action<int>>
            {
                {NeckCase1, GrazeWoundWeight},
                {NeckCase2, FleshWoundWeight},
                {NeckCase3, PenetratingWoundWeight},
                {NeckCase4, PerforeatinWoundWeight},
                {NeckCase5, AvulsiveWoundWeight},
            };
            
            UpperBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {UpperBodyCase1, GrazeWoundWeight},
                {UpperBodyCase2, FleshWoundWeight},
                {UpperBodyCase3, PenetratingWoundWeight},
                {UpperBodyCase4, PerforeatinWoundWeight},
                {UpperBodyCase5, AvulsiveWoundWeight},
            };
            
            LowerBodyActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LowerBodyCase1, GrazeWoundWeight},
                {LowerBodyCase2, FleshWoundWeight},
                {LowerBodyCase3, PenetratingWoundWeight},
                {LowerBodyCase4, PerforeatinWoundWeight},
                {LowerBodyCase5, AvulsiveWoundWeight},
            };
            
            ArmActions = new StaticWeightedRandomizer<Action<int>>
            {
                {ArmCase1, GrazeWoundWeight},
                {ArmCase2, FleshWoundWeight},
                {ArmCase3, PenetratingWoundWeight},
                {ArmCase4, PerforeatinWoundWeight},
                {ArmCase5, AvulsiveWoundWeight},
            };
            
            LegActions = new StaticWeightedRandomizer<Action<int>>
            {
                {LegCase1, GrazeWoundWeight},
                {LegCase2, FleshWoundWeight},
                {LegCase3, PenetratingWoundWeight},
                {LegCase4, PerforeatinWoundWeight},
                {LegCase5, AvulsiveWoundWeight},
            };
        }

        private void DefaultGrazeWound(int entity)
        {
            CreateWound("Graze GSW", entity, DamageMultiplier * 10f,
                BleeedingMultiplier * 0.05f, PainMultiplier * 20f);
        }

        private void CreateGrazeWound(string position, int entity)
        {
            CreateWound($"Graze GSW on {position}", entity, DamageMultiplier * 10f,
                BleeedingMultiplier * 0.05f, PainMultiplier * 20f);
        }

        private void CreateFleshWound(string position, int entity, float arteryDamageChance)
        {
            CreateWound($"Flesh GSW on {position}", entity, DamageMultiplier * 15f,
                BleeedingMultiplier * 0.1f, PainMultiplier * 30f, arteryDamageChance);
        }

        private void CreatePenetratingWound(string position, int entity, 
            float arteryDamageChance, params DamageTypes?[] possibleCrits)
        {
            CreateWound($"Penetrating GSW on {position}", entity, 
                DamageMultiplier * 20f, BleeedingMultiplier * 0.2f, PainMultiplier * 40f,
                arteryDamageChance, possibleCrits);
        }

        private void CreatePerforatingWound(string position, int entity,
            float arteryDamageChance, params DamageTypes?[] possibleCrits)
        {
            CreateWound($"Perforating GSW on {position}", entity, 
                DamageMultiplier * 20f, BleeedingMultiplier * 0.25f, PainMultiplier * 45f,
                arteryDamageChance, possibleCrits);
        }

        private void CreateAvulsiveWound(string position, int entity,
            float arteryDamageChance, params DamageTypes?[] possibleCrits)
        {
            CreateWound($"Avulsive GSW on {position}", entity, 
                DamageMultiplier * 30f, BleeedingMultiplier * 0.35f, PainMultiplier * 55f,
                arteryDamageChance, possibleCrits);
        }

        #region HeadCases

        private void HeadCase1(int pedEntity)
        {
            CreateGrazeWound("head", pedEntity);
        }
        private void HeadCase2(int pedEntity)
        {
            CreateWound("Damaged ear", pedEntity,
                DamageMultiplier * 10f, BleeedingMultiplier * 0.5f, PainMultiplier * 10f);
        }
        private void HeadCase3(int pedEntity)
        {
            CreateWound("Heavy brain damage", pedEntity, 
                DamageMultiplier * 70f, BleeedingMultiplier * 3f, PainMultiplier * 100);
        }
        private void HeadCase4(int pedEntity)
        {
            CreateWound($"{WeaponClass} bullet fly through your head", pedEntity, 
                DamageMultiplier * 70, BleeedingMultiplier * 3f, PainMultiplier * 100);
        }
        private void HeadCase5(int pedEntity)
        {
            CreateWound($"{WeaponClass} bullet torn apart your brain", pedEntity, 
                DamageMultiplier * 70f, BleeedingMultiplier * 3f, PainMultiplier * 100);
        }

        #endregion

        #region NeckCases

        private void NeckCase1(int entity)
        {
            CreateGrazeWound("neck", entity);
        }
        private void NeckCase2(int entity)
        {
            CreateFleshWound("neck", entity, 0.05f);
        }
        private void NeckCase3(int entity)
        {
            CreatePenetratingWound("neck", entity, 0.2f);
        }
        private void NeckCase4(int entity)
        {
            CreatePerforatingWound("neck", entity, 0.1f);
        }
        private void NeckCase5(int entity)
        {
            CreateAvulsiveWound("neck", entity, 0.3f);
        }

        #endregion

        #region UpperBodyCases

        private void UpperBodyCase1(int entity)
        {
            CreateGrazeWound("chest", entity);
        }
        private void UpperBodyCase2(int entity)
        {
            CreateFleshWound("chest", entity, 0.1f);
        }
        private void UpperBodyCase3(int entity)
        {
            CreatePenetratingWound("chest", entity, 0.2f, DamageTypes.NERVES_DAMAGED,
                DamageTypes.LUNGS_DAMAGED, DamageTypes.HEART_DAMAGED, null, null);
        }
        private void UpperBodyCase4(int entity)
        {
            CreatePerforatingWound("chest", entity, 0.2f, DamageTypes.NERVES_DAMAGED,
                DamageTypes.LUNGS_DAMAGED, DamageTypes.HEART_DAMAGED, null, null);
        }
        private void UpperBodyCase5(int entity)
        {
            CreateAvulsiveWound("chest", entity, 0.3f, DamageTypes.NERVES_DAMAGED,
                DamageTypes.LUNGS_DAMAGED, DamageTypes.HEART_DAMAGED);
        }

        #endregion

        #region LowerBodyCases

        private void LowerBodyCase1(int entity)
        {
            CreateGrazeWound("lower body", entity);
        }
        private void LowerBodyCase2(int entity)
        {
            CreateFleshWound("lower body", entity, 0.1f);
        }
        private void LowerBodyCase3(int entity)
        {
            CreatePenetratingWound("lower body", entity, 0.2f, DamageTypes.NERVES_DAMAGED,
                DamageTypes.STOMACH_DAMAGED, DamageTypes.GUTS_DAMAGED, null, null);
        }
        private void LowerBodyCase4(int entity)
        {
            CreatePerforatingWound("lower body", entity, 0.2f, DamageTypes.NERVES_DAMAGED,
                DamageTypes.STOMACH_DAMAGED, DamageTypes.GUTS_DAMAGED, null, null);
        }
        private void LowerBodyCase5(int entity)
        {
            CreateAvulsiveWound("lower body", entity, 0.3f, DamageTypes.NERVES_DAMAGED,
                DamageTypes.STOMACH_DAMAGED, DamageTypes.GUTS_DAMAGED);
        }

        #endregion

        #region ArmCases

        private void ArmCase1(int entity)
        {
            CreateGrazeWound("arm", entity);
        }
        private void ArmCase2(int entity)
        {
            CreateFleshWound("arm", entity, 0.05f);
        }
        private void ArmCase3(int entity)
        {
            CreatePenetratingWound("arm", entity, 0.1f, DamageTypes.ARMS_DAMAGED);
        }
        private void ArmCase4(int entity)
        {
            CreatePerforatingWound("arm", entity, 0.1f, DamageTypes.ARMS_DAMAGED);
        }
        private void ArmCase5(int entity)
        {
            CreateAvulsiveWound("arm", entity, 0.2f, DamageTypes.ARMS_DAMAGED);
        }

        #endregion

        #region LegCases

        private void LegCase1(int entity)
        {
            CreateGrazeWound("leg", entity);
        }
        private void LegCase2(int entity)
        {
            CreateFleshWound("leg", entity, 0.1f);
        }
        private void LegCase3(int entity)
        {
            CreatePenetratingWound("leg", entity, 0.2f, DamageTypes.LEGS_DAMAGED);
        }
        private void LegCase4(int entity)
        {
            CreatePerforatingWound("leg", entity, 0.2f, DamageTypes.LEGS_DAMAGED);
        }
        private void LegCase5(int entity)
        {
            CreateAvulsiveWound("leg", entity, 0.3f, DamageTypes.LEGS_DAMAGED);
        }

        #endregion
    }
}